namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.QuotationEvaluation
open System.Collections.Generic
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices

[<StepProcessor("FSCL_RETURN_TYPE_TO_OUTPUT_ARG_REPLACING_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies = [|"FSCL_ARGS_BUILDING_PREPROCESSING_PROCESSOR"|])>]
type ReturnTypeToOutputArgProcessor() =
    inherit FunctionPreprocessingProcessor()
    (*
    let ExtractAllocationParameters(e: Expr) =
        match e with
        | Patterns.Call(o, methodInfo, args) ->               
            let elementType = 
                if (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
                        Some(methodInfo.GetGenericArguments().[0])
                else
                    None
            if elementType.IsSome then
                Some(elementType.Value, args)
            else
                None
        | _ ->
            None *)

    member private this.LiftArgumentsAndKernelCalls(e: Expr,
                                                    args: Dictionary<string, obj>,
                                                    localSize: int array,
                                                    globalSize: int array) =
        match e with
        | Patterns.Call(o, m, arguments) ->
            if m.DeclaringType.Name = "KernelLanguage" && (m.Name = "get_global_size") then
                Expr.Value(globalSize.[arguments.[0].EvalUntyped() :?> int])
            else if m.DeclaringType.Name = "KernelLanguage" && (m.Name = "get_local_size") then
                Expr.Value(localSize.[arguments.[0].EvalUntyped() :?> int])
            else if m.DeclaringType.Name = "KernelLanguage" && (m.Name = "get_num_groups") then
                let gs = globalSize.[arguments.[0].EvalUntyped() :?> int]
                let ls = localSize.[arguments.[0].EvalUntyped() :?> int]
                Expr.Value(int (Math.Ceiling(float gs / float ls)))
            else if m.DeclaringType.Name = "KernelLanguage" && (m.Name = "get_work_dim") then
                Expr.Value(globalSize.Rank)
            else
                e
        | Patterns.Var(v) ->
            if (args.ContainsKey(v.Name)) then
                Expr.Value(args.[v.Name])
            else
                e
        | ExprShape.ShapeVar(v) ->
            e
        | ExprShape.ShapeLambda(l, b) ->
            failwith "Error in substituting parameters"
        | ExprShape.ShapeCombination(c, argsList) ->
            ExprShape.RebuildShapeCombination(c, List.map(fun (e: Expr) -> this.LiftArgumentsAndKernelCalls(e, args, localSize, globalSize)) argsList)


    member private this.EvaluateReturnedBufferAllocationSize(t: Type,
                                                             sizes: Expr list,
                                                             args: Dictionary<string, obj>, 
                                                             localSize: int array,
                                                             globalSize: int array) =        
        let intSizes = sizes |> 
                        List.map(fun (e:Expr) -> this.LiftArgumentsAndKernelCalls(e, args, localSize, globalSize).EvalUntyped() :?> int)
        ExplicitAllocationSize(intSizes |> List.toArray)           

    member private this.AddReturnTypeVar(kernel:FunctionInfo, var:Var, args:Expr list) =
        if (var.IsMutable) then
            raise (new CompilerException("A kernel returned variable must be immutable"))
                        
        if not (kernel.CustomInfo.ContainsKey("KERNEL_RETURN_TYPE")) then
            kernel.CustomInfo.Add("KERNEL_RETURN_TYPE", [ (var, args) ])
        else 
            let current = kernel.CustomInfo.["KERNEL_RETURN_TYPE"] :?> (Var * Expr list) list
            // If not already added
            if (List.tryFind(fun (v:Var, args:Expr list) -> v = var) current).IsNone then
                kernel.CustomInfo.["KERNEL_RETURN_TYPE"] <- current @ [ (var, args) ]

    member private this.CorrectSignature(kernel:FunctionInfo, step:FunctionPreprocessingStep) =    
        if kernel.CustomInfo.ContainsKey("KERNEL_RETURN_TYPE") then
            let returnedVars = kernel.CustomInfo.["KERNEL_RETURN_TYPE"] :?> (Var * Expr list) list

            // Fix signature and kernel parameters
            let kernelInfo = kernel :?> KernelInfo
                        
            // Change return type 
            kernelInfo.ReturnType <- typeof<unit>
            
            // Get flow graph nodes matching the current kernel    
            let nodes = FlowGraphManager.GetKernelNodes(step.FunctionInfo.ID, step.FlowGraph)

            // Add return arrays
            for (v, sizes) in returnedVars do
                let pInfo = new KernelParameterInfo(v.Name, v.Type)
                pInfo.IsReturnParameter <- true
                kernelInfo.Parameters.Add(pInfo)
            
                // Set new argument    
                for item in nodes do
                    FlowGraphManager.SetNodeInput(item, 
                                                  pInfo.Name, 
                                                  ReturnedBufferAllocationSize(
                                                    fun(args, localSize, globalSize) ->
                                                        this.EvaluateReturnedBufferAllocationSize(v.Type.GetElementType(), sizes, args, localSize, globalSize)))
            
            // Change connections bound to the return types of this kernel
            // NB: this modifies the call graph
            //for i = 0 to returnedVars.Length - 1 do
                //step.ChangeKernelOutputPoint(ReturnValue(i), OutArgument((fst returnedVars.[i]).Name))
       
    member private this.FindReturnedArraysAllocationExpression(expr:Expr, step:FunctionPreprocessingStep, kernel:FunctionInfo) =
        match expr with
        | Patterns.Let(var, value, body) ->                        
            match value with
            | Patterns.Call(o, methodInfo, args) ->               
                if (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
                    // Only zero create allocation is permitted and it must be assigned to a non mutable variable
                    this.AddReturnTypeVar(kernel, var, args)
                for a in args do
                    this.FindReturnedArraysAllocationExpression(a, step, kernel)
            | _ ->
                this.FindReturnedArraysAllocationExpression(value, step, kernel)
            this.FindReturnedArraysAllocationExpression(body, step, kernel)
        | ExprShape.ShapeLambda(v, e) ->   
            this.FindReturnedArraysAllocationExpression(e, step, kernel)
        | ExprShape.ShapeCombination(o, args) ->   
            List.iter(fun (e:Expr) ->  this.FindReturnedArraysAllocationExpression(e, step, kernel)) args
        | ExprShape.ShapeVar(v) ->
            ()
        
    override this.Run(fInfo, en) =
        let engine = en :?> FunctionPreprocessingStep
        (*
        // Split components types in case of tuple return type
        let setOfReturnedTypes = 
            if FSharpType.IsTuple(fInfo.Signature.ReturnType) then
                FSharpType.GetTupleElements(fInfo.Signature.ReturnType)
            else
                [| fInfo.Signature.ReturnType |] *)
        // Look for declaration of a variable for each element in the set of returned types
        this.FindReturnedArraysAllocationExpression(fInfo.Body, engine, fInfo)
        // Fix signature
        this.CorrectSignature(fInfo, engine)
       
