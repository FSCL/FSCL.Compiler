namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open System
open Microsoft.FSharp.Quotations

type ConditionalAssignmentProcessor() =   
    let rec MoveAssignmentIntoBody(var:Var, expr, engine:KernelBodyTransformationStage) =
        match expr with
        | Patterns.Sequential (e1, e2) ->
            Expr.Sequential(e1, MoveAssignmentIntoBody (var, e2, engine))
        | Patterns.IfThenElse(condinner, ifbinner, elsebinner) ->
            Expr.IfThenElse(condinner, MoveAssignmentIntoBody(var, ifbinner, engine), MoveAssignmentIntoBody(var, elsebinner, engine))     
        | Patterns.Let (e, v, body) ->
            Expr.Let(e, v, MoveAssignmentIntoBody(var, body, engine))
        | Patterns.Var (v) ->
            Expr.VarSet(var, Expr.Var(v))
        | Patterns.Value (v) ->
            Expr.VarSet(var, Expr.Value(v))
        | Patterns.Call (e, i, a) ->
            if e.IsSome then
                Expr.VarSet(var, Expr.Call(e.Value, i, a))
            else
                Expr.VarSet(var, Expr.Call(i, a))
        | _ ->
            raise (KernelTransformationException("Cannot determine variable assignment in if-then-else construct. Try to transform v = if .. else ..; into v; if .. v <- .. else .. v <- .."))

    let rec MoveArraySetIntoBody(o:Expr option, mi:MethodInfo, a:Expr list, substituteIndex:int, expr, engine:KernelBodyTransformationStage) =
        match expr with
        | Patterns.Sequential (e1, e2) ->
            Expr.Sequential(e1, MoveArraySetIntoBody (o, mi, a, substituteIndex, e2, engine))
        | Patterns.IfThenElse(condinner, ifbinner, elsebinner) ->
            Expr.IfThenElse(condinner, MoveArraySetIntoBody(o, mi, a, substituteIndex, ifbinner, engine), MoveArraySetIntoBody(o, mi, a, substituteIndex, elsebinner, engine))     
        | Patterns.Let (e, v, body) ->
            Expr.Let(e, v, MoveArraySetIntoBody(o, mi, a, substituteIndex, body, engine))
        | Patterns.Var (v) ->
            Expr.Call(mi, List.mapi(fun i el -> if i = substituteIndex then Expr.Var(v) else el) a)
        | Patterns.Value (v, t) ->
            Expr.Call(mi, List.mapi(fun i el -> 
                if i = substituteIndex then 
                    Expr.Value(v, t)
                else el) a)
        | Patterns.Call (subo, subi, suba) ->
            if subo.IsSome then
                Expr.Call(mi, List.mapi(fun i el -> if i = substituteIndex then Expr.Call(subo.Value, subi, suba) else el) a)
            else
                Expr.Call(mi, List.mapi(fun i el -> if i = substituteIndex then Expr.Call(subi, suba) else el) a)
        | _ ->
            raise (KernelTransformationException("Cannot determine variable assignment in if-then-else construct. Try to transform v = if .. else ..; into v; if .. v <- .. else .. v <- .."))

                                     
    interface GenericProcessor with
        member this.Handle(expr, engine:KernelBodyTransformationStage) =
            match expr with
            | Patterns.Let(v, e, body) ->
                match e with
                | Patterns.IfThenElse(cond, ib, eb) ->                    
                    let fixedExpr = MoveAssignmentIntoBody(v, e, engine)
                    let result = engine.Process(fixedExpr)
                    (true, Some(engine.Process(v.Type) + " " + engine.Process(v) + ";\n" + result))
                | _ ->
                    (false, None)                    
            | Patterns.VarSet (v, e) ->
                match e with
                | Patterns.IfThenElse(cond, ib, eb) ->                    
                    let fixedExpr = MoveAssignmentIntoBody(v, e, engine)
                    let result = engine.Process(fixedExpr)
                    (true, Some(result))
                | _ ->
                    (false, None)                  
            | Patterns.Call (e, mi, a) ->
                if mi.DeclaringType.Name = "IntrinsicFunctions" then                    
                    if mi.Name = "SetArray" || mi.Name = "SetArray2D" || mi.Name = "SetArray3D" then
                        let substituteIndex = a.Length - 1

                        match a.[substituteIndex] with
                        | Patterns.IfThenElse(cond, ib, eb) ->                    
                            let fixedExpr = MoveArraySetIntoBody(e, mi, a, substituteIndex, a.[substituteIndex], engine)
                            let result = engine.Process(fixedExpr)
                            (true, Some(result))
                        | _ ->
                            (false, None)
                    else
                        (false, None)
                else
                    (false, None)
            | _ ->
                (false, None)                    