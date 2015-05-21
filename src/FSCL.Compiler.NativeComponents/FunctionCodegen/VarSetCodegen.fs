namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VAR_SET_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type VarSetCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run((expr, cont), en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.VarSet (v, e) ->
            Some(v.Name + " = " + cont(e) + ";\n")
        | _ ->
            None