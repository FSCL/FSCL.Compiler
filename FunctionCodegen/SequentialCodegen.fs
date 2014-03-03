namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_SEQUENTIAL_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type SequentialCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.Sequential(e1, e2) ->
            Some(engine.Continue(e1) + "\n" + engine.Continue(e2))
        | _ ->
            None