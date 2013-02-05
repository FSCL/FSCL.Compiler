namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_INTEGER_RANGE_LOOP_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type IntegerRangeLoopPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with
            | Patterns.ForIntegerRangeLoop(var, startexpr, endexp, body) ->
                Some("for(" + engine.TypeManager.Print(var.Type) + " " + var.Name + " = " + engine.Continue(startexpr) + "; " + var.Name + " <= " + engine.Continue(endexp) + ";" + var.Name + "++) {\n" + engine.Continue(body) + "\n}\n")
            | _ ->
                None
           