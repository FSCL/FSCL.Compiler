namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Collections.Generic
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices
open FSCL.Compiler.Util
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

//RETURN_TYPE_TO_OUTPUT_ARG_REPLACING
[<StepProcessor("FSCL_ARGS_PREP_LIFTING_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP")>]
type ArgsPreparationLiftingProcessor() =
    inherit FunctionPreprocessingProcessor()
            
    let rec LiftTuplePreparationWithCall(e: Expr) =
        let rec liftTupleInternal(e:Expr, applicationArgs: Expr list) =            
            match e with
            | Patterns.Let(v, Patterns.TupleGet(t, i), b) ->
                liftTupleInternal(b, applicationArgs)                
            | Patterns.Call(ob, mi, a) ->
                match mi with 
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                    if ob.IsSome then
                        Expr.Call(ob.Value, mi, applicationArgs |> List.map (fun i -> LiftTuplePreparationWithCall(i)))
                    else
                        Expr.Call(mi, applicationArgs |> List.map (fun i -> LiftTuplePreparationWithCall(i)))
                | _ ->
                    if ob.IsSome then
                        Expr.Call(ob.Value, mi, a |> List.map (fun i -> LiftTuplePreparationWithCall(i)))
                    else
                        Expr.Call(mi, a |> List.map (fun i -> LiftTuplePreparationWithCall(i)))
            | _ ->
                e                
                       
        match e with
        | Patterns.Let(_, Patterns.NewTuple(tl), b) ->
            liftTupleInternal(b, tl)
        | ExprShape.ShapeVar(v) ->
            e
        | ExprShape.ShapeLambda(v, l) ->
            Expr.Lambda(v, LiftTuplePreparationWithCall(l))
        | ExprShape.ShapeCombination(o, l) ->
            ExprShape.RebuildShapeCombination(o, l |> List.map (fun i -> LiftTuplePreparationWithCall(i)))

    let rec LiftApplicationWithCall(e: Expr) =
        let rec liftApplicationInternal(e:Expr, applicationArgs: Expr list) =            
            match e with
            | Patterns.Application(l, a) ->
                liftApplicationInternal(l, [ a ] @ applicationArgs)                
            | Patterns.Lambda(v, b) ->
                liftApplicationInternal(b, applicationArgs)
            | Patterns.Call(ob, mi, a) ->
                match mi with 
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                    if ob.IsSome then
                        Expr.Call(ob.Value, mi, applicationArgs |> List.map (fun i -> LiftApplicationWithCall(i)))
                    else
                        Expr.Call(mi, applicationArgs |> List.map (fun i -> LiftApplicationWithCall(i)))
                | _ ->
                    if ob.IsSome then
                        Expr.Call(ob.Value, mi, a |> List.map (fun i -> LiftApplicationWithCall(i)))
                    else
                        Expr.Call(mi, a |> List.map (fun i -> LiftApplicationWithCall(i)))
            | _ ->
                e                
                        
        match e with
        | Patterns.Application(_, _) ->
            liftApplicationInternal(e, [])
        | ExprShape.ShapeVar(v) ->
            e
        | ExprShape.ShapeLambda(v, l) ->
            Expr.Lambda(v, LiftApplicationWithCall(l))
        | ExprShape.ShapeCombination(o, l) ->
            ExprShape.RebuildShapeCombination(o, l |> List.map (fun i -> LiftApplicationWithCall(i)))
            
    override this.Run(fInfo, en, opts) =
        let engine = en :?> FunctionPreprocessingStep
        // Remove preamble of calls and args binding
        fInfo.Body <- 
            LiftTuplePreparationWithCall(
                LiftApplicationWithCall(
                    (LiftCurriedOrTupledArgs(fInfo.Body)).Value  |> fst))
        ()
