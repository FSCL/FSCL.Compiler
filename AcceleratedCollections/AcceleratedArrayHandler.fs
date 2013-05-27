namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System

type internal IAcceleratedCollectionHandler =
    abstract member Process: MethodInfo * Expr list -> KernelModule option

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

type AcceleratedArrayMapHandler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, args) =
            let kernelModule = KernelModule()
            // Extract the map function 
            match AcceleratedCollectionUtil.FilterCall(args.[0], id) with
            | Some(expr, functionInfo, args) ->
                // Check if the referred function has a ReflectedDefinition attribute
                match functionInfo with
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                    // Now create the kernel
                    // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                    let inputArrayType = Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType()
                    let outputArrayType = Array.CreateInstance(functionInfo.ReturnType, 0).GetType()
                   (* let inputEmptyArray = FilterCall(<@ Array.empty @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| inputArrayElementType |])).Value
                    // We need to get the type of a array whose elements type is the same of the functionInfo return value
                    let outputArrayElementType = functionInfo.ReturnType
                    let outputEmptyArray = FilterCall(<@ Array.empty @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| outputArrayElementType |])).Value
                    *)
                    // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output                    
                    let inputArrayPlaceholder = Expr.Var(Quotations.Var("input_array", inputArrayType))
                    let outputArrayPlaceholder = Expr.Var(Quotations.Var("output_array", outputArrayType))
                    
                    // Now we can create the signature and define parameter name
                    let signature = DynamicMethod("ArrayMap_" + functionInfo.Name, typeof<unit>, [| inputArrayType; outputArrayType |])
                    signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                    signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                    
                    // Finally, create the body of the kernel
                    let globalIdVar = Quotations.Var("global_id", typeof<int>)
                    let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType())
                    let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                    let body = 
                        Expr.Let(globalIdVar,
                                 Expr.Call(AcceleratedCollectionUtil.FilterCall(<@ get_global_id @>, fun(e, mi, a) -> mi).Value, [ Expr.Value(0) ]),
                                 Expr.Call(setElementMethodInfo,
                                           [ outputArrayPlaceholder;
                                             Expr.Var(globalIdVar);
                                             Expr.Call(functionInfo,
                                                        [ Expr.Call(getElementMethodInfo,
                                                                    [ inputArrayPlaceholder;
                                                                       Expr.Var(globalIdVar) 
                                                                    ])
                                                        ])
                                           ]))

                    kernelModule.Source <- KernelInfo(signature, body)                                                           
                    Some(kernelModule)
                | _ ->
                    None
            | _ ->
                None
                        (*
type AcceleratedArrayReduceHandler() =
    let placeholderComp(a:int, b:int) =
        a + b
    let template = 
        <@
            fun(g_idata:int[], [<Local>]sdata:int[], n, g_odata:int[]) ->
                let tid = get_local_id(0)
                let i = get_group_id(0) * (get_local_size(0) * 2) + get_local_id(0)

                sdata.[tid] <- if(i < n) then g_idata.[i] else 0
                if (i + get_local_size(0) < n) then 
                    sdata.[tid] <- placeholderComp(sdata.[tid], g_idata.[i + get_local_size(0)])

                barrier(CLK_LOCAL_MEM_FENCE)
                // do reduction in shared mem
                let mutable s = get_local_size(0) >>> 1
                while (s > 0) do 
                    if (tid < s) then
                        sdata.[tid] <- placeholderComp(sdata.[tid], sdata.[tid + s])
                    barrier(CLK_LOCAL_MEM_FENCE)
                    s <- s >>> 1

                if (tid = 0) then 
                    g_odata.[get_group_id(0)] <- sdata.[0]
        @>

    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, args) =
            let kernelModule = KernelModule()
            // Extract the map function 
            match AcceleratedCollectionUtil.FilterCall(args.[0], id) with
            | Some(expr, functionInfo, args) ->
                // Check if the referred function has a ReflectedDefinition attribute
                match functionInfo with
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                    // Now create the kernel
                    // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                    let inputArrayType = Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType()
                    let outputArrayType = Array.CreateInstance(functionInfo.ReturnType, 0).GetType()
                    // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output                    
                    let inputArrayPlaceholder = Quotations.Var("input_array", inputArrayType)
                    let outputArrayPlaceholder = Quotations.Var("output_array", outputArrayType)
                    let localArrayPlaceholder = Quotations.Var("local_array", outputArrayType)
                    
                    // Now we can create the signature and define parameter name
                    let signature = DynamicMethod("ArrayReduce_" + functionInfo.Name, typeof<unit>, [| inputArrayType; outputArrayType; outputArrayType |])
                    signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                    signature.DefineParameter(2, ParameterAttributes.In, "local_array") |> ignore
                    signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                    
                    // Finally, create the body of the kernel
                    let templateBody, templateParameters = TemplateAdaptor.GetKernelFromLambda(template)   
                    let parameterMatching = new Dictionary<Var, Var>()
                    parameterMatching.Add(templateParameters.[0], inputArrayPlaceholder)
                    parameterMatching.Add(templateParameters.[1], localArrayPlaceholder)
                    parameterMatching.Add(templateParameters.[2], outputArrayPlaceholder)
                    let newBody = TemplateAdaptor.SubstitutePlaceholders(templateBody, parameterMatching, ("placeholderComp", functionInfo))                                    
                    Some(kernelModule)
                | _ ->
                    None
            | _ ->
                None
            
            *)