namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VAR_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type VarCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run((expr, cont), en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.Var(v) ->
            
            let returnPrefix = 
                if(engine.FunctionInfo.CustomInfo.ContainsKey("FUNCTION_RETURN_EXPRESSIONS")) then
                    let returnTags = 
                        engine.FunctionInfo.CustomInfo.["FUNCTION_RETURN_EXPRESSIONS"] :?> Expr list
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                else
                    ""
            let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""
            Some(returnPrefix + v.Name + returnPostfix)
        | _ ->
            None