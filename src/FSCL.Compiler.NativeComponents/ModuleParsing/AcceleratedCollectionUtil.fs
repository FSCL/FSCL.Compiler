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
open FSCL.Compiler.ModuleParsing
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

module AcceleratedCollectionUtil =
    let GenKernelName (prefix: string, parameterTypes: Type list, utilityFunction: string) =
        String.concat "_" ([prefix] @ (List.map (fun (t:Type) -> t.Name.Replace(".", "")) parameterTypes) @ [utilityFunction])
    
    let BuildCallExpr(ob: Expr option, mi: MethodInfo, a: Expr list) =
        if ob.IsSome then
            Expr.Call(ob.Value, mi, a)
        else
            Expr.Call(mi, a)

    let GetDefaultValueExpr(t:Type) =
        if t.IsPrimitive then
            // Primitive type
            Expr.Value(Activator.CreateInstance(t), t)
        else if FSharpType.IsRecord(t) then
            // Record
            Expr.DefaultValue(t)
        else 
            if t.GetCustomAttribute<FSCL.VectorTypeAttribute>() <> null then
                // Vector type
                Expr.DefaultValue(t) 
            else if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum) && (t <> typeof<unit>) && (t <> typeof<System.Void>)) then   
                // Struct
                Expr.DefaultValue(t)
            else
                failwith ("Cannot create a default init expr for type " + t.ToString())
                  
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
            
    // Important function of new language
    // Should be able to determine in CollectionFun(op) if op is
    // 1) A lambda preparing a call to an utility function
    // 2) A lambda that should be turned into an utility function
    // 3) A sub-expression (CollectionFun is not a kernel)
    let ParseOperatorLambda(fr: Expr, step: ModuleParsingStep, currEnv: Var list) =  
        let rec isSubExpr(k: KFGNode) =
            match k with
            | :? KFGKernelNode ->
                true
            | :? KFGCollectionCompositionNode ->
                true
            | :? KFGSequentialFunctionNode ->
                let inp = (k :?> KFGSequentialFunctionNode).InputNodes |> 
                                Seq.map(fun i -> isSubExpr(i :?> KFGNode)) |>
                                Seq.reduce (||)
                inp
            | _ ->
                false
//                
//        let rec collectOutsiders(k: IKFGNode, l:List<OutsiderRef>) =
//            match k.Type with
//            | KFGNodeType.OutsiderDataNode ->
//                let dn = k :?> KFGOutsiderDataNode 
//                let existing = l |> Seq.tryFind(fun r ->
//                                                    match r, dn.Outsider with
//                                                    | ValueRef(d1), ValueRef(d2) ->
//                                                        d1.Equals(d2)
//                                                    | VarRef(v1), VarRef(v2) ->
//                                                        v1 = v2
//                                                    | _, _ ->
//                                                        false)   
//                if not existing.IsSome then
//                    l.Add(dn.Outsider)
//                | _ ->
//                    ()
//            | _ ->
//                k.Input |> Seq.iter(fun i -> collectOutsiders(i, l))
           
        let rec checkIfPreparationToUtilityFunction(expr, lambdaParams: Var list) =
            match expr with
            | Patterns.Lambda(v, e) -> 
                checkIfPreparationToUtilityFunction(e, lambdaParams @ [ v ])
            | Patterns.Let(v, 
                            Patterns.TupleGet(
                                Patterns.Var(tv), i), b) when tv.Name = "tupledArg" ->
                checkIfPreparationToUtilityFunction(b, lambdaParams @ [ tv ])
            | _ ->           
                match expr with
                | CallToUtilityFunctionMethodInfo (e, mi, a, b) ->
                    // Check if params are exactly the same of lambda
                    let p = mi.GetParameters()
                    let mutable isMatch = p.Length = lambdaParams.Length
                    let mutable i = 0
                    while isMatch && i < Math.Min(p.Length, lambdaParams.Length) do
                        if p.[i].ParameterType <> lambdaParams.[i].Type then
                            isMatch <- false
                        else
                            i <- i + 1
                    if isMatch then
                        Some(e, mi, a, b, lambdaParams)
                    else
                        None
                | _ ->
                    None
                    
        match fr with
        | Patterns.Lambda(v, e) -> 
            match checkIfPreparationToUtilityFunction(fr, []) with
            | Some(e, mi, a, b, lambdaParams) ->
                // Simple preparation for an utility function call: CollectionFun(myUtilityFun)
                let thisVar = GetThisVariable(b)
                Some(thisVar, e, mi, lambdaParams, b), None, true
            | _ ->         
                // Try parse sub-expression
                // Get the params of the operator lambda
                // These are the (additional) env vars for what's inside the operator
                let newEnv, lambdaBody = GetLambdaEnvironment(fr) 

                // Process lambdaBody
                let subExpr = step.Process(lambdaBody, newEnv)

                // Check if any kernel call in the graph rooted in subExpr
                if isSubExpr(subExpr :?> KFGNode) then
                    //collectOutsiders(subExpr, outsiders)
                    // Now check which outsider is in the evn of this collection function
                    None, Some(subExpr, newEnv), false
                else
                    // Otherwise, this is a computing lambda (must turn into method)
//                    // Now check which outsider is in the evn of this collection function
//                    // Those who are not should be outsider of this function too
//                    let thisOutsiders = new List<OutsiderRef>()
//                    for o in outsiders do
//                        match o with 
//                        | VarRef(v) when (newEnv |> List.tryFind(fun i -> i = v)).IsSome ->
//                            ()
//                        | o ->
//                            thisOutsiders.Add(o)

                    match QuotationAnalysis.KernelParsing.LambdaToMethod(fr, false) with
                    | Some(mi, paramInfo, parameters) ->
                        Some(None, None, mi, parameters, fr), None, true
                    | _ ->
                        None, None, false
        | _ ->
            None, None, false

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
        
    let GetNewZeroArrayMethodInfo(ty) =
        let get = GetGenericMethodInfoFromExpr(<@@ Array.zeroCreate<int> 2 @@>, ty)
        get
                
    let GetKernelFromCollectionFunctionTemplate(expr:Expr) = 
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
            
                        
            

            
                        
            

                     
            
            