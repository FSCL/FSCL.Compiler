namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type WhileLoopPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.WhileLoop(cond, body) ->
                Some("while(" + engine.Continue(cond) + ") {\n" + engine.Continue(body) + "\n}\n")
            | _ ->
                None