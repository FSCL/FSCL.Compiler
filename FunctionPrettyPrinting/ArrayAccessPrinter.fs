namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<StepProcessor("FSCL_ARRAY_ACCESS_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type ArrayAccessPrinter() =                 
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =        
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with
            | Patterns.Call(o, methodInfo, args) ->
                if methodInfo.DeclaringType.Name = "IntrinsicFunctions" then
                    let returnTags = engine.FunctionInfo.CustomInfo.["RETURN_EXPRESSIONS"] :?> Expr list
                    let returnPrefix = 
                        if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                            "return "
                        else
                            ""
                    let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

                    let arrayName = engine.Continue(args.[0])
                    if methodInfo.Name = "GetArray" then
                        Some(returnPrefix  + arrayName + "[" + engine.Continue(args.[1]) + "]" + returnPostfix)
                    elif methodInfo.Name = "SetArray" then
                        Some(arrayName + "[" + engine.Continue(args.[1]) + "] = " + engine.Continue(args.[2]) + ";\n")
                    else
                        None
                else
                    None
            | _ ->
                None