namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VALUE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type ValueCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, en) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.Value(v, t) ->
            let returnTags = engine.FunctionInfo.CustomInfo.["RETURN_EXPRESSIONS"] :?> Expr list
            let returnPrefix = 
                if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                    "return "
                else
                    ""
            let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

            if (v = null) then
                Some("")
            else
                Some(returnPrefix + v.ToString() + returnPostfix)
        | _ ->
            None