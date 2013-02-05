namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open FSCL.Compiler.Types
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_DECLARATION_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP",
                [| "FSCL_FOR_RANGE_PRETTY_PRINTING_PROCESSOR" |])>]
type DeclarationPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with
            | Patterns.Let(v, value, body) ->
                Some(engine.TypeManager.Print(v.Type) + " " + v.Name + " = " + engine.Continue(value) + ";\n" + engine.Continue(body))
            | _ ->
                None