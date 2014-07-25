namespace FSCL.Compiler.AcceleratedCollections

open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open System.Reflection
open System.Collections.Generic
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System
open FSCL.Compiler.Util
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.RuntimeHelpers

module AcceleratedCollectionUtil =
    let GenKernelName (prefix: string, parameterTypes: Type list, utilityFunction: string) =
        String.concat "_" ([prefix] @ (List.map (fun (t:Type) -> t.Name.Replace(".", "")) parameterTypes) @ [utilityFunction])
    
    let ToTupledFunction(f: Expr) = 
        let rec convertToTupledInternal(tupledVar: Var, tupledIndex: int, e: Expr) =
            match e with
            | Patterns.Lambda(v, body) ->
                Expr.Let(v, 
                         Expr.TupleGet(Expr.Var(tupledVar), tupledIndex), 
                         convertToTupledInternal(tupledVar, tupledIndex + 1, body)) 
            | _ ->
                e
        let rec extractParamTypesInternal(currentList: Type list, e: Expr) =
            match e with
            | Patterns.Lambda(v, body) ->
                extractParamTypesInternal(currentList @ [ v.Type ], body)
            | _ ->
                currentList
                   
        match f with
        | Patterns.Lambda(v, e) ->
            if v.Name = "tupledArg" then
                // Already tupled
                f
            else
                let types = extractParamTypesInternal([], f)
                let tupledVarType = FSharpType.MakeTupleType(types |> List.toArray)
                let tupledVar = Quotations.Var("tupledArg", tupledVarType)
                Expr.Lambda(tupledVar, convertToTupledInternal(tupledVar, 0, e))
        | _ ->
            failwith "Cannot convert to tupled an expression that doesn't contain a function"

    // Check if the expr is a function reference (name)
    let rec FilterCall(expr, f) =                 
        match expr with
        | Patterns.Lambda(v, e) -> 
            FilterCall (e, f)
        | Patterns.Let (v, e1, e2) ->
            FilterCall (e2, f)
        | Patterns.Call (e, mi, a) ->
            Some(f(e, mi, a))
        | _ ->
            None 
            
    // Check if the argument is a lambda expression 
    let GetComputationalLambdaOrMethodInsideLambda(arg, expr) =        
        let rec GetLambda(expr, var:Var) =         
            match expr with
            | Patterns.Let (dv, e1, e2) ->
                match e1 with
                | Patterns.Lambda(v, e) ->  
                    if dv = var then
                        // Check if this is a computational lambda, that is it is not lambda(lambda(...call)) where
                        // the call is to a reflected method
                        QuotationAnalysis.GetComputationalLambdaOrReflectedMethodInfo(e1)
                    else
                        GetLambda(e2, var)
                | _ ->
                    GetLambda(e2, var)
            | _ ->
                None, None 
          
        match arg with
        | Patterns.Var(v) ->
            GetLambda(expr, v)            
        | Patterns.Lambda(v, e) ->
            QuotationAnalysis.GetComputationalLambdaOrReflectedMethodInfo(arg)
        | _ ->
            None, None

    let ExtractComputationFunction(args: Expr list, root) =     
        let calledMethod, lambda = GetComputationalLambdaOrMethodInsideLambda(args.[0], root)
        // If lambda we create a method for it
        let computationFunction =                
            match calledMethod, lambda with
            | Some(_), Some(_) ->
                failwith ("Computation extracted resulted in both a method called and an in-place lambda")                
            | Some(o, mi, args, b, lambdaParams), None ->
                // Lambda containing method to apply: fun it -> DoSomething x y z it
                // We must close DoSomething replacing params that are references to stuff outside quotation (!= it)
                // More precisely, if some paramters of mi are not contained in lambdaParams
                // this means that the lambda is something like fun a b c -> myMethod a b c othPar
                // So othPar must be evaluated, replacing it's current value inside myMethod
                // Otherwise, the kernel would not be able to invoke it (it doesn't manage othPar)
                Some(QuotationAnalysis.LiftNonLambdaParamsFromMethodCalledInLambda(mi, args, b, lambdaParams))
            | None, Some(l) ->
                // Computational lambda to apply to collection
                match QuotationAnalysis.LambdaToMethod(l, false) with                
                | Some(m, paramInfo, paramVars, b, _, _, _) ->
                    Some(m, paramVars, b)
                | _ ->
                    failwith ("Cannot parse the body of the computation function " + root.ToString())
            | None, None ->
                // No lambda but method ref used as function to apply to collection
                FilterCall(args.[0], 
                    fun (e, mi, a) ->                         
                        match mi with
                        | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                            match QuotationAnalysis.GetCurriedOrTupledArgs(body) with
                            | Some(paramVars) ->
                                (mi, paramVars, body)
                            | _ ->
                                failwith ("Cannot parse the body of the computation function " + mi.Name)
                        | _ ->
                            failwith ("Cannot parse the body of the computation function " + mi.Name))
        lambda, computationFunction

    (* 
     * Replace the arguments of a call
     * This is useful since inside <@ Array.arrfun(f) @> f is represented by Lambda(x, Call(f, [x]))
     * After the kernel generation, we want to replace x with something like "input_array[global_index]",
     * i.e. the element of the kernel input array associated to a particular OpenCL work item
     *)
    let rec ReplaceCallArgs(expr, newArgs) =                 
        match expr with
        | Patterns.Lambda(v, e) -> 
            ReplaceCallArgs (e, newArgs)
        | Patterns.Let (v, e1, e2) ->
            ReplaceCallArgs (e2, newArgs)
        | Patterns.Call (e, mi, a) ->
            if e.IsSome then
                Some(Expr.Call(e.Value, mi, newArgs))
            else
                Some(Expr.Call(mi, newArgs))
        | _ ->
            None 
            
    // Instantiate a quoted generic method
    let GetGenericMethodInfoFromExpr(q, ty:System.Type) = 
        let gminfo = 
            match q with 
            | Patterns.Call(_,mi,_) -> mi.GetGenericMethodDefinition()
            | _ -> failwith "unexpected failure decoding quotation"
        gminfo.MakeGenericMethod [| ty |]

    // Get the appropriate get and set MethodInfo to read and write an array
    let GetArrayAccessMethodInfo(ty) =
        let get = GetGenericMethodInfoFromExpr(<@@ LanguagePrimitives.IntrinsicFunctions.GetArray<int> null 0 @@>, ty)
        let set = GetGenericMethodInfoFromExpr(<@@ LanguagePrimitives.IntrinsicFunctions.SetArray<int> null 0 0 @@>, ty)
        (get, set)
        
    let GetArrayLengthMethodInfo(ty) =
        let arr = [| 0 |]
        let get = GetGenericMethodInfoFromExpr(<@@ arr.GetLength(0) @@>, ty)
        get
        
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
            
                        
            

                     
            
            