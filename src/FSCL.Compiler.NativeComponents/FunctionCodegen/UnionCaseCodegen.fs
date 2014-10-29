namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open FSCL.Compiler.Util.ReflectionUtil
open Microsoft.FSharp.Reflection

[<StepProcessor("FSCL_UNION_CASE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type UnionCaseCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, st, opts) =
        let step = st :?> FunctionCodegenStep
        match expr with
        | Patterns.NewUnionCase(ui, args) ->
            let returnPrefix = 
                if(step.FunctionInfo.CustomInfo.ContainsKey("FUNCTION_RETURN_EXPRESSIONS")) then
                    let returnTags = 
                        step.FunctionInfo.CustomInfo.["FUNCTION_RETURN_EXPRESSIONS"] :?> Expr list
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                else
                    ""
            let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

            if ui.DeclaringType.IsOption then
                // Struct initialisation
                if args.Length > 0 then
                    let gencode = 
                        Some(
                            returnPrefix + "(" + step.TypeManager.Print(ui.DeclaringType) + ") " + 
                            " { .Value = " + step.Continue(args.[0]) + ", .IsSome = 1 }" + returnPostfix) 
                    gencode                          
                else
                    let gencode = 
                        Some(
                            returnPrefix + "(" + step.TypeManager.Print(ui.DeclaringType) + ") " + 
                            " { .Value = " + step.Continue(args.[0]) + ", .IsSome = 1 }" + returnPostfix)
                    gencode  
            else
                Some(returnPrefix + ui.Name + returnPostfix)
        | _ ->
            None