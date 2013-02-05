namespace FSCL.Compiler.Processors

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<StepProcessor("FSCL_STRUCT_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP",
                [| "FSCL_FOR_RANGE_PRETTY_PRINTING_PROCESSOR" |], 
                [| "FSCL_DECLARATION_PRETTY_PRINTING_PROCESSOR" |])>] 
type StructPrinter() =                 
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.Let(v, value, body) ->
                match value with
                | Patterns.DefaultValue(t) ->
                    if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then
                        Some(engine.TypeManager.Print(v.Type) + " " + v.Name + ";\n" + engine.Continue(body))
                    else
                        None
                | _ ->
                    None
            | _ ->
                None