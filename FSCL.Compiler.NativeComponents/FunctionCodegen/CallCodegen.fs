namespace FSCL.Compiler.FunctionCodegen

open FSCL
open System
open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.FSharp.Linq.RuntimeHelpers

[<StepProcessor("FSCL_CALL_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP",
                Dependencies = [| "FSCL_ARRAY_ACCESS_CODEGEN_PROCESSOR";
                                  "FSCL_DECLARATION_CODEGEN_PROCESSOR";
                                  "FSCL_ARITH_OP_CODEGEN_PROCESSOR" |])>]
                                  
///
///<summary>
///The function codegen step whose behavior is to generate the target representation of method calls 
///</summary>
///  
type CallCodegen() =
    inherit FunctionBodyCodegenProcessor()

    ///
    ///<summary>
    ///The method called to execute the processor
    ///</summary>
    ///<param name="fi">The AST node (expression) to process</param>
    ///<param name="en">The owner step</param>
    ///<returns>
    ///The target code for the method call (a function call in the target)if the AST node can be processed (i.e. if the source node is a method call)
    ///</returns>
    ///  
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        // Ignore call to __local
        | DerivedPatterns.SpecificCall <@ local @> (o, mi, a) ->
            None
        | Patterns.Call (o, mi, a) ->
            // Check if the call is the last thing done in the function body
            // If so, prepend "return"
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
            
            let args = String.concat ", " (List.map (fun (e:Expr) -> engine.Continue(e)) a)
            // Vector OpenCL operators
            if (mi.DeclaringType.GetCustomAttribute<VectorTypeAttribute>() <> null && mi.Name = "vload") then
                // vload function
                let vType = mi.ReturnType
                let vCount = 
                    if vType.GetField("w") <> null then
                        4
                    else if vType.GetField("z") <> null then
                        3
                    else 
                        2
                Some(returnPrefix + "vload" + vCount.ToString() + "(" + args + ")" + returnPostfix)
            else if (mi.GetCustomAttribute<VectorTypeConversionAttribute>() <> null) then
                // vload function
                let sat = mi.Name.EndsWith("Sat")
                let rounding = LeafExpressionConverter.EvaluateQuotation(a.[1]) :?> VectorTypeConversionRoundingMode option
                if rounding.IsSome then
                    Some(returnPrefix + "convert_" + engine.TypeManager.Print(mi.ReturnType) + (if sat then "_sat" else "") + "_" + rounding.Value.ToString() + "(" + engine.Continue(a.[0]) + ")" + returnPostfix)
                else
                    Some(returnPrefix + "convert_" + engine.TypeManager.Print(mi.ReturnType) + (if sat then "_sat" else "") + "(" + engine.Continue(a.[0]) + ")" + returnPostfix)
            // Pointer arithmetic
            else if (mi.Name = "[]`1.pasum") then
                // Pointer arithmetic sum
                Some(returnPrefix + "(" + engine.Continue(a.[0]) + ") + (" + engine.Continue(a.[1]) + ")" + returnPostfix)
            else if (mi.Name = "[]`1.pasub") then
                // Pointer arithmetic subtraction
                Some(returnPrefix + "(" + engine.Continue(a.[0]) + ") - (" + engine.Continue(a.[1]) + ")" + returnPostfix)
            else
                if mi.DeclaringType <> null && mi.DeclaringType.Name = "Language" &&  mi.Name = "barrier" then
                    // the function is defined in FSCL
                    Some(returnPrefix + mi.Name + "(" + args + ");" + returnPostfix)
                else
                    Some(returnPrefix + mi.Name + "(" + args + ")" + returnPostfix)
        | _ ->
            None