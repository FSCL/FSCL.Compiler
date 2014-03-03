namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open FSCL.Compiler.Types
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_DECLARATION_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP",
                Dependencies =[| "FSCL_FOR_RANGE_CODEGEN_PROCESSOR" |])>]
type DeclarationCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.Let(v, value, body) ->
            Some(engine.TypeManager.Print(v.Type) + " " + v.Name + " = " + engine.Continue(value) + ";\n" + engine.Continue(body))
        | _ ->
            None