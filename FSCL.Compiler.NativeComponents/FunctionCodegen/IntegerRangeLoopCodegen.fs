namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_INTEGER_RANGE_LOOP_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type IntegerRangeLoopCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.ForIntegerRangeLoop(var, startexpr, endexp, body) ->
            Some("for(" + engine.TypeManager.Print(var.Type) + " " + var.Name + " = " + engine.Continue(startexpr) + "; " + var.Name + " <= " + engine.Continue(endexp) + ";" + var.Name + "++) {\n" + engine.Continue(body) + "\n}\n")
        | _ ->
            None
           