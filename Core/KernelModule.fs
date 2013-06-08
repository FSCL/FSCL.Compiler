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
type KernelParameterInfo(parameterInfo:ParameterInfo) =
    ///
    ///<summary>
    /// .NET-related information about the kernel (method) parameter
    ///</summary>
    ///
    member val Info = parameterInfo with get, set 
    ///
    ///<summary>
    /// The set of additional parameters generated to access this parameter
    ///</summary>
    ///<remarks>
    /// Additional size parameters are generated only for vector (array) parameters, one for each vector parameter dimension
    ///</remarks>
    ///
    member val SizeParameters = ([]:KernelParameterInfo list) with get, set
    member val SizeParameterNames = ([]:string list) with get, set    
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
    
///
///<summary>
/// The set of information about utility functions collected and maintained by the compiler
///</summary>
///
[<AllowNullLiteral>]
type FunctionInfo(source: MethodInfo, expr:Expr) =
    ///
    ///<summary>
    /// The original signature of the function
    ///</summary>
    ///
    member val Source = source with get
    ///
    ///<summary>
    /// The processed signature of the function. Signature processing includes generating additional parameters to address arrays, flatten arrays, replace ref variables with sigletons and so on
    ///</summary>
    ///
    member val Signature = source with get, set
    ///
    ///<summary>
    /// The body of the function
    ///</summary>
    ///
    member val Body = expr with get, set
    ///
    ///<summary>
    /// The generated target code
    ///</summary>
    ///
    member val Codegen = "" with get, set
    ///
    ///<summary>
    /// The set of information about function parameters
    ///</summary>
    ///
    member val ParameterInfo = new Dictionary<String, KernelParameterInfo>() with get
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the function
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    
///
///<summary>
/// The set of information about kernels collected and maintained by the compiler
///</summary>
///<remarks>
/// This type inherits from FunctionInfo without exposing any additional property/member. The set
/// of information contained in FunctionInfo is in fact enough expressive to represent a kernel. 
/// From another point of view, a function can be considered a special case of a kernel, where the address-space is fixed, some
/// OpenCL functions cannot be called (e.g. get_global_id) and with some other restrictions.
/// KernelInfo is kept an independent, different class from FunctionInfo with the purpose to trigger different compiler processing on the basis of the
/// actual type.
///</remarks>
///
[<AllowNullLiteral>]
type KernelInfo(methodInfo: MethodInfo, expr:Expr) =
    inherit FunctionInfo(methodInfo, expr)
      
///
///<summary>
/// The main information built and returned by the compiler, represeting the result of the compilation process together with a set of meta-information generated during the compilation itself
///</summary>
///
[<AllowNullLiteral>]
type KernelModule() =  
    ///
    ///<summary>
    /// The set of utility functions (i.e. functions called somewere in one or more kernels)
    ///</summary>
    ///
    member val Functions = ([]:FunctionInfo list) with get, set
    ///
    ///<summary>
    /// The set of kernels
    ///</summary>
    ///
    member val Kernels = ([]:KernelInfo list) with get, set
    ///
    ///<summary>
    /// The set of global compiler directives (e.g. #pragma OPENCL EXTENSION cl_khr_byte_addressable_store : enable)
    ///</summary>
    ///
    member val Directives = ([]:string list) with get, set
    ///
    ///<summary>
    /// The set of global custom types used in one or more kernels/functions
    ///</summary>
    ///
    member val GlobalTypes = ([]:Type list) with get, set    
    ///
    ///<summary>
    /// The source of the kernel. This is the .NET pair (signature, body) of the original F# kernel, where "original" means before any compiler processing, such
    // as additional parameters generation, list-to-array transformation and multi-dimensional array flattening.
    ///</summary>
    ///
    member val Source:KernelInfo = null with get, set
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the module
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    
