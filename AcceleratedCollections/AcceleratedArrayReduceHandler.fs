namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System.Collections.Generic
open System
open FSCL.Compiler.Core.Util
open Microsoft.FSharp.Reflection
open AcceleratedCollectionUtil
open FSCL.Compiler.Tools
open System.Runtime.InteropServices
open Microsoft.FSharp.Linq.RuntimeHelpers

type AcceleratedArrayReduceHandler() =
    let placeholderComp (a:int) (b:int) =
        a + b

    // NEW: Two-Stage reduction instead of multi-stage
    let template = 
        <@
            fun(g_idata:int[], [<Local>]sdata:int[], g_odata:int[]) ->
                let mutable global_index = get_global_id(0)

                let mutable accumulator = 0
                while (global_index < g_idata.Length) do
                    accumulator <- placeholderComp accumulator g_idata.[global_index]
                    global_index <- global_index + get_global_size(0)

                let local_index = get_local_id(0)
                sdata.[local_index] <- accumulator
                barrier(CLK_LOCAL_MEM_FENCE)

                let mutable offset = get_local_size(0) / 2
                while(offset > 0) do
                    if(local_index < offset) then
                        sdata.[local_index] <- placeholderComp (sdata.[local_index]) (sdata.[local_index + offset])
                    offset <- offset / 2
                    barrier(CLK_LOCAL_MEM_FENCE)
                
                if local_index = 0 then
                    g_odata.[get_group_id(0)] <- sdata.[0]
                (*
                let tid = get_local_id(0)
                let i = get_group_id(0) * (get_local_size(0) * 2) + get_local_id(0)

                if(i < g_idata.Length) then 
                    sdata.[tid] <- g_idata.[i] 
                else 
                    sdata.[tid] <- 0
                if (i + get_local_size(0) < g_idata.Length) then 
                    sdata.[tid] <- placeholderComp (sdata.[tid]) (g_idata.[i + get_local_size(0)])

                barrier(CLK_LOCAL_MEM_FENCE)
                // do reduction in shared mem
                let mutable s = get_local_size(0) >>> 1
                while (s > 0) do 
                    if (tid < s) then
                        sdata.[tid] <- placeholderComp (sdata.[tid]) (sdata.[tid + s])
                    barrier(CLK_LOCAL_MEM_FENCE)
                    s <- s >>> 1

                if (tid = 0) then 
                    g_odata.[get_group_id(0)] <- sdata.[0]
                *)
        @>
             
    let rec SubstitutePlaceholders(e:Expr, parameters:Dictionary<Var, Var>, accumulatorPlaceholder:Var, actualFunction: MethodInfo) =  
        // Build a call expr
        let RebuildCall(o:Expr option, m: MethodInfo, args:Expr list) =
            if o.IsSome && (not m.IsStatic) then
                Expr.Call(o.Value, m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)
            else
                Expr.Call(m, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)  
            
        match e with
        | Patterns.Var(v) ->       
            // Substitute parameter with the new one (of the correct type)
            if v.Name = "accumulator" then
                Expr.Var(accumulatorPlaceholder)
            else if parameters.ContainsKey(v) then
                Expr.Var(parameters.[v])
            else
                e
        | Patterns.Call(o, m, args) ->   
            // If this is the placeholder for the utility function (to be applied to each pari of elements)         
            if m.Name = "placeholderComp" then
                RebuildCall(o, actualFunction, List.map(fun (e:Expr) -> SubstitutePlaceholders(e, parameters, accumulatorPlaceholder, actualFunction)) args)
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
                                                let outputParameterType = actualFunction.GetParameters().[1].ParameterType
                                                // Conversion method (ToDouble, ToSingle, ToInt, ...)
                                                Expr.Value(Activator.CreateInstance(outputParameterType), outputParameterType)
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
                Expr.Let(accumulatorPlaceholder, Expr.Coerce(value, accumulatorPlaceholder.Type), SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, actualFunction))
            // a and b are "special" vars that hold the params of the reduce function
            else if v.Name = "a" then
                let a = Quotations.Var("a", actualFunction.GetParameters().[0].ParameterType, false)
                parameters.Add(v, a)
                Expr.Let(a, SubstitutePlaceholders(value, parameters, accumulatorPlaceholder, actualFunction), 
                            SubstitutePlaceholders(body, parameters, accumulatorPlaceholder, actualFunction))            
            else if v.Name = "b" then
                let b = Quotations.Var("b", actualFunction.GetParameters().[1].ParameterType, false)
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

    let kernelName (prefix: string, parameterTypes: Type list, utilityFunction: string) =
        String.concat "_" ([prefix] @ (List.map (fun (t:Type) -> t.Name.Replace(".", "")) parameterTypes) @ [utilityFunction])

    member this.EvaluateAndApply(e:Expr) (a:obj) (b:obj) =
        let f = LeafExpressionConverter.EvaluateQuotation(e)
        let fm = f.GetType().GetMethod("Invoke")
        let r1 = fm.Invoke(f, [| a |])
        let r2m = r1.GetType().GetMethod("Invoke")
        let r2 = r2m.Invoke(r1, [| b |])
        r2

    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, args, root, step) =            
            let kernelModule = new KernelModule()
            (*
                Array map looks like: Array.map fun collection
                At first we check if fun is a lambda (first argument)
                and in this case we transform it into a method
                Secondly, we iterate parsing on the second argument (collection)
                since it might be a subkernel
            *)
            let lambda = GetLambdaArgument(args.[0], root)
            let mutable isLambda = false
            let computationFunction =                
                match lambda with
                | Some(l) ->
                    isLambda <- true
                    QuotationAnalysis.LambdaToMethod(l)
                | None ->
                    AcceleratedCollectionUtil.FilterCall(args.[0], 
                        fun (e, mi, a) ->                         
                            match mi with
                            | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                                (mi, body)
                            | _ ->
                                failwith ("Cannot parse the body of the computation function " + mi.Name))
            // Merge with the eventual subkernel
            let subkernel =
                try
                    step.Process(args.[1])
                with
                    :? CompilerException -> null
            if subkernel <> null then
                kernelModule.MergeWith(subkernel)
                
            // Extract the reduce function 
            match computationFunction with
            | Some(functionInfo, body) ->
                // Create on-the-fly module to host the kernel                
                // The dynamic module that hosts the generated kernels
                let assemblyName = IDGenerator.GenerateUniqueID("FSCL.Compiler.Plugins.AcceleratedCollections.AcceleratedArray");
                let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
                let moduleBuilder = assemblyBuilder.DefineDynamicModule("AcceleratedArrayModule");

                // Now create the kernel
                // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                let inputArrayType = Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType()
                let outputArrayType = Array.CreateInstance(functionInfo.ReturnType, 0).GetType()
                // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output                    
                let inputArrayPlaceholder = Quotations.Var("input_array", inputArrayType)
                let outputArrayPlaceholder = Quotations.Var("output_array", outputArrayType)
                let localArrayPlaceholder = Quotations.Var("local_array", outputArrayType)
                let accumulatorPlaceholder = Quotations.Var("accumulator", outputArrayType.GetElementType())
                    
                // Now we can create the signature and define parameter name in the dynamic module
                // DynamicMethod would be simpler and would not require a dynamic module but unfortunately it doesn't support
                // Custom attributes for ites parameters. We instead have to mark the second parameter of the kernel with [Local]
                let methodBuilder = moduleBuilder.DefineGlobalMethod(
                                        kernelName("ArrayReduce", 
                                                    [functionInfo.GetParameters().[0].ParameterType; 
                                                    functionInfo.GetParameters().[1].ParameterType],
                                                    functionInfo.Name), 
                                        MethodAttributes.Public ||| MethodAttributes.Static, typeof<unit>, 
                                        [| inputArrayType; outputArrayType; outputArrayType |])
                let attributeBuilder = new CustomAttributeBuilder(typeof<LocalAttribute>.GetConstructors().[0], [||])
                let paramBuilder = methodBuilder.DefineParameter(1, ParameterAttributes.In, "input_array")
                let paramBuilder = methodBuilder.DefineParameter(2, ParameterAttributes.In, "local_array")
                paramBuilder.SetCustomAttribute(attributeBuilder)
                let paramBuilder = methodBuilder.DefineParameter(3, ParameterAttributes.In, "output_array")
                // Body (simple return) of the method must be set to build the module and get the MethodInfo that we need as signature
                methodBuilder.GetILGenerator().Emit(OpCodes.Ret)
                moduleBuilder.CreateGlobalFunctions()
                    
                // Finally, create the body of the kernel
                let templateBody, templateParameters = AcceleratedCollectionUtil.GetKernelFromLambda(template)   
                let parameterMatching = new Dictionary<Var, Var>()
                parameterMatching.Add(templateParameters.[0], inputArrayPlaceholder)
                parameterMatching.Add(templateParameters.[1], localArrayPlaceholder)
                parameterMatching.Add(templateParameters.[2], outputArrayPlaceholder)

                // Replace functions and references to parameters
                let functionMatching = new Dictionary<string, MethodInfo>()
                let newBody = SubstitutePlaceholders(templateBody, parameterMatching, accumulatorPlaceholder, functionInfo)  
                    
                // Setup kernel module and return  
                let signature = moduleBuilder.GetMethod(methodBuilder.Name) 
                let kInfo = new AcceleratedKernelInfo(signature, newBody, "Array.reduce", body)
                kInfo.CustomInfo.Add("IS_ACCELERATED_COLLECTION_KERNEL", true)
                
                kernelModule.AddKernel(kInfo) 
                // Update call graph
                kernelModule.FlowGraph <- FlowGraphNode(kInfo.ID)
                                    
                // Add the computation function and connect it to the kernel
                let reduceFunctionInfo = new FunctionInfo(functionInfo, body, isLambda)
                
                // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                if isLambda then
                    kInfo.CustomInfo.Add("ReduceFunction", lambda.Value)
                else
                    kInfo.CustomInfo.Add("ReduceFunction", fst computationFunction.Value)
                                    
                // Store the called function (runtime execution will use it to perform latest iterations of reduction)
                kernelModule.AddFunction(reduceFunctionInfo)
                kernelModule.GetKernel(kInfo.ID).RequiredFunctions.Add(reduceFunctionInfo.ID) |> ignore
                // Define arguments for call graph
                let argExpressions = new Dictionary<string, Expr>()
                if subkernel <> null then       
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array",
                                                  KernelOutput(subkernel.FlowGraph, 0))                                
                else
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array",
                                                  ActualArgument(args.[1]))        
                // Set local array input value                                   
                FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                             "local_array",
                                             ImplicitValue)            
                
                // Return module                             
                Some(kernelModule)
            | _ ->
                None