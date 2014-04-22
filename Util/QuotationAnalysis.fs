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
open FSCL.Compiler.Language
open FSCL.Compiler
open System.Collections.ObjectModel

module QuotationAnalysis =
    let private ParseKernelMetadataFunctions(expr, 
                                             kernelAttrs: KernelMetaCollection,
                                             returnAttrs: ParamMetaCollection) =
        let rec ParseMetadataInternal(expr) =
            match expr with
            | Patterns.Call(o, mi, args) ->
                
                let kernelAttrFunction = mi.GetCustomAttribute<KernelMetadataFunctionAttribute>()
                let returnAttrFunction = mi.GetCustomAttribute<ReturnMetadataFunctionAttribute>()
                let paramAttrFunction = mi.GetCustomAttribute<ParameterMetadataFunctionAttribute>()

                if kernelAttrFunction <> null || returnAttrFunction <> null then
                    // Get attribute type
                    let attrType = 
                        if kernelAttrFunction <> null  then
                            kernelAttrFunction.Metadata
                        else
                            returnAttrFunction.Metadata
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
                        if (kernelAttrFunction <> null) then
                            let attr = constr.Invoke(attrArgs) :?> KernelMetadataAttribute
                            kernelAttrs.AddOrSet(attr)
                        else
                            // Return attr function
                            let attr = constr.Invoke(attrArgs) :?> ParameterMetadataAttribute
                            returnAttrs.AddOrSet(attr)
                        // Continue processing body
                        ParseMetadataInternal(args.[args.Length - 1])
                else if paramAttrFunction <> null then
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
                
    let private ParseParameterMetadataFunctions(expr, 
                                                attrs: ParamMetaCollection) =
        let rec ParseMetadataInternal(expr) =
            match expr with
            | Patterns.Call(o, mi, args) ->
                let paramAttrFunction = mi.GetCustomAttribute<ParameterMetadataFunctionAttribute>()
                let kernelAttrFunction = mi.GetCustomAttribute<KernelMetadataFunctionAttribute>()
                let returnAttrFunction = mi.GetCustomAttribute<ReturnMetadataFunctionAttribute>()

                if paramAttrFunction <> null then
                    // Get attribute type
                    let attrType = paramAttrFunction.Metadata
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
                else if kernelAttrFunction <> null || returnAttrFunction <> null then
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
            if typeof<ParameterMetadataAttribute>.IsAssignableFrom(attr.GetType()) then
                if not (returnAttrs.Contains(attr.GetType())) then
                    returnAttrs.Add(attr :?> ParameterMetadataAttribute)
            else if typeof<KernelMetadataAttribute>.IsAssignableFrom(attr.GetType()) then
                if not (attrs.Contains(attr.GetType())) then
                    attrs.Add(attr :?> KernelMetadataAttribute)
        
    let ParseParameterMetadata(expr) =
        // Process dynamic attributes
        let attrs = new ParamMetaCollection()
        let cleanExpr = ParseParameterMetadataFunctions(expr, attrs)

        // Return
        (cleanExpr, attrs)
        
    let MergeWithStaticParameterMeta(attrs: ParamMetaCollection, 
                                     p: ParameterInfo) =

        // Merge with static attributes
        let staticAttrs = p.GetCustomAttributes()
        for attr in staticAttrs do
            if typeof<ParameterMetadataAttribute>.IsAssignableFrom(attr.GetType()) then
                if not (attrs.Contains(attr.GetType())) then
                    attrs.Add(attr :?> ParameterMetadataAttribute)
                   
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

    // Exptract and lift tupled or curried paramters
    let LiftArgs(expr) =
        match LiftTupledArgs(expr) with
        | Some(b, p) ->
            Some(b, p)
        | None ->
            match LiftCurriedArgs(expr) with
            | Some(b, p) ->
                Some(b, p)
            | _ ->
                None
        
    let rec ParseCall(expr) =                 
        match expr with
        | Patterns.Lambda(v, e) -> 
            ParseCall (e)
        | Patterns.Let (v, e1, e2) ->
            ParseCall (e2)
        | Patterns.Call (e, mi, a) ->
            Some(e, mi, a)
        | _ ->
            None 
            
    let rec GetKernelFromName(e) =     
        let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)
        match expr with
        | Patterns.Lambda(v, e) -> 
            GetKernelFromName (e)
        | Patterns.Let (v, e1, e2) ->
            GetKernelFromName (e2)
        | Patterns.Call (e, mi, a) ->
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->          
                // Extract parameters vars
                match LiftArgs(b) with
                | Some(liftBody, paramVars) ->       
                    MergeWithStaticKernelMeta(kernelAttrs, returnAttrs, mi)
                    let parameters = mi.GetParameters() |> Array.toList
                    let attrs = new List<ParamMetaCollection>()
                    for p in parameters do
                        let paramAttrs = new ParamMetaCollection()
                        MergeWithStaticParameterMeta(paramAttrs, p)
                        attrs.Add(paramAttrs)
                    Some(mi, paramVars, liftBody, kernelAttrs, returnAttrs, attrs)
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
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->                
                // Extract parameters vars
                match LiftArgs(b) with
                | Some(liftBody, paramVars) ->                    
                    MergeWithStaticKernelMeta(kernelAttrs, returnAttrs, mi)
                    let parameters = mi.GetParameters() |> Array.toList
                    let attrs = new List<ParamMetaCollection>()
                    let args = new List<Expr>()
                    for i = 0 to parameters.Length - 1 do
                        let paramAttrs = new ParamMetaCollection()
                        let cleanArgs, paramAttrs = ParseParameterMetadata(a.[i])
                        MergeWithStaticParameterMeta(paramAttrs, parameters.[i])
                        attrs.Add(paramAttrs)
                        args.Add(cleanArgs)
                    Some(mi, paramVars, liftBody, List.ofSeq args, kernelAttrs, returnAttrs, attrs)
                | _ ->
                    None
            | _ ->
                None
        | _ ->
            None
            
    let rec GetKernelFromMethodInfo(mi: MethodInfo) =                    
        match mi with
        | DerivedPatterns.MethodWithReflectedDefinition(b) ->     
            // Extract parameters vars
            match LiftArgs(b) with
            | Some(liftBody, paramVars) ->                    
                let kernelMeta = new KernelMetaCollection()
                let returnMeta = new ParamMetaCollection()
                MergeWithStaticKernelMeta(kernelMeta, returnMeta, mi)

                let parameters = mi.GetParameters() |> Array.toList
                let attrs = new List<ParamMetaCollection>()
                for p in parameters do
                    let paramAttrs = new ParamMetaCollection()
                    MergeWithStaticParameterMeta(paramAttrs, p)
                    attrs.Add(paramAttrs)
                Some(mi, paramVars, liftBody, kernelMeta, returnMeta, attrs)
            | _ ->
                None
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
        
    let LambdaToMethod(e) = 
        let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)

        let mutable body = expr
        let mutable parameters = []
        
        // Extract args from lambda
        match LiftTupledArgs(expr) with
        | Some(b, p) ->
            body <- b
            parameters <- p
        | None ->
            match LiftCurriedArgs(expr) with
            | Some(b, p) ->
                body <- b
                parameters <- p
            | _ ->
                ()

        // If no lifting occurred this is not a lambda
        if IsComputationalLambda(expr) then
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
                                    MethodAttributes.Public ||| MethodAttributes.Static, body.Type, 
                                    Array.ofList(List.map(fun (v: Var) -> v.Type) parameters))
            for p = 1 to parameters.Length do
                methodBuilder.DefineParameter(p, ParameterAttributes.None, parameters.[p-1].Name) |> ignore
            // Body (simple return) of the method must be set to build the module and get the MethodInfo that we need as signature
            methodBuilder.GetILGenerator().Emit(OpCodes.Ret)
            moduleBuilder.CreateGlobalFunctions()
            let signature = moduleBuilder.GetMethod(methodBuilder.Name) 
            Some(signature, parameters, body, kernelAttrs, returnAttrs, ReadOnlyMetaCollection.EmptyParamMetaCollection(parameters.Length))
        else
            None

    
        

