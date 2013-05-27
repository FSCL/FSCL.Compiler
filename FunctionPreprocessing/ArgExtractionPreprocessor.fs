namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System

[<StepProcessor("FSCL_ARG_EXTRACTION_PREPROCESSING_PROCESSOR", "FSCL_FUNCTION_PREPROCESSING_STEP")>] 
type ArgExtractionPreprocessor() =
    let rec LiftArgExtraction (expr, parameters: ParameterInfo[]) =
        match expr with
        | Patterns.Lambda(v, e) ->
            if v.Name = "tupledArg" then
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

    interface FunctionPreprocessingProcessor with
        member this.Process(fi, en) =
            fi.Body <- LiftArgExtraction(fi.Body, fi.Signature.GetParameters())
            

