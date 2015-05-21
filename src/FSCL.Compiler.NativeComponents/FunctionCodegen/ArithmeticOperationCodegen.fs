namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Reflection

///
///<summary>
///The function codegen step processor whose behavior is to produce the target code for arithmetic and logic operations
///</summary>
///  
[<StepProcessor("FSCL_ARITH_OP_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type ArithmeticOperationCodegen() =
    inherit FunctionBodyCodegenProcessor()

    let HandleBinaryOp (op, a:Expr list, cont) =
        "(" + cont(a.[0]) + ")" + op + "(" + cont(a.[1]) + ")"
    let HandleUnaryOp (op, a:Expr list, cont) =
        op + cont(a.[0])
        
    ///
    ///<summary>
    ///The method called to execute the processor
    ///</summary>
    ///<param name="fi">The AST node (expression) to process</param>
    ///<param name="en">The owner step</param>
    ///<returns>
    ///The target code for the arithmetic or logic expression if the AST node can be processed (i.e. if the source node is an arithmetic or logic expression expression)
    ///</returns>
    ///  
    override this.Run((expr, cont), s, opts) =
        let step = s :?> FunctionCodegenStep
        match expr with 
        | Patterns.Call(o, mi, args) ->            
            let returnPrefix = 
                if step.FunctionInfo.CustomInfo.ContainsKey("FUNCTION_RETURN_EXPRESSIONS") then
                    let returnTags = step.FunctionInfo.CustomInfo.["FUNCTION_RETURN_EXPRESSIONS"] :?> Expr list
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                else
                    ""
            let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""

            match expr with
            | DerivedPatterns.SpecificCall <@ (>) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" > ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (<) @> (e, t, a)  -> 
                Some(returnPrefix + HandleBinaryOp(" < ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (>=) @> (e, t, a)  -> 
                Some(returnPrefix + HandleBinaryOp(" >= ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (<=) @> (e, t, a)  -> 
                Some(returnPrefix + HandleBinaryOp(" <= ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (=) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" == ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (<>) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" != ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (+) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" + ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (*) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" * ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (-) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" - ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (/) @> (e, t, a) -> 
                Some(HandleBinaryOp(" / ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (%) @> (e, t, a) -> 
                Some(returnPrefix + returnPrefix + HandleBinaryOp(" % ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (&&) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" && ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (||) @> (e, t, a) ->
                Some(returnPrefix + HandleBinaryOp(" || ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (&&&) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" & ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (|||) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" | ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (^^^) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" ^ ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (~~~) @> (e, t, a) -> 
                Some(returnPrefix + HandleUnaryOp(" ~ ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (not) @> (e, t, a) -> 
                Some(returnPrefix + HandleUnaryOp(" ! ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (>>>) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" >> ", a, cont) + returnPostfix)
            | DerivedPatterns.SpecificCall <@ (<<<) @> (e, t, a) -> 
                Some(returnPrefix + HandleBinaryOp(" << ", a, cont) + returnPostfix)
            | _ ->
                None
        | _ ->
            None