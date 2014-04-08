namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.Language
open System.Collections.Generic
open System.Reflection
open System.Collections.Generic
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System
open FSCL.Compiler.Util

module AcceleratedCollectionUtil =
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
            
    // Check if the argument is a lambda expression of the reference of a lambda expression declared in expr
    let GetLambdaArgument(arg, expr) =        
        let rec GetLambda(expr, var:Var) =         
            match expr with
            | Patterns.Let (dv, e1, e2) ->
                match e1 with
                | Patterns.Lambda(v, e) ->  
                    if dv = var then
                        // Check if this is a computational lambda, that is it is not lambda(lambda(...call)) where
                        // the call is to a reflected method
                        if QuotationAnalysis.IsComputationalLambda(e1) then
                            Some(e1)
                        else
                            None
                    else
                        GetLambda(e2, var)
                | _ ->
                    GetLambda(e2, var)
            | _ ->
                None 
          
        match arg with
        | Patterns.Var(v) ->
            match GetLambda(expr, v) with
            | Some(l) ->
                Some(l)
            | None ->
                None
        | Patterns.Lambda(v, e) -> 
            if QuotationAnalysis.IsComputationalLambda(arg) then
                Some(arg)
            else
                None
        | _ ->
            None

    let ExtractComputationFunction(args: Expr list, root) =     
        let lambda = GetLambdaArgument(args.[0], root)
        let computationFunction =                
            match lambda with
            | Some(l) ->
                match QuotationAnalysis.LambdaToMethod(l) with                
                | Some(m, b, _, _, _) ->
                    Some(m, b)
                | _ ->
                    failwith ("Cannot parse the body of the computation function " + root.ToString())
            | None ->
                FilterCall(args.[0], 
                    fun (e, mi, a) ->                         
                        match mi with
                        | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                            (mi, body)
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
            
                        
            

                     
            
            