namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open FSCL

///
///<summary>
/// Base classes for  Attributes (Attributes with matching functions to be used in kernel calls)
///</summary>
///
[<AllowNullLiteral>]
type MetadataAttribute() =
    inherit Attribute()
   
///
///<summary>
/// Base classes for  Attributes associated to kernels
///</summary>
/// 
[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)>]
type KernelMetadataAttribute() =
    inherit MetadataAttribute()
    
///
///<summary>
/// Base classes for  Attributes associated to parameters
///</summary>
///
[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)>]
type ParameterMetadataAttribute() =
    inherit MetadataAttribute()

type IParamMetaCollection =
    interface
        abstract member Collection: IReadOnlyDictionary<Type, ParameterMetadataAttribute>
        abstract member Contains<'T when 'T :> ParameterMetadataAttribute> : unit -> bool
        abstract member Contains: Type -> bool
        abstract member Get<'T when 'T :> ParameterMetadataAttribute> : unit -> 'T
        abstract member Get : Type -> ParameterMetadataAttribute
    end
        
type IKernelMetaCollection =
    interface
        abstract member Collection: IReadOnlyDictionary<Type, KernelMetadataAttribute>
        abstract member Contains<'T when 'T :> KernelMetadataAttribute> : unit -> bool
        abstract member Contains: Type -> bool
        abstract member Get<'T when 'T :> KernelMetadataAttribute> : unit -> 'T
        abstract member Get : Type -> KernelMetadataAttribute
    end

type ParamMetaCollection() = 
    let container = new Dictionary<Type, ParameterMetadataAttribute>()
    member this.Add(item: 'T when 'T :> ParameterMetadataAttribute) =
        container.Add(item.GetType(), item)
    member this.AddOrSet(item: 'T when 'T :> ParameterMetadataAttribute) =
        if not (container.ContainsKey(item.GetType())) then
            container.Add(item.GetType(), item)
        else
            container.[item.GetType()] <- item            
    member this.AddIfNotExist(item: 'T when 'T :> ParameterMetadataAttribute) =
        if not (container.ContainsKey(item.GetType())) then
            container.Add(item.GetType(), item)
    member this.Contains<'T when 'T :> ParameterMetadataAttribute>() =
        this.Contains(typeof<'T>)
    member this.Contains(t: Type) =
        container.ContainsKey(t)
    member this.Get(t: Type) =
        if not (container.ContainsKey(t)) then
            Activator.CreateInstance(t) :?> ParameterMetadataAttribute
        else        
            container.[t]
    member this.Get<'T when 'T :> ParameterMetadataAttribute>() =
        this.Get(typeof<'T>) :?> 'T
    member this.Collection = container :> IReadOnlyDictionary<Type, ParameterMetadataAttribute>

    // Interface implementation
    interface IParamMetaCollection with
        member this.Contains<'T when 'T :> ParameterMetadataAttribute>() =
            this.Contains<'T>()
        member this.Contains(t: Type) =
            this.Contains(t)
        member this.Get<'T when 'T :> ParameterMetadataAttribute>() =
           this.Get(typeof<'T>) :?> 'T
        member this.Get(t: Type) =
            this.Get(t)
        member this.Collection = 
            this.Collection
        
type KernelMetaCollection() = 
    let container = new Dictionary<Type, KernelMetadataAttribute>()
    member this.Add(item: 'T when 'T :> KernelMetadataAttribute) =
        container.Add(item.GetType(), item)
    member this.AddOrSet(item: 'T when 'T :> KernelMetadataAttribute) =
        if not (container.ContainsKey(item.GetType())) then
            container.Add(item.GetType(), item)
        else
            container.[item.GetType()] <- item
    member this.AddIfNotExist(item: 'T when 'T :> KernelMetadataAttribute) =
        if not (container.ContainsKey(item.GetType())) then
            container.Add(item.GetType(), item)
    member this.Contains<'T when 'T :> KernelMetadataAttribute>() =
        this.Contains(typeof<'T>)
    member this.Contains(t: Type) =
        container.ContainsKey(t)
    member this.Get(t: Type) =
        if not (container.ContainsKey(t)) then
            Activator.CreateInstance(t) :?> KernelMetadataAttribute
        else        
            container.[t]
    member this.Get<'T when 'T :> KernelMetadataAttribute>() =
        this.Get(typeof<'T>) :?> 'T
    member this.Collection = container :> IReadOnlyDictionary<Type, KernelMetadataAttribute>

    // Interface implementation
    interface IKernelMetaCollection with
        member this.Contains<'T when 'T :> KernelMetadataAttribute>() =
            this.Contains<'T>()
        member this.Contains(t: Type) =
            this.Contains(t)
        member this.Get<'T when 'T :> KernelMetadataAttribute>() =
           this.Get(typeof<'T>) :?> 'T
        member this.Get(t: Type) =
            this.Get(t)
        member this.Collection = 
            this.Collection
        
type ReadOnlyMetaCollection(kernelMeta: KernelMetaCollection,
                            returnMeta: ParamMetaCollection,
                            paramMeta: List<ParamMetaCollection>) =

    member val KernelMeta = kernelMeta :> IKernelMetaCollection with get
    member val ParamMeta = 
        let roList = new List<IParamMetaCollection>()
        for item in paramMeta do
            roList.Add(item)
        roList.AsReadOnly() 
        with get
    member val ReturnMeta = returnMeta with get

    static member EmptyParamMetaCollection(count: int) =
        let l = new List<ParamMetaCollection>()
        for i = 0 to count - 1 do
            l.Add(ParamMetaCollection())
        l

type MetadataComparer() =
    abstract member MetaEquals: MetadataAttribute * MetadataAttribute -> bool
    default this.MetaEquals(meta1, meta2) =
        meta1 = meta2

[<AllowNullLiteral>]
type UseMetadataAttribute(meta: Type, comparer: Type) =
    inherit Attribute()
    
    member val MetadataType = meta with get
    member val Comparer = comparer with get

    new(meta:Type) =
        UseMetadataAttribute(meta, typeof<MetadataComparer>)