namespace FSCL.Compiler.FunctionTransformation

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Core.LanguagePrimitives

[<StepProcessor("FSCL_RIGHT_HAND_LET_INLINER_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR" |])>]
type RightHandlLetInliner() =     
    inherit FunctionTransformationProcessor()

    let rec InlineLetRec (v:Var, value:Expr, root:Expr) =
        match root with
        | Patterns.Var(va) ->
            if (va = v) then
                value
            else
               root
        | Patterns.Let(othv, othvalue, othbody) ->
            let newRoot = InlineLetRec(othv, othvalue, othbody)
            InlineLetRec(v, value, newRoot)
        | ExprShape.ShapeVar(va) ->
            if (va = v) then
                value
            else
               root            
        | ExprShape.ShapeLambda(lv, lb) ->
            Expr.Lambda(lv, InlineLetRec(v, value, lb))
        | ExprShape.ShapeCombination(o, args) ->
            ExprShape.RebuildShapeCombination(o, List.map (fun (e:Expr) -> InlineLetRec(v, value, e)) args)
            
    let rec InlineLet(expr:Expr) =
        match expr with
        | Patterns.Let(v, value, body) ->
            let newBody = InlineLetRec(v, value, body)
            newBody
        | ExprShape.ShapeVar(va) ->
            expr           
        | ExprShape.ShapeLambda(lv, lb) ->
            Expr.Lambda(lv, InlineLet(lb))
        | ExprShape.ShapeCombination(o, args) ->
            ExprShape.RebuildShapeCombination(o, List.map (fun (e:Expr) -> InlineLet(e)) args)

    override this.Run((expr, cont, def), en, opts) =
        let engine = en :?> FunctionTransformationStep
        match expr with
        | Patterns.Call(o, methodInfo, args) ->
            if o.IsNone then
                Expr.Call(methodInfo, List.map (fun (e:Expr) -> InlineLet(e)) args)
            else
                Expr.Call(o.Value, methodInfo, List.map (fun (e:Expr) -> InlineLet(e)) args)                
        | Patterns.VarSet(v, value) ->
            Expr.VarSet(v, InlineLet(value))
        | ExprShape.ShapeVar(va) ->
            expr           
        | ExprShape.ShapeLambda(lv, lb) ->
            Expr.Lambda(lv, cont(lb))
        | ExprShape.ShapeCombination(o, args) ->
            ExprShape.RebuildShapeCombination(o, List.map (fun (e:Expr) -> cont(e)) args)