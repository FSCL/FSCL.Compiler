namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Reflection.Emit

[<StepProcessor("FSCL_GENERIC_INSTANTIATION_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP")>]
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
        
    let rec LiftArgExtraction (expr, parameters: ParameterInfo[]) =
        match expr with
        | Patterns.Lambda(v, e) ->
            if v.Name = "tupledArg" then
                LiftArgExtraction(e, parameters)
            else
                let el = Array.tryFind (fun (p:ParameterInfo) -> p.Name = v.Name) parameters
                if el.IsSome then
                    LiftArgExtraction (e, parameters)
                else
                    expr
        | Patterns.Let(v, value, body) ->
            let el = Array.tryFind (fun (p:ParameterInfo) -> p.Name = v.Name) parameters
            if el.IsSome then
                LiftArgExtraction (body, parameters)
            else
                expr
        | _ ->
            expr

    interface ModulePreprocessingProcessor with
        member this.Process(m, en) =
            let engine = en :?> ModulePreprocessingStep
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
                        let cleanBody = LiftArgExtraction(b, newSignature.GetParameters())
                        let kInfo = new KernelInfo(newSignature, cleanBody)
                        m.Kernels <- m.Kernels @ [ kInfo ]
                    else
                        // Add new element to the instances
                        let paramVarList = new List<Var * ParameterInfo>() 
                        let cleanBody = LiftArgExtraction(b, instance.GetParameters())
                        let kInfo = new KernelInfo(instance, cleanBody)
                        m.Kernels <- m.Kernels @ [ kInfo ]
                | _ ->
                    ()
            