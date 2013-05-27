namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Reflection.Emit

[<StepProcessor("FSCL_GENERIC_INSTANTIATION_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP", Dependencies = [| "FSCL_STRUCT_DISCOVERY_PROCESSOR" |])>]
type GenericInstantiator() =      
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
        
    interface ModulePreprocessingProcessor with
        member this.Process(m, en) =
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
            