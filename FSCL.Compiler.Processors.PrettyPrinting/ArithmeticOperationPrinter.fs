namespace FSCL.Compiler.Processors

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
                match expr with
                | DerivedPatterns.SpecificCall <@ (>) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" > ", a, engine))
                | DerivedPatterns.SpecificCall <@ (<) @> (e, t, a)  -> 
                    Some(HandleBinaryOp(" < ", a, engine))
                | DerivedPatterns.SpecificCall <@ (>=) @> (e, t, a)  -> 
                    Some(HandleBinaryOp(" >= ", a, engine))
                | DerivedPatterns.SpecificCall <@ (<=) @> (e, t, a)  -> 
                    Some(HandleBinaryOp(" <= ", a, engine))
                | DerivedPatterns.SpecificCall <@ (=) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" == ", a, engine))
                | DerivedPatterns.SpecificCall <@ (<>) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" != ", a, engine))
                | DerivedPatterns.SpecificCall <@ (+) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" + ", a, engine))
                | DerivedPatterns.SpecificCall <@ (*) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" * ", a, engine))
                | DerivedPatterns.SpecificCall <@ (-) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" - ", a, engine))
                | DerivedPatterns.SpecificCall <@ (/) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" / ", a, engine))
                | DerivedPatterns.SpecificCall <@ (%) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" % ", a, engine))
                | DerivedPatterns.SpecificCall <@ (&&) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" && ", a, engine))
                | DerivedPatterns.SpecificCall <@ (||) @> (e, t, a) ->
                    Some(HandleBinaryOp(" || ", a, engine))
                | DerivedPatterns.SpecificCall <@ (&&&) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" & ", a, engine))
                | DerivedPatterns.SpecificCall <@ (|||) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" | ", a, engine))
                | DerivedPatterns.SpecificCall <@ (^^^) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" ^ ", a, engine))
                | DerivedPatterns.SpecificCall <@ (~~~) @> (e, t, a) -> 
                    Some(HandleUnaryOp(" ~ ", a, engine))
                | DerivedPatterns.SpecificCall <@ (not) @> (e, t, a) -> 
                    Some(HandleUnaryOp(" ! ", a, engine))
                | DerivedPatterns.SpecificCall <@ (>>>) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" >> ", a, engine))
                | DerivedPatterns.SpecificCall <@ (<<<) @> (e, t, a) -> 
                    Some(HandleBinaryOp(" << ", a, engine))
                | _ ->
                    None
            | _ ->
                None