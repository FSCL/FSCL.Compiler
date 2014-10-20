namespace FSCL.Compiler.AcceleratedCollections

open FSCL.Compiler
open FSCL.Language
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System.Collections.Generic
open System
open FSCL.Compiler.Util
open Microsoft.FSharp.Reflection
open AcceleratedCollectionUtil
open System.Runtime.InteropServices
open Microsoft.FSharp.Linq.RuntimeHelpers

open FSCL.Compiler.Util.QuotationAnalysis.FunctionsManipulation
open FSCL.Compiler.Util.QuotationAnalysis.KernelParsing
open FSCL.Compiler.Util.QuotationAnalysis.MetadataExtraction

type AcceleratedArrayReduceHandler() =
    let placeholderComp (a:int) (b:int) =
        a + b

    let cpu_template = 
        <@
            fun(g_idata:int[], g_odata:int[], block: int, wi: WorkItemInfo) ->
                let mutable global_index = wi.GlobalID(0) * block
                let up = (wi.GlobalID(0) + 1) * block
                let mutable upper_bound = Math.Min(up, g_idata.Length)
                
                if upper_bound > g_idata.Length then
                    upper_bound <- g_idata.Length

                // We don't know which is the neutral value for placeholderComp so we need to
                // initialize it with an element of the input array
                let mutable accumulator = 0
                if global_index < upper_bound then
                    accumulator <- g_idata.[global_index]
                    global_index <- global_index + 1

                while global_index < upper_bound do
                    accumulator <- placeholderComp accumulator g_idata.[global_index]
                    global_index <- global_index + 1

                g_odata.[wi.GroupID(0)] <- accumulator
        @>

    // NEW: Two-Stage reduction instead of multi-stage
    let gpu_template = 
        <@
            fun(g_idata:int[], [<Local>]sdata:int[], g_odata:int[], wi:WorkItemInfo) ->
                let global_index = wi.GlobalID(0)
                let global_size = wi.GlobalSize(0)
                let mutable accumulator = g_idata.[global_index]
                for gi in global_index + global_size .. global_size .. g_idata.Length - 1 do
                    accumulator <- placeholderComp accumulator g_idata.[gi]
                                        
                let local_index = wi.LocalID(0)
                sdata.[local_index] <- accumulator
                wi.Barrier(CLK_LOCAL_MEM_FENCE)

                let mutable offset = wi.LocalSize(0) / 2
                while(offset > 0) do
                    if(local_index < offset) then
                        sdata.[local_index] <- placeholderComp (sdata.[local_index]) (sdata.[local_index + offset])
                    offset <- offset / 2
                    wi.Barrier(CLK_LOCAL_MEM_FENCE)
                
                if local_index = 0 then
                    g_odata.[wi.GroupID(0)] <- sdata.[0]
        @>
             
    let rec SubstitutePlaceholders(e:Expr, parameters:Dictionary<Var, Var>, accumulatorPlaceholder:Var, actualFunction: MethodInfo option) =  
        // Build a call expr
        let RebuildCall(o:Expr option, m: MethodInfo, args:Expr list) =
            if o.IsSome && (not m.IsStatic) then
                Expr.Call(o.Value, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)
            else
                Expr.Call(m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)  
            
        match e with
        | Patterns.Var(v) ->
            if v.Name = "accumulator" then
                Expr.Var(accumulatorPlaceholder)
            else if parameters.ContainsKey(v) then
                Expr.Var(parameters.[v])
            else
                e
        | Patterns.Call(o, m, args) ->   
            // If this is the placeholder for the utility function (to be applied to each pari of elements)         
            if m.Name = "placeholderComp" then
                if actualFunction.IsSome then
                    RebuildCall(o, actualFunction.Value, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)
                else
                    let sumM = AcceleratedCollectionUtil.FilterCall(<@@ (+) @@>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| accumulatorPlaceholder.Type; accumulatorPlaceholder.Type; accumulatorPlaceholder.Type |])).Value
                    Expr.Call(sumM, (List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args))
            // If this is an access to array (a parameter)
            else if m.DeclaringType.Name = "IntrinsicFunctions" then
                match args.[0] with
                | Patterns.Var(v) ->
                    if m.Name = "GetArray" then
                        // Find the placeholder holding the variable
                        if (parameters.ContainsKey(v)) then
                            // Recursively process the arguments, except the array reference
                            let arrayGet, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(parameters.[v].Type.GetElementType())
                            Expr.Call(arrayGet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, accumulatorPlaceholder, actualFunction) ])
                        else
                            RebuildCall(o, m, args)
                    else if m.Name = "SetArray" then
                        // Find the placeholder holding the variable
                        if (parameters.ContainsKey(v)) then
                            // Recursively process the arguments, except the array reference)
                            let _, arraySet = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(parameters.[v].Type.GetElementType())
                            // If the value is const (e.g. 0) then it must be converted to the new array element type
                            let newValue = match args.[2] with
                                            | Patterns.Value(o, t) ->
                                                if actualFunction.IsSome then
                                                    let outputParameterType = actualFunction.Value.GetParameters().[1].ParameterType
                                                    // Conversion method (ToDouble, ToSingle, ToInt, ...)
                                                    Expr.Value(Activator.CreateInstance(outputParameterType), outputParameterType)
                                                else
                                                    args.[2]
                                            | _ ->
                                                SubstitutePlaceholders(args.[2], parameters, accumulatorPlaceholder, actualFunction)
                            Expr.Call(arraySet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, accumulatorPlaceholder, actualFunction); newValue ])
                                                           
                        else
                            RebuildCall(o, m, args)
                    else
                         RebuildCall(o, m,args)
                | _ ->
                    RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)                  
            // Otherwise process children and return the same call
            else
                RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)
        | Patterns.Let(v, value, body) ->
            if v.Name = "accumulator" then
                Expr.Let(accumulatorPlaceholder,   
                         AcceleratedCollectionUtil.GetDefaultValueExpr(accumulatorPlaceholder.Type),
                         SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, actualFunction))
            // a and b are "special" vars that hold the params of the reduce function
            else if v.Name = "a" then
                let newVarType = 
                    if actualFunction.IsSome then
                        actualFunction.Value.GetParameters().[0].ParameterType
                    else
                        accumulatorPlaceholder.Type
                let a = Quotations.Var("a", newVarType, false)
                parameters.Add(v, a)
                Expr.Let(a, SubstitutePlaceholders(value, parameters, accumulatorPlaceholder, actualFunction), 
                            SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, actualFunction))
            else if v.Name = "b" then
                let newVarType = 
                    if actualFunction.IsSome then
                        actualFunction.Value.GetParameters().[0].ParameterType
                    else
                        accumulatorPlaceholder.Type
                let b = Quotations.Var("b", newVarType, false)
                // Remember for successive references to a and b
                parameters.Add(v, b)
                Expr.Let(b, SubstitutePlaceholders(value, parameters, accumulatorPlaceholder, actualFunction), SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, actualFunction))        
            else
                Expr.Let(v, SubstitutePlaceholders(value, parameters, accumulatorPlaceholder, actualFunction), SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, actualFunction))
        | ExprShape.ShapeLambda(v, b) ->
            Expr.Lambda(v, SubstitutePlaceholders(b, parameters, accumulatorPlaceholder, actualFunction))                    
        | ExprShape.ShapeCombination(o, l) ->
            match e with
            | Patterns.IfThenElse(cond, ifb, elseb) ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction) 
                    // Trick to adapt "0" in (sdata.[tid] <- if(i < n) then g_idata.[i] else 0) in case of other type of values (double: 0.0)
                    nl.Add(ne)
                ExprShape.RebuildShapeCombination(o, List.ofSeq(nl))
            | _ ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction) 
                    nl.Add(ne)
                ExprShape.RebuildShapeCombination(o, List.ofSeq(nl))
        | _ ->
            e

    member this.EvaluateAndApply(e:Expr) (a:obj) (b:obj) =
        let f = LeafExpressionConverter.EvaluateQuotation(e)
        let fm = f.GetType().GetMethod("Invoke")
        let r1 = fm.Invoke(f, [| a |])
        let r2m = r1.GetType().GetMethod("Invoke")
        let r2 = r2m.Invoke(r1, [| b |])
        r2

    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, cleanArgs, root, meta, step) =       
            (*
                Array map looks like: Array.map fun collection
                At first we check if fun is a lambda (first argument)
                and in this case we transform it into a method
                Secondly, we iterate parsing on the second argument (collection)
                since it might be a subkernel
            *)
            let isArraySum = methodInfo.Name = "Sum"
            let lambda, computationFunction =                
                if isArraySum then
                    None, None
                else
                    AcceleratedCollectionUtil.ExtractComputationFunction(cleanArgs, root)
                                            
            // Extract the reduce function 
            if isArraySum || computationFunction.IsSome then
                
                // Create on-the-fly module to host the kernel                
                // The dynamic module that hosts the generated kernels
                let assemblyName = IDGenerator.GenerateUniqueID("FSCL.Compiler.Plugins.AcceleratedCollections.AcceleratedArray");
                let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
                let moduleBuilder = assemblyBuilder.DefineDynamicModule("AcceleratedArrayModule");

                // Now create the kernel
                // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                let inputArrayType, outputArrayType =                     
                    match computationFunction with
                    | Some(functionInfo, functionParamVars, body) ->
                        (Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType(),  Array.CreateInstance(functionInfo.ReturnType, 0).GetType())
                    | _ ->
                        (methodInfo.GetParameters().[0].ParameterType, methodInfo.GetParameters().[0].ParameterType)
                // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output       
                
                // Check device target
                let targetType = meta.KernelMeta.Get<DeviceTypeAttribute>()
            
                let kModule = 
                    // GPU CODE
                    match targetType.Type with
                    | DeviceType.Gpu ->                    
                        // Now we can create the signature and define parameter name in the dynamic module                                                                        
                        let signature, name, appliedFunctionBody =    
                            match computationFunction with
                            | Some(functionInfo, functionParamVars, body) ->
                               (DynamicMethod("ArrayReduce_" + functionInfo.Name, outputArrayType, [| inputArrayType; outputArrayType; outputArrayType; typeof<WorkItemInfo> |]), "Array.reduce", Some(body))
                            | _ ->
                               (DynamicMethod("ArraySum", outputArrayType, [| inputArrayType; outputArrayType; outputArrayType; typeof<WorkItemInfo> |]), "Array.sum", None)
                                
                        signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                        signature.DefineParameter(2, ParameterAttributes.In, "local_array") |> ignore
                        signature.DefineParameter(3, ParameterAttributes.In, "output_array") |> ignore
                        signature.DefineParameter(4, ParameterAttributes.In, "wi") |> ignore
                        
                        // Create parameters placeholders
                        let inputHolder = Quotations.Var("input_array", inputArrayType)
                        let localHolder = Quotations.Var("local_array", outputArrayType)
                        let outputHolder = Quotations.Var("output_array", outputArrayType)
                        let wiHolder = Quotations.Var("wi", typeof<WorkItemInfo>)
                        let accumulatorPlaceholder = Quotations.Var("accumulator", outputArrayType.GetElementType())
                        let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputHolder.Type; localHolder.Type; outputHolder.Type; wiHolder.Type |]))

                        // Finally, create the body of the kernel
                        let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromLambda(gpu_template)   
                        let parameterMatching = new Dictionary<Var, Var>()
                        parameterMatching.Add(templateParameters.[0], inputHolder)
                        parameterMatching.Add(templateParameters.[1], localHolder)
                        parameterMatching.Add(templateParameters.[2], outputHolder)
                        parameterMatching.Add(templateParameters.[3], wiHolder)

                        // Replace functions and references to parameters
                        let functionMatching = new Dictionary<string, MethodInfo>()
                        let fInfo = 
                            match computationFunction with
                            | Some(functionInfo, functionParamVars, body) ->
                                Some(functionInfo)
                            | _ ->
                                None
                                         
                        let newBody = SubstitutePlaceholders(templateBody, parameterMatching, accumulatorPlaceholder, fInfo)  
                        let finalKernel = 
                            Expr.Lambda(tupleHolder,
                                Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                    Expr.Let(localHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                        Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                            Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 3),
                                                newBody)))))

                        let methodParams = signature.GetParameters()
                        let kInfo = new AcceleratedKernelInfo(signature, 
                                                              [ methodParams.[0]; methodParams.[1]; methodParams.[2] ],
                                                              [ inputHolder; localHolder; outputHolder ],
                                                              finalKernel, 
                                                              meta, 
                                                              name, appliedFunctionBody)
                        let kernelModule = new KernelModule(kInfo, cleanArgs)
                        
                        kernelModule                
                    |_ ->
                        // CPU CODE                                                              
                        let signature, name, appliedFunctionBody =    
                            match computationFunction with
                            | Some(functionInfo, functionParamVars, body) ->
                               (DynamicMethod("ArrayReduce_" + functionInfo.Name, outputArrayType, [| inputArrayType; outputArrayType; outputArrayType; typeof<WorkItemInfo> |]), "Array.reduce", Some(body))
                            | _ ->
                               (DynamicMethod("ArraySum", outputArrayType, [| inputArrayType; outputArrayType; outputArrayType; typeof<WorkItemInfo> |]), "Array.sum", None)
                                                 
                        // Now we can create the signature and define parameter name in the dynamic module                                        
                        signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                        signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                        signature.DefineParameter(3, ParameterAttributes.In, "block") |> ignore
                        signature.DefineParameter(4, ParameterAttributes.In, "wi") |> ignore
                    
                        // Create parameters placeholders
                        let inputHolder = Quotations.Var("input_array", inputArrayType)
                        let blockHolder = Quotations.Var("block", typeof<int>)
                        let outputHolder = Quotations.Var("output_array", outputArrayType)
                        let accumulatorPlaceholder = Quotations.Var("accumulator", outputArrayType.GetElementType())
                        let wiHolder = Quotations.Var("wi", typeof<WorkItemInfo>)
                        let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputHolder.Type; outputHolder.Type; blockHolder.Type; wiHolder.Type |]))

                        // Finally, create the body of the kernel
                        let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromLambda(cpu_template)   
                        let parameterMatching = new Dictionary<Var, Var>()
                        parameterMatching.Add(templateParameters.[0], inputHolder)
                        parameterMatching.Add(templateParameters.[1], outputHolder)
                        parameterMatching.Add(templateParameters.[2], blockHolder)
                        parameterMatching.Add(templateParameters.[3], wiHolder)

                        // Replace functions and references to parameters
                        let functionMatching = new Dictionary<string, MethodInfo>()
                        let fInfo = 
                            match computationFunction with
                            | Some(functionInfo, functionParamVars, body) ->
                                Some(functionInfo)
                            | _ ->
                                None
                        let newBody = SubstitutePlaceholders(templateBody, parameterMatching, accumulatorPlaceholder, fInfo)  
                        let finalKernel = 
                            Expr.Lambda(tupleHolder,
                                Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                    Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                        Expr.Let(blockHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                            Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 3),
                                                newBody)))))
                    
                        // Setup kernel module and return
                        let methodParams = signature.GetParameters()
                        let kInfo = new AcceleratedKernelInfo(signature, 
                                                                [ methodParams.[0]; methodParams.[1]; methodParams.[2] ],
                                                                [ inputHolder; outputHolder; blockHolder ],
                                                                finalKernel, 
                                                                meta, 
                                                                name, appliedFunctionBody)
                        let kernelModule = new KernelModule(kInfo, cleanArgs)
                        
                        kernelModule 

                // Add applied function      
                match computationFunction with
                | Some(functionInfo, functionParamVars, body) ->
                    let reduceFunctionInfo = new FunctionInfo(functionInfo, 
                                                              functionInfo.GetParameters() |> List.ofArray,
                                                              functionParamVars,
                                                              None,
                                                              body, lambda.IsSome)
                
                    // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                    if lambda.IsSome then
                        kModule.Kernel.CustomInfo.Add("ReduceFunction", lambda.Value)
                    else
                        // ExtractComputationFunction may have lifted some paramters that are referencing stuff outside the quotation, so 
                        // a new methodinfo is generated with no body. So we can't invoke it, and therefore we add as ReduceFunction the body instead of the methodinfo
                        kModule.Kernel.CustomInfo.Add("ReduceFunction", match computationFunction.Value with a, _, b -> b)
                                    
                    // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                    kModule.Functions.Add(reduceFunctionInfo.ID, reduceFunctionInfo)
                | _ ->
                    // Array.sum: reduce function in (+)
                    kModule.Kernel.CustomInfo.Add("ReduceFunction", 
                                                  ExtractMethodInfo(<@ (+) @>).Value.GetGenericMethodDefinition().MakeGenericMethod([| inputArrayType.GetElementType(); inputArrayType.GetElementType();  outputArrayType.GetElementType() |]))
                // Return module                             
                Some(kModule)
            else
                None