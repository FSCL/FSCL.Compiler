namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System

[<StepProcessor("FSCL_ARG_LIFTING_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP")>] 
type ArgExtractionPreprocessor() =
    inherit FunctionTransformationProcessor()
    let rec LiftArgExtraction (expr, parameters:Dictionary<String, KernelParameterInfo>) =
        match expr with
        | Patterns.Lambda(v, e) ->
            if v.Name = "tupledArg" then
                LiftArgExtraction(e, parameters)                
            else if v.Name = "this" then
                LiftArgExtraction(e, parameters)
            else
                if parameters.ContainsKey(v.Name) then
                    LiftArgExtraction (e, parameters)
                else
                    expr
        | Patterns.Let(v, value, body) ->
            if parameters.ContainsKey(v.Name) then
                LiftArgExtraction (body, parameters)
            else
                expr
        | _ ->
            expr
        
    override this.Run(exp, en) =
        let step = en :?> FunctionTransformationStep
        LiftArgExtraction(exp, step.FunctionInfo.Parameters)
            

