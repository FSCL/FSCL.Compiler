﻿namespace FSCL.Compiler.Util

open System
open System.Text
open System.Security.Cryptography
open System.Reflection
open System.Reflection.Emit
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns
open Microsoft.FSharp.Linq.RuntimeHelpers
open FSCL.Language
open FSCL.Compiler
open FSCL
open System.Collections.ObjectModel

module QuotationAnalysis =
    module MetadataExtraction =
        let private ParseKernelMetadataFunctions(expr, 
                                                 kernelAttrs: KernelMetaCollection,
                                                 returnAttrs: ParamMetaCollection) =
            let rec ParseMetadataInternal(expr) =
                match expr with
                | Patterns.Call(o, mi, args) ->
                
                    let attrFunction = mi.GetCustomAttribute<MetadataFunctionAttribute>()
                    if attrFunction <> null then
                        if (attrFunction.Target = MetadataFunctionTarget.KernelFunction || 
                            attrFunction.Target = MetadataFunctionTarget.KernelReturnType) then
                            // Get attribute type
                            let attrType = 
                                if attrFunction.Target = MetadataFunctionTarget.KernelFunction then
                                    attrFunction.Metadata
                                else
                                    attrFunction.Metadata
                            // First n - 1 args are the parameters to instantiate attribute, last is one is target (forwarded)
                            let attrArgs = args |> 
                                            Seq.take(args.Length - 1) |> 
                                            Seq.map(fun (e:Expr) -> LeafExpressionConverter.EvaluateQuotation(e)) |> 
                                            Seq.toArray
                            let attrArgsType = args |> 
                                                Seq.take(args.Length - 1) |> 
                                                Seq.map(fun (e:Expr) -> e.Type) |> 
                                                Seq.toArray
                            // Instantiate attribute
                            let constr = attrType.GetConstructor(attrArgsType)
                            if constr = null then
                                raise (new CompilerException("Cannot instantiate attribute " + (attrType.ToString()) + " cause a proper constructor cannot be found"))
                            else
                                if attrFunction.Target = MetadataFunctionTarget.KernelFunction then
                                    let attr = constr.Invoke(attrArgs) :?> KernelMetadataAttribute
                                    kernelAttrs.AddOrSet(attr)
                                else
                                    // Return attr function
                                    let attr = constr.Invoke(attrArgs) :?> ParameterMetadataAttribute
                                    returnAttrs.AddOrSet(attr)
                                // Continue processing body
                                ParseMetadataInternal(args.[args.Length - 1])
                        else if attrFunction.Target = MetadataFunctionTarget.KernelParameter then
                            let attrArgs = args |> 
                                            List.map(fun (e:Expr) -> ParseMetadataInternal(e))
                            if o.IsSome then
                                Expr.Call(o.Value, mi, attrArgs)
                            else
                                Expr.Call(mi, attrArgs)
                        else
                            expr
                    else
                        expr
                | _ ->
                    expr
            ParseMetadataInternal(expr)
                
        let private ParseParameterMetadataFunctions(expr, 
                                                    attrs: ParamMetaCollection) =
            let rec ParseMetadataInternal(expr) =
                match expr with
                | Patterns.Call(o, mi, args) ->
                    let attrFunction = mi.GetCustomAttribute<MetadataFunctionAttribute>()

                    if attrFunction <> null then
                        if attrFunction.Target = MetadataFunctionTarget.KernelParameter then
                            // Get attribute type
                            let attrType = attrFunction.Metadata
                            // First n - 1 args are the parameters to instantiate attribute, last is one is target (forwarded)
                            let attrArgs = args |> 
                                            Seq.take(args.Length - 1) |> 
                                            Seq.map(fun (e:Expr) -> LeafExpressionConverter.EvaluateQuotation(e)) |> 
                                            Seq.toArray
                            let attrArgsType = args |> 
                                                Seq.take(args.Length - 1) |> 
                                                Seq.map(fun (e:Expr) -> e.Type) |> 
                                                Seq.toArray
                            // Instantiate attribute
                            let constr = attrType.GetConstructor(attrArgsType)
                            if constr = null then
                                raise (new CompilerException("Cannot instantiate attribute " + (attrType.ToString()) + " cause a proper constructor cannot be found"))
                            else
                                // Return attr function
                                let attr = constr.Invoke(attrArgs) :?> ParameterMetadataAttribute
                                attrs.AddOrSet(attr)

                                // Continue processing body
                                ParseMetadataInternal(args.[args.Length - 1])
                        else 
                            let attrArgs = args |> 
                                            List.map(fun (e:Expr) -> ParseMetadataInternal(e))
                            if o.IsSome then
                                Expr.Call(o.Value, mi, attrArgs)
                            else
                                Expr.Call(mi, attrArgs)
                    else
                        expr
                | _ ->
                    expr
            ParseMetadataInternal(expr)

        let ParseKernelMetadata(expr) =
            // Process attributes functions
            let attrs = new KernelMetaCollection()
            let returnAttrs = new ParamMetaCollection()
            let cleanExpr = ParseKernelMetadataFunctions(expr, attrs, returnAttrs)
        
            // Return
            (cleanExpr, attrs, returnAttrs)
        
        let MergeWithStaticKernelMeta(attrs: KernelMetaCollection, 
                                      returnAttrs: ParamMetaCollection, 
                                      m: MethodInfo) =
                                  
            let staticAttrs = m.GetCustomAttributes()
            for attr in staticAttrs do
                if attr :? ParameterMetadataAttribute then
                    if not (returnAttrs.Contains(attr.GetType())) then
                        returnAttrs.Add(attr :?> ParameterMetadataAttribute)
                else if attr :? KernelMetadataAttribute then
                    if not (attrs.Contains(attr.GetType())) then
                        attrs.Add(attr :?> KernelMetadataAttribute)

        let MergeWithStaticParameterMeta(attrs: ParamMetaCollection, 
                                         p: ParameterInfo) =
            // Merge with static attributes
            let staticAttrs = p.GetCustomAttributes()
            for attr in staticAttrs do
                if attr :? ParameterMetadataAttribute then
                    if not (attrs.Contains(attr.GetType())) then
                        attrs.Add(attr :?> ParameterMetadataAttribute)

        let ParseParameterMetadata(expr) =
            // Process dynamic attributes
            let attrs = new ParamMetaCollection()
            let cleanExpr = ParseParameterMetadataFunctions(expr, attrs)

            // Return
            (cleanExpr, attrs)
        
    module FunctionsManipulation =    
        let lambdaEnvStopName = "__environment__stop__"

        // Extract and lift function parameters in tupled form                               
        let LiftTupledArgs(expr) =
            let rec GetCallArgs(expr, parameters: Var list) =
                match expr with
                | Patterns.Let(v, value, body) ->
                    match value with
                    | Patterns.TupleGet(te, i) ->
                        GetCallArgs(body, parameters @ [ v ])
                    | _ ->
                        (expr, parameters)
                | _ ->
                    (expr, parameters)
                
            match expr with
            | Patterns.Lambda(v, Patterns.Lambda(vt, et)) when v.Name = "this" && vt.Name = "tupledArg" ->
                Some(GetCallArgs(et, []))
            | Patterns.Lambda(vt, et) when vt.Name = "tupledArg" ->
                Some(GetCallArgs(et, []))
            | _ ->
                None
            
        // Extract and lift function parameters in curried form
        let LiftCurriedArgs(expr) =
            let rec GetCallArgs(expr, parameters: Var list) =
                match expr with
                | Patterns.Lambda(v, body) ->
                    GetCallArgs(body, parameters @ [ v ])
                | _ ->
                    (expr, parameters)
                
            match expr with
            | Patterns.Lambda(v, e) ->
                Some(GetCallArgs(e, [v]))
            | _ ->
                None
             
        // Extract and lift tupled or curried paramters
        let LiftCurriedOrTupledArgs(expr) =
            match LiftTupledArgs(expr) with
            | Some(b, p) ->
                Some(b, p)
            | None ->
                match LiftCurriedArgs(expr) with
                | Some(b, p) ->
                    Some(b, p)
                | _ ->
                    None
                    
        // Extract lambda and args from an application of lambda
        let LiftLambdaApplication(expr) =
            let rec LiftLambdaApplicationRec(expr, args: Expr list) =
                match expr with
                | Patterns.Application(l, a) ->
                    LiftLambdaApplicationRec(l, a::args)
                | _ ->
                    expr, args

            match expr with
            | Patterns.Application(l, a) ->
                Some(LiftLambdaApplicationRec(l, [ a ]))
            | _ ->
                None
        
        // Extract and lift function parameters in tupled form                               
        let GetTupledArgs(expr) =
            let rec GetCallArgs(expr, parameters: Var list) =
                match expr with
                | Patterns.Let(v, value, body) ->
                    match value with
                    | Patterns.TupleGet(te, i) ->
                        GetCallArgs(body, parameters @ [ v ])
                    | _ ->
                        (parameters)
                | _ ->
                    (parameters)
                
            match expr with
            | Patterns.Lambda(v, e) ->
                if v.Name = "this" then
                    match e with
                    | Patterns.Lambda(vTuple, eTuple) ->                        
                        if vTuple.Name = "tupledArg" then
                            Some(v), Some(GetCallArgs(eTuple, []))
                        else
                            None, None
                    | _ ->
                        None, None
                else
                    if v.Name = "tupledArg" then
                        None, Some(GetCallArgs(e, []))
                    else
                        None, None
            | _ ->
                None, None
            
        // Extract and lift function parameters in curried form
        let GetCurriedArgs(expr) =
            let rec GetCallArgs(expr, parameters: Var list) =
                match expr with
                | Patterns.Lambda(v, body) ->
                    GetCallArgs(body, parameters @ [v])
                | _ ->
                    parameters
                
            match expr with
            | Patterns.Lambda(v, e) ->
                if v.Name = "this" then
                    Some(v), Some(GetCallArgs(e, []))
                else
                    None, Some(GetCallArgs(e, [ v ]))
            | _ ->
                None, None

        let GetCurriedOrTupledArgs(expr) =
            match GetTupledArgs(expr) with
            | th, Some(v) ->
                th, Some(v)
            | _, None ->
                match GetCurriedArgs(expr) with
                | th, Some(v) ->
                    th, Some(v)
                | _ ->
                    None, None

        let GetThisVariable(expr) =
            match expr with
            | Patterns.Lambda(v, e) ->
                if v.Name = "this" then
                    Some(v)
                else
                    None
            | _ ->
                None
                
        let rec SearchThisVariable(expr) =
            match expr with
            | ExprShape.ShapeVar(v) when v.Name = "this" ->
                Some(v)
            | ExprShape.ShapeVar(v) ->
                None
            | ExprShape.ShapeLambda(v, l) when v.Name = "this" ->
                Some(v)
            | ExprShape.ShapeLambda(v, l) ->
                SearchThisVariable(l)
            | ExprShape.ShapeCombination(o, l) ->
                l |> List.tryPick(SearchThisVariable)
        
        let rec ExtractCall(expr: Expr) =   
            let rec ExtractCallInternal(expr, boundVarExprs: Expr list, unboundVars: Var list) =
                match expr with
                | Patterns.Lambda(v, e) -> 
                    ExtractCallInternal (e, boundVarExprs, unboundVars @ [ v ])
                | Patterns.Let (v, e1, e2) ->
                    ExtractCallInternal (e2, boundVarExprs @ [ e1 ], unboundVars)
                | Patterns.Call (e, mi, a) ->
                    Some(e, mi, a, boundVarExprs, unboundVars)
                | _ ->
                    None       
            ExtractCallInternal(expr, [], []) 
            
        let rec ExtractCallWithReflectedDefinition(expr: Expr) =   
            let rec ExtractCallInternal(expr, boundVarExprs: Expr list, unboundVars: Var list) =
                match expr with
                | Patterns.Lambda(v, e) -> 
                    ExtractCallInternal (e, boundVarExprs, unboundVars @ [ v ])
                | Patterns.Let (v, e1, e2) ->
                    ExtractCallInternal (e2, boundVarExprs @ [ e1 ], unboundVars)
                | Patterns.Call (e, mi, a) ->
                    match mi with
                    | DerivedPatterns.MethodWithReflectedDefinition(body) ->   
                        Some(e, mi, body, a, boundVarExprs, unboundVars)
                    | _ ->
                        None
                | _ ->
                    None       
            ExtractCallInternal(expr, [], [])          
                
        let rec ExtractMethodInfo(expr) =                 
            match ExtractCall(expr) with
            | Some(_, mi, _, _, _) -> 
                Some(mi)
            | _ ->
                None 
                
        // In some closures we get let V = V in b, where V = V (same type and name)
        let rec ClearLambdaBodyFromLetLet(e:Expr) =
            match e with
            | Patterns.Let(v1, Patterns.Var(v2), body) when v1.Name = v2.Name && v1.Type = v2.Type ->
                let eq = v1 = v2
                let b = body
                body.Substitute(fun v ->
                                    if v = v1 then
                                        Some(Expr.Var(v2))
                                    else
                                        None) |> ClearLambdaBodyFromLetLet
            | _ ->
                e

        // Environment stuff
        let GetLambdaEnvironment(e:Expr) =
            let rec SetLambdaBody(env, l:Var list) =
                match env with
                | Patterns.Lambda(v, body) when v.Name = "tupledArg" ->
                    SetLambdaBody(body, l @ [v])
                | Patterns.Lambda(v, body) ->
                    SetLambdaBody(body, l @ [v])
                | Patterns.Let(v, Patterns.TupleGet(tv, i), body) ->
                    SetLambdaBody(body, l @ [v])
                | e ->
                    l, ClearLambdaBodyFromLetLet(e)          
                
            SetLambdaBody(e, [])


                
        let ApplyLambdaEnvironment(env:Var list, e:Expr) =
            let rec SetLambdaBody(e:Expr, l:Var list) =
                match l with
                | [] ->
                    e
                | head::tail ->
                    Expr.Lambda(head, SetLambdaBody(e, tail))
                    
            SetLambdaBody(e, env)

        let rec ReplaceVar(e:Expr, oldVar:Var, newVar:Var) =
            match e with
            | ExprShape.ShapeVar(v) when v = oldVar ->
                Expr.Var(newVar)
            | ExprShape.ShapeVar(v) ->
                e
            | ExprShape.ShapeLambda(v, l) ->
                Expr.Lambda(v, ReplaceVar(l, oldVar, newVar))
            | ExprShape.ShapeCombination(o, l) ->
                ExprShape.RebuildShapeCombination(o, l |> List.map(fun el -> ReplaceVar(el, oldVar, newVar)))
                
        let rec ReplaceExprWithVar(e:Expr, oldExpr:Expr, newVar:Var) =
            if e = oldExpr then
                Expr.Var(newVar)
            else
                match e with
                | ExprShape.ShapeVar(v)->
                    e
                | ExprShape.ShapeLambda(v, l) ->
                    Expr.Lambda(v, ReplaceExprWithVar(l, oldExpr, newVar))
                | ExprShape.ShapeCombination(o, l) ->
                    ExprShape.RebuildShapeCombination(o, l |> List.map(fun el -> ReplaceExprWithVar(el, oldExpr, newVar)))
        
        let rec ReplaceVarWithValue(e:Expr, oldVar:Var, o:obj) =
            match e with
            | ExprShape.ShapeVar(v) when v = oldVar ->
                Expr.Value(o)    
            | ExprShape.ShapeVar(_) ->
                e        
            | ExprShape.ShapeLambda(v, l) ->
                Expr.Lambda(v, ReplaceVarWithValue(l, oldVar, o))
            | ExprShape.ShapeCombination(o, l) ->
                ExprShape.RebuildShapeCombination(o, l |> List.map(fun el -> ReplaceVarWithValue(el, oldVar, o)))
                
        let rec ReplaceVarsWithValues(e:Expr, matching:Dictionary<Var, obj>) =
            match e with
            | ExprShape.ShapeVar(v) when matching.ContainsKey(v) ->
                Expr.Value(matching.[v])    
            | ExprShape.ShapeVar(_) ->
                e        
            | ExprShape.ShapeLambda(v, l) ->
                Expr.Lambda(v, ReplaceVarsWithValues(l, matching))
            | ExprShape.ShapeCombination(o, l) ->
                ExprShape.RebuildShapeCombination(o, l |> List.map(fun el -> ReplaceVarsWithValues(el, matching)))
        
    module KernelParsing =
        open MetadataExtraction
        open FunctionsManipulation
        open ReflectionUtil

        let PipelineMethods = 
            [ ExtractMethodInfo(<@ (|>) @>).Value.TryGetGenericMethodDefinition(); 
              ExtractMethodInfo(<@ (||>) @>).Value.TryGetGenericMethodDefinition();
              ExtractMethodInfo(<@ (|||>) @>).Value.TryGetGenericMethodDefinition() ]
              
        let ReplaceEnvRefsWithParamRefs(b:Expr) =
            let nameIndex = ref 0
            let rec analyzeInternal(e: Expr,  
                                    localState: HashSet<Var>, 
                                    envVars: List<Var * Var>,
                                    outVals: List<Expr * Var>) =
                match e with
                | Patterns.Let(v, va, b) ->
                    let va2 = analyzeInternal(va, localState, envVars, outVals)
                    localState.Add(v) |> ignore
                    let b2 = analyzeInternal(b, localState, envVars, outVals)
                    localState.Remove(v) |> ignore
                    Expr.Let(v, va2, b2)
                | Patterns.ForIntegerRangeLoop(v, st, en, b) ->                    
                    let st2 = analyzeInternal(st, localState, envVars, outVals)             
                    let en2 = analyzeInternal(en, localState, envVars, outVals)
                    localState.Add(v) |> ignore            
                    let b2 = analyzeInternal(b, localState, envVars, outVals)
                    localState.Remove(v) |> ignore
                    Expr.ForIntegerRangeLoop(v, st2, en2, b2)
                | Patterns.Value(o, t) when t.IsArray ->
                    let item = outVals |> Seq.tryFind(fun (e, v) -> e.Equals(o))
                    if item.IsNone then
                       // Create holder for this val
                       let newVar = Quotations.Var("outValParameter" + nameIndex.Value.ToString(), o.GetType())
                       nameIndex := !nameIndex + 1
                       outVals.Add(e, newVar)
                       Expr.Var(newVar)
                    else
                       Expr.Var(item.Value |> snd) 
                | Patterns.PropertyGet(o, pi, a) ->
                    // Check if this is a constant define
                    let attr = pi.GetCustomAttribute<ConstantDefineAttribute>()
                    let newExpr = 
                        if attr <> null then
                            e
                        else
                            let item = outVals |> Seq.tryFind(fun (v) -> e.Equals(o))
                            if item.IsNone && o.IsNone then                           
                               // Create holder for this val
                               let newVar = Quotations.Var("outValParameter" + nameIndex.Value.ToString(), pi.PropertyType)
                               nameIndex := !nameIndex + 1
                               outVals.Add(e, newVar)
                               Expr.Var(newVar)
                            else
                               Expr.Var(item.Value |> snd) 
                    newExpr
                | Patterns.FieldGet(o, fi) ->
                    // Check if this is a constant define
                    let attr = fi.GetCustomAttribute<ConstantDefineAttribute>()                    
                    let newExpr = 
                        if attr <> null then
                            e
                        else
                            let item = outVals |> Seq.tryFind(fun (v) -> e.Equals(o))
                            if item.IsNone && o.IsNone then                           
                               // Create holder for this val
                               let newVar = Quotations.Var("outValParameter" + nameIndex.Value.ToString(), fi.FieldType)
                               nameIndex := !nameIndex + 1
                               outVals.Add(e, newVar)
                               Expr.Var(newVar)
                            else
                               Expr.Var(item.Value |> snd) 
                    newExpr
                | ExprShape.ShapeVar(v) ->
                    if (localState.Contains(v) |> not) then
                        let item = envVars |> Seq.tryFind(fun (it) -> it.Equals(v))
                        if item.IsNone then                           
                            // Create holder for this val
                            let newVar = Quotations.Var("envVarParameter" + nameIndex.Value.ToString(), v.Type)
                            nameIndex := !nameIndex + 1
                            envVars.Add(v, newVar)
                            Expr.Var(newVar)
                        else
                            Expr.Var(item.Value |> snd) 
                    else
                        e
                | ExprShape.ShapeLambda(v, b) -> 
                    localState.Add(v) |> ignore
                    let b2 = analyzeInternal(b, localState, envVars, outVals)
                    localState.Remove(v) |> ignore
                    Expr.Lambda(v, b2)
                | ExprShape.ShapeCombination(o, a) -> 
                    ExprShape.RebuildShapeCombination(o, a |> List.map(fun e -> analyzeInternal(e, localState, envVars, outVals)))
            
            let envVars = new List<Var * Var>() 
            let outVals = new List<Expr * Var>()
            let local = new HashSet<Var>()
            let newExpr = analyzeInternal(b, local, envVars, outVals)
            newExpr, envVars, outVals
                    
        let ExtractEnvRefs(b:Expr) =
            let rec analyzeInternal(e: Expr,  
                                    localState: HashSet<Var>, 
                                    envVars: List<Var>,
                                    outVals: List<Expr>) =
                match e with
                | Patterns.Let(v, va, b) ->
                    analyzeInternal(va, localState, envVars, outVals)
                    localState.Add(v) |> ignore
                    analyzeInternal(b, localState, envVars, outVals)
                    localState.Remove(v) |> ignore
                | Patterns.ForIntegerRangeLoop(v, st, en, b) ->                    
                    analyzeInternal(st, localState, envVars, outVals)             
                    analyzeInternal(en, localState, envVars, outVals)
                    localState.Add(v) |> ignore            
                    analyzeInternal(b, localState, envVars, outVals)
                    localState.Remove(v) |> ignore
                | Patterns.Value(o, t) when t.IsArray ->
                    let item = outVals |> Seq.tryFind(fun e -> e.Equals(o))
                    if item.IsNone then
                       // Create holder for this val
                       outVals.Add(e)
                | Patterns.PropertyGet(o, pi, a) ->
                    // Check if this is a constant define
                    let attr = pi.GetCustomAttribute<ConstantDefineAttribute>()
                    if attr = null then
                        let item = outVals |> Seq.tryFind(fun (v) -> e.Equals(o))
                        if item.IsNone && o.IsNone then                           
                            // Create holder for this val
                            outVals.Add(e)
                | Patterns.FieldGet(o, fi) ->
                    // Check if this is a constant define
                    let attr = fi.GetCustomAttribute<ConstantDefineAttribute>()                    
                    if attr = null then
                        let item = outVals |> Seq.tryFind(fun (v) -> e.Equals(o))
                        if item.IsNone && o.IsNone then                           
                            // Create holder for this val
                            outVals.Add(e)
                | ExprShape.ShapeVar(v) ->
                    if (localState.Contains(v) |> not) then
                        let item = envVars |> Seq.tryFind(fun (it) -> it.Equals(v))
                        if item.IsNone then                           
                            // Create holder for this val
                            envVars.Add(v)
                | ExprShape.ShapeLambda(v, b) -> 
                    localState.Add(v) |> ignore
                    analyzeInternal(b, localState, envVars, outVals)
                    localState.Remove(v) |> ignore
                | ExprShape.ShapeCombination(o, a) -> 
                    a |> List.iter(fun e -> analyzeInternal(e, localState, envVars, outVals))
            
            let envVars = new List<Var>() 
            let outVals = new List<Expr>()
            let local = new HashSet<Var>()
            analyzeInternal(b, local, envVars, outVals)
            envVars, outVals
                    
        (*
        let LambdaToMethod(expr:Expr, checkIsKernel: bool) =
            let mutable parameters = []
        
            // Extract args from lambda
            let shouldProcess, returnType =
                match LiftCurriedOrTupledArgs(expr) with
                | Some(b, ps) ->
                    parameters <- ps |> List.filter(fun p -> not (typeof<WorkItemInfo>.IsAssignableFrom(p.Type)))
                    let shouldProcess = (not checkIsKernel) || (parameters.Length < ps.Length)
                    shouldProcess, b.Type
                | None ->
                    false, typeof<int>
            
            // If no lifting occurred this is not a lambda
            if shouldProcess then
                // Get MD5 o identify the lambda 
                let md5 = MD5.Create()
                let hash = md5.ComputeHash(Encoding.UTF8.GetBytes(expr.ToString()))
                let sb = new StringBuilder("lambda_")
                for i = 0 to hash.Length - 1 do
                    sb.Append(hash.[i].ToString("x2")) |> ignore

                let assemblyName = sb.ToString() + "_module";
                let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
                let moduleBuilder = assemblyBuilder.DefineDynamicModule(sb.ToString() + "_module");
                let methodBuilder = moduleBuilder.DefineGlobalMethod(
                                        sb.ToString(),
                                        MethodAttributes.Public ||| MethodAttributes.Static, returnType, 
                                        Array.ofList(List.map(fun (v: Var) -> v.Type) parameters))

                for p = 1 to parameters.Length do
                    methodBuilder.DefineParameter(p, ParameterAttributes.None, parameters.[p-1].Name) |> ignore
                // Body (simple return) of the method must be set to build the module and get the MethodInfo that we need as signature
                methodBuilder.GetILGenerator().Emit(OpCodes.Ret)
                moduleBuilder.CreateGlobalFunctions()
                let signature = moduleBuilder.GetMethod(methodBuilder.Name) 
                Some(signature, signature.GetParameters(), parameters)
            else
                None
                  *)  
        // Active patterns                         
        let (|KernelMethodInfo|_|) (mi: MethodInfo) =
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                if mi.GetCustomAttribute<KernelAttribute>() <> null then
                    Some(b)
                else
                    None
            | _ ->
                None 

        let (|UtilityFunctionMethodInfo|_|) (mi: MethodInfo) =
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                if mi.GetCustomAttribute<KernelAttribute>() = null then
                    Some(b)
                else
                    None
            | _ ->
                None
                   
        let (|CallToKernelMethodInfo|_|) (e: Expr) =
            match e with
            | Patterns.Call(o, mi, a) ->
                match mi with
                | KernelMethodInfo(b) ->
                    Some(o, mi, a, b)
                | _ ->
                    None
            | _ ->
                None
                
        let (|CallToUtilityFunctionMethodInfo|_|) (e: Expr) =
            match e with
            | Patterns.Call(o, mi, a) ->
                match mi with
                | UtilityFunctionMethodInfo(b) ->
                    Some(o, mi, a, b)
                | _ ->
                    None
            | _ ->
                None
                                
        let (|UtilityFunctionLambda|_|) (l: Expr) =         
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(l)
         
            // Check if this lambda encodes a kernel
            let shouldProcess, parameters, returnType =
                match LiftCurriedOrTupledArgs(l) with
                | Some(b, ps) ->
                    let filteredParams = ps |> List.filter(fun p -> typeof<WorkItemInfo>.IsAssignableFrom(p.Type) |> not)
                    true, ps, b.Type
                | None ->
                    false, [], typeof<int>

            if shouldProcess then
                let name = 
                    ("lambda_" + (parameters |> List.map(fun p -> p.Type.ToString()) |> String.concat "_") + "_" + returnType.ToString()).Replace(".","_").Replace("+","_")
                                            
                // We have everything to build the module, we must clean and set the args only   
                // Extract parameters vars                
                Some(name, l, parameters, returnType)                                   
            else
                None
                              
        let (|KernelLambdaApplication|_|) (e: Expr) =         
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)
         
            match LiftLambdaApplication(e) with
            | Some(l, a) ->
                //match LambdaToMethod(l, true) with
                //| Some(signature, paramInfo, parameters) ->
                
                    // Check if this lambda encodes a kernel
                    let shouldProcess, parameters, returnType =
                        match LiftCurriedOrTupledArgs(l) with
                        | Some(b, ps) ->
                            let filteredParams = ps |> List.filter(fun p -> typeof<WorkItemInfo>.IsAssignableFrom(p.Type) |> not)
                            let shouldProcess = filteredParams.Length < ps.Length
                            shouldProcess, ps, b.Type
                        | None ->
                            false, [], typeof<int>

                    if shouldProcess then
                        let name = 
                            "lambda_" + (parameters |> List.map(fun p -> p.Type.ToString()) |> String.concat "_") + "_" + returnType.ToString()
                        
                        // We have everything to build the module, we must clean and set the args only   
                        // Extract parameters vars                    
                        let arguments = new List<Expr>()
                        let mutable workItemInfoArg = None
                        for i = 0 to a.Length - 1 do
                            if not (typeof<WorkItemInfo>.IsAssignableFrom(a.[i].Type)) then
                                arguments.Add(a.[i])
                            else
                                workItemInfoArg <- Some(a.[i])

                        let attrs = new List<ParamMetaCollection>()
                        let args = new List<Expr>()
                        for i = 0 to arguments.Count - 1 do
                            let paramAttrs = new ParamMetaCollection()
                            let cleanArg, paramAttrs = ParseParameterMetadata(arguments.[i])
                            attrs.Add(paramAttrs)
                            args.Add(cleanArg)
                        Some(None, None, name, returnType, parameters, l, args |> List.ofSeq, workItemInfoArg, kernelAttrs, returnAttrs, attrs)                                   
                    else
                        None
            | _ ->
                None
                        
        // Check if an expression is an application of a lambda
        // and evaluates it properly setting the references to 
        // data declared outside the environment 
        let (|SequentialLambdaApplication|_|) (e: Expr) =
            match LiftLambdaApplication(e) with
            | Some(l, a) ->
                // Check free vars
                let env = e.GetFreeVars() |> List.ofSeq
                // To evaluate we must add environment
                let newL = ApplyLambdaEnvironment(env, l)
                // Now evaluate
                let lambdaValue = LeafExpressionConverter.EvaluateQuotation(newL)
                let invokeMethod = lambdaValue.GetType().GetMethod("Invoke")
                Some(invokeMethod, a, env)
            | _ ->
                None               
                                
        let (|KernelCall|_|) (e: Expr) =             
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)

            let callData =
                match expr with            
                | Patterns.Call (ob, mi, a) ->
                    // Immediate call
                    match mi with
                    | KernelMethodInfo(body) ->   
                        let thisVar = GetThisVariable(body)
                        Some(thisVar, ob, mi, a, body)
                    | _ ->
                        // Sequential call 
                        None
                | Patterns.Let(_, Patterns.NewTuple(l), b) ->
                    // This is tupled-args proparation to call a tupled function
                    // Check b contains a reflected definition method call                    
                    match ExtractCallWithReflectedDefinition(b) with
                    | Some(ob, mi, body, _, _, _) -> 
                        // Arguments must be extracted from the new tuple (already extracted, it's "l")
                        let thisVar = GetThisVariable(body) 
                        Some(thisVar, ob, mi, l, body)
                    | _ ->
                        None
                | Patterns.Application(Patterns.Let(clovar, 
                                                    Patterns.Lambda(clolv, clolval), 
                                                    Patterns.Lambda(clovv, clovval)), ar) ->
                    // Application of a closure
                    match ExtractCallWithReflectedDefinition(clolval) with
                    | Some(ob, mi, body, _, _, _) -> 
                        // Ok this is it
                        // Convert potential tupledArg into arg list
                        let a =
                            match ar with
                            | Patterns.NewTuple(elements) ->
                                elements
                            | _ ->
                                [ ar ]
                        //let outsideRefs = ExtractRefsToEnvironment(body, false)
                        let thisVar = GetThisVariable(body) 
                        Some(thisVar, ob, mi, a, body)
                    | _ ->
                        None                                                                       
                | Patterns.Application(item, arg) ->
                    // This may be the application of a lambda preparing the call to a reflected method info
                    // Lift application
                    match LiftLambdaApplication(expr) with
                    | Some(cont, ar) ->
                        // Check if l is a lambda with reflectedDefinition
                        match cont with
                        | Patterns.Lambda(l, b) ->
                            // Check if there is a call to a reflected method
                            match ExtractCallWithReflectedDefinition(cont) with
                            | Some(ob, mi, body, _, _, _) -> 
                                // Ok this is it
                                // Convert potential tupledArg into arg list
                                let a =
                                    if ar.Length > 0 then
                                        match ar.[0] with
                                        | Patterns.NewTuple(elements) ->
                                            elements
                                        | _ ->
                                            ar
                                    else
                                        ar
                                let thisVar = GetThisVariable(body)
                                //let outsideRefs = ExtractRefsToEnvironment(body, false)
                                Some(thisVar, ob, mi, a, body)
                            | _ ->
                                None
                        | _ ->
                            None
                    | _ ->
                        None
                | _ ->
                    None
              
            match callData with
            | None ->
                None
            | Some(obv, ob, mi, a, b) ->    
                // If b is None then sequentialFun
                // Extract parameters vars
                match GetCurriedOrTupledArgs(b) with
                | thisVar, Some(paramVars) ->                    
                    MergeWithStaticKernelMeta(kernelAttrs, returnAttrs, mi)
                    let origParams = mi.GetParameters()
                    let parameters = new List<ParameterInfo>()
                    let parameterVars = new List<Var>()
                    let arguments = new List<Expr>()
                    let mutable workItemInfoArg = None
                    for i = 0 to origParams.Length - 1 do
                        if typeof<WorkItemInfo> <> (origParams.[i].ParameterType) then
                            parameters.Add(origParams.[i])
                            parameterVars.Add(paramVars.[i])
                            arguments.Add(a.[i])
                        else
                            workItemInfoArg <- Some(a.[i])
                    let attrs = new List<ParamMetaCollection>()
                    let args = new List<Expr>()
                    for i = 0 to parameters.Count - 1 do
                        let paramAttrs = new ParamMetaCollection()
                        let cleanArg, paramAttrs = ParseParameterMetadata(arguments.[i])
                        MergeWithStaticParameterMeta(paramAttrs, parameters.[i])
                        attrs.Add(paramAttrs)
                        args.Add(cleanArg)
                    Some(obv, ob, mi, parameters |> Seq.toArray, parameterVars |> List.ofSeq, b, args |> List.ofSeq, workItemInfoArg, kernelAttrs, returnAttrs, attrs)
                | _ ->
                    None
                                
                                              
        let (|UtilityFunctionCall|_|) (expr: Expr) =             
            let callData =
                match expr with            
                | Patterns.Call (ob, mi, a) ->
                    // Immediate call
                    match mi with
                    | UtilityFunctionMethodInfo(body) ->  
                        //let outsiders = ExtractRefsToEnvironment(body, false) 
                        let thisVar = GetThisVariable(body)
                        Some(thisVar, ob, mi, a, body)
                    | _ ->
                        // Sequential call 
                        None
                | Patterns.Let(_, Patterns.NewTuple(l), b) ->
                    // This is tupled-args proparation to call a tupled function
                    // Check b contains a reflected definition method call                    
                    match ExtractCallWithReflectedDefinition(b) with
                    | Some(ob, mi, body, _, _, _) -> 
                        // Arguments must be extracted from the new tuple (already extracted, it's "l")
                        //let outsiders = ExtractRefsToEnvironment(body, false)
                        let thisVar = GetThisVariable(body) 
                        Some(thisVar, ob, mi, l, body)
                    | _ ->
                        None
                | _ ->
                    None
              
            match callData with
            | None ->
                None
            | Some(obv, ob, mi, a, b) ->    
                // If b is None then sequentialFun
                // Extract parameters vars
                match GetCurriedOrTupledArgs(b) with
                | thisVar, Some(paramVars) ->                    
                    //MergeWithStaticKernelMeta(kernelAttrs, returnAttrs, mi)
                    let origParams = mi.GetParameters()
                    let parameters = new List<ParameterInfo>()
                    let parameterVars = new List<Var>()
                    let arguments = new List<Expr>()
                    let mutable workItemInfoArg = None
                    for i = 0 to origParams.Length - 1 do
                        if typeof<WorkItemInfo> <> (origParams.[i].ParameterType) then
                            parameters.Add(origParams.[i])
                            parameterVars.Add(paramVars.[i])
                            arguments.Add(a.[i])
                        else
                            workItemInfoArg <- Some(a.[i])
                    Some(obv, ob, mi, parameters |> Seq.toArray, parameterVars |> List.ofSeq, b, arguments |> List.ofSeq, workItemInfoArg)
                | _ ->
                    None
                                  
                
        let rec CompositionToCallOrApplication(e: Expr) =    
            match e with
            | Patterns.Call(o, mi, a) ->
                if (PipelineMethods |> List.tryFind(fun i -> i = mi.TryGetGenericMethodDefinition())).IsSome then
                    let compositionArgs = a |> List.map(fun i -> CompositionToCallOrApplication(i))
                    match compositionArgs |> Seq.last with
                    | Patterns.Lambda(v, b) ->
                        // This may be a preparation to call a reflected method or a full lambda
                        match ExtractCall(compositionArgs |> Seq.last) with
                        | Some(o, mi, a, boundExpr, unboundVar) ->
                            // It's a call
                            // Here parsing may give wrong results, cannot do anything but hoping for good chance
                            (*
                                OLD ->
                                Problem is the following.
                                If we have 
                                    data |> myKernel
                                the tree of the right side is Lambda(a, Lambda(b ... Call(None, myKernel, ...))
                                In this case I want to get the call to myKernel

                                If we have
                                    data |> (fun i1 i2 ... -> doSomething)
                                the tree os the right size is still Lambda(a, Lambda(b ... Lambda(i1, Lambda(i2, Call(None, doSomething, ...)))
                                that is, Is still have a sequence of Lambda and a call at the end
                                In this case, anyway, I don't want to extract "doSomething", but Lambda(i1, Lambda(i2, ...))                           
                                <- OLD
                            *)
                            try 
                                if o.IsSome then
                                    Expr.Call(o.Value, mi, (a |> Seq.take(a.Length - unboundVar.Length) |> List.ofSeq) @ (compositionArgs |> Seq.take(unboundVar.Length) |> List.ofSeq))
                                else
                                    Expr.Call(mi, (a |> Seq.take(a.Length - unboundVar.Length) |> List.ofSeq) @ (compositionArgs |> Seq.take(unboundVar.Length) |> List.ofSeq))
                            with
                            | :? Exception ->
                                // Very likely gone too deep, try recognize this as lambda
                                let mutable res = compositionArgs |> Seq.last 
                                for i = 0 to compositionArgs.Length - 2 do
                                    res <- Expr.Application(res, compositionArgs.[i])
                                res
                        | _ ->
                            // A fully applied lambda
                            let mutable res = compositionArgs |> Seq.last 
                            for i = 0 to compositionArgs.Length - 2 do
                                res <- Expr.Application(res, compositionArgs.[i])
                            res

                    | Patterns.Application(l, a) ->
                        // A partially applied lambda
                        let mutable res = compositionArgs |> Seq.last 
                        for i = 0 to compositionArgs.Length - 2 do
                            res <- Expr.Application(res, compositionArgs.[i])
                        res
                    | Patterns.Let(v, va, b) ->
                        // This is generally the case of collection functions                    
                        match ExtractCall(compositionArgs |> Seq.last) with
                        | Some(o, mi, a, boundExpr, unboundVar) ->
                            // It's a call
                            if o.IsSome then
                                Expr.Call(o.Value, mi, (boundExpr |> Seq.take(a.Length - unboundVar.Length) |> List.ofSeq) @ (compositionArgs |> Seq.take(unboundVar.Length) |> List.ofSeq))
                            else
                                Expr.Call(mi, (boundExpr |> Seq.take(a.Length - unboundVar.Length) |> List.ofSeq) @ (compositionArgs |> Seq.take(unboundVar.Length) |> List.ofSeq))
                        | _ ->
                            // A fully applied lambda
                            let mutable res = compositionArgs |> Seq.last 
                            for i = 0 to compositionArgs.Length - 2 do
                                res <- Expr.Application(res, compositionArgs.[i])
                            res            
                    | _ ->
                        failwith "error"
                else
                    let compositionArgs = a |> List.map(fun i -> CompositionToCallOrApplication(i))
                    if o.IsSome then
                        Expr.Call(o.Value, mi, compositionArgs)
                    else
                        Expr.Call(mi, compositionArgs)
                        
            | ExprShape.ShapeVar(v) ->
                e
            | ExprShape.ShapeLambda(l, b) ->
                Expr.Lambda(l, CompositionToCallOrApplication(b))
            | ExprShape.ShapeCombination(o, a) ->
                ExprShape.RebuildShapeCombination(o, a |> List.map(fun i -> CompositionToCallOrApplication(i)))
              
        let rec GetKernelFromMethodInfo(mi: MethodInfo) =                    
            match mi with
            | KernelMethodInfo(body) ->  
                // Fix: check if Lambda(this, body) for instance methods
                // Extract parameters vars
                match GetCurriedOrTupledArgs(body) with
                | thisVar, Some(paramVars) ->                    
                    let kernelMeta = new KernelMetaCollection()
                    let returnMeta = new ParamMetaCollection()
                    MergeWithStaticKernelMeta(kernelMeta, returnMeta, mi)
                
                    let methodParams = mi.GetParameters()
                    let parameters = methodParams |> List.ofArray |> List.filter(fun p -> not (typeof<WorkItemInfo>.IsAssignableFrom(p.ParameterType)))
                    let pVars = paramVars |> List.filter(fun p -> not (typeof<WorkItemInfo>.IsAssignableFrom(p.Type)))
                    let attrs = new List<ParamMetaCollection>()
                    for p in parameters do
                        let paramAttrs = new ParamMetaCollection()
                        MergeWithStaticParameterMeta(paramAttrs, p)
                        attrs.Add(paramAttrs)
                    Some(thisVar, None, mi, parameters, pVars, body, kernelMeta, returnMeta, attrs)
                | _ ->
                    None
            | _ ->
                None
                
        (*
        let DiscoverUtilityFunctionsAndEnvRefs(body) =
            let rec DiscoverInner(e:Expr, 
                                  localState: HashSet<Var>, 
                                  envVars: List<Var>,
                                  outVals: List<Expr>,
                                  constDef: Dictionary<string, Var option * Expr option * obj>,
                                  utilityFunctions: Dictionary<FunctionInfoID, IFunctionInfo>) =
                match e with       
                | UtilityFunctionCall (obv, ob, mi, parameters, paramVars, body, args, workItemInfo) ->
                    if not (utilityFunctions.ContainsKey(FunctionInfoID.MethodID(mi))) then                        
                        let subFoundFunctions = Dictionary<FunctionInfoID, IFunctionInfo>()
                        let subEnvVars = new List<Var>() 
                        let subOutVals = new List<Expr>()
                        let subLocal = new HashSet<Var>()
                        let subConstDef = new Dictionary<string, Var option * Expr option * obj>()
                        DiscoverInner(body, subLocal, subEnvVars, subOutVals, subConstDef, subFoundFunctions)

                        let fi = new FunctionInfo(mi, 
                                                  parameters |> List.ofArray,
                                                  paramVars,
                                                  subEnvVars,
                                                  subOutVals,
                                                  workItemInfo,
                                                  body,
                                                  new List<FunctionInfoID>(subFoundFunctions.Keys), false)
                        utilityFunctions.Add(fi.ID, fi)
                        // Add all sub utilities found
                        for item in subFoundFunctions do
                            if not (utilityFunctions.ContainsKey(item.Key)) then
                                utilityFunctions.Add(item.Key, item.Value)
                        // Add all sub refs
                        for item in subEnvVars do
                            if not(envVars.Contains(item)) then
                                envVars.Add(item)
                        for item in subOutVals do
                            let it = outVals |> Seq.tryFind(fun (v) -> e.Equals(item))
                            if it.IsNone then
                                outVals.Add(item)
                        // Add sub const def
                        for item in subConstDef do
                            if not (constDef.ContainsKey(item.Key)) then
                                constDef.Add(item.Key, item.Value)
                | _ ->
                    ()

                match e with
                | Patterns.Let(v, va, b) ->
                    DiscoverInner(va, localState, envVars, outVals, constDef, utilityFunctions)
                    localState.Add(v) |> ignore
                    DiscoverInner(b, localState, envVars, outVals, constDef, utilityFunctions)
                    localState.Remove(v) |> ignore
                | Patterns.Lambda(v, b) ->
                    localState.Add(v) |> ignore
                    DiscoverInner(b, localState, envVars, outVals, constDef, utilityFunctions)
                    localState.Remove(v) |> ignore
                | Patterns.ForIntegerRangeLoop(v, st, en, b) ->                    
                    DiscoverInner(st, localState, envVars, outVals, constDef, utilityFunctions)             
                    DiscoverInner(en, localState, envVars, outVals, constDef, utilityFunctions)
                    localState.Add(v) |> ignore            
                    DiscoverInner(b, localState, envVars, outVals, constDef, utilityFunctions)
                    localState.Remove(v) |> ignore
                | Patterns.Value(o, t) when t.IsArray ->
                    let item = outVals |> Seq.tryFind(fun (v) -> e.Equals(o))
                    if item.IsNone && (typeof<WorkItemInfo>.IsAssignableFrom(t) |> not) then
                       outVals.Add(e)
                | Patterns.PropertyGet(o, pi, a) ->
                    // Check if this is a constant define
                    let attr = pi.GetCustomAttribute<ConstantDefineAttribute>()
                    if attr = null then
                        let item = outVals |> Seq.tryFind(fun (v) -> e.Equals(o))
                        if item.IsNone && o.IsNone && (typeof<WorkItemInfo>.IsAssignableFrom(pi.PropertyType) |> not) then
                           outVals.Add(e)
                        a |> List.iter(fun e -> DiscoverInner(e, localState, envVars, outVals, constDef, utilityFunctions))
                    else
                        // Constant define
                        if not (constDef.ContainsKey(pi.Name) || (typeof<WorkItemInfo>.IsAssignableFrom(pi.PropertyType))) then
                            if o.IsSome then
                                let placeholder = Quotations.Var("ph", o.Value.Type)
                                constDef.Add(pi.Name, (Some(placeholder), o,
                                                    LeafExpressionConverter.EvaluateQuotation(
                                                        Expr.Lambda(placeholder, Expr.PropertyGet(Expr.Var(placeholder), pi)))))
                            else
                                let placeholder = Quotations.Var("ph", typeof<unit>)
                                constDef.Add(pi.Name, (None, o,
                                                    LeafExpressionConverter.EvaluateQuotation(
                                                        Expr.Lambda(placeholder, Expr.PropertyGet(pi)))))
                | Patterns.FieldGet(o, fi) ->
                    // Check if this is a constant define
                    let attr = fi.GetCustomAttribute<ConstantDefineAttribute>()
                    if attr = null then
                        let item = outVals |> Seq.tryFind(fun (v) -> e.Equals(o))
                        if item.IsNone && o.IsNone && (typeof<WorkItemInfo>.IsAssignableFrom(fi.FieldType) |> not) then
                           outVals.Add(e)
                    else
                        // Constant define
                        if not (constDef.ContainsKey(fi.Name) || (typeof<WorkItemInfo>.IsAssignableFrom(fi.FieldType))) then
                            if o.IsSome then
                                let placeholder = Quotations.Var("ph", o.Value.Type)
                                constDef.Add(fi.Name, (Some(placeholder), o,
                                                    LeafExpressionConverter.EvaluateQuotation(
                                                        Expr.Lambda(placeholder, Expr.FieldGet(Expr.Var(placeholder), fi)))))
                            else
                                let placeholder = Quotations.Var("ph", typeof<unit>)
                                constDef.Add(fi.Name, (None, o,
                                                    LeafExpressionConverter.EvaluateQuotation(
                                                        Expr.Lambda(placeholder, Expr.FieldGet(fi)))))
                | ExprShape.ShapeVar(v) ->
                    if (localState.Contains(v) |> not) && (typeof<WorkItemInfo>.IsAssignableFrom(v.Type) |> not) && (envVars.Contains(v) |> not) then
                        envVars.Add(v) 
                | ExprShape.ShapeLambda(v, b) -> 
                    localState.Add(v) |> ignore
                    DiscoverInner(b, localState, envVars, outVals, constDef, utilityFunctions)
                    localState.Remove(v) |> ignore
                | ExprShape.ShapeCombination(o, a) -> 
                    a |> List.iter(fun e -> DiscoverInner(e, localState, envVars, outVals, constDef, utilityFunctions))
            
            let foundFunctions = Dictionary<FunctionInfoID, IFunctionInfo>()
            let envVars = new List<Var>() 
            let outVals = new List<Expr>()
            let local = new HashSet<Var>()
            let constDef = new Dictionary<string, Var option * Expr option * obj>()
            DiscoverInner(body, local, envVars, outVals, constDef, foundFunctions)
            (envVars, outVals, constDef, foundFunctions)
            *)
                
                



            
        


    
        

