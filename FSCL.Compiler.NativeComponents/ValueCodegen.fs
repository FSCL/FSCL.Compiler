namespace FSCL.Compiler.FunctionCodegen

open FSCL
open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_VALUE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type ValueCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.Value(v, t) ->
            let returnPrefix = 
                if(engine.FunctionInfo.CustomInfo.ContainsKey("RETURN_EXPRESSIONS")) then
                    let returnTags = 
                        engine.FunctionInfo.CustomInfo.["RETURN_EXPRESSIONS"] :?> Expr list
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                else
                    ""
            let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

            if (v = null) then
                Some("")
            else
                // If value of vector type must codegen appropriately
                if v.GetType().GetCustomAttribute<VectorTypeAttribute>() <> null then
                    let mutable code = "(" + engine.TypeManager.Print(v.GetType()) + ")("
                    let components = v.GetType().GetProperty("Components").GetValue(v) :?> System.Array
                    for i = 0 to components.Length - 2 do
                        code <- code + components.GetValue(i).ToString() + ","
                    code <- code + components.GetValue(components.Length - 1).ToString() + ")"
                    Some(returnPrefix + code + returnPostfix)
                else
                    Some(returnPrefix + v.ToString() + returnPostfix)
        | _ ->
            None