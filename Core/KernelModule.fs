namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
    
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
    member val CallGraph = new ModuleCallGraph() with get
    ///
    ///<summary>
    /// The set of kernels
    ///</summary>
    ///
    //member val Kernels = ([]:KernelInfo list) with get, set
    ///
    ///<summary>
    /// The set of global compiler directives (e.g. #pragma OPENCL EXTENSION cl_khr_byte_addressable_store : enable)
    ///</summary>
    ///
    ///
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the module
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    
