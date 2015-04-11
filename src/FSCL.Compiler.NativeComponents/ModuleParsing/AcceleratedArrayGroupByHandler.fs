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

type AcceleratedArrayGroupByHandler() =
    let placeholderComp (a:int) =
        a

    let cpu_template = 
        <@
            fun(input: int[], 
                wi: WorkItemInfo) ->
                    let output = Array.zeroCreate<int * int> (input.Length)

                    let i = wi.GlobalID(0)
                    let n = wi.GlobalSize(0)
                    let iKey = placeholderComp(input.[i])
                    // Compute position of in[i] in output
                    let mutable pos = 0
                    for j = 0 to n - 1 do
                        let jKey = placeholderComp(input.[j]) // broadcasted
                        let smaller = (jKey < iKey) || (jKey = iKey && j < i)  // in[j] < in[i] ?
                        pos <- if smaller then pos + 1 else pos + 0
                    output.[pos] <- (iKey, input.[i])

                    output
        @>

    let gpu_template = 
        cpu_template

    let rec SubstitutePlaceholders(e:Expr, 
                                   parameters:Dictionary<Var, Var>,
                                   returnType: Type, 
                                   actualFunction: MethodInfo, 
                                   actualFunctionInstance: Expr option) =  
        // Build a call expr
        let RebuildCall(o:Expr option, m: MethodInfo, args:Expr list) =
            if o.IsSome && (not m.IsStatic) then
                Expr.Call(o.Value, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance)) args)
            else
                Expr.Call(m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance)) args)  
            
        match e with
        | Patterns.Var(v) ->
            if parameters.ContainsKey(v) then
                Expr.Var(parameters.[v])
            else
                e
        // Return expression
        | Patterns.Let(var, Patterns.Call(o, methodInfo, args), body) when
                    (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") ||
                    (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") ->    
            // Only zero create allocation is permitted and it must be assigned to a non mutable variable
            let na = args |> List.map(fun a -> SubstitutePlaceholders(a, parameters, returnType, actualFunction, actualFunctionInstance))
            let nv = Quotations.Var(var.Name, returnType)
            parameters.Add(var, nv)
            let nb = SubstitutePlaceholders(body, parameters, returnType, actualFunction, actualFunctionInstance)
            Expr.Let(nv, Expr.Call(ZeroCreateMethod(returnType.GetElementType()), na), nb)                    
        | Patterns.Call(o, m, args) ->   
            // If this is the placeholder for the utility function (to be applied to each pari of elements)         
            if m.Name = "placeholderComp" then
                RebuildCall(actualFunctionInstance, actualFunction, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance)) args)
            // If this is an access to array (a parameter)
            else if m.DeclaringType.Name = "IntrinsicFunctions" then
                match args.[0] with
                | Patterns.Var(v) ->
                    if m.Name = "GetArray" then
                        // Find the placeholder holding the variable
                        if (parameters.ContainsKey(v)) then
                            // Recursively process the arguments, except the array reference
                            let arrayGet, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(parameters.[v].Type.GetElementType())
                            Expr.Call(arrayGet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, returnType, actualFunction, actualFunctionInstance) ])
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
                                                let outputParameterType = actualFunction.GetParameters().[1].ParameterType
                                                // Conversion method (ToDouble, ToSingle, ToInt, ...)
                                                Expr.Value(Activator.CreateInstance(outputParameterType), outputParameterType)
                                            | _ ->
                                                SubstitutePlaceholders(args.[2], parameters, returnType, actualFunction, actualFunctionInstance)
                            Expr.Call(arraySet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, returnType, actualFunction, actualFunctionInstance); newValue ])
                                                           
                        else
                            RebuildCall(o, m, args)
                    else
                         RebuildCall(o, m,args)
                | _ ->
                    RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance)) args)                  
            // Otherwise process children and return the same call
            else
                RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance)) args)
        | Patterns.Let(v, value, body) ->
            // a and b are "special" vars that hold the params of the reduce function
            if v.Name = "a" then
                let newVarType = 
                    actualFunction.GetParameters().[0].ParameterType
                let a = Quotations.Var("a", newVarType, false)
                parameters.Add(v, a)
                Expr.Let(a, SubstitutePlaceholders(value, parameters, returnType, actualFunction, actualFunctionInstance), 
                            SubstitutePlaceholders(body, parameters, returnType, actualFunction, actualFunctionInstance))
            else if v.Name = "b" then
                let newVarType = 
                    actualFunction.GetParameters().[0].ParameterType
                let b = Quotations.Var("b", newVarType, false)
                // Remember for successive references to a and b
                parameters.Add(v, b)
                Expr.Let(b, SubstitutePlaceholders(value, parameters, returnType, actualFunction, actualFunctionInstance), SubstitutePlaceholders(body, parameters, returnType, actualFunction, actualFunctionInstance))        
            else
                Expr.Let(v, SubstitutePlaceholders(value, parameters, returnType, actualFunction, actualFunctionInstance), SubstitutePlaceholders(body, parameters, returnType, actualFunction, actualFunctionInstance))
        
        | ExprShape.ShapeLambda(v, b) ->
            Expr.Lambda(v, SubstitutePlaceholders(b, parameters, returnType, actualFunction, actualFunctionInstance))                    
        | ExprShape.ShapeCombination(o, l) ->
            match e with
            | Patterns.IfThenElse(cond, ifb, elseb) ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance) 
                    // Trick to adapt "0" in (sdata.[tid] <- if(i < n) then g_idata.[i] else 0) in case of other type of values (double: 0.0)
                    nl.Add(ne)
                ExprShape.RebuildShapeCombination(o, List.ofSeq(nl))
            | Patterns.NewTuple(args) ->
                let nl = args |> List.map(fun e -> SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance)) 
                Expr.NewTuple(nl)
            | _ ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, returnType, actualFunction, actualFunctionInstance) 
                    nl.Add(ne)
                ExprShape.RebuildShapeCombination(o, List.ofSeq(nl))
        | _ ->
            e
            
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, cleanArgs, root, meta, step, env) =  
            // Inspect operator
            let computationFunction, subExpr, isLambda =                
                AcceleratedCollectionUtil.ParseOperatorLambda(cleanArgs.[0], step, env)
                                
            match subExpr with
            | Some(kfg, newEnv) ->
                // This coll fun is a composition 
                let node = new KFGCollectionCompositionNode(methodInfo, kfg, newEnv)
                
                // Create data node for outsiders
//                for o in outsiders do 
//                    node.InputNodes.Add(new KFGOutsiderDataNode(o))

                // Parse arguments
                let subnode = step.Process(cleanArgs.[1], env)
                node.InputNodes.Add(subnode)
                Some(node :> IKFGNode)   
            | _ ->
                // This coll fun is a kernel
                match computationFunction with
                | Some(thisVar, ob, functionInfo, functionParamVars, functionBody) ->
                              
                    // Create on-the-fly module to host the kernel                
                    // The dynamic module that hosts the generated kernels
                    let assemblyName = IDGenerator.GenerateUniqueID("FSCL.Compiler.Plugins.AcceleratedCollections.AcceleratedArray");
                    let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
                    let moduleBuilder = assemblyBuilder.DefineDynamicModule("AcceleratedArrayModule");

                    // Now create the kernel
                    // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                    let inputArrayType, outputArrayType =                     
                        match computationFunction with
                        | Some(thisVar, ob, functionInfo, functionParamVars, body) ->
                            (Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType(),  
                             Array.CreateInstance(FSharpType.MakeTupleType([| functionInfo.ReturnType; functionInfo.GetParameters().[0].ParameterType |]), 0).GetType())
                        | _ ->
                            failwith "Error in extracting computation function for accelerated collection function Array.groupBy"
                
                    // Check device target
                    let targetType = meta.KernelMeta.Get<DeviceTypeAttribute>()
            
                    // CPU AND GPU CODE (TODO: OPTIMIZE FOR THE TWO KINDS OF DEVICE)                                                      
                    let signature, name, appliedFunctionBody =    
                        match computationFunction with
                        | Some(thisVar, ob, functionInfo, functionParamVars, body) ->
                            (DynamicMethod("ArrayGroupBy_" + functionInfo.Name, outputArrayType, [| inputArrayType; typeof<WorkItemInfo> |]), "Array.groupBy", Some(body))
                        | _ ->
                            failwith "Error in extracting computation function for accelerated collection function Array.groupBy"
                                  
                    // Now we can create the signature and define parameter name in the dynamic module                                        
                    signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                    signature.DefineParameter(2, ParameterAttributes.In, "wi") |> ignore
                    
                    // Create parameters placeholders
                    let inputHolder = Quotations.Var("input_array", inputArrayType)
                    let outputHolder = Quotations.Var("output_array", outputArrayType)
                    let wiHolder = Quotations.Var("wi", typeof<WorkItemInfo>)
                    let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputHolder.Type; wiHolder.Type |]))

                    // Finally, create the body of the kernel
                    let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromCollectionFunctionTemplate(cpu_template)   
                    let parameterMatching = new Dictionary<Var, Var>()
                    parameterMatching.Add(templateParameters.[0], inputHolder)
                    parameterMatching.Add(templateParameters.[1], wiHolder)

                    // Replace functions and references to parameters
                    let functionMatching = new Dictionary<string, MethodInfo>()
                    let fInfo, thisObj = 
                        match computationFunction with
                        | Some(thisVar, ob, functionInfo, functionParamVars, body) ->
                            Some(functionInfo), ob
                        | _ ->
                            None, None
                    let newBody = SubstitutePlaceholders(templateBody, parameterMatching, outputArrayType, fInfo.Value, thisObj)  
                    let finalKernel = 
                        Expr.Lambda(tupleHolder,
                            Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                    newBody)))
                    
                    // Setup kernel module and return
                    let methodParams = signature.GetParameters()
                    let envVars, outVals = 
                        QuotationAnalysis.KernelParsing.ExtractEnvRefs(functionBody)
                        
                    // Add applied function      
                    let sortByFunctionInfo = new FunctionInfo(functionInfo, 
                                                                functionInfo.GetParameters() |> List.ofArray,
                                                                functionParamVars,
                                                                envVars,
                                                                outVals,
                                                                None,
                                                                functionBody, isLambda)
                    
                    let kInfo = new AcceleratedKernelInfo(signature, 
                                                            [ methodParams.[0] ],
                                                            [ inputHolder ],
                                                            envVars,
                                                            outVals,
                                                            finalKernel, 
                                                            meta, 
                                                            name, Some(sortByFunctionInfo :> IFunctionInfo), appliedFunctionBody)
                    let kernelModule = new KernelModule(None, None, kInfo)
                
                    // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                    kernelModule.Functions.Add(sortByFunctionInfo.ID, sortByFunctionInfo)
                    kernelModule.Kernel.CalledFunctions.Add(sortByFunctionInfo.ID)
                                        
                    // Create node
                    let node = new KFGKernelNode(kernelModule)
                    
                    // Create data node for outsiders
//                    for o in outsiders do 
//                        node.InputNodes.Add(new KFGOutsiderDataNode(o))

                    // Parse arguments
                    let subnode =
                        step.Process(cleanArgs.[1], env)
                    node.InputNodes.Add(subnode)

                    Some(node :> IKFGNode)  
                | _ ->
                    None
