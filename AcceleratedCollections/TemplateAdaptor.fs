namespace FSCL.Compiler.Plugins.AcceleratedCollections

open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System.Collections.Generic

module TemplateAdaptor =
    let GetKernelFromLambda(expr:Expr) = 
        let rec LiftTupledArgs(body: Expr, l:Var list) =
            match body with
            | Patterns.Let(v, value, b) ->
                 match value with
                 | Patterns.TupleGet(a, i) ->
                    LiftTupledArgs(b, l @ [v])
                 | _ ->
                    (body, l)
            | _ ->
                (body, l)
                                   
        match expr with
        | Patterns.Lambda(v, e) -> 
            if v.Name = "tupledArg" then
                let kernelData = LiftTupledArgs(e, [])
                kernelData
            else
                failwith "Template has no tupled args"                
        | _ ->
            failwith "No lambda found inside template"
                        
            
    let rec SubstitutePlaceholders(e:Expr, parameters:Dictionary<Var, Var>, computation:string * MethodInfo) =
        let oldComp, newComp = computation
        match e with
        | Patterns.Var(v) ->       
            if parameters.ContainsKey(v) then
                Expr.Var(parameters.[v])
            else
                e
        | Patterns.Call(o, m, args) ->
            if m.Name = oldComp then
                if o.IsSome then
                    Expr.Call(o.Value, newComp, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, computation)) args)
                else
                    Expr.Call(newComp, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, computation)) args)
            else
                e
        | ExprShape.ShapeLambda(v, e) ->
            SubstitutePlaceholders(e, parameters, computation)
        | ExprShape.ShapeCombination(o, l) ->
            ExprShape.RebuildShapeCombination(o, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, computation)) l)
        | _ ->
            e
                    
                
