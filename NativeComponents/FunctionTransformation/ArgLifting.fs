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
    let rec LiftArgExtraction (expr, f:FunctionInfo) =
        match expr with
        | Patterns.Lambda(v, e) ->
            if v.Name = "tupledArg" then
                LiftArgExtraction(e, f)                
            else if v.Name = "this" then
                LiftArgExtraction(e, f)
            else
                if f.GetParameter(v.Name).IsSome then
                    LiftArgExtraction (e, f)
                else
                    expr
        | Patterns.Let(v, value, body) ->
            if f.GetParameter(v.Name).IsSome then
                LiftArgExtraction (body, f)
            else
                expr
        | _ ->
            expr
        
    override this.Run(exp, en) =
        let step = en :?> FunctionTransformationStep
        LiftArgExtraction(exp, step.FunctionInfo)
            

