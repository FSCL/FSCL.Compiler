namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_CALL_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP",
                Dependencies = [| "FSCL_ARRAY_ACCESS_PRETTY_PRINTING_PROCESSOR";
                                  "FSCL_ARITH_OP_PRETTY_PRINTING_PROCESSOR" |])>]
type CallPrinter() =
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with
            | Patterns.Call (o, mi, a) ->
                let returnTags = engine.FunctionInfo.CustomInfo.["RETURN_EXPRESSIONS"] :?> Expr list
                let returnPrefix = 
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

                let args = String.concat ", " (List.map (fun (e:Expr) -> engine.Continue(e)) a)
                if mi.DeclaringType.Name = "KernelLanguage" &&  mi.Name = "barrier" then
                    // the function is defined in FSCL
                    Some(returnPrefix + mi.Name + "(" + args + ");" + returnPostfix)
                else
                    Some(returnPrefix + mi.Name + "(" + args + ")" + returnPostfix)
            | _ ->
                None