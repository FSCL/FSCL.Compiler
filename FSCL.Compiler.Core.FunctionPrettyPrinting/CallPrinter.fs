namespace FSCL.Compiler.FunctionPrettyPrinting

open FSCL.Compiler
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_CALL_PRETTY_PRINTING_PROCESSOR", "FSCL_FUNCTION_PRETTY_PRINTING_STEP",
                [| "FSCL_ARRAY_ACCESS_PRETTY_PRINTING_PROCESSOR";
                   "FSCL_ARITH_OP_PRETTY_PRINTING_PROCESSOR" |])>]
type CallPrinter() =
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionPrettyPrintingStep
            match expr with
            | Patterns.Call (o, mi, a) ->
                let args = String.concat ", " (List.map (fun (e:Expr) -> engine.Continue(e)) a)
                if mi.DeclaringType.Name = "KernelFunctions" &&  mi.Name = "barrier" then
                    // the function is defined in FSCL
                    Some(mi.Name + "(" + args + ");")
                else
                    Some(mi.Name + "(" + args + ")")
            | _ ->
                None