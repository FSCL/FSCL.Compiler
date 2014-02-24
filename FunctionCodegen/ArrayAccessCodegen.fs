namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<StepProcessor("FSCL_ARRAY_ACCESS_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
///
///<summary>
///The function codegen step processor whose behavior is to produce the target representation of array accesses (read, write)
///</summary>
///  
type ArrayAccessCodegen() =                 
    inherit FunctionBodyCodegenProcessor()
    ///
    ///<summary>
    ///The method called to execute the processor
    ///</summary>
    ///<param name="fi">The AST node (expression) to process</param>
    ///<param name="en">The owner step</param>
    ///<returns>
    ///The target code for the array access if the AST node can be processed (i.e. if the source node is an array access expression)
    ///</returns>
    ///  
    override this.Run(expr, en) =        
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.Call(o, methodInfo, args) ->
            if methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "IntrinsicFunctions" then
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

                let arrayName = engine.Continue(args.[0])
                if methodInfo.Name = "GetArray" then
                    Some(returnPrefix  + arrayName + "[" + engine.Continue(args.[1]) + "]" + returnPostfix)
                elif methodInfo.Name = "SetArray" then
                    Some(arrayName + "[" + engine.Continue(args.[1]) + "] = " + engine.Continue(args.[2]) + ";\n")
                else
                    None
            else
                None
        | _ ->
            None