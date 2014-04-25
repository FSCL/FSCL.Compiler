namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Compiler.Language
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Collections.Generic
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices

//RETURN_TYPE_TO_OUTPUT_ARG_REPLACING
[<StepProcessor("FSCL_DYNAMIC_ARRAY_TO_PARAMETER_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies = [|"FSCL_KERNEL_RETURN_DISCOVERY_PROCESSOR"|])>]
type DynamicArrayToParameterProcessor() =
    inherit FunctionPreprocessingProcessor()
    
    member private this.AddDynamicArrayParameter(step: FunctionPreprocessingStep, kernel:FunctionInfo, var:Var, allocationArgs:Expr array) =
        if (var.IsMutable) then
            raise (new CompilerException("A kernel dynamic array must be immutable"))
                   
        let kernelInfo = kernel :?> KernelInfo
        
        // Fix signature and kernel parameters
        let pInfo =         
            // Check if RETURN_VARIABLE is set in kernel custom infos. This means, during return var discovery no original parameter has
            // been found that match that var. It may match this one        
            if kernel.CustomInfo.ContainsKey("RETURN_VARIABLE") then
                let rv = kernel.CustomInfo.["RETURN_VARIABLE"] :?> Var     
                if rv = var then
                    // Match: we must set the additionar parameter meta accordingly  
                    kernel.CustomInfo.Remove("RETURN_VARIABLE") |> ignore                  
                    let p = FunctionParameter(var.Name, 
                                                var, 
                                                DynamicParameter(allocationArgs),
                                                Some(kernelInfo.Meta.ReturnMeta :> IParamMetaCollection))
                    p.IsReturned <- true
                    p
                else
                    // No match, no meta
                    new FunctionParameter(var.Name, 
                                            var, 
                                            DynamicParameter(allocationArgs),
                                            None)
            else
                // No return variable or return variable already set
                new FunctionParameter(var.Name, 
                                        var, 
                                        DynamicParameter(allocationArgs),
                                        None)
        // Add parameter
        kernelInfo.GeneratedParameters.Add(pInfo)
       
    member private this.FindArrayAllocationExpression(expr:Expr, step:FunctionPreprocessingStep, kernel:FunctionInfo) =
        match expr with
        | Patterns.Let(var, value, body) ->                        
            match value with
            // Do not consider __local data
            | DerivedPatterns.SpecificCall <@ local @> (args) ->
                ()
            | Patterns.Call(o, methodInfo, args) ->               
                if (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
                    // Only zero create allocation is permitted and it must be assigned to a non mutable variable
                    this.AddDynamicArrayParameter(step, kernel, var, args |> List.toArray)
                for a in args do
                    this.FindArrayAllocationExpression(a, step, kernel)
            | _ ->
                this.FindArrayAllocationExpression(value, step, kernel)
            this.FindArrayAllocationExpression(body, step, kernel)
        | ExprShape.ShapeLambda(v, e) ->   
            this.FindArrayAllocationExpression(e, step, kernel)
        | ExprShape.ShapeCombination(o, args) ->   
            List.iter(fun (e:Expr) ->  this.FindArrayAllocationExpression(e, step, kernel)) args
        | ExprShape.ShapeVar(v) ->
            ()
        
    override this.Run(fInfo, en, opts) =
        let engine = en :?> FunctionPreprocessingStep
        if (fInfo :? KernelInfo) then
            this.FindArrayAllocationExpression(fInfo.Body, engine, fInfo)
       
