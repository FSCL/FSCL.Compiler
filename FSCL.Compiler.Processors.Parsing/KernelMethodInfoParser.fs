namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type KernelMethodInfoParser() =      
   (* let InstantiateGenericKernel(mi:MethodInfo, tm: TypeManager) =
        let kernelInstances = seq {
            if not (mi.IsGenericMethod) then
                yield mi
            else
                // Instantiate kernel for each combination of generic parameters
                let genMi = mi.GetGenericMethodDefinition()
                let types = genMi.GetGenericArguments()
                let combinations = CombinationGenerator.Generator.getPerms (types.Length) (tm.ManagedTypes)
                for combination in combinations do                   
                    yield genMi.MakeGenericMethod(Array.ofSeq combination)
            }
        ((if not (mi.IsGenericMethod) then mi else mi.GetGenericMethodDefinition()), kernelInstances)
        *)

    let rec GetKernelFromName(mi, k:ModuleParsingStep) =       
        match mi with
        | DerivedPatterns.MethodWithReflectedDefinition(b) ->
            Some(mi, b)
        | _ ->
            None
        
    interface ModuleParsingProcessor with
        member this.Handle(mi, engine:ModuleParsingStep) =
            if (mi.GetType() = typeof<MethodInfo>) then
                match GetKernelFromName(mi :?> MethodInfo, engine) with
                | Some(mi, b) -> 
                    let km = engine.NewKernelModule()
                    km.Source <- new KernelInfo(mi, b)
                    Some(km)
                | _ ->
                    None
            else
                None
            