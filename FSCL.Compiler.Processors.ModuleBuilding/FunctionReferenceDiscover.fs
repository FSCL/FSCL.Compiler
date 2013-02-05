namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_FUNCTIONS_DISCOVERY_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP",
                [| "FSCL_GENERIC_INSTANTIATION_PROCESSOR" |])>] 
type FunctionReferenceDiscover() =      
    let InstantiateGenericKernel(mi:MethodInfo, tm:TypeManager) =
        let mutable kernelInstances = [ ]
        let mutable methodInfo = mi
        if mi.IsGenericMethod then
            methodInfo <- mi.GetGenericMethodDefinition()
            // Instantiate kernel for each combination of generic parameters
            let genMi = mi.GetGenericMethodDefinition()
            let types = genMi.GetGenericArguments()
            let combinations = CombinationGenerator.Generator.getPerms (types.Length) (tm.ManagedGenericInstances)
            for combination in combinations do                   
                kernelInstances <- kernelInstances @ [ genMi.MakeGenericMethod(Array.ofSeq combination) ] 
        else           
            kernelInstances <- [ mi ] 
        (methodInfo, kernelInstances)
        
    let DiscoverFunctionRef(k:KernelInfo) =
        let foundFunctions = Dictionary<MethodInfo, FunctionInfo>()

        let rec DiscoverFunctionRefInner(expr) =
            match expr with
            | Patterns.Call(o, mi, args) ->
                List.iter (fun el -> DiscoverFunctionRefInner(el)) args
                match mi with
                | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                    if not (foundFunctions.ContainsKey(mi)) then
                        foundFunctions.Add(mi, new FunctionInfo(mi, b))
                | _ ->
                    ()
            | ExprShape.ShapeLambda(v, a) ->
                DiscoverFunctionRefInner(a)
            | ExprShape.ShapeCombination(o, list) ->
                List.iter (fun el -> DiscoverFunctionRefInner(el)) list
            | _ ->
                ()

        DiscoverFunctionRefInner(k.Body)

    interface ModulePreprocessingProcessor with
        member this.Process(m, en) =
            let engine = en :?> ModulePreprocessingStep
            for k in m.Kernels do
                DiscoverFunctionRef(k)
            