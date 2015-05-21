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

//RETURN_TYPE_TO_OUTPUT_ARG_REPLACING
[<StepProcessor("FSCL_ARRAY_SIZE_LAMBDA_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies = [|"FSCL_ADD_LENGTH_ARGS_PREPROCESSING_PROCESSOR"|])>]
type PrecomputeDynamicReturnArraySize() =
    inherit FunctionPreprocessingProcessor()
    
    let PrepareLambdaWrappingParams(e: Expr,
                                    kInfo: KernelInfo) =
        let rec findWorkItemInfoVar(e:Expr) =
            match e with
            | ExprShape.ShapeVar(v) when typeof<WorkItemInfo>.IsAssignableFrom(v.Type) ->
                Some(v)
            | ExprShape.ShapeVar(v) ->
                None
            | ExprShape.ShapeLambda(l, b) ->
                findWorkItemInfoVar(b)
            | ExprShape.ShapeCombination(o, args) ->
                args |> List.tryPick(findWorkItemInfoVar)

        let workItemInfo = 
            match findWorkItemInfoVar(e) with
            | Some(v) ->
                v
            | _ ->
                Quotations.Var("workItemInfoFakePlaceholder", typeof<WorkItemInfo>)

        let thisVar = 
            Util.QuotationAnalysis.FunctionsManipulation.SearchThisVariable(e)
                        
        let rec ReplaceArrayLengthWithRefToParam(e: Expr) =
            match e with
            // Return allocation expression can contain a call to global_size, local_size, num_groups or work_dim
            | Patterns.Call(o, methodInfo, arguments) ->
                // Get length replaced with appropriate size parameter
                if methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "Array" && (methodInfo.Name = "GetLength" || methodInfo.Name = "GetLongLength") then
                    match o.Value with
                    | Patterns.Var(v) ->
                        let arraySizeParameters = kInfo.Parameters |> Seq.pick(fun p ->
                                                                                    if p.OriginalPlaceholder = v then
                                                                                        Some(p.SizeParameters |> Seq.map(fun i -> i.OriginalPlaceholder) |> Seq.toList)
                                                                                    else
                                                                                        None)
                        if arguments.[0].GetFreeVars() |> Seq.length > 0 then
                            raise (new CompilerException("Invalid return array allocation expression: the allocation expression is not constant"))
                        else
                            let index = LeafExpressionConverter.EvaluateQuotation(arguments.[0]) :?> int
                            if methodInfo.Name = "GetLongLength" then
                                let newExpr = Expr.Var(arraySizeParameters.[index])
                                <@@ (int64)(%%newExpr) @@>
                            else
                                Expr.Var(arraySizeParameters.[index])
                    | _ ->
                        raise (new CompilerException("A dynamic return array cannot be allocated referencing other arrays different from the arguments"))
                else
                    if o.IsSome then
                        Expr.Call(o.Value, methodInfo, arguments |> List.map ReplaceArrayLengthWithRefToParam)
                    else
                        Expr.Call(methodInfo, arguments |> List.map ReplaceArrayLengthWithRefToParam)                    
            | Patterns.PropertyGet(eOpt, pInfo, args) ->
                if eOpt.IsSome && eOpt.Value.Type.IsArray then
                    match eOpt.Value with
                    | Patterns.Var(v) ->
                        if pInfo.DeclaringType.Name = "Array" && (pInfo.Name = "Length" || pInfo.Name = "LongLength") then
                            let arraySizeParameters = kInfo.Parameters |> Seq.pick(fun p ->
                                                                                    if p.OriginalPlaceholder = v then
                                                                                        Some(p.SizeParameters |> Seq.map(fun i -> i.OriginalPlaceholder) |> Seq.toList)
                                                                                    else
                                                                                        None)
                        
                            // If local then replace with allocation expression for the proper index
                            let totalCountExpr = 
                                if arraySizeParameters.Length = 1 then
                                    Expr.Var(arraySizeParameters.[0])
                                else if arraySizeParameters.Length = 2 then
                                    <@@ (%%(Expr.Var(arraySizeParameters.[0]))) *
                                        (%%(Expr.Var(arraySizeParameters.[1])))
                                    @@>
                                else
                                    <@@ (%%(Expr.Var(arraySizeParameters.[0]))) *
                                        (%%(Expr.Var(arraySizeParameters.[1]))) *
                                        (%%(Expr.Var(arraySizeParameters.[2])))
                                    @@>                                    
                                    
                            if pInfo.Name = "LongLength" then
                                <@@ (int64)(%%totalCountExpr) @@>
                            else
                                totalCountExpr
                        else
                            if eOpt.IsSome then
                                Expr.PropertyGet(eOpt.Value, pInfo, args |> List.map ReplaceArrayLengthWithRefToParam)
                            else
                                Expr.PropertyGet(pInfo, args |> List.map ReplaceArrayLengthWithRefToParam)
                    | _ ->
                        if eOpt.IsSome then
                            Expr.PropertyGet(eOpt.Value, pInfo, args |> List.map ReplaceArrayLengthWithRefToParam)
                        else
                            Expr.PropertyGet(pInfo, args |> List.map ReplaceArrayLengthWithRefToParam)
                else
                    if eOpt.IsSome then
                        Expr.PropertyGet(eOpt.Value, pInfo, args |> List.map ReplaceArrayLengthWithRefToParam)
                    else
                        Expr.PropertyGet(pInfo, args |> List.map ReplaceArrayLengthWithRefToParam)
            | ExprShape.ShapeVar(v) ->
                e
            | ExprShape.ShapeLambda(l, b) ->
                Expr.Lambda(l, ReplaceArrayLengthWithRefToParam b)
            | ExprShape.ShapeCombination(o, args) ->
                ExprShape.RebuildShapeCombination(o, args |> List.map ReplaceArrayLengthWithRefToParam)

        let body =
            ReplaceArrayLengthWithRefToParam(e)

        // Now build the lambda
        // Keep all the params except return arrays, input arrays (keep relative size params)
        let parameters = 
            let pl = new List<Var>()
            if thisVar.IsSome then
                pl.Add(thisVar.Value)
            let sizepl = new List<Var>()
            for p in kInfo.Parameters do
                match p.ParameterType with
                | FunctionParameterType.DynamicReturnArrayParameter(_) ->
                    sizepl.AddRange(p.SizeParameters |> Seq.map(fun p -> p.OriginalPlaceholder))
                | FunctionParameterType.SizeParameter ->
                    if sizepl.Contains(p.OriginalPlaceholder) |> not then
                        pl.Add(p.OriginalPlaceholder)
                | _ ->
                    if p.DataType.IsArray |> not then
                        pl.Add(p.OriginalPlaceholder)
            pl.Add(workItemInfo)
            pl |> Seq.toList

        let lambda =
            (parameters, body) ||> 
            List.foldBack(fun v expr ->
                            Expr.Lambda(v, expr))
        LeafExpressionConverter.EvaluateQuotation(lambda)
                
    let SizeEvaluator lambda (thisObj, args: obj list, workItemInfo: obj) =
        let mutable v = lambda
        if thisObj <> null then
            v <- v.GetType().GetMethod("Invoke").Invoke(v, [| thisObj |])
        for a in args do
            v <- v.GetType().GetMethod("Invoke").Invoke(v, [| a |])
        v <- v.GetType().GetMethod("Invoke").Invoke(v, [| workItemInfo |]) 
        v :?> int
        
    override this.Run(fInfo, en, opts) =
        let engine = en :?> FunctionPreprocessingStep
        if (fInfo :? KernelInfo) then
            for p in fInfo.Parameters do
            match p.ParameterType with
            | FunctionParameterType.DynamicReturnArrayParameter(args) ->
                for i = 0 to args.Length - 1 do
                    let expr, _ = args.[i] 
                    let evaluator = SizeEvaluator (PrepareLambdaWrappingParams(expr, fInfo :?> KernelInfo))
                    args.[i] <- expr, evaluator
            | _ ->
                ()
       
