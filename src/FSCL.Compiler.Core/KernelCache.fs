namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel

type KernelCacheEntry(m:IKernelModule) = 
    member val Module = m with get

type KernelCache(verifier, entryCreator: IKernelModule -> KernelCacheEntry) =
    let entries = new Dictionary<FunctionInfoID, List<ReadOnlyMetaCollection * KernelCacheEntry>>()
    member this.TryGet(id: FunctionInfoID, 
                       meta: ReadOnlyMetaCollection) =
        if entries.ContainsKey(id) then
            let potentialKernels = entries.[id]
            // Check if compatible kernel meta in cached kernels
            let item = Seq.tryFind(fun (cachedMeta: ReadOnlyMetaCollection, cachedKernel: KernelCacheEntry) ->
                                        verifier(cachedMeta, meta)) potentialKernels
            match item with
            | Some(m, k) ->
                Some(k.Module)
            | _ ->
                None
        else
            None  
                     
    member this.Put(m: IKernelModule) =
        if not (entries.ContainsKey(m.Kernel.ID)) then
            entries.Add(m.Kernel.ID, new List<ReadOnlyMetaCollection * KernelCacheEntry>())
        entries.[m.Kernel.ID].Add(m.Kernel.Meta, entryCreator(m))
     