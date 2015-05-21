namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_WHILE_LOOP_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type WhileLoopCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run((expr, cont), en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.WhileLoop(cond, body) ->
            Some("while(" + cont(cond) + ") {\n" + cont(body) + "\n}\n")
        | _ ->
            None