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
    let private ParseDynamicAttributes<'T>(expr, attrs: Dictionary<Type, 'T>) =
        let rec ParseDynamicAttributesInternal(expr) =
            match expr with
            | Patterns.Call(o, mi, args) ->
                let dynamicAttributeFunction = mi.GetCustomAttribute<DynamicAttributeFunctionAttribute>()
                if dynamicAttributeFunction <> null then
                    // Get attribute type
                    let attrType = dynamicAttributeFunction.Attribute
                    if typeof<'T> = attrType then
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
                            let attr = constr.Invoke(attrArgs) :?> 'T
                            if not (attrs.ContainsKey(attrType)) then
                                attrs.Add(attrType, attr)
                            else
                                attrs.[attrType] <- attr
                            // Continue processing body
                            ParseDynamicAttributesInternal(args.[args.Length - 1])
                    else
                        let attrArgs = args |> 
                                       List.map(fun (e:Expr) -> ParseDynamicAttributesInternal(e))
                        if o.IsSome then
                            Expr.Call(o.Value, mi, attrArgs)
                        else
                            Expr.Call(mi, attrArgs)
                else
                    expr
            | _ ->
                expr
        ParseDynamicAttributesInternal(expr)

    let ParseDynamicKernelAttributeFunctions(expr) =
        // Process dynamic attributes
        let attrs = new DynamicKernelAttributeCollection()
        (ParseDynamicAttributes<DynamicKernelAttributeAttribute>(expr, attrs), attrs)
        
    let ParseDynamicParameterAttributeFunctions(expr) =
        // Process dynamic attributes
        let attrs = new DynamicParameterAttributeCollection()
        (ParseDynamicAttributes<DynamicParameterAttributeAttribute>(expr, attrs), attrs)
                                        
    let ParseDynamicKernelAttributes(m: MethodInfo) =
        let dictionary = new DynamicKernelAttributeCollection()        
        for item in m.GetCustomAttributes() do
            if typeof<DynamicKernelAttributeAttribute>.IsAssignableFrom(item.GetType()) then
                dictionary.Add(item.GetType(), item :?> DynamicKernelAttributeAttribute)
        new ReadOnlyDynamicKernelAttributeCollection(dictionary)
        
    let ParseDynamicParameterAttributes(p: ParameterInfo) =
        let dictionary = new DynamicParameterAttributeCollection()        
        for item in p.GetCustomAttributes() do
            if typeof<DynamicParameterAttributeAttribute>.IsAssignableFrom(item.GetType()) then
                dictionary.Add(item.GetType(), item :?> DynamicParameterAttributeAttribute)
        new ReadOnlyDynamicParameterAttributeCollection(dictionary)
                
    let LiftTupledCallArgs(expr) =
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
            
    let LiftCurriedCallArgs(expr) =
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
        let expr, attrs = ParseDynamicKernelAttributeFunctions(e)
        match expr with
        | Patterns.Lambda(v, e) -> 
            GetKernelFromName (e)
        | Patterns.Let (v, e1, e2) ->
            GetKernelFromName (e2)
        | Patterns.Call (e, mi, a) ->
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                Some(mi, b, attrs)
            | _ ->
                None
        | _ ->
            None
                
    let rec GetKernelFromCall(e) =                    
        let expr, attrs = ParseDynamicKernelAttributeFunctions(e)
        match expr with
        | Patterns.Call (e, mi, a) ->
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                let cleanArgs, paramAttrs = a |> List.map (fun (pe:Expr) -> ParseDynamicParameterAttributeFunctions(pe)) |> List.unzip
                Some(mi, cleanArgs, b, attrs, paramAttrs)
            | _ ->
                None
        | _ ->
            None
            
    let rec GetKernelFromMethodInfo(mi: MethodInfo) =                    
        match mi with
        | DerivedPatterns.MethodWithReflectedDefinition(b) ->
            Some(mi, b)
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
        let expr, attrs = ParseDynamicKernelAttributeFunctions(e)

        let mutable body = expr
        let mutable parameters = []
        
        // Extract args from lambda
        match LiftTupledCallArgs(expr) with
        | Some(b, p) ->
            body <- b
            parameters <- p
        | None ->
            match LiftCurriedCallArgs(expr) with
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
            Some(signature, body, attrs)
        else
            None
    
        

