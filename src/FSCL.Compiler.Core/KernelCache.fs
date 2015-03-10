namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel

[<AllowNullLiteral>]
type KernelCacheEntry(m:IKernelModule) = 
    member val Module = m with get
    
[<AllowNullLiteral>]
type KernelCache(verifier, entryCreator: IKernelModule -> KernelCacheEntry) =
    let entries = new Dictionary<FunctionInfoID, List<ReadOnlyMetaCollection * KernelCacheEntry>>()
    member val EntryCreator = entryCreator
        with get
    member this.TryGet(id: FunctionInfoID, 
                       meta: ReadOnlyMetaCollection) =
        if entries.ContainsKey(id) then
            let potentialKernels = entries.[id]
            // Check if compatible kernel meta in cached kernels
            let item = Seq.tryFind(fun (cachedMeta: ReadOnlyMetaCollection, cachedKernel: KernelCacheEntry) ->
                                        verifier(cachedMeta, meta)) potentialKernels
            match item with
            | Some(m, k) ->
                Some(k)
            | _ ->
                None
        else
            None  
                     
    member this.Put(m: IKernelModule) =
        if not (entries.ContainsKey(m.Kernel.ID)) then
            entries.Add(m.Kernel.ID, new List<ReadOnlyMetaCollection * KernelCacheEntry>())
        let entry = entryCreator(m)
        entries.[m.Kernel.ID].Add(m.Kernel.Meta, entry)
        entry

    static member NullCache() =
        new KernelCache((fun (_,_) -> false), (fun a -> new KernelCacheEntry(a)))
     