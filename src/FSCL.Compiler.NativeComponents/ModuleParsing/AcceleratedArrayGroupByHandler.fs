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
                output: (int * int)[],
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
                    output.[pos] <- (iKey, input.[i])
        @>

    let gpu_template = 
        cpu_template

    let rec SubstitutePlaceholders(e:Expr, parameters:Dictionary<Var, Var>, actualFunction: MethodInfo, actualFunctionInstance: Expr option) =  
        // Build a call expr
        let RebuildCall(o:Expr option, m: MethodInfo, args:Expr list) =
            if o.IsSome && (not m.IsStatic) then
                Expr.Call(o.Value, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance)) args)
            else
                Expr.Call(m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance)) args)  
            
        match e with
        | Patterns.Var(v) ->
            if parameters.ContainsKey(v) then
                Expr.Var(parameters.[v])
            else
                e
        | Patterns.Call(o, m, args) ->   
            // If this is the placeholder for the utility function (to be applied to each pari of elements)         
            if m.Name = "placeholderComp" then
                RebuildCall(actualFunctionInstance, actualFunction, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance)) args)
            // If this is an access to array (a parameter)
            else if m.DeclaringType.Name = "IntrinsicFunctions" then
                match args.[0] with
                | Patterns.Var(v) ->
                    if m.Name = "GetArray" then
                        // Find the placeholder holding the variable
                        if (parameters.ContainsKey(v)) then
                            // Recursively process the arguments, except the array reference
                            let arrayGet, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(parameters.[v].Type.GetElementType())
                            Expr.Call(arrayGet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, actualFunction, actualFunctionInstance) ])
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
                                                SubstitutePlaceholders(args.[2], parameters, actualFunction, actualFunctionInstance)
                            Expr.Call(arraySet, [ Expr.Var(parameters.[v]); SubstitutePlaceholders(args.[1], parameters, actualFunction, actualFunctionInstance); newValue ])
                                                           
                        else
                            RebuildCall(o, m, args)
                    else
                         RebuildCall(o, m,args)
                | _ ->
                    RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance)) args)                  
            // Otherwise process children and return the same call
            else
                RebuildCall(o, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance)) args)
        | Patterns.Let(v, value, body) ->
            // a and b are "special" vars that hold the params of the reduce function
            if v.Name = "a" then
                let newVarType = 
                    actualFunction.GetParameters().[0].ParameterType
                let a = Quotations.Var("a", newVarType, false)
                parameters.Add(v, a)
                Expr.Let(a, SubstitutePlaceholders(value, parameters, actualFunction, actualFunctionInstance), 
                            SubstitutePlaceholders(body, parameters, actualFunction, actualFunctionInstance))
            else if v.Name = "b" then
                let newVarType = 
                    actualFunction.GetParameters().[0].ParameterType
                let b = Quotations.Var("b", newVarType, false)
                // Remember for successive references to a and b
                parameters.Add(v, b)
                Expr.Let(b, SubstitutePlaceholders(value, parameters, actualFunction, actualFunctionInstance), SubstitutePlaceholders(body, parameters, actualFunction, actualFunctionInstance))        
            else
                Expr.Let(v, SubstitutePlaceholders(value, parameters, actualFunction, actualFunctionInstance), SubstitutePlaceholders(body, parameters, actualFunction, actualFunctionInstance))
        
        | ExprShape.ShapeLambda(v, b) ->
            Expr.Lambda(v, SubstitutePlaceholders(b, parameters, actualFunction, actualFunctionInstance))                    
        | ExprShape.ShapeCombination(o, l) ->
            match e with
            | Patterns.IfThenElse(cond, ifb, elseb) ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance) 
                    // Trick to adapt "0" in (sdata.[tid] <- if(i < n) then g_idata.[i] else 0) in case of other type of values (double: 0.0)
                    nl.Add(ne)
                ExprShape.RebuildShapeCombination(o, List.ofSeq(nl))
            | Patterns.NewTuple(args) ->
                let nl = args |> List.map(fun e -> SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance)) 
                Expr.NewTuple(nl)
            | _ ->
                let nl = new List<Expr>();
                for e in l do 
                    let ne = SubstitutePlaceholders(e, parameters, actualFunction, actualFunctionInstance) 
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
                | Some(_, _, functionInfo, functionParamVars, functionBody) ->
                              
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
            
                    let kModule = 
                        // CPU AND GPU CODE (TODO: OPTIMIZE FOR THE TWO KINDS OF DEVICE)                                                      
                        let signature, name, appliedFunctionBody =    
                            match computationFunction with
                            | Some(thisVar, ob, functionInfo, functionParamVars, body) ->
                                (DynamicMethod("ArrayGroupBy_" + functionInfo.Name, outputArrayType, [| inputArrayType; outputArrayType; outputArrayType; typeof<WorkItemInfo> |]), "Array.reduce", Some(body))
                            | _ ->
                                failwith "Error in extracting computation function for accelerated collection function Array.groupBy"
                                  
                        // Now we can create the signature and define parameter name in the dynamic module                                        
                        signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                        signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                        signature.DefineParameter(3, ParameterAttributes.In, "wi") |> ignore
                    
                        // Create parameters placeholders
                        let inputHolder = Quotations.Var("input_array", inputArrayType)
                        let outputHolder = Quotations.Var("output_array", outputArrayType)
                        let wiHolder = Quotations.Var("wi", typeof<WorkItemInfo>)
                        let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputHolder.Type; outputHolder.Type; wiHolder.Type |]))

                        // Finally, create the body of the kernel
                        let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromCollectionFunctionTemplate(cpu_template)   
                        let parameterMatching = new Dictionary<Var, Var>()
                        parameterMatching.Add(templateParameters.[0], inputHolder)
                        parameterMatching.Add(templateParameters.[1], outputHolder)
                        parameterMatching.Add(templateParameters.[2], wiHolder)

                        // Replace functions and references to parameters
                        let functionMatching = new Dictionary<string, MethodInfo>()
                        let fInfo, thisObj = 
                            match computationFunction with
                            | Some(thisVar, ob, functionInfo, functionParamVars, body) ->
                                Some(functionInfo), ob
                            | _ ->
                                None, None
                        let newBody = SubstitutePlaceholders(templateBody, parameterMatching, fInfo.Value, thisObj)  
                        let finalKernel = 
                            Expr.Lambda(tupleHolder,
                                Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                    Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                        Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                            newBody))))
                    
                        // Setup kernel module and return
                        let methodParams = signature.GetParameters()
                        let funBody, envVars, outVals = 
                            QuotationAnalysis.KernelParsing.ReplaceEnvRefsWithParamRefs(functionBody)
                        let kInfo = new AcceleratedKernelInfo(signature, 
                                                                [ methodParams.[0]; methodParams.[1] ],
                                                                [ inputHolder; outputHolder ],
                                                                envVars,
                                                                outVals,
                                                                finalKernel, 
                                                                meta, 
                                                                name, appliedFunctionBody)
                        let kernelModule = new KernelModule(None, None, kInfo)
                        
                        kernelModule 

                    // Add applied function      
                    match computationFunction with
                    | Some(thisVar, ob, functionInfo, functionParamVars, body) ->
                        let sortByFunctionInfo = new FunctionInfo(functionInfo, 
                                                                  functionInfo.GetParameters() |> List.ofArray,
                                                                  functionParamVars,
                                                                  kModule.Kernel.EnvVarsUsed,
                                                                  kModule.Kernel.OutValsUsed,
                                                                  None,
                                                                  body, isLambda)
                
                        // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                        if isLambda then
                            kModule.Kernel.CustomInfo.Add("GroupByFunction", isLambda)
                        else
                            // ExtractComputationFunction may have lifted some paramters that are referencing stuff outside the quotation, so 
                            // a new methodinfo is generated with no body. So we can't invoke it, and therefore we add as ReduceFunction the body instead of the methodinfo
                            kModule.Kernel.CustomInfo.Add("GroupByFunction", body)
                                    
                        // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                        kModule.Functions.Add(sortByFunctionInfo.ID, sortByFunctionInfo)
                        kModule.Kernel.CalledFunctions.Add(sortByFunctionInfo.ID)
                    | _ ->
                        ()
                    
                    // Create node
                    let node = new KFGKernelNode(kModule)
                    
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
