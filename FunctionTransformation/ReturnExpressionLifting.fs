namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_RETURN_LIFTING_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_ARG_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_RETURN_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_GLOBAL_VAR_REF_TRANSFORMATION_PROCESSOR";
                                  "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR";
                                  "FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR";
                                  "FSCL_REF_VAR_TRANSFORMATION_PROCESSOR" |])>]
type ReturnLifting() =       
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

    let LiftReturn (expr:Expr, retV:Quotations.Var list, engine:FunctionTransformationStep) =
        if isPotentialReturnExpression then
            match expr with
            | Patterns.Let(v, value, body) ->
                if retV.Length = 1 then
                    match body with
                    | Patterns.Var(var) ->                    
                        if var.Name = retV.[0].Name then
                            Expr.Let(v, value, Expr.Value(()))
                        else
                            Expr.Let(v, value, Expr.Var(var))
                    | _ ->                                        
                        Expr.Let(v, value, engine.Continue(body))
                else
                    // Look for return tuple expression
                    match body with
                    | Patterns.NewTuple(args) -> 
                        // If all the tuple components are var refs                       
                        if isVarsTuple(args) then
                            let vl = List.map(fun (e:Expr) -> 
                                                match e with 
                                                | Patterns.Var(v) ->
                                                    v
                                                | _ ->
                                                    failwith "Error") args
                            // If the refs exactly match the set of returned vars
                            if matchLists(vl, retV) then
                                Expr.Let(v, value, Expr.Value(()))
                            else
                                Expr.Let(v, value, Expr.NewTuple(args))                                
                        else 
                            Expr.Let(v, value, Expr.NewTuple(args))
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
                // If all the tuple components are var refs                       
                if isVarsTuple(args) then
                    let vl = List.map(fun (e:Expr) -> 
                                        match e with 
                                        | Patterns.Var(v) ->
                                            v
                                        | _ ->
                                            failwith "Error") args
                    // If the refs exactly match the set of returned vars
                    if matchLists(vl, retV) then
                        Expr.Value(())
                    else
                        Expr.NewTuple(List.map(fun (e:Expr) -> engine.Continue(e)) args)                                
                else 
                    Expr.NewTuple(List.map(fun (e:Expr) -> engine.Continue(e)) args)    

            | ExprShape.ShapeLambda(v, e) ->
                let e1 = engine.Continue(e)
                if retV.Length = 1 then
                    match e1 with
                    | Patterns.Var(var) ->
                        if var.Name = retV.[0].Name then
                            Expr.Lambda(v, Expr.Value(()))
                        else
                            Expr.Lambda(v, e1)
                    | _ ->
                        Expr.Lambda(v, e1)
                else
                    // Look for return tuple expression
                    match e1 with
                    | Patterns.NewTuple(args) -> 
                        // If all the tuple components are var refs                       
                        if isVarsTuple(args) then
                            let vl = List.map(fun (e:Expr) -> 
                                                match e with 
                                                | Patterns.Var(v) ->
                                                    v
                                                | _ ->
                                                    failwith "Error") args
                            // If the refs exactly match the set of returned vars
                            if matchLists(vl, retV) then
                                Expr.Lambda(v, Expr.Value(()))
                            else
                                Expr.Lambda(v, e1)                             
                        else 
                            Expr.Lambda(v, e1)  
                    | _ ->       
                        Expr.Lambda(v, e1)          

            | ExprShape.ShapeVar(var) ->
                if retV.Length = 1 then
                    if var.Name = retV.[0].Name then   
                        Expr.Value(())
                    else         
                        expr
                else
                    expr

            | ExprShape.ShapeCombination(o, args) ->
                let processed = List.map (fun e -> engine.Continue(e)) args
                ExprShape.RebuildShapeCombination(o, processed)
        else
            engine.Default(expr)
            
    override this.Run(expr, en) =
        let engine = en :?> FunctionTransformationStep
        if not (engine.FunctionInfo.CustomInfo.ContainsKey("KERNEL_RETURN_TYPE")) then
            engine.Default(expr)
        else
            let vars, exprs = List.unzip(engine.FunctionInfo.CustomInfo.["KERNEL_RETURN_TYPE"] :?> (Var * Expr list) list)            
            LiftReturn(expr, vars, engine)

