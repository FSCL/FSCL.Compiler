namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VAR_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type VarPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.Var(v) ->
                Some(v.Name)
            | _ ->
                None