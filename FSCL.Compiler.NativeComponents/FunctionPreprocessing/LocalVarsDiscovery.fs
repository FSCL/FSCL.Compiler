namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Collections.Generic
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices

[<StepProcessor("FSCL_LOCAL_VARS_DISCOVERY_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies=[| "FSCL_ARGS_PREP_LIFTING_PREPROCESSING_PROCESSOR" |])>]
type LocalVarsDictionaryProcessor() =
    inherit FunctionPreprocessingProcessor()
    
    member private this.AddDynamicArrayParameter(step: FunctionPreprocessingStep, kernel:FunctionInfo, var:Var, allocationArgs:Expr array) =
        if (var.IsMutable) then
            raise (new CompilerException("A kernel dynamic array must be immutable"))
                   
        // Fix signature and kernel parameters
        let kernelInfo = kernel :?> KernelInfo

        // Add parameter
        let pInfo = new FunctionParameter(var.Name, 
                                          var, 
                                          DynamicParameter(allocationArgs),
                                          None)
        kernelInfo.GeneratedParameters.Add(pInfo)
       
    member private this.FindLocalVars(expr:Expr, step:FunctionPreprocessingStep, kernel:KernelInfo) =
        match expr with
        | Patterns.Let(var, value, body) ->                        
            match value with
            // Do not consider __local data
            | DerivedPatterns.SpecificCall <@ local @> (o, mi, a) ->
                if a.Length > 0 then
                    match a.[0] with
                    | Patterns.Call(o, methodInfo, args) ->               
                        if (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") then
                            // Local 1D vector
                            let vType = var.Type.GetElementType()
                            kernel.LocalVars.Add(var, (vType, Some [ args.[0] ]))
                        else if (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") then
                            // Local 2D vector
                            let vType = var.Type.GetElementType()
                            kernel.LocalVars.Add(var, (vType, Some [ args.[0]; args.[1] ]))
                        else if (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
                            // Local 3D vector
                            let vType = var.Type.GetElementType()
                            kernel.LocalVars.Add(var, (vType, Some [ args.[0]; args.[1]; args.[2] ]))
                    | _ ->
                        ()
                else
                    // Local scalar
                    kernel.LocalVars.Add(var, (var.Type, None))
            | _ ->
                ()
            this.FindLocalVars(value, step, kernel)
            this.FindLocalVars(body, step, kernel)
        | ExprShape.ShapeLambda(v, e) ->   
            this.FindLocalVars(e, step, kernel)
        | ExprShape.ShapeCombination(o, args) ->   
            List.iter(fun (e:Expr) ->  this.FindLocalVars(e, step, kernel)) args
        | ExprShape.ShapeVar(v) ->
            ()
        
    override this.Run(fInfo, en, opts) =
        let engine = en :?> FunctionPreprocessingStep
        if (fInfo :? KernelInfo) then
            this.FindLocalVars(fInfo.Body, engine, fInfo :?> KernelInfo)
       
