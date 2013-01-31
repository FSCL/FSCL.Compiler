namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type ValuePrinter() =   
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(e, engine:FunctionPrettyPrintingStep) =
            match e with
            | Patterns.Value(v, t) ->
                if (v = null) then
                    Some("")
                else
                    Some(v.ToString())
            | _ ->
                None