namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Reflection

[<StepProcessor("FSCL_ARITH_OP_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type ArithmeticOperationPrinter() =
    let HandleBinaryOp (op, a:Expr list, engine:FunctionPrettyPrintingStep) =
        "(" + engine.Continue(a.[0]) + ")" + op + "(" + engine.Continue(a.[1]) + ")"
    let HandleUnaryOp (op, a:Expr list, engine:FunctionPrettyPrintingStep) =
        op + engine.Continue(a.[0])

    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with 
            | Patterns.Call(o, mi, args) ->
                let returnTags = engine.FunctionInfo.CustomInfo.["RETURN_EXPRESSIONS"] :?> Expr list
                let returnPrefix = 
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

                match expr with
                | DerivedPatterns.SpecificCall <@ (>) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" > ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (<) @> (e, t, a)  -> 
                    Some(returnPrefix + HandleBinaryOp(" < ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (>=) @> (e, t, a)  -> 
                    Some(returnPrefix + HandleBinaryOp(" >= ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (<=) @> (e, t, a)  -> 
                    Some(returnPrefix + HandleBinaryOp(" <= ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (=) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" == ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (<>) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" != ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (+) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" + ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (*) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" * ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (-) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" - ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (/) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" / ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (%) @> (e, t, a) -> 
                    Some(returnPrefix + returnPrefix + HandleBinaryOp(" % ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (&&) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" && ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (||) @> (e, t, a) ->
                    Some(returnPrefix + HandleBinaryOp(" || ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (&&&) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" & ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (|||) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" | ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (^^^) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" ^ ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (~~~) @> (e, t, a) -> 
                    Some(returnPrefix + HandleUnaryOp(" ~ ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (not) @> (e, t, a) -> 
                    Some(returnPrefix + HandleUnaryOp(" ! ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (>>>) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" >> ", a, engine) + returnPostfix)
                | DerivedPatterns.SpecificCall <@ (<<<) @> (e, t, a) -> 
                    Some(returnPrefix + HandleBinaryOp(" << ", a, engine) + returnPostfix)
                | _ ->
                    None
            | _ ->
                None