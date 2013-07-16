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
type KernelModule(kcg: KernelCallGraph) =  
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
    member val Source:KernelCallGraph = kcg with get
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the module
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    
