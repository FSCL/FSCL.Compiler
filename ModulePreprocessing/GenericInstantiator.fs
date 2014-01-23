namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Reflection.Emit
(*
[<StepProcessor("FSCL_GENERIC_INSTANTIATION_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP", Dependencies = [| "FSCL_STRUCT_DISCOVERY_PROCESSOR" |])>]
type GenericInstantiator() =      
    inherit ModulePreprocessingProcessor()

    let InstantiateGenericKernel(mi:MethodInfo, tm:TypeManager) =
        let mutable kernelInstances = [ ]
        let mutable methodInfo = mi
        if mi.IsGenericMethod then
            methodInfo <- mi.GetGenericMethodDefinition()
            // Instantiate kernel for each combination of generic parameters
            let genMi = mi.GetGenericMethodDefinition()
            let types = genMi.GetGenericArguments()
            let combinations = CombinationGenerator.Generator.getPerms (types.Length) tm.ManagedGenericInstances
            for combination in combinations do         
                // Create instantiated method   
                let m = genMi.MakeGenericMethod(Array.ofSeq combination)   
                // Rename instance based on actual types
                let newName = String.concat "_" ([m.Name] @ List.map(fun (t: Type) -> t.Name.ToString()) combination)
                // We do not create dynamic method now, cause otherwise checking the body throws an exception. We simply return the new name to be replaced by the caller
                kernelInstances <- kernelInstances @ [ (m, newName) ] 
        else           
            kernelInstances <- [ (mi, "") ] 
        (methodInfo, kernelInstances)
        
    override this.Run(m, en) =
        let engine = en :?> ModulePreprocessingStep
        if(m.Source.Signature.IsGenericMethodDefinition) then  
            let (genericKernel, instances) = InstantiateGenericKernel(m.Source.Signature, engine.TypeManager)
            for (instance, newName) in instances do    
                match instance with
                | DerivedPatterns.MethodWithReflectedDefinition(b) ->  
                    // If a new name is set then the method was generic and instances must be renamed
                    if (newName <> "") then
                        let newSignature = new DynamicMethod(newName, instance.ReturnType, Array.map (fun (p:ParameterInfo) -> p.ParameterType) (instance.GetParameters()))
                        // Define parameters
                        Array.iteri (fun i (p:ParameterInfo) ->
                            newSignature.DefineParameter(i + 1, p.Attributes, p.Name) |> ignore) (instance.GetParameters())  
                        // Add new element to the instances
                        let paramVarList = new List<Var * ParameterInfo>() 
                        let kInfo = new KernelInfo(newSignature, b)
                        m.Kernels <- m.Kernels @ [ kInfo ]
                    else
                        // Add new element to the instances
                        let paramVarList = new List<Var * ParameterInfo>() 
                        let kInfo = new KernelInfo(instance, b)
                        m.Kernels <- m.Kernels @ [ kInfo ]
                | _ ->
                    ()
        else            
            let kInfo = new KernelInfo(m.Source.Signature, m.Source.Body)
            m.Kernels <- m.Kernels @ [ kInfo ]
            *)

[<StepProcessor("FSCL_GENERIC_INSTANTIATION_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP", Dependencies = [| "FSCL_STRUCT_DISCOVERY_PROCESSOR" |])>]
type GenericInstantiator() =      
    inherit ModulePreprocessingProcessor()

    override this.Run(m, en) =
        let engine = en :?> ModulePreprocessingStep
        // Populate the module call graph with kernels
        for k in m.Source.Kernels do
            m.CallGraph.AddKernel(k)
        for k in m.Source.Functions do
            m.CallGraph.AddFunction(k)
        // Set connections
        for k in m.Source.KernelIDs do
            for connSet in m.Source.GetOutputConnections(k) do
                for conn in connSet.Value do
                    m.CallGraph.AddConnection(k, connSet.Key, conn.Key, conn.Value)
            for connSet in m.Source.GetOutputCalls(k) do
                m.CallGraph.AddCall(k, connSet.Key)
        for k in m.Source.FunctionIDs do
            for connSet in m.Source.GetOutputCalls(k) do
                m.CallGraph.AddCall(k, connSet.Key)
        