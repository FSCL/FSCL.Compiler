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
    
//type KernelCacheComparer() =
//    interface IEqualityComparer<FunctionInfoID> with
//        member this.Equals(o1, o2) =
//            match o1, o2 with
//            | MethodID(mi1), MethodID(mi2) ->
//                mi1 = mi2
//            | LambdaID(e1), LambdaID(e2) ->
//                QuotationComparison.AreStructuralEquivalent(e1, e2, Map.empty)
//            | CollectionFunctionID(m1, e1), CollectionFunctionID(m2, e2) ->
//                m1 = m2 && 
//                ((e1.IsNone && e2.IsNone) || 
//                    (e1.IsSome && e2.IsSome && QuotationComparison.AreStructuralEquivalent(e1.Value, e2.Value, Map.empty)))
//            | _, _ ->
//                false
//
//        member this.GetHashCode(o) =
//            let code = 
//                match o with
//                | MethodID(mi)
//                | CollectionFunctionID(mi, None) ->
//                    mi.GetHashCode()
//                | LambdaID(e) ->
//                    QuotationComparison.ComputeHashCode(e)
//                | CollectionFunctionID(m, e) ->
//                    m.GetHashCode() ^^^ QuotationComparison.ComputeHashCode(e.Value)
//            code

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

     