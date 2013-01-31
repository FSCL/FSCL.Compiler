namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type UnionCasePrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(e, engine:FunctionPrettyPrintingStep) =
            match e with
            | Patterns.NewUnionCase(ui, args) ->
                Some(ui.Name)
            | _ ->
                None