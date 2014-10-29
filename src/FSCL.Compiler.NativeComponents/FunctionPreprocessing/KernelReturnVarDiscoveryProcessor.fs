namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<StepProcessor("FSCL_KERNEL_RETURN_DISCOVERY_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies = [| "FSCL_LOCAL_VARS_DISCOVERY_PREPROCESSING_PROCESSOR" |])>]
type KernelReturnExpressionDiscoveryProcessor() =                
    inherit FunctionPreprocessingProcessor()

    let rec ReplaceScalarReturnExpressions(expr: Expr, retExprs: Expr list, autoArray: FunctionParameter) =
        let GetGenericMethodInfoFromExpr (q, ty:System.Type) = 
            let gminfo = 
                match q with 
                | Patterns.Call(_,mi,_) -> mi.GetGenericMethodDefinition()
                | _ -> failwith "unexpected failure decoding quotation at ilreflect startup"
            gminfo.MakeGenericMethod [| ty |]
        let writeArrayMethod = GetGenericMethodInfoFromExpr(<@@ LanguagePrimitives.IntrinsicFunctions.SetArray<int> null 0 0 @@>, autoArray.DataType.GetElementType())
        
        if (retExprs |> List.tryFind(fun item -> item = expr)).IsSome then
            Expr.Call(writeArrayMethod, [ Expr.Var(autoArray.Placeholder); Expr.Value(0); expr ])
        else
            match expr with
            | ExprShape.ShapeVar(v1) ->
                expr
            | ExprShape.ShapeLambda(v1, l1) ->
                Expr.Lambda(v1, ReplaceScalarReturnExpressions(l1, retExprs, autoArray))
            | ExprShape.ShapeCombination(o1, l1) ->
                ExprShape.RebuildShapeCombination(o1, l1 |> List.map(fun item -> ReplaceScalarReturnExpressions(item, retExprs, autoArray)))
            
        
    let rec SearchReturnExpressions(expr:Expr, returnExpression: List<Expr * bool>, engine:FunctionPreprocessingStep) =
        match expr with
        | Patterns.Let(v, value, body) ->
            SearchReturnExpressions(body, returnExpression, engine)
        | Patterns.Sequential(e1, e2) ->
            SearchReturnExpressions(e2, returnExpression, engine)
        | Patterns.IfThenElse(e, ifb, elseb) ->
            SearchReturnExpressions(ifb, returnExpression, engine)
            SearchReturnExpressions(elseb, returnExpression, engine)
        | Patterns.Lambda(v, e) ->
            SearchReturnExpressions(e, returnExpression, engine)            
        | _ ->
            // This could be a return expression if its type is subtype of return type
            let returnType = expr.Type
            if returnType <> typeof<unit> && engine.FunctionInfo.ReturnType.IsAssignableFrom(returnType) then
                // Verify that return expression is 
                // A) An Array and therefore a reference to a parameter  (possibly generated) or
                // B) A scalar value, reference to parameter, variable or something else
                if expr.Type.IsArray then
                    match expr with
                    | Patterns.Var(v) -> 
                        if returnExpression.Count = 0 then
                            returnExpression.Add(expr, true)
                        if (returnExpression |> Seq.tryFind(fun re -> 
                             match re with
                             | Patterns.Var(ov), _ ->
                                 ov <> v
                             | _, _ ->
                                 true)).IsSome then
                            raise (new CompilerException("Cannot return different variables/parameters of type array or ref cell from within a kernel"))                        
                    | _ ->
                        raise (new CompilerException("If an array type is returned from a kernel it must be a reference to a parameter or to a local array"))
                else
                    // If this is not an array type then add to return exprs
                    raise (new CompilerException("Only a reference to a parameter or local variable of type array can be returned from a kernel"))
                    //returnExpression.Add(expr, false)
                                    
            
    override this.Run(info, s, opts) =    
        let engine = s :?> FunctionPreprocessingStep
        let returnExprs = new List<Expr * bool>()

        if info :? KernelInfo then
            SearchReturnExpressions(info.Body, returnExprs, engine)

            // Verify that return expressions are all references
            if returnExprs.Count > 0 then
                // If array
                if returnExprs.[0] |> snd then
                    let returnVariable = 
                        match returnExprs.[0] |> fst with
                        | Patterns.Var(v) ->
                            v
                        | _ ->
                            failwith "Not possible"
                    // Set return parameter to void
                    engine.FunctionInfo.ReturnType <- typeof<unit>
                    // Mark parameter returned as ReturnParameter                
                    let p = Seq.tryFind(fun (e:FunctionParameter) -> e.OriginalPlaceholder = returnVariable) (engine.FunctionInfo.Parameters)
                    if p.IsNone then
                        //No parameter found, the dynamic array to param processor may solve this
                        engine.FunctionInfo.CustomInfo.Add("RETURN_ARRAY_VAR", returnVariable) //raise (new CompilerException("Kernels can only return a reference to a parameter or to a dynamically allocated array"))
                    else      
                        p.Value.IsReturned <- true
                else
                    failwith "Not possible"
                    (*
                    // We must create an automatic arrays to store the scalar value
                    // And replace return expressions with a write into it
                    let returnExprs = returnExprs |> List.ofSeq |> List.unzip |> fst

                    // Set return parameter to void
                    engine.FunctionInfo.ReturnType <- typeof<unit>

                    // If some of the return exprs is a ref to a scalar parm use the relative metadata
                    let p = 
                        returnExprs |> Seq.tryFind(fun e -> 
                                                    match e with 
                                                    | Patterns.Var(v) ->
                                                        (Seq.tryFind(fun (e:FunctionParameter) -> e.OriginalPlaceholder = v) (engine.FunctionInfo.Parameters)).IsSome
                                                    | _ ->
                                                        false)
                    let meta =
                        if p.IsSome then
                            match p.Value with 
                            | Patterns.Var(v) ->
                                Seq.tryFind(fun (e:FunctionParameter) -> e.OriginalPlaceholder = v) (engine.FunctionInfo.Parameters) 
                            | _ ->
                                failwith "Not possible"
                        else
                            None
                            
                    // Create an array parameter
                    let pInfo = new FunctionParameter("returnedArrayForScalar",
                                                      Quotations.Var("returnedArrayForScalar", returnExprs.[0].Type.MakeArrayType()),
                                                      FunctionParameterType.AutoArrayParameter,
                                                      if meta.IsSome then 
                                                        Some(meta.Value.Meta)
                                                      else
                                                        None)                 
                    pInfo.IsReturned <- true                                         
                    (engine.FunctionInfo :?> KernelInfo).GeneratedParameters.Add(pInfo)   

                    // Replace return expressions right now (we do not have to wait for local var discovery since the returned item is not an array)
                    engine.FunctionInfo.Body <- ReplaceScalarReturnExpressions(engine.FunctionInfo.Body, returnExprs, pInfo)
                    ()
                    *)
        