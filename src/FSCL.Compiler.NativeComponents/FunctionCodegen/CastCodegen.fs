namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open System.Reflection

[<StepProcessor("FSCL_CAST_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>] 
type CastCodegen() =                 
    inherit FunctionBodyCodegenProcessor()
    override this.Run((expr, cont), st, opts) =
        let step = st :?> FunctionCodegenStep
        match expr with    
        | Patterns.Coerce(e, t) ->
            // Skip System.Array coercion (this is performed when calling pasum and pasum for pointer arithmetic)
            if t = typeof<System.Array> then
                Some(cont(e))
            else
                Some("(" + step.TypeManager.Print(t) + ")" + cont(e))
        | _ ->
            None