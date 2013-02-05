namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VAR_SET_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type VarSetPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.VarSet (v, e) ->
                Some(v.Name + " = " + engine.Continue(e) + ";")
            | _ ->
                None