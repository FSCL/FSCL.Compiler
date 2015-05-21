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
            fun(g_idata:int[], 
                [<TransferMode(TransferMode.NoTransfer, TransferMode.NoTransfer)>]g_odata:int[],
                block: int, wi: WorkItemInfo) ->
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
            fun(g_idata:int[], 
                [<Local>]sdata:int[], 
                [<TransferMode(TransferMode.NoTransfer, TransferMode.NoTransfer)>]g_odata:int[], 
                wi:WorkItemInfo) ->

                let global_index = wi.GlobalID(0)
                let global_size = wi.GlobalSize(0)
                let mutable accumulator = g_idata.[global_index]
                for gi in global_index + global_size .. global_size .. g_idata.Length - 1 do
                    accumulator <- placeholderComp accumulator g_idata.[gi]
                                        
                let local_index = wi.LocalID(0)
                sdata.[local_index] <- accumulator
                wi.LocalBarrier()

                let mutable offset = wi.LocalSize(0) / 2
                while(offset > 0) do
                    if(local_index < offset) then
                        sdata.[local_index] <- placeholderComp (sdata.[local_index]) (sdata.[local_index + offset])
                    offset <- offset / 2
                    wi.LocalBarrier()
                
                if local_index = 0 then
                    g_odata.[wi.GroupID(0)] <- sdata.[0]
        @>
             
    let rec SubstitutePlaceholders(e:Expr, 
                                   parameters:Dictionary<Var, Var>, 
                                   accumulatorPlaceholder:Var, 
                                   utilityFunction: Expr option,
                                   utilityFunctionInputType: Type,
                                   utilityFunctionReturnType: Type) =  
        // Build a call expr
        let RebuildCall(o:Expr option, m: MethodInfo, args:Expr list) =
            if o.IsSome && (not m.IsStatic) then
                Expr.Call(o.Value, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)
            else
                Expr.Call(m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)  
            
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
                if utilityFunction.IsSome then
                    AcceleratedCollectionUtil.BuildApplication(utilityFunction.Value, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)
                else
                    let sumM = AcceleratedCollectionUtil.FilterCall(<@@ (+) @@>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| accumulatorPlaceholder.Type; accumulatorPlaceholder.Type; accumulatorPlaceholder.Type |])).Value
                    Expr.Call(sumM, (List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args))
            // If this is an access to array (a parameter)
            else if m.DeclaringType.Name = "IntrinsicFunctions" then
                match args.[0] with
                | Patterns.Var(v) ->
                    if m.Name = "GetArray" then
                        // Find the placeholder holding the variable
                        if (parameters.ContainsKey(v)) then
                            // Recursively process the arguments, except the array reference
                            let arrayGet, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(parameters.[v].Type.GetElementType(), 1)
                            Expr.Call(arrayGet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType) ])
                        else
                            RebuildCall(o, m, args)
                    else if m.Name = "SetArray" then
                        // Find the placeholder holding the variable
                        if (parameters.ContainsKey(v)) then
                            // Recursively process the arguments, except the array reference)
                            let _, arraySet = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(parameters.[v].Type.GetElementType(), 1)
                            // If the value is const (e.g. 0) then it must be converted to the new array element type
                            let newValue = match args.[2] with
                                            | Patterns.Value(o, t) ->
                                                if utilityFunction.IsSome then
                                                    let outputParameterType = utilityFunctionReturnType
                                                    // Conversion method (ToDouble, ToSingle, ToInt, ...)
                                                    Expr.Value(Activator.CreateInstance(outputParameterType), outputParameterType)
                                                else
                                                    args.[2]
                                            | _ ->
                                                SubstitutePlaceholders(args.[2], parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)
                            Expr.Call(arraySet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType); newValue ])
                                                           
                        else
                            RebuildCall(o, m, args)
                    else
                         RebuildCall(o, m,args)
                | _ ->
                    RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)                  
            // Otherwise process children and return the same call
            else
                RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)
        | Patterns.Let(v, value, body) ->
            if v.Name = "accumulator" then
                Expr.Let(accumulatorPlaceholder,   
                         AcceleratedCollectionUtil.GetDefaultValueExpr(accumulatorPlaceholder.Type),
                         SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))
            // a and b are "special" vars that hold the params of the reduce function
            else if v.Name = "a" then
                let newVarType = 
                    if utilityFunction.IsSome then
                        utilityFunctionInputType
                    else
                        accumulatorPlaceholder.Type
                let a = Quotations.Var("a", newVarType, false)
                parameters.Add(v, a)
                Expr.Let(a, SubstitutePlaceholders(value, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType), 
                            SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))
            else if v.Name = "b" then
                let newVarType = 
                    if utilityFunction.IsSome then
                        utilityFunctionInputType
                    else
                        accumulatorPlaceholder.Type
                let b = Quotations.Var("b", newVarType, false)
                // Remember for successive references to a and b
                parameters.Add(v, b)
                let newValue = SubstitutePlaceholders(value, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)
                let newBody = SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)
                Expr.Let(b, newValue, newBody)        
            else
                Expr.Let(v, SubstitutePlaceholders(value, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType), SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))
        | ExprShape.ShapeLambda(v, b) ->
            Expr.Lambda(v, SubstitutePlaceholders(b, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))                    
        | ExprShape.ShapeCombination(o, l) ->
            match e with
            | Patterns.IfThenElse(cond, ifb, elseb) ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType) 
                    // Trick to adapt "0" in (sdata.[tid] <- if(i < n) then g_idata.[i] else 0) in case of other type of values (double: 0.0)
                    nl.Add(ne)
                ExprShape.RebuildShapeCombination(o, List.ofSeq(nl))
            | _ ->
                let nl = new List<Expr>();
                for ee in l do 
                    let ne = SubstitutePlaceholders(ee, parameters, accumulatorPlaceholder, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType) 
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
        member this.Process(methodInfo, cleanArgs, root, meta, step, env, opts) =     
            let isArraySum = methodInfo.Name = "Sum"
            let computationFunction, subExpr =                
                if isArraySum then
                    None, None
                else
                    // Inspect operator
                    AcceleratedCollectionUtil.ParseOperatorLambda(cleanArgs.[0], step, env, opts)
                                
            match subExpr with
            | Some(kfg, newEnv) ->
                // This coll fun is a composition 
                let node = new KFGCollectionCompositionNode(methodInfo, kfg, newEnv)
                
                // Create data node for outsiders
//                for o in outsiders do 
//                    node.InputNodes.Add(new KFGOutsiderDataNode(o))

                // Parse arguments
                let subnode = step.Process(cleanArgs.[1], env, opts)
                node.InputNodes.Add(subnode)
                Some(node :> IKFGNode)   
            | _ ->                               
                // Extract the reduce function 
                if isArraySum || computationFunction.IsSome then
                
                    // Create on-the-fly module to host the kernel                
                    // The dynamic module that hosts the generated kernels
                    //let mutable outsiders = new List<OutsiderRef>()

                    // Now create the kernel
                    // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                    let inputArrayType, outputType =   
                        if isArraySum then
                            methodInfo.GetParameters().[0].ParameterType, methodInfo.ReturnType   
                        else
                            methodInfo.GetParameters().[1].ParameterType, methodInfo.ReturnType   
                    // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output       
                
                    // Check device target
                    let targetType = meta.KernelMeta.Get<DeviceTypeAttribute>()
                                                              
                    let kernelName, runtimeName =    
                        match computationFunction with
                        | Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                            "ArrayReduce_" + functionName, "Array.reduce"
                        | _ ->
                            "ArraySum", "Array.sum"

                    let kModule = 
                        // GPU CODE
                        match targetType.Type with
                        | DeviceType.Gpu ->                    
                            // Create parameters placeholders
                            let inputHolder = Quotations.Var("input_array", inputArrayType)
                            let localHolder = Quotations.Var("local_array", outputType.MakeArrayType())
                            let outputHolder = Quotations.Var("output_array", outputType.MakeArrayType())
                            let wiHolder = Quotations.Var("wi", typeof<WorkItemInfo>)
                            let accumulatorPlaceholder = Quotations.Var("accumulator", outputType)
                            let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputHolder.Type; localHolder.Type; outputHolder.Type; wiHolder.Type |]))

                            // Finally, create the body of the kernel
                            let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromCollectionFunctionTemplate(gpu_template)   
                            let parameterMatching = new Dictionary<Var, Var>()
                            parameterMatching.Add(templateParameters.[0], inputHolder)
                            parameterMatching.Add(templateParameters.[1], localHolder)
                            parameterMatching.Add(templateParameters.[2], outputHolder)
                            parameterMatching.Add(templateParameters.[3], wiHolder)

                            // Replace functions and references to parameters
                            let functionMatching = new Dictionary<string, MethodInfo>()
                            let thisVar, thisObj, functionBody =
                                match computationFunction with
                                | Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                                    thisVar, ob, Some(functionBody)
                                | _ ->
                                    None, None, None
                                         
                            let newBody = SubstitutePlaceholders(templateBody, parameterMatching, accumulatorPlaceholder, functionBody, inputArrayType.GetElementType(), outputType)  
                            let finalKernel = 
                                Expr.Lambda(tupleHolder,
                                    Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                        Expr.Let(localHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                            Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                                Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 3),
                                                    newBody)))))

                                    
                            // Add applied function    
                            match computationFunction with
                            | Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                                let envVars, outVals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(functionBody)                            
                                let reduceFunctionInfo = new FunctionInfo(functionName,
                                                                          functionInfo, 
                                                                          functionParamVars,
                                                                          functionReturnType,
                                                                          envVars, outVals,
                                                                          functionBody)
                                                                          
                                let kInfo = new AcceleratedKernelInfo(kernelName, 
                                                                      methodInfo,
                                                                      [ inputHolder; localHolder; outputHolder ],
                                                                      outputType.MakeArrayType(),
                                                                      envVars, outVals,
                                                                      finalKernel, 
                                                                      meta, 
                                                                      runtimeName, Some(reduceFunctionInfo :> IFunctionInfo), Some(functionBody))
                                let kernelModule = new KernelModule(thisVar, thisObj, kInfo)
                
                                // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                                kernelModule.Kernel.CustomInfo.Add("ReduceFunction", functionBody)
                                    
                                // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                                kernelModule.Functions.Add(reduceFunctionInfo.ID, reduceFunctionInfo)
                                kernelModule.Kernel.CalledFunctions.Add(reduceFunctionInfo.ID)
                                kernelModule
                            | _ ->
                                // Array.sum: reduce function in (+)
                                let kInfo = new AcceleratedKernelInfo(kernelName, 
                                                                      methodInfo,
                                                                      [ inputHolder; localHolder; outputHolder ],
                                                                      outputType.MakeArrayType(),
                                                                      new List<Var>(), new List<Expr>(),
                                                                      finalKernel, 
                                                                      meta, 
                                                                      runtimeName, None, None)
                                let kernelModule = new KernelModule(thisVar, thisObj, kInfo)                
                                kernelModule.Kernel.CustomInfo.Add("ReduceFunction", 
                                                                ExtractMethodInfo(<@ (+) @>).Value.GetGenericMethodDefinition().MakeGenericMethod([| inputArrayType.GetElementType(); inputArrayType.GetElementType(); outputType |]))
                                kernelModule  
                                
                                              
                        | _ ->
                            // CPU CODE               
                    
                            // Create parameters placeholders
                            let inputHolder = Quotations.Var("input_array", inputArrayType)
                            let blockHolder = Quotations.Var("block", typeof<int>)
                            let outputHolder = Quotations.Var("output_array", outputType.MakeArrayType())
                            let accumulatorPlaceholder = Quotations.Var("accumulator", outputType)
                            let wiHolder = Quotations.Var("wi", typeof<WorkItemInfo>)
                            let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputHolder.Type; outputHolder.Type; blockHolder.Type; wiHolder.Type |]))

                            // Finally, create the body of the kernel
                            let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromCollectionFunctionTemplate(cpu_template)   
                            let parameterMatching = new Dictionary<Var, Var>()
                            parameterMatching.Add(templateParameters.[0], inputHolder)
                            parameterMatching.Add(templateParameters.[1], outputHolder)
                            parameterMatching.Add(templateParameters.[2], blockHolder)
                            parameterMatching.Add(templateParameters.[3], wiHolder)

                            // Replace functions and references to parameters
                            let functionMatching = new Dictionary<string, MethodInfo>()
                            let thisVar, thisObj, functionBody =
                                match computationFunction with
                                | Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                                    thisVar, ob, Some(functionBody)
                                | _ ->
                                    None, None, None

                            let newBody = SubstitutePlaceholders(templateBody, parameterMatching, accumulatorPlaceholder, functionBody, inputArrayType.GetElementType(), outputType)  
                            let finalKernel = 
                                Expr.Lambda(tupleHolder,
                                    Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                        Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                            Expr.Let(blockHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                                Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 3),
                                                    newBody)))))
                    
                            // Setup kernel module and return
                            //let methodParams = signature.GetParameters()
                            
                            // Add applied function    
                            match computationFunction with
                            | Some(_, _, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                                let envVars, outVals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(functionBody)                            
                                let reduceFunctionInfo = new FunctionInfo(functionName, 
                                                                          functionInfo,
                                                                          //functionInfo.GetParameters() |> List.ofArray,
                                                                          functionParamVars,
                                                                          functionReturnType,
                                                                          envVars, outVals,
                                                                          functionBody)
                                                                          
                                let kInfo = new AcceleratedKernelInfo(kernelName, 
                                                                      methodInfo,
                                                                        [ inputHolder; outputHolder; blockHolder ],
                                                                        outputType.MakeArrayType(),
                                                                        envVars, outVals,
                                                                        finalKernel, 
                                                                        meta, 
                                                                        runtimeName, Some(reduceFunctionInfo :> IFunctionInfo), Some(functionBody))
                                let kernelModule = new KernelModule(None, None, kInfo)
                                        
                                // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                                kernelModule.Kernel.CustomInfo.Add("ReduceFunction", functionBody)
                                    
                                // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                                kernelModule.Functions.Add(reduceFunctionInfo.ID, reduceFunctionInfo)
                                kernelModule.Kernel.CalledFunctions.Add(reduceFunctionInfo.ID)
                                kernelModule
                            | _ ->
                                // Array.sum: reduce function in (+)
                                let kInfo = new AcceleratedKernelInfo(kernelName, 
                                                                      methodInfo,
                                                                        //[ methodParams.[0]; methodParams.[1]; methodParams.[2] ],
                                                                        [ inputHolder; outputHolder; blockHolder ],
                                                                        outputType.MakeArrayType(),
                                                                        new List<Var>(), new List<Expr>(),
                                                                        finalKernel, 
                                                                        meta, 
                                                                        runtimeName, None, None)
                                let kernelModule = new KernelModule(None, None, kInfo)                
                                kernelModule.Kernel.CustomInfo.Add("ReduceFunction", 
                                                                ExtractMethodInfo(<@ (+) @>).Value.GetGenericMethodDefinition().MakeGenericMethod([| inputArrayType.GetElementType(); inputArrayType.GetElementType(); outputType |]))
                                kernelModule  

                                   
                    // Create node
                    let node = new KFGKernelNode(kModule)
                    
                    // Parse arguments
                    let subnode =
                        if isArraySum then
                            step.Process(cleanArgs.[0], env, opts)
                        else
                            step.Process(cleanArgs.[1], env, opts)
                    node.InputNodes.Add(subnode)

                    Some(node :> IKFGNode)  
                else
                    None