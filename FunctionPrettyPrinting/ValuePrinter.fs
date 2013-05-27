namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VALUE_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type ValuePrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with
            | Patterns.Value(v, t) ->
                let returnTags = engine.FunctionInfo.CustomInfo.["RETURN_EXPRESSIONS"] :?> Expr list
                let returnPrefix = 
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

                if (v = null) then
                    Some("")
                else
                    Some(returnPrefix + v.ToString() + returnPostfix)
            | _ ->
                None