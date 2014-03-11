namespace FSCL.Compiler.FunctionCodegen

open System
open FSCL.Compiler
open System.Collections.Generic
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_CALL_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP",
                Dependencies = [| "FSCL_ARRAY_ACCESS_CODEGEN_PROCESSOR";
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
            if mi.DeclaringType <> null && mi.DeclaringType.Name = "Language" &&  mi.Name = "barrier" then
                // the function is defined in FSCL
                Some(returnPrefix + mi.Name + "(" + args + ");" + returnPostfix)
            else
                Some(returnPrefix + mi.Name + "(" + args + ")" + returnPostfix)
        | _ ->
            None