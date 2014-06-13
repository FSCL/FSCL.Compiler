namespace FSCL.Compiler.FunctionCodegen

open System
open FSCL.Compiler
open FSCL.Language
open FSCL.Compiler.Util
open System.Collections.Generic
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_LOCAL_DEC_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP",
                Dependencies = [| "FSCL_ARRAY_ACCESS_CODEGEN_PROCESSOR";
                                  "FSCL_ARITH_OP_CODEGEN_PROCESSOR" |])>]
                                  
///
///<summary>
///The function codegen step whose behavior is to generate the target representation of local declarations
///</summary>
///  
type LocalDecCodegen() =
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
        | Patterns.Let(variable, value, body) ->
            match value with
            | DerivedPatterns.SpecificCall <@ local @> (o, tl, a) ->
                match a.[0] with 
                | Patterns.Call(_, methodInfo, args) ->
                    // Check that the alloc is Array.zeroCreate or ArrayXD.zerocreate
                    if (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") then
                        let mutable code = "local " + engine.TypeManager.Print(variable.Type) + " " + variable.Name + "[" + engine.Continue(args.[0]) + "];\n"
                        Some(code)
                    else if (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") then
                        let mutable code = "local " + engine.TypeManager.Print(variable.Type) + " " + variable.Name + "[" + engine.Continue(args.[0]) + "][" + engine.Continue(args.[1]) + "];\n"
                        Some(code)
                    else if (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
                        let mutable code = "local " + engine.TypeManager.Print(variable.Type) + " " + variable.Name + "[" + engine.Continue(args.[0]) + "][" + engine.Continue(args.[1]) + "][" + engine.Continue(args.[2]) + "];\n"
                        Some(code)
                    else
                        None
                | _ ->
                    None
            | _ ->
                None
        | _ ->
            None