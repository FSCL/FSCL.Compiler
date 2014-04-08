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
                "FSCL_FUNCTION_PREPROCESSING_STEP")>]
                //Dependencies = [|"FSCL_ARGS_BUILDING_PREPROCESSING_PROCESSOR"|])>]
type DynamicArrayToParameterProcessor() =
    inherit FunctionPreprocessingProcessor()
    
    member private this.AddDynamicArrayParameter(step: FunctionPreprocessingStep, kernel:FunctionInfo, var:Var, allocationArgs:Expr array) =
        if (var.IsMutable) then
            raise (new CompilerException("A kernel dynamic array must be immutable"))
                   
        // Fix signature and kernel parameters
        let kernelInfo = kernel :?> KernelInfo

        // Add parameter
        let pInfo = new FunctionParameter(var.Name, 
                                          var.Type, 
                                          DynamicParameter(allocationArgs),
                                          None)
        kernelInfo.GeneratedParameters.Add(pInfo)
       
    member private this.FindArrayAllocationExpression(expr:Expr, step:FunctionPreprocessingStep, kernel:FunctionInfo) =
        match expr with
        | Patterns.Let(var, value, body) ->                        
            match value with
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
       
