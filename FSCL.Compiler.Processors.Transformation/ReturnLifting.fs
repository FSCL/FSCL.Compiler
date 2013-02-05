namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_RETURN_LIFTING_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                [| "FSCL_RETURN_TYPE_TRANSFORMATION_PROCESSOR";
                   "FSCL_GLOBAL_VAR_REF_TRANSFORMATION_PROCESSOR";
                   "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR";
                   "FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR";
                   "FSCL_REF_VAR_TRANSFORMATION_PROCESSOR" |])>]
type ReturnLifting() =       
    let rec LiftReturn (expr:Expr, retV:Quotations.Var, potentialReturn, engine:FunctionTransformationStep) =
        match expr with
        | Patterns.Let(v, value, body) ->
            let pv = engine.Continue(value)
            let pb = engine.Continue(body)
            match pb with
            | Patterns.Var(var) ->
                let (retV, args) = engine.FunctionInfo.CustomInfo.["KERNEL_RETURN_TYPE"] :?> (Var * Expr list)
                if var.Name = retV.Name then
                    Expr.Let(v, pv, Expr.Value(()))
                else
                    Expr.Let(v, pv, Expr.Var(var))
            | _ ->
                Expr.Let(v, value, LiftReturn(body, retV, true, engine))
        | Patterns.Sequential(e1, e2) ->
            let pe1 = engine.Continue(e1)
            let pe2 = engine.Continue(e2)
            Expr.Sequential(LiftReturn(pe1, retV, false, engine), LiftReturn(pe2, retV, true, engine))
        | ExprShape.ShapeLambda(v, e) ->
            let e1 = engine.Continue(e)
            match e1 with
            | Patterns.Var(var) ->
                if var.Name = retV.Name then
                    Expr.Lambda(v, Expr.Value(()))
                else
                    Expr.Lambda(v, e1)
            | _ ->
                Expr.Lambda(v, LiftReturn(e, retV, true, engine))
        | ExprShape.ShapeVar(var) ->
            if var.Name = retV.Name && potentialReturn then   
                Expr.Value(())
            else         
                expr
        | ExprShape.ShapeCombination(o, args) ->
            let processed = List.map (fun e -> engine.Continue(e)) args
            ExprShape.RebuildShapeCombination(o, List.map (fun el -> LiftReturn(el, retV, false, engine)) processed)
            
    interface FunctionTransformationProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionTransformationStep
            if not (engine.FunctionInfo.CustomInfo.ContainsKey("KERNEL_RETURN_TYPE")) then
                engine.Default(expr)
            else
                let retV, _ = engine.FunctionInfo.CustomInfo.["KERNEL_RETURN_TYPE"] :?> (Var * Expr list)
                LiftReturn(expr, retV, true, engine)

