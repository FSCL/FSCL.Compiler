namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel
open Cloo

///
///<summary>
/// Base classes for Dynamic Attributes (Attributes with matching functions to be used in kernel calls)
///</summary>
///
[<AllowNullLiteral>]
type DynamicAttributeAttribute() =
    inherit Attribute()
    
[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)>]
type DynamicKernelAttributeAttribute() =
    inherit DynamicAttributeAttribute()
    
[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)>]
type DynamicParameterAttributeAttribute() =
    inherit DynamicAttributeAttribute()
    

type DynamicParameterAttributeCollection = Dictionary<Type, DynamicParameterAttributeAttribute>
type DynamicKernelAttributeCollection = Dictionary<Type, DynamicKernelAttributeAttribute>
type ReadOnlyDynamicParameterAttributeCollection = ReadOnlyDictionary<Type, DynamicParameterAttributeAttribute>
type ReadOnlyDynamicKernelAttributeCollection = ReadOnlyDictionary<Type, DynamicKernelAttributeAttribute>

///
///<summary>
/// Enumeration describing the address spaces exposed by OpenCL
///</summary>
///
type AddressSpace =
| GlobalSpace
| ConstantSpace
| LocalSpace
| PrivateSpace
| AutoSpace

///
///<summary>
/// Enumeration describing the access mode to a kernel parameter (R, W, RW or not used)
///</summary>
///
[<Flags>]
type AccessMode =
| NoAccess = 0
| ReadAccess = 1
| WriteAccess = 2

///
///<summary>
/// Enumeration describing the transfer contraints to a kernel parameter (NoTransfer, NoTransferBack, Transfer)
///</summary>
///
[<Flags>]
type TransferMode =
| NoTransfer = 1
| NoTransferBack = 2
| Transfer = 0
   
///
///<summary>
/// The set of information about a kernel parameter collected and maintained by the compiler
///</summary>
///
type KernelParameterInfo(name:string, t: Type, ?methodInfoParameter: ParameterInfo) =
    let dynamicAttributes = 
        let dictionary = new DynamicParameterAttributeCollection()
        if methodInfoParameter.IsSome then
            for item in methodInfoParameter.Value.GetCustomAttributes() do
                if typeof<DynamicParameterAttributeAttribute>.IsAssignableFrom(item.GetType()) then
                    dictionary.Add(item.GetType(), item :?> DynamicParameterAttributeAttribute)
            new ReadOnlyDynamicParameterAttributeCollection(dictionary)
        else
            new ReadOnlyDynamicParameterAttributeCollection(new DynamicParameterAttributeCollection())
    ///
    ///<summary>
    /// .NET-related information about the kernel (method) parameter
    ///</summary>
    ///
    member val Name = name with get 
    member val Type = t with get, set
    member val IsSizeParameter = false with get, set
    member val IsReturnParameter = false with get, set
    member val IsDynamicArrayParameter = false with get, set

    member val DynamicAllocationArguments:Expr list = [] with get, set
    ///
    ///<summary>
    /// The set of additional parameters generated to access this parameter
    ///</summary>
    ///<remarks>
    /// Additional size parameters are generated only for vector (array) parameters, one for each vector parameter dimension
    ///</remarks>
    ///
    member val SizeParameters = new List<KernelParameterInfo>() with get
    ///
    ///<summary>
    /// The access mode of this parameter
    ///</summary>
    ///
    member val Access = AccessMode.NoAccess with get, set
    ///
    ///<summary>
    /// The return expression for this parameter (only if return parameter is true)
    ///</summary>
    ///
    member val Expr = None with get, set
    ///
    ///<summary>
    /// Variable holding the parameter inside the kernel body in the abstract syntax tree
    ///</summary>
    ///
    member val Placeholder:Var option = None with get, set
    ///
    ///<summary>
    /// The static attributes of the method parameter (if some)
    ///</summary>
    ///
    member val Attributes = dynamicAttributes with get

    