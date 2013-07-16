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
    let rec LiftArgExtraction (expr, parameters: ParameterInfo[]) =
        match expr with
        | Patterns.Lambda(v, e) ->
            if v.Name = "tupledArg" then
                LiftArgExtraction(e, parameters)                
            else if v.Name = "this" then
                LiftArgExtraction(e, parameters)
            else
                let el = Array.tryFind (fun (p:ParameterInfo) -> p.Name = v.Name) parameters
                if el.IsSome then
                    LiftArgExtraction (e, parameters)
                else
                    expr
        | Patterns.Let(v, value, body) ->
            let el = Array.tryFind (fun (p:ParameterInfo) -> p.Name = v.Name) parameters
            if el.IsSome then
                LiftArgExtraction (body, parameters)
            else
                expr
        | _ ->
            expr
        
    override this.Run(exp, en) =
        let step = en :?> FunctionTransformationStep
        LiftArgExtraction(exp, step.FunctionInfo.Signature.GetParameters())
            

