namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic

///
///<summary>
/// Enumeration describing the address spaces exposed by OpenCL
///</summary>
///
type KernelParameterAddressSpace =
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
type KernelParameterAccessMode =
| ReadOnly
| WriteOnly
| ReadWrite
| NoAccess

///
///<summary>
/// The set of information about a kernel parameter collected and maintained by the compiler
///</summary>
///
type KernelParameterInfo(name:string, t: Type) =
    ///
    ///<summary>
    /// .NET-related information about the kernel (method) parameter
    ///</summary>
    ///
    member val Name = name with get 
    member val Type = t with get, set
    ///
    ///<summary>
    /// The set of additional parameters generated to access this parameter
    ///</summary>
    ///<remarks>
    /// Additional size parameters are generated only for vector (array) parameters, one for each vector parameter dimension
    ///</remarks>
    ///
    member val SizeParameters = new Dictionary<String, KernelParameterInfo>() with get
    ///
    ///<summary>
    /// The OpenCL address-space of this parameter
    ///</summary>
    ///
    member val AddressSpace = KernelParameterAddressSpace.AutoSpace with get, set
    ///
    ///<summary>
    /// The access mode of this parameter
    ///</summary>
    ///
    member val Access = KernelParameterAccessMode.NoAccess with get, set
    // For kernel return type
    member val Expr = None with get, set
    // Actual arg
    member val ArgumentExpression = None with get, set
    ///
    ///<summary>
    /// Variable holding the parameter inside the kernel body in the abstract syntax tree
    ///</summary>
    ///
    member val Placeholder:Var option = None with get, set
    
type TemporaryKernelParameterInfo(parameterInfo:ParameterInfo) =
    member val Info = parameterInfo with get, set
    member val SizeParameters = ([]:string list) with get, set
    member val AddressSpace = KernelParameterAddressSpace.AutoSpace with get, set
    member val Access = KernelParameterAccessMode.NoAccess with get, set
    // For kernel return type
    member val Expr = None with get, set
    