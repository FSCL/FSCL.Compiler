namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_PROPERTY_ACCESS_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type PropertyAccessCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.PropertyGet (o, pi, a) ->
            // We are not considering args (.NET indexed properties)
            if o.IsSome then
                Some(engine.Continue(o.Value) + "." + pi.Name)
            else
                Some(pi.Name)
        | _ ->
            None