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

type AcceleratedArraySortHandler() =
    let placeholderComp (a:int) =
        a

    let cpu_template = 
        <@
            fun(input: int[], 
                output: int[],
                wi: WorkItemInfo) ->
                    let i = wi.GlobalID(0)
                    let n = wi.GlobalSize(0)
                    let iKey = placeholderComp(input.[i])
                    // Compute position of in[i] in output
                    let mutable pos = 0
                    for j = 0 to n - 1 do
                        let jKey = placeholderComp(input.[j]) // broadcasted
                        let smaller = (jKey < iKey) || (jKey = iKey && j < i)  // in[j] < in[i] ?
                        pos <- if smaller then pos + 1 else pos + 0
                    output.[pos] <- input.[i]
        @>

    // NEW: Two-Stage reduction instead of multi-stage
    let gpu_template = 
        cpu_template
        (*
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
        *)
    let rec SubstitutePlaceholders(e:Expr, 
                                   parameters:Dictionary<Var, Var>, 
                                   returnType: Type,
                                   utilityFunction: Expr option,
                                   utilityFunctionInputType: Type,
                                   utilityFunctionReturnType: Type) =
        // Build a call expr
        let RebuildCall(o:Expr option, m: MethodInfo, args:Expr list) =
            if o.IsSome && (not m.IsStatic) then
                Expr.Call(o.Value, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)
            else
                Expr.Call(m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)  
            
        match e with
        | Patterns.Var(v) ->
            if parameters.ContainsKey(v) then
                Expr.Var(parameters.[v])
            else
                e
        | Patterns.Call(o, m, args) ->   
            // If this is the placeholder for the utility function (to be applied to each pari of elements)         
            if m.Name = "placeholderComp" then
                if utilityFunction.IsSome then
                    AcceleratedCollectionUtil.BuildApplication(utilityFunction.Value, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)
                else
                    // The key is the value
                    args.[0]
            // If this is an access to array (a parameter)
            else if m.DeclaringType.Name = "IntrinsicFunctions" then
                match args.[0] with
                | Patterns.Var(v) ->
                    if m.Name = "GetArray" then
                        // Find the placeholder holding the variable
                        if (parameters.ContainsKey(v)) then
                            // Recursively process the arguments, except the array reference
                            let arrayGet, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(parameters.[v].Type.GetElementType(), 1)
                            Expr.Call(arrayGet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType) ])
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
                                                SubstitutePlaceholders(args.[2], parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)
                            Expr.Call(arraySet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType); newValue ])
                                                           
                        else
                            RebuildCall(o, m, args)
                    else
                         RebuildCall(o, m,args)
                | _ ->
                    RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)                  
            // Otherwise process children and return the same call
            else
                RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType)) args)
        | Patterns.Let(v, value, body) ->
            // a and b are "special" vars that hold the params of the reduce function
            if v.Name = "a" then
                let newVarType = 
                    if utilityFunction.IsSome then
                        utilityFunctionInputType
                    else
                        failwith "Error"
                let a = Quotations.Var("a", newVarType, false)
                parameters.Add(v, a)
                Expr.Let(a, SubstitutePlaceholders(value, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType), 
                            SubstitutePlaceholders(body, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))
            else if v.Name = "b" then
                let newVarType = 
                    if utilityFunction.IsSome then
                        utilityFunctionInputType
                    else
                        failwith "Error"
                let b = Quotations.Var("b", newVarType, false)
                // Remember for successive references to a and b
                parameters.Add(v, b)
                Expr.Let(b, SubstitutePlaceholders(value, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType), SubstitutePlaceholders(body, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))        
            else
                Expr.Let(v, SubstitutePlaceholders(value, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType), SubstitutePlaceholders(body, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))
        
        | ExprShape.ShapeLambda(v, b) ->
            Expr.Lambda(v, SubstitutePlaceholders(b, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType))                    
        | ExprShape.ShapeCombination(o, l) ->
            match e with
            | Patterns.IfThenElse(cond, ifb, elseb) ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType) 
                    // Trick to adapt "0" in (sdata.[tid] <- if(i < n) then g_idata.[i] else 0) in case of other type of values (double: 0.0)
                    nl.Add(ne)
                ExprShape.RebuildShapeCombination(o, List.ofSeq(nl))
            | _ ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, returnType, utilityFunction, utilityFunctionInputType, utilityFunctionReturnType) 
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
            let isSort = methodInfo.Name = "Sort"
            let computationFunction, subExpr =                
                if isSort then
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
                if isSort || computationFunction.IsSome then
                
                    // Now create the kernel
                    // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                    let inputArrayType, outputArrayType =                     
                        match computationFunction with
                        |  Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                            functionParamVars.[0].Type.MakeArrayType(), functionReturnType.MakeArrayType()
                        | _ ->
                            (methodInfo.GetParameters().[0].ParameterType, methodInfo.GetParameters().[0].ParameterType)
                
                    // Check device target
                    let targetType = meta.KernelMeta.Get<DeviceTypeAttribute>()
            
                    // CPU AND GPU CODE (TODO: OPTIMIZE FOR THE TWO KINDS OF DEVICE)                                                      
                    let thisVar, ob, utilityFunction, functionReturnType, kernelName, runtimeName =    
                        match computationFunction with
                        |  Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                            (thisVar, ob, Some(functionBody), functionReturnType, "ArraySortBy_" + functionName, "Array.sortBy")
                        | _ ->
                            (None, None, None, methodInfo.GetParameters().[0].ParameterType, "ArraySort", "Array.sort")
                    
                    // Create parameters placeholders
                    let inputHolder = Quotations.Var("input_array", inputArrayType)
                    let outputHolder = Quotations.Var("output_array", outputArrayType)
                    let wiHolder = Quotations.Var("wi", typeof<WorkItemInfo>)
                    let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| wiHolder.Type; inputHolder.Type; outputHolder.Type |]))

                    // Finally, create the body of the kernel
                    let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromCollectionFunctionTemplate(cpu_template)   
                    let parameterMatching = new Dictionary<Var, Var>()
                    parameterMatching.Add(templateParameters.[0], inputHolder)
                    parameterMatching.Add(templateParameters.[1], outputHolder)
                    parameterMatching.Add(templateParameters.[2], wiHolder)

                    // Replace functions and references to parameters
                    let functionMatching = new Dictionary<string, MethodInfo>()
                    let newBody = SubstitutePlaceholders(templateBody, parameterMatching, outputArrayType, utilityFunction, functionReturnType, functionReturnType)  
                    let finalKernel = 
                        Expr.Lambda(tupleHolder,
                            Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                    Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                        newBody))))
                    
                   
                    // Add applied function      
                    let kernelModule =
                        match computationFunction with
                        | Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                            let envVars, outVals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(functionBody)
                            let sortByFunctionInfo = new FunctionInfo(functionName,
                                                                      functionInfo,
                                                                      functionParamVars,
                                                                      functionReturnType,
                                                                      envVars, outVals,
                                                                      functionBody)
                           
                            let kInfo = new AcceleratedKernelInfo(kernelName, 
                                                                  methodInfo,
                                                                    //[ methodParams.[0]; methodParams.[1]; methodParams.[2] ],
                                                                    [ inputHolder; outputHolder ],
                                                                    outputArrayType,
                                                                    envVars, outVals,
                                                                    finalKernel, 
                                                                    meta, 
                                                                    runtimeName, Some(sortByFunctionInfo :> IFunctionInfo), 
                                                                    Some(finalKernel))

                            let kernelModule = new KernelModule(thisVar, ob, kInfo)
                
                            // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                            kernelModule.Kernel.CustomInfo.Add("SortFunction", cleanArgs.[0])
                                    
                            // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                            kernelModule.Functions.Add(sortByFunctionInfo.ID, sortByFunctionInfo)
                            kernelModule.Kernel.CalledFunctions.Add(sortByFunctionInfo.ID)
                            kernelModule
                        | _ ->                            
                            let kInfo = new AcceleratedKernelInfo(kernelName, 
                                                                  methodInfo,
                                                                    //[ methodParams.[0]; methodParams.[1]; methodParams.[2] ],
                                                                    [ inputHolder; outputHolder ],
                                                                    outputArrayType,
                                                                    new List<Var>(), new List<Expr>(),
                                                                    finalKernel, 
                                                                    meta, 
                                                                    runtimeName, None, 
                                                                    None)

                            let kernelModule = new KernelModule(thisVar, ob, kInfo)
                            kernelModule
                                        
                    // Create node
                    let node = new KFGKernelNode(kernelModule)
                    
                    // Parse arguments
                    let subnode =
                        if isSort then
                            step.Process(cleanArgs.[0], env, opts)
                        else
                            step.Process(cleanArgs.[1], env, opts)
                    node.InputNodes.Add(subnode)

                    Some(node :> IKFGNode)  
                else
                    None
