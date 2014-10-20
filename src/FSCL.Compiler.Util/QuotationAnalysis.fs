namespace FSCL.Compiler.Util

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
                        if attrFunction.Target = MetadataFunctionTarget.KernelFunction then
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
            | Patterns.Lambda(v, e) ->
                if v.Name = "tupledArg" then
                    Some(GetCallArgs(e, []))
                else
                    None
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
                Some(LiftLambdaApplicationRec(expr, []))
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
                if v.Name = "tupledArg" then
                    Some(GetCallArgs(e, []))
                else
                    None
            | _ ->
                None
            
        // Extract and lift function parameters in curried form
        let GetCurriedArgs(expr) =
            let rec GetCallArgs(expr, parameters: Var list) =
                match expr with
                | Patterns.Lambda(v, body) ->
                    GetCallArgs(body, parameters @ [ v ])
                | _ ->
                    (parameters)
                
            match expr with
            | Patterns.Lambda(v, e) ->
                Some(GetCallArgs(e, [v]))
            | _ ->
                None

        let GetCurriedOrTupledArgs(expr) =
            match GetTupledArgs(expr) with
            | Some(p) ->
                Some(p)
            | None ->
                match GetCurriedArgs(expr) with
                | Some(p) ->
                    Some(p)
                | _ ->
                    None

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
                    | DerivedPatterns.MethodWithReflectedDefinition(_) ->   
                        Some(e, mi, a, boundVarExprs, unboundVars)
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

        let IsComputationalLambda(expr:Expr) =    
            let rec isCallToReflectedMethod(expr) =
                match expr with
                | Patterns.Lambda(v, e) -> 
                    isCallToReflectedMethod(e)
                | Patterns.Call (e, mi, a) ->
                    match mi with
                    | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                        true
                    | _ ->
                        false
                | _ ->
                    false
           
            match expr with 
            | Patterns.Lambda(v, e) -> 
                not (isCallToReflectedMethod(e))
            | _ ->
                false  
            
        let GetComputationalLambdaOrReflectedMethodInfo(expr:Expr) =    
            let rec getCallToReflectedMethod(expr, lambdaParams: Var list) =
                match expr with
                | Patterns.Lambda(v, e) -> 
                    getCallToReflectedMethod(e, lambdaParams @ [ v ])
                | Patterns.Call (e, mi, a) ->
                    match mi with
                    | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                        Some(e, mi, a, b, lambdaParams)
                    | _ ->
                        None
                | _ ->
                    None
           
            match expr with 
            | Patterns.Lambda(v, e) -> 
                match getCallToReflectedMethod(expr, []) with
                | Some(e, mi, a, b, lambdaParams) ->
                    Some(e, mi, a, b, lambdaParams), None
                | _ ->
                    None, Some(expr)
            | _ ->
                None, None  
        
        let RemoveLambdaArgsBindingForVar(e: Expr, v: Var) =
            let rec removeInternal(e: Expr, v: Var) =
                match e with
                | ExprShape.ShapeLambda(thisv, b) ->                
                    if thisv = v then
                        b
                    else
                        Expr.Lambda(thisv, removeInternal(b, v))
                | _ ->
                    e
            removeInternal(e, v)
                    
        let LiftNonLambdaParamsFromMethodCalledInLambda(mi: MethodInfo, args: Expr list, body: Expr, lambdaParams: Var list) =            
            let IsExpressionContainingRefToOneOfVars(e: Expr, varList: Var list) =
                let rec findInternal(e: Expr, vs: Var list) =
                    match e with
                    | ExprShape.ShapeVar(thisv) ->
                        let it = List.tryFind(fun el -> el = thisv) vs
                        it.IsSome
                    | ExprShape.ShapeLambda(l, b) ->
                        findInternal(b, vs)
                    | ExprShape.ShapeCombination(it, args) ->
                        if args.Length > 0 then
                            args |> List.map(fun it -> findInternal(it, vs)) |> List.reduce(fun a b -> a || b)
                        else
                            false
                findInternal(e, varList)
            // Lambda containing method to apply: fun it -> DoSomething x y z it
            // We must restruct DoSomething replacing params that are references to stuff outside quotation (!= it)
            // More precisely, if some paramters of mi are not contained in lambdaParams
            // this means that the lambda is something like fun a b c -> myMethod a b c othPar
            // So othPar must be evaluated, replacing it's current value inside myMethod
            // Otherwise, the kernel would not be able to invoke it (it doesn't manage othPar)
            // Example: Array.map (fun item -> DoSomething item otherParam). We replace otherParam with its value
            // and we create a signature without otherParam
        
            // At first, extract arg binding vars from method body
            let miParamVars, miReturnType =
                match LiftCurriedOrTupledArgs(body) with 
                | Some(b, p) ->
                    p, b.Type
                | None ->
                    raise (new CompilerException("Cannot find parameters vars binding inside method body\n" + body.ToString()))
        
            // Now let's see which argument is not a reference to a lambda var and which is not
            let removedParamVars = new List<Var>()
            let keptParamVars = new List<Var>(miParamVars)
            let keptParamIndx = new List<int>(seq { for i = 0 to miParamVars.Length - 1 do yield i })
            let mutable finalBody = body     
            for argIndex = 0 to args.Length - 1 do
                let methodArg = args.[argIndex]
                match methodArg with
                | Patterns.Var(v) ->
                    let isLambdaParam = List.tryFind(fun (lp: Var) -> lp = v) lambdaParams
                    if isLambdaParam.IsNone then
                        // Evaluate 
                        let itemValue = LeafExpressionConverter.EvaluateQuotation(methodArg)
                        removedParamVars.Add(miParamVars.[argIndex])
                        keptParamVars.Remove(miParamVars.[argIndex]) |> ignore
                        keptParamIndx.Remove(argIndex) |> ignore
                        finalBody <- finalBody.Substitute(fun bv ->
                                                            if bv = miParamVars.[argIndex] then
                                                                Some(Expr.Value(itemValue, methodArg.Type))
                                                            else
                                                                None)
                | _ ->
                    // In case one argument that is not a lambda param two situations may happen
                    // 1) It's a value or expression with values and non-lambda vars like: 0.0f * myGlobalVar + ...
                    // We can evaluate
                    // 2) It's an expression with values and lambda vars like: Array.map(fun p -> DoSomething (p + 1))
                    // we can't evaluate, let's throw exception
                    let cantEval = IsExpressionContainingRefToOneOfVars(methodArg, lambdaParams)
                    if cantEval then
                        raise (new CompilerException("Passing an expression containing references to lambda parameters to a method called inside a lambda in not supported"))
                    else
                        let itemValue = LeafExpressionConverter.EvaluateQuotation(methodArg)
                        removedParamVars.Add(miParamVars.[argIndex])
                        keptParamVars.Remove(miParamVars.[argIndex]) |> ignore
                        keptParamIndx.Remove(argIndex) |> ignore
                        finalBody <- finalBody.Substitute(fun bv ->
                                                            if bv = miParamVars.[argIndex] then
                                                                Some(Expr.Value(itemValue, methodArg.Type))
                                                            else
                                                                None)
            // Now rebuild method args binding removing the ones substituted with values
            for v in removedParamVars do
                finalBody <- RemoveLambdaArgsBindingForVar(finalBody, v)

            // Finally, create a new signature
            (*
            let signature = DynamicMethod(mi.Name, mi.ReturnType, keptParamVars |> Seq.map(fun (v:Var) -> v.Type) |> Seq.toArray)
            let mutable paramIndex = 1
            for idx in keptParamIndx do
                signature.DefineParameter(paramIndex, ParameterAttributes.In, miParamVars.[paramIndex - 1].Name) |> ignore
                paramIndex <- paramIndex + 1
            *)
            // Real method in assembly (otherwise when matching an Expr containing a call to the method .NET throws an exception    
            let assemblyName = mi.Name + "_module";
            let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
            let moduleBuilder = assemblyBuilder.DefineDynamicModule(mi.Name + "_module");
            let methodBuilder = moduleBuilder.DefineGlobalMethod(
                                    mi.Name,
                                    MethodAttributes.Public ||| MethodAttributes.Static, mi.ReturnType, 
                                    Array.ofSeq(Seq.map(fun (v: Var) -> v.Type) keptParamVars))
            for p = 0 to keptParamIndx.Count - 1 do
                methodBuilder.DefineParameter(p + 1, ParameterAttributes.In, miParamVars.[keptParamIndx.[p]].Name) |> ignore
            // Body (simple return) of the method must be set to build the module and get the MethodInfo that we need as signature
            methodBuilder.GetILGenerator().Emit(OpCodes.Ret)
            moduleBuilder.CreateGlobalFunctions()
            let signature = moduleBuilder.GetMethod(methodBuilder.Name) 

            (signature, keptParamVars |> Seq.toList, finalBody)

    module KernelParsing =
        open MetadataExtraction
        open FunctionsManipulation
        open ReflectionUtil
        
        let PipelineMethods = 
            [ ExtractMethodInfo(<@ (|>) @>).Value.TryGetGenericMethodDefinition(); 
              ExtractMethodInfo(<@ (||>) @>).Value.TryGetGenericMethodDefinition();
              ExtractMethodInfo(<@ (|||>) @>).Value.TryGetGenericMethodDefinition() ]

        let LambdaToMethod(e, isKernel: bool, kernelOnlyIfWorkItemInfoParam: bool) = 
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)

            let body = expr
            let mutable parameters = []
        
            // Extract args from lambda
            let shouldProcess, returnType =
                match LiftCurriedOrTupledArgs(expr) with
                | Some(b, ps) ->
                    parameters <- ps |> List.filter(fun p -> not (typeof<WorkItemInfo>.IsAssignableFrom(p.Type)))
                    let shouldProcess = not isKernel || not kernelOnlyIfWorkItemInfoParam || (parameters.Length < ps.Length)
                    shouldProcess, b.Type
                | None ->
                    false, typeof<int>

            // If no lifting occurred this is not a lambda
            if shouldProcess && IsComputationalLambda(expr) then
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
                Some(signature, signature.GetParameters() |> Array.toList, parameters, body, kernelAttrs, returnAttrs, ReadOnlyMetaCollection.EmptyParamMetaCollection(parameters.Length))
            else
                // No lifting occurred or this should not made parallel
                None

        // Parsing utilities      
        let rec GetKernelFromName(e) =     
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)
            match expr with
            | Patterns.Lambda(v, e) -> 
                GetKernelFromName (e)
            | Patterns.Let (v, e1, e2) ->
                GetKernelFromName (e2)
            | Patterns.Call (e, mi, a) ->
                match mi with
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->    
                    // Fix: check if Lambda(this, body) for instance methods
                    let b = match body with
                            | Patterns.Lambda(v, b) ->
                                if (v.Name = "this") then
                                    b
                                else
                                    body
                            | _ ->
                                body
                    // Extract parameters vars
                    match GetCurriedOrTupledArgs(b) with
                    | Some(paramVars) ->       
                        MergeWithStaticKernelMeta(kernelAttrs, returnAttrs, mi)
                        // Take parameters except the last one (workItemInfo)
                        let origParams = mi.GetParameters()
                        let parameters = new List<ParameterInfo>()
                        let parameterVars = new List<Var>()
                        let mutable workItemInfoArg = None
                        for i = 0 to origParams.Length - 1 do
                            if not (typeof<WorkItemInfo>.IsAssignableFrom(origParams.[i].ParameterType)) then
                                parameters.Add(origParams.[i])
                                parameterVars.Add(paramVars.[i])
                        let attrs = new List<ParamMetaCollection>()
                        for p in parameters do
                            let paramAttrs = new ParamMetaCollection()
                            MergeWithStaticParameterMeta(paramAttrs, p)
                            attrs.Add(paramAttrs)
                        Some(mi, parameters |> List.ofSeq, parameterVars |> List.ofSeq, b, kernelAttrs, returnAttrs, attrs)
                    | _ ->
                        None
                | _ ->
                    None
            | _ ->
                None
                
        let rec GetKernelFromCall(e) =                
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)

            match expr with            
            | Patterns.Call (e, mi, a) ->
                match mi with
                | DerivedPatterns.MethodWithReflectedDefinition(body) -> 
                    // Fix: check if Lambda(this, body) for instance methods
                    let b = match body with
                            | Patterns.Lambda(v, b) ->
                                if (v.Name = "this") then
                                    b
                                else
                                    body
                            | _ ->
                                body               
                    // Extract parameters vars
                    match GetCurriedOrTupledArgs(b) with
                    | Some(paramVars) ->                    
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
                        Some(mi, parameters |> List.ofSeq, parameterVars |> List.ofSeq, b, args |> List.ofSeq, workItemInfoArg, kernelAttrs, returnAttrs, attrs)
                    | _ ->
                        None
                | _ ->
                    None
            | _ ->
                None
            
        let rec GetKernelFromApplication(e) =                
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)
         
            match LiftLambdaApplication(e) with
            | Some(l, a) ->
                // Convert to method
                match LambdaToMethod(l, true, true) with
                | Some(mi, paramInfo, paramVars, b, kMeta, rMeta, pMeta) -> 
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
                        MergeWithStaticParameterMeta(paramAttrs, paramInfo.[i])
                        attrs.Add(paramAttrs)
                        args.Add(cleanArg)
                    Some(mi, paramInfo, paramVars, b, args |> List.ofSeq, workItemInfoArg, kernelAttrs, returnAttrs, attrs)                    
                | _ ->
                    None
            | _ ->
                None

        let rec GetKernelFromComposition(ex: Expr) =    
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(ex)
            
            let valid, (methodInfo, lambda), compositionArgs = 
                match expr with
                | DerivedPatterns.SpecificCall <@ (|>) @> (e, tl, compArgs) ->
                    true, GetComputationalLambdaOrReflectedMethodInfo(compArgs.[1]), compArgs
                | DerivedPatterns.SpecificCall <@ (||>) @> (e, tl, compArgs) ->
                    true, GetComputationalLambdaOrReflectedMethodInfo(compArgs.[2]), compArgs
                | DerivedPatterns.SpecificCall <@ (|||>) @> (e, tl, compArgs) ->
                    true, GetComputationalLambdaOrReflectedMethodInfo(compArgs.[3]), compArgs
                | _ ->
                    false, (None, None), []
            
            if valid then 
                if methodInfo.IsSome then
                    match methodInfo.Value with
                    | o, mi, partialArgs, body, lambdaVars ->                        
                        let b = match body with
                                | Patterns.Lambda(v, b) ->
                                    if (v.Name = "this") then
                                        b
                                    else
                                        body
                                | _ ->
                                    body    
                        // In COMP1 |> COMP2 a1 .. aN the args of COMP2 are a1, .. aN, COMP1 (+1 at the end)
                        let methodInfoArgs = new List<Expr>(partialArgs)
                        methodInfoArgs.RemoveRange(methodInfoArgs.Count - lambdaVars.Length, lambdaVars.Length)
                        methodInfoArgs.AddRange(compositionArgs)
                        methodInfoArgs.RemoveAt(methodInfoArgs.Count - 1)

                        // Extract parameters vars
                        match GetCurriedOrTupledArgs(b) with
                        | Some(paramVars) ->                 
                            MergeWithStaticKernelMeta(kernelAttrs, returnAttrs, mi)
                            let origParams = mi.GetParameters()
                            let parameters = new List<ParameterInfo>()
                            let parameterVars = new List<Var>()
                            let arguments = new List<Expr>()
                            let mutable workItemInfoArg = None
                            for i = 0 to origParams.Length - 1 do
                                if not (typeof<WorkItemInfo>.IsAssignableFrom(origParams.[i].ParameterType)) then
                                    parameters.Add(origParams.[i])
                                    parameterVars.Add(paramVars.[i])
                                    arguments.Add(methodInfoArgs.[i])
                                else
                                    workItemInfoArg <- Some(methodInfoArgs.[i])
                            let attrs = new List<ParamMetaCollection>()
                            let args = new List<Expr>()
                            for i = 0 to parameters.Count - 1 do
                                let paramAttrs = new ParamMetaCollection()
                                let cleanArg, paramAttrs = ParseParameterMetadata(arguments.[i])
                                MergeWithStaticParameterMeta(paramAttrs, parameters.[i])
                                attrs.Add(paramAttrs)
                                args.Add(cleanArg)
                            Some(mi, parameters |> List.ofSeq, parameterVars |> List.ofSeq, b, args |> List.ofSeq, workItemInfoArg, kernelAttrs, returnAttrs, attrs)                      
                        | _ ->
                            None
                else if lambda.IsSome then
                    // The right hand side of composition is a lambda                
                    match LambdaToMethod(lambda.Value, true, true) with
                    | Some(mi, paramInfo, paramVars, b, kMeta, rMeta, pMeta) -> 
                        // We have everything to build the module, we must clean and set the args only   
                        // Extract parameters vars                    
                        let arguments = new List<Expr>()
                        let mutable workItemInfoArg = None
                        for i = 0 to compositionArgs.Length - 2 do
                            if not (typeof<WorkItemInfo>.IsAssignableFrom(compositionArgs.[i].Type)) then
                                arguments.Add(compositionArgs.[i])
                            else
                                workItemInfoArg <- Some(compositionArgs.[i])

                        let attrs = new List<ParamMetaCollection>()
                        let args = new List<Expr>()
                        for i = 0 to arguments.Count - 1 do
                            let paramAttrs = new ParamMetaCollection()
                            let cleanArg, paramAttrs = ParseParameterMetadata(arguments.[i])
                            MergeWithStaticParameterMeta(paramAttrs, paramInfo.[i])
                            attrs.Add(paramAttrs)
                            args.Add(cleanArg)
                        Some(mi, paramInfo, paramVars, b, args |> List.ofSeq, workItemInfoArg, kernelAttrs, returnAttrs, attrs)                    
                    | _ ->
                        None
                else
                    // Right side is a lambda partially applied
                    let lift = LiftLambdaApplication(compositionArgs |> Seq.last)
                    match lift with
                    | Some(l, partialArgs) ->
                        // The right hand side of composition is a lambda                
                        match LambdaToMethod(l, true, true) with
                        | Some(mi, paramInfo, paramVars, b, kMeta, rMeta, pMeta) -> 
                            // We have everything to build the module, we must clean and set the args only   
                            // Extract parameters vars      
                            let mergedArgs = partialArgs @ compositionArgs              
                            let arguments = new List<Expr>()
                            let mutable workItemInfoArg = None
                            for i = 0 to mergedArgs.Length - 2 do
                                if not (typeof<WorkItemInfo>.IsAssignableFrom(mergedArgs.[i].Type)) then
                                    arguments.Add(mergedArgs.[i])
                                else
                                    workItemInfoArg <- Some(mergedArgs.[i])

                            let attrs = new List<ParamMetaCollection>()
                            let args = new List<Expr>()
                            for i = 0 to arguments.Count - 1 do
                                let paramAttrs = new ParamMetaCollection()
                                let cleanArg, paramAttrs = ParseParameterMetadata(arguments.[i])
                                MergeWithStaticParameterMeta(paramAttrs, paramInfo.[i])
                                attrs.Add(paramAttrs)
                                args.Add(cleanArg)
                            Some(mi, paramInfo, paramVars, b, args |> List.ofSeq, workItemInfoArg, kernelAttrs, returnAttrs, attrs)                    
                        | _ ->
                            None
                    | _ ->
                        None
            else
                None

        let rec GetKernelFromMethodInfo(mi: MethodInfo) =                    
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(body) ->  
                // Fix: check if Lambda(this, body) for instance methods
                let b = match body with
                        | Patterns.Lambda(v, b) ->
                            if (v.Name = "this") then
                                b
                            else
                                body
                        | _ ->
                            body   
                // Extract parameters vars
                match GetCurriedOrTupledArgs(b) with
                | Some(paramVars) ->                    
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
                    Some(mi, parameters, pVars, b, kernelMeta, returnMeta, attrs)
                | _ ->
                    None
            | _ ->
                None
                
        let rec CompositionToCallOrApplication(ex: Expr, specificCalls: ((Expr option * MethodInfo * Expr list) -> bool) option) =    
            (*let valid, compositionArgs = 
                match ex with
                | DerivedPatterns.SpecificCall <@ (|>) @> (e, tl, compArgs) 
                | DerivedPatterns.SpecificCall <@ (||>) @> (e, tl, compArgs) 
                | DerivedPatterns.SpecificCall <@ (|||>) @> (e, tl, compArgs) ->
                    true, compArgs
                | _ ->
                    false, []*)
            let valid, compositionArgs = 
                match ex with
                | Patterns.Call(o, mi, a) ->
                    if (PipelineMethods |> List.tryFind(fun i -> i = mi.TryGetGenericMethodDefinition())).IsSome then
                        true, a
                    else
                        false, []
                | _ ->
                    false, []

            if valid then 
                match compositionArgs |> Seq.last with
                | Patterns.Lambda(v, b) ->
                    // This may be a preparation to call a reflected method or a full lambda
                    match ExtractCallWithReflectedDefinition(compositionArgs |> Seq.last) with
                    | Some(o, mi, a, boundExpr, unboundVar) ->
                        // It's a call
                        // Here parsing may give wrong results, cannot do anything but hoping for good chance
                        (*
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
                        *)
                        if o.IsSome then
                            Expr.Call(o.Value, mi, (a |> Seq.take(a.Length - unboundVar.Length) |> List.ofSeq) @ (compositionArgs |> Seq.take(unboundVar.Length) |> List.ofSeq))
                        else
                            Expr.Call(mi, (a |> Seq.take(a.Length - unboundVar.Length) |> List.ofSeq) @ (compositionArgs |> Seq.take(unboundVar.Length) |> List.ofSeq))
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
                | Let(v, va, b) ->
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
                    ex
            else
                ex

            
        


    
        

