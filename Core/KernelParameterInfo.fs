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
/// The set of information about a kernel parameter collected and maintained by the compiler
///</summary>
///
type KernelParameterInfo(name:string, 
                         t: Type, 
                         methodInfoParameter: ParameterInfo, 
                         arg: Expr option,
                         dynamicMetadata: DynamicParameterMetadataCollection) =
    let metadata = 
        let dictionary = 
            if dynamicMetadata <> null then
                new DynamicParameterMetadataCollection(dynamicMetadata)
            else
                new DynamicParameterMetadataCollection()
        if methodInfoParameter <> null then
            for item in methodInfoParameter.GetCustomAttributes() do
                if typeof<DynamicParameterMetadataAttribute>.IsAssignableFrom(item.GetType()) then
                    if not (dictionary.ContainsKey(item.GetType())) then
                        dictionary.Add(item.GetType(), item :?> DynamicParameterMetadataAttribute)
        dictionary
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
    member val Access = 
        if (t.IsArray) then
            AccessMode.ReadAccess
        else
            AccessMode.NoAccess
        with get, set
    ///
    ///<summary>
    /// The return expression for this parameter (only if return parameter is true)
    ///</summary>
    ///
    member val ReturnExpr = None with get, set
    ///
    ///<summary>
    /// The actual arguments ofthe call if this kernel is resulting from parsing a call
    ///</summary>
    ///
    member val CallExpr = arg with get
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
    member val Metadata = metadata with get

    member this.GetMetadata<'T when 'T :> DynamicParameterMetadataAttribute and 'T : null>() =
        if this.Metadata.ContainsKey(typeof<'T>) then
            this.Metadata.[typeof<'T>] :?> 'T
        else
            null

    