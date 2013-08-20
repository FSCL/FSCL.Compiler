namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Reflection

[<StepProcessor("FSCL_RETURN_TYPE_TO_OUTPUT_ARG_REPLACING_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies = [|"FSCL_ARGS_BUILDING_PREPROCESSING_PROCESSOR"|])>]
type ReturnTypeToOutputArgProcessor() =
    inherit FunctionPreprocessingProcessor()

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
                
            // Add return arrays
            for i = 0 to returnedVars.Length - 1 do
                let pInfo = new KernelParameterInfo((fst returnedVars.[i]).Name, (fst returnedVars.[i]).Type)
                pInfo.IsReturnParameter <- true
                kernelInfo.Parameters.Add(pInfo)
            
            // Change connections bound to the return types of this kernel
            // NB: this modifies the call graph
            for i = 0 to returnedVars.Length - 1 do
                step.ChangeKernelOutputPoint(ReturnValue(i), OutArgument((fst returnedVars.[i]).Name))
       
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
       
