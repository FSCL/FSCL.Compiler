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
open FSCL.Compiler.AcceleratedCollections

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

    let rec LiftApplicationWithCall(e: Expr, utilityFunction: Expr option) =
        let rec liftApplicationInternal(e:Expr, applicationArgs: Expr list) =            
            match e with
            | Patterns.Application(l, a)  ->
                let lambda, args = Util.QuotationAnalysis.FunctionsManipulation.LiftLambdaApplication(e).Value
                if utilityFunction.IsSome && utilityFunction.Value = lambda then
                    e
                else
                    liftApplicationInternal(l, [ a ] @ applicationArgs)                
            | Patterns.Lambda(v, b) ->
                liftApplicationInternal(b, applicationArgs)
            | Patterns.Call(ob, mi, a) ->
                match mi with 
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                    if ob.IsSome then
                        Expr.Call(ob.Value, mi, applicationArgs |> List.map (fun i -> LiftApplicationWithCall(i, utilityFunction)))
                    else
                        Expr.Call(mi, applicationArgs |> List.map (fun i -> LiftApplicationWithCall(i, utilityFunction)))
                | _ ->
                    if ob.IsSome then
                        Expr.Call(ob.Value, mi, a |> List.map (fun i -> LiftApplicationWithCall(i, utilityFunction)))
                    else
                        Expr.Call(mi, a |> List.map (fun i -> LiftApplicationWithCall(i, utilityFunction)))
            | _ ->
                e                
                        
        match e with
        | Patterns.Application(_, _) ->
            liftApplicationInternal(e, [])
        | ExprShape.ShapeVar(v) ->
            e
        | ExprShape.ShapeLambda(v, l) ->
            Expr.Lambda(v, LiftApplicationWithCall(l, utilityFunction))
        | ExprShape.ShapeCombination(o, l) ->
            ExprShape.RebuildShapeCombination(o, l |> List.map (fun i -> LiftApplicationWithCall(i, utilityFunction)))
            
    override this.Run(fInfo, st, opts) =
        let step = st :?> FunctionPreprocessingStep
        // Remove preamble of calls and args binding
        // But do not remove Application of the AcceleratedCollection utility function
        fInfo.Body <- 
            LiftTuplePreparationWithCall(
                LiftApplicationWithCall(
                    LiftCurriedOrTupledArgs(fInfo.Body).Value |> fst, 
                    if step.FunctionInfo :? AcceleratedKernelInfo then
                        (step.FunctionInfo :?> AcceleratedKernelInfo).AppliedFunctionLambda
                    else
                        None))
        ()