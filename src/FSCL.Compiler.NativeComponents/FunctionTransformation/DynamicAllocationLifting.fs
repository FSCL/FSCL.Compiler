namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Reflection

[<StepProcessor("FSCL_DYNAMIC_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR", 
                "FSCL_FUNCTION_TRANSFORMATION_STEP")>]//,
               // Dependencies = [| "Z" |])>]
type DynamicAllocationLiftingProcessor() =
    inherit FunctionTransformationProcessor()
            
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionTransformationStep
        match expr with
        | Patterns.Let(var, value, body) ->
            match value with
            | Patterns.Call(o, methodInfo, args) ->               
                if methodInfo.DeclaringType <> null &&
                   ((methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate")) then
                    (*if engine.FunctionInfo.CustomInfo.ContainsKey("RETURN_TYPE") then
                        let returnedVars = engine.FunctionInfo.CustomInfo.["RETURN_TYPE"] :?> (Var * Expr list) list                        
                        if (List.tryFind(fun (v:Var, args:Expr list) -> v = var) returnedVars).IsSome then*)
                            engine.Continue(body)
                        //else
                            //engine.Default(expr)
                    //else
                        //engine.Default(expr)
                else
                    engine.Default(expr)
            | _ ->           
                engine.Default(expr)
        | _ ->
            engine.Default(expr)
