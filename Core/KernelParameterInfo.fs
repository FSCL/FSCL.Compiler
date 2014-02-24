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
[<Flags>]
type KernelParameterAccessMode =
| NoAccess = 0
| ReadAccess = 1
| WriteAccess = 2

///
///<summary>
/// Enumeration describing the transfer contraints to a kernel parameter (NoTransfer, NoTransferBack, Transfer)
///</summary>
///
[<Flags>]
type KernelParameterTransferMode =
| NoTransfer = 1
| NoTransferBack = 2
| Transfer = 0

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
    member val IsSizeParameter = false with get, set
    member val IsReturnParameter = false with get, set
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
    ///
    ///<summary>
    /// The transfer mode of this parameter
    ///</summary>
    ///
    member val Transfer = KernelParameterTransferMode.Transfer with get, set

    // For kernel return type
    member val Expr = None with get, set
    ///
    ///<summary>
    /// Variable holding the parameter inside the kernel body in the abstract syntax tree
    ///</summary>
    ///
    member val Placeholder:Var option = None with get, set
    