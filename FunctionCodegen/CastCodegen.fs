namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open System.Reflection

[<StepProcessor("FSCL_CAST_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>] 
type CastCodegen() =                 
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, s, opts) =
        let step = s :?> FunctionCodegenStep
        match expr with    
        | Patterns.Coerce(e, t) ->
            Some("(" + step.TypeManager.Print(t) + ")" + step.Continue(e))
        | _ ->
            None