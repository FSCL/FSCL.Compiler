namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_INTEGER_RANGE_LOOP_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
type IntegerRangeLoopCodegen() =   
    inherit FunctionBodyCodegenProcessor()
    override this.Run((expr, cont), st, opts) =
        let step = st :?> FunctionCodegenStep
        match expr with
        | Patterns.ForIntegerRangeLoop(var, startexpr, endexp, body) ->
            Some("for(" + step.TypeManager.Print(var.Type) + " " + var.Name + " = " + cont(startexpr) + "; " + var.Name + " <= " + cont(endexp) + ";" + var.Name + "++) {\n" + cont(body) + "\n}\n")
        | _ ->
            None
           