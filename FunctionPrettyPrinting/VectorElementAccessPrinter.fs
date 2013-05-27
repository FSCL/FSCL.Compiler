namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open System.Reflection

[<StepProcessor("FSCL_VECTOR_ELEMENT_ACCESS_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP")>] 
type VectorElementAccessPRinter() =                 
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with    
            | Patterns.PropertyGet(e, propertyInfo, args) ->
                if e.IsSome then
                    let t = e.Value.Type
                    if (t.Assembly.GetName().Name = "FSCL.Compiler.Core.Language") then
                        Some(engine.Continue(e.Value) + "." + propertyInfo.Name)
                    else
                        None
                else 
                    None
            | Patterns.PropertySet(e, propertyInfo, args, body) ->                
                if e.IsSome then
                    let t = e.Value.Type
                    if (t.Assembly.GetName().Name = "FSCL.Compiler.Core.Language") then
                        Some(engine.Continue(e.Value) + "." + propertyInfo.Name + " = " + engine.Continue(body) + ";\n")
                    else
                        None
                else 
                    None
            | _ ->
                None