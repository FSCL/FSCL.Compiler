namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type IfThenElsePrinter() =   
    let rec LiftAndOrOperator(expr:Expr, engine:FunctionPrettyPrintingStep) =
        match expr with
        | Patterns.IfThenElse(condinner, ifbinner, elsebinner) ->
            match ifbinner with
            | Patterns.Value(o, t) ->
                if(t = typeof<bool>) then
                    if (o :?> bool) then
                        Some(engine.Continue(condinner) + " || " + engine.Continue(elsebinner))
                    else
                        None
                else
                    None
            | _ ->
                match elsebinner with  
                | Patterns.Value(o, t) ->
                    if(t = typeof<bool>) then   
                        if (not (o :?> bool)) then
                            Some(engine.Continue(condinner) + " && " + engine.Continue(ifbinner))
                        else
                            None
                    else
                        None      
                | _ ->
                None      
        | _ ->
            None              

    interface FunctionBodyPrettyPrintingProcessor with
        member this.Handle(expr, engine:FunctionPrettyPrintingStep) =
            match expr with
            | Patterns.IfThenElse(cond, ifb, elseb) ->
                let checkBoolOp = LiftAndOrOperator(expr, engine)
                if checkBoolOp.IsSome then
                    Some(checkBoolOp.Value)
                else
                    // Fix: if null (Microsoft.Fsharp.Core.Unit) don't generate else branch
                    if elseb.Type = typeof<Microsoft.FSharp.Core.unit> && elseb = Expr.Value<Microsoft.FSharp.Core.unit>(()) then
                        Some("if(" + engine.Continue(cond) + ") {\n" + engine.Continue(ifb) + "}\n")
                    else
                        Some("if(" + engine.Continue(cond) + ") {\n" + engine.Continue(ifb) + "}\nelse {\n" + engine.Continue(elseb) + "\n}\n")
            | _ ->
                None