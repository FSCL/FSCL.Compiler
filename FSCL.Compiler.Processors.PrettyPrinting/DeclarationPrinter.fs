namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DeclarationPrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.Let(v, value, body) ->
                Some(engine.TypeManager.Print(v.Type) + " " + v.Name + " = " + engine.Continue(value) + ";\n" + engine.Continue(body))
            | _ ->
                None