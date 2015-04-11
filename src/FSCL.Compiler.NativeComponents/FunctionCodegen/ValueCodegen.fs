namespace FSCL.Compiler.FunctionCodegen

open FSCL
open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.RuntimeHelpers

[<StepProcessor("FSCL_VALUE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type ValueCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, en, opts) =
        let step = en :?> FunctionCodegenStep
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

        match expr with
        | Patterns.NewTuple(args) ->
            let mutable gencode = returnPrefix + "(" + step.TypeManager.Print(expr.Type) + ") { "
            // Gen fields init
            let fields = args
            for i = 0 to fields.Length - 1 do
                gencode <- gencode + ".Item" + i.ToString() + " = " + step.Continue(args.[i])
                if i < fields.Length - 1 then
                    gencode <- gencode + ", "
            Some(gencode + " }" + returnPostfix)  

        | Patterns.Value(v, t) ->
            if (v = null) then
                Some("")
            else
                // If value of vector type must codegen appropriately
                if v.GetType().GetCustomAttribute<VectorTypeAttribute>() <> null then
                    let mutable code = "(" + step.TypeManager.Print(v.GetType()) + ")("
                    let components = v.GetType().GetProperty("Components").GetValue(v) :?> System.Array
                    for i = 0 to components.Length - 2 do
                        code <- code + components.GetValue(i).ToString() + ","
                    code <- code + components.GetValue(components.Length - 1).ToString() + ")"
                    Some(returnPrefix + code + returnPostfix)
                // Bool values are mapped to int
                else if v.GetType() = typeof<bool> then
                    if v :?> bool then
                        Some(returnPrefix + "1" + returnPostfix)
                    else
                        Some(returnPrefix + "0" + returnPostfix)
                // MAX/MIN const
                else if v.GetType() = typeof<int> && (v :?> int) = System.Int32.MaxValue then
                    Some("INT_MAX")
                else if v.GetType() = typeof<int> && (v :?> int) = System.Int32.MinValue then
                    Some("INT_MIN")
                else if v.GetType() = typeof<float32> && (v :?> float32) = System.Single.MaxValue then
                    Some("FLT_MAX")
                else if v.GetType() = typeof<float32> && (v :?> float32) = System.Single.MinValue then
                    Some("FLT_MIN")
                else if v.GetType() = typeof<double> && (v :?> double) = System.Double.MaxValue then
                    Some("DBL_MAX")
                else if v.GetType() = typeof<double> && (v :?> double) = System.Double.MinValue then
                    Some("DBL_MIN")
                // Default
                else
                    Some(returnPrefix + v.ToString() + returnPostfix)
        | Patterns.DefaultValue(t) ->
            // We handle DefaultValue by evaluating the expr (getting a const value) and then processing the constant node
            Some(step.Continue(Expr.Value(LeafExpressionConverter.EvaluateQuotation(expr), expr.Type)))
        | _ ->
            None