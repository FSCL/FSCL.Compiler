namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_UNION_CASE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type UnionCaseCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(e, en) =
        let engine = en :?> FunctionCodegenStep
        match e with
        | Patterns.NewUnionCase(ui, args) ->
            Some(ui.Name)
        | _ ->
            None