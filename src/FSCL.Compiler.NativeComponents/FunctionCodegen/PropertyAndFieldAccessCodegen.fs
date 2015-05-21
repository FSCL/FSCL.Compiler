namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_PROPERTY_ACCESS_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type PropertyAndFieldAccessCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run((expr, cont), st, opts) =
        let step = st :?> FunctionCodegenStep
        match expr with
        | Patterns.PropertyGet (o, pi, a) ->
            // We are not considering args (.NET indexed properties)
            // We do not print "this" object.
            if o.IsSome then
                match o.Value with
                | Patterns.Var(v) when v.Name = "this" ->
                    Some(pi.Name)
                | _ ->
                    Some(cont(o.Value) + "." + pi.Name)
            else
                Some(pi.Name)
        | Patterns.FieldGet (o, fi) ->
            // We are not considering args (.NET indexed properties)
            // We do not print "this" object.
            if o.IsSome then
                match o.Value with
                | Patterns.Var(v) when v.Name = "this" ->
                    Some(fi.Name)
                | _ ->
                    Some(cont(o.Value) + "." + fi.Name)
            else
                Some(fi.Name)
        | _ ->
            None