namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultIfThenElseProcessor() =   
    let rec LiftAndOrOperator(expr:Expr, engine:KernelBodyTransformationStage) =
        match expr with
        | Patterns.IfThenElse(condinner, ifbinner, elsebinner) ->
            match ifbinner with
            | Patterns.Value(o, t) ->
                if(t = typeof<bool>) then
                    if (o :?> bool) then
                        Some(engine.Process(condinner) + " || " + engine.Process(elsebinner))
                    else
                        None
                else
                    None
            | _ ->
                match elsebinner with  
                | Patterns.Value(o, t) ->
                    if(t = typeof<bool>) then   
                        if (not (o :?> bool)) then
                            Some(engine.Process(condinner) + " && " + engine.Process(ifbinner))
                        else
                            None
                    else
                        None      
                | _ ->
                None      
        | _ ->
            None              

    interface IfThenElseProcessor with
        member this.Handle(expr, cond, ifb, elseb, engine:KernelBodyTransformationStage) =
            let checkBoolOp = LiftAndOrOperator(expr, engine)
            if checkBoolOp.IsSome then
                (true, checkBoolOp)
            else
                // Fix: if null (Microsoft.Fsharp.Core.Unit) don't generate else branch
                if elseb.Type = typeof<Microsoft.FSharp.Core.unit> && elseb = Expr.Value<Microsoft.FSharp.Core.unit>(()) then
                    (true, Some("if(" + engine.Process(cond) + ") {\n" + engine.Process(ifb) + "}\n"))
                else
                    (true, Some("if(" + engine.Process(cond) + ") {\n" + engine.Process(ifb) + "}\nelse {\n" + engine.Process(elseb) + "\n}\n"))