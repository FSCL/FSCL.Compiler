namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_UNION_CASE_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>]
type UnionCasePrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(e, engine:FunctionPrettyPrintingStep) =
            match e with
            | Patterns.NewUnionCase(ui, args) ->
                Some(ui.Name)
            | _ ->
                None