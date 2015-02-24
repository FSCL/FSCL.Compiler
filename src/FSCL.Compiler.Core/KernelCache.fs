namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel

type KernelCacheItem(info, code, defines) =
    member val Kernel:IKernelInfo = info with get 
    member val OpenCLCode:String = code with get, set
    member val DynamicDefines: IReadOnlyDictionary<string, Var option * Expr option * obj> = defines with get
    // List of devices and kernel instances potentially executing the kernel
    
type KernelCache() =
    member val Kernels = Dictionary<FunctionInfoID, List<ReadOnlyMetaCollection * KernelCacheItem>>() 
        with get    
    member this.TryFindCompatibleOpenCLCachedKernel(id: FunctionInfoID, 
                                                    meta: ReadOnlyMetaCollection,
                                                    openCLMetadataVerifier: ReadOnlyMetaCollection * ReadOnlyMetaCollection -> bool) =
        if this.Kernels.ContainsKey(id) then
            let potentialKernels = this.Kernels.[id]
            // Check if compatible kernel meta in cached kernels
            let item = Seq.tryFind(fun (cachedMeta: ReadOnlyMetaCollection, cachedKernel: KernelCacheItem) ->
                                        openCLMetadataVerifier(cachedMeta, meta)) potentialKernels
            match item with
            | Some(m, k) ->
                Some(k)
            | _ ->
                None
        else
            None  