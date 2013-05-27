namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VAR_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type VarPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with
            | Patterns.Var(v) ->
                let returnTags = engine.FunctionInfo.CustomInfo.["RETURN_EXPRESSIONS"] :?> Expr list
                let returnPrefix = 
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""
                Some(returnPrefix + v.Name + returnPostfix)
            | _ ->
                None