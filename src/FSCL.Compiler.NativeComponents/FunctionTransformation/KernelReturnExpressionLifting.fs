namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_RETURN_LIFTING_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_FUNCTION_RETURN_DISCOVERY_PROCESSOR";
                                  "FSCL_DYNAMIC_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR";
                                  "FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR";
                                  "FSCL_REF_VAR_TRANSFORMATION_PROCESSOR" |])>]
type KernelReturnLifting() =       
    inherit FunctionTransformationProcessor()

    let mutable isPotentialReturnExpression = true
    
    // Check if a list of expressions is a list of Var expressions (e.g. (a, b, c) where a, b and c are var refs)
    let isVarsTuple(args: Expr list) =
        let mutable result = true
        for e in args do
            match e with
            | Patterns.Var(v) ->
                ()
            | _ ->
                result <- false
        result

    let matchLists(a: Var list, b: Var list) =
        let temp = new List<Var>(a)
        let mutable result = true
        for v in b do
            if temp.Count = 0 then
                result <- false
            if temp.Contains(v) then
                temp.Remove(v) |> ignore
            else
                result <- false
        result && (temp.Count = 0)

    let LiftReturnArrayRef (expr:Expr, retV:Quotations.Var, engine:FunctionTransformationStep) =
        if isPotentialReturnExpression then
            match expr with
            | Patterns.Let(v, value, body) ->                
                match body with
                | Patterns.Var(var) ->                    
                    if var.Name = retV.Name then
                        Expr.Let(v, value, Expr.Value(()))
                    else
                        Expr.Let(v, value, Expr.Var(var))
                | _ ->                                        
                    Expr.Let(v, value, engine.Continue(body))
            | Patterns.Sequential(e1, e2) ->
                isPotentialReturnExpression <- false
                let pe1 = engine.Continue(e1)
                isPotentialReturnExpression <- true
                let pe2 = engine.Continue(e2)
                Expr.Sequential(pe1, pe2)
                
            | Patterns.IfThenElse(cond, ifexp, elsexp) ->
                let pe1 = engine.Continue(ifexp)
                let pe2 = engine.Continue(elsexp)
                Expr.IfThenElse(cond, pe1, pe2)
                        
            | Patterns.NewTuple(args) -> 
                Expr.NewTuple(List.map(fun (e:Expr) -> engine.Continue(e)) args)    

            | ExprShape.ShapeLambda(v, e) ->
                let e1 = engine.Continue(e)
                match e1 with
                | Patterns.Var(var) ->
                    if var.Name = retV.Name then
                        Expr.Lambda(v, Expr.Value(()))
                    else
                        Expr.Lambda(v, e1)
                | _ ->
                    Expr.Lambda(v, e1)                
            | ExprShape.ShapeVar(var) ->
                if var.Name = retV.Name then   
                    Expr.Value(())
                else         
                    expr

            | ExprShape.ShapeCombination(o, args) ->
                let processed = List.map (fun e -> engine.Continue(e)) args
                ExprShape.RebuildShapeCombination(o, processed)
        else
            engine.Default(expr)
              
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionTransformationStep
        if (engine.FunctionInfo :? KernelInfo)  then
            // Check if there is a parameter returned
            let p = Seq.tryFind(fun (e:FunctionParameter) -> e.IsReturned) (engine.FunctionInfo.Parameters)
            if p.IsSome then
                if not p.Value.IsAutoArrayParameter then
                    let rv = p.Value.OriginalPlaceholder
                    LiftReturnArrayRef(expr, rv, engine)
                else
                    expr
            else
                expr
        else
            expr

