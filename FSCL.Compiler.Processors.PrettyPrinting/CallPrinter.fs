namespace FSCL.Compiler.Processors

open FSCL.Compiler
open Microsoft.FSharp.Quotations

type CallPrinter() =
    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
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