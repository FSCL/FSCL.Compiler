namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic

[<AllowNullLiteral>]
type DeviceAttribute(platform: int, device: int) =
    inherit Attribute()

    member val Platform = platform with get
    member val Device = device with get

///
///<summary>
/// The set of information about utility functions collected and maintained by the compiler
///</summary>
///
[<AllowNullLiteral>]
type FunctionInfo(id: MethodInfo, expr:Expr) =
    ///
    ///<summary>
    /// The original signature of the function
    ///</summary>
    ///
    member val ID = id with get   
    ///
    ///<summary>
    /// The name of the function
    ///</summary>
    ///
    member val Name = id.Name with get, set
    ///
    ///<summary>
    /// The set of information about function parameters
    ///</summary>
    ///
    member val Parameters = new List<KernelParameterInfo>() with get
    ///
    ///<summary>
    /// The function return type
    ///</summary>
    ///
    member val ReturnType = id.ReturnType with get, set
    ///
    ///<summary>
    /// The body of the function
    ///</summary>
    ///
    member val OriginalBody = expr with get
    ///
    ///<summary>
    /// The body of the function
    ///</summary>
    ///
    member val Body = expr with get, set
    ///
    ///<summary>
    /// Whether the processing of this function whould be skipped 
    ///</summary>
    ///
    member val Skip = false with get, set
    ///
    ///<summary>
    /// The generated target code
    ///</summary>
    ///
    member val Code = "" with get, set      
    ///
    ///<summary>
    /// Whether this function has been generated from a lambda
    ///</summary>
    ///
    member val IsLambda = false with get, set      
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the function
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    
    member this.GetParameter(name) =
        Seq.tryFind(fun (p: KernelParameterInfo) -> p.Name = name) (this.Parameters) 
   
   
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

    member val Device:DeviceAttribute = null 
        with get, set             