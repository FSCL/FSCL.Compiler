namespace FSCL.Compiler.Processors

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<StepProcessor("FSCL_STRUCT_ACCESS_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>] 
type StructAccessPrinter() =                 
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.PropertyGet(e, propertyInfo, args) ->
                if e.IsSome then
                    let t = e.Value.Type
                    if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then
                        Some(engine.Continue(e.Value) + "." + propertyInfo.Name)
                    else
                        None
                else 
                    None
            | Patterns.PropertySet(e, propertyInfo, args, body) ->                
                if e.IsSome then
                    let t = e.Value.Type
                    if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then
                        Some(engine.Continue(e.Value) + "." + propertyInfo.Name + " = " + engine.Continue(body))
                    else
                        None
                else 
                    None
            | _ ->
                None