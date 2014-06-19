namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

///
///<summary>
///The function codegen step processors whose behavior is to produce the target code for "for x in a .. b do" expressions
///</summary>
///  
[<StepProcessor("FSCL_FOR_RANGE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type ForInCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    ///
    ///<summary>
    ///The method called to execute the processor
    ///</summary>
    ///<param name="fi">The AST node (expression) to process</param>
    ///<param name="en">The owner step</param>
    ///<returns>
    ///The target code for the for-in-range expression if the AST node can be processed
    ///</returns>
    ///  
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.Let (inputSequence, value, body) ->
            match value with 
            | Patterns.Call(ob, mi, a) ->
                if mi.Name = "op_RangeStep" then
                    // The args is the beginning, the step and the end of the iteration
                    let starte, stepe, ende = a.[0], a.[1], a.[2]
                    match body with
                    | Patterns.Let(enumerator, value, body) ->
                        match body with
                        | Patterns.TryFinally (trye, fine) ->
                            match trye with
                                | Patterns.WhileLoop(cond, body) ->
                                    match body with
                                    | Patterns.Let(v, value, body) ->
                                        match value with
                                        | Patterns.PropertyGet(e, pi, a) ->
                                            // Ok, that's an input sequence!
                                            Some("for(" + engine.TypeManager.Print(v.Type) + " " + v.Name + " = " + engine.Continue(starte) + "; " + v.Name + " <= " + engine.Continue(ende) + "; " + v.Name + "+=" + engine.Continue(stepe) + ") {\n" + engine.Continue(body) + "\n}\n")                                                
                                        | _ -> None
                                    | _ -> None
                                | _ -> None
                            | _ -> None
                        | _ -> None
                    | _ -> None                                           
                else
                    None
            | _ -> None  
        | _ -> None