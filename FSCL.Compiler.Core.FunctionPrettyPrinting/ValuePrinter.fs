namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VALUE_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type ValuePrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(e, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match e with
            | Patterns.Value(v, t) ->
                if (v = null) then
                    Some("")
                else
                    Some(v.ToString())
            | _ ->
                None