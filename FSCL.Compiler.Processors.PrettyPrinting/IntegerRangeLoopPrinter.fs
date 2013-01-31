namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type IntegerRangeLoopPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.ForIntegerRangeLoop(var, startexpr, endexp, body) ->
                Some("for(" + engine.TypeManager.Print(var.Type) + " " + var.Name + " = " + engine.Continue(startexpr) + "; " + var.Name + " <= " + engine.Continue(endexp) + ";" + var.Name + "++) {\n" + engine.Continue(body) + "\n}\n")
            | _ ->
                None
           