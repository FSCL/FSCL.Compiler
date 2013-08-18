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
    member val internal functionStorage = new Dictionary<MethodInfo, FunctionInfo>()    
    member val internal kernelStorage = new Dictionary<MethodInfo, KernelInfo>()  
    member val internal globalTypesStorage = new Dictionary<Type, unit>()   
    member val internal directivesStorage = new Dictionary<string, unit>() 
        
    // Global-types-related methods
    member this.HasGlobalType(info: Type) =
        this.globalTypesStorage.ContainsKey(info)

    member this.AddGlobalType(info: Type) =
        if not (this.globalTypesStorage.ContainsKey(info)) then
            this.globalTypesStorage.Add(info, ())
            
    member this.RemoveGlobalType(info: Type) =
        if this.globalTypesStorage.ContainsKey(info) then
            // Remove connections
            for kernel in this.kernelStorage do
                kernel.Value.GlobalTypes.Remove(info) |> ignore
            for f in this.functionStorage do
                f.Value.GlobalTypes.Remove(info) |> ignore
            // Remove the item
            this.globalTypesStorage.Remove(info) |> ignore
            
    // Directives-related methods
    member this.HasDirective(info: string) =
        this.directivesStorage.ContainsKey(info)

    member this.AddDirective(info: string) =
        if not (this.directivesStorage.ContainsKey(info)) then
            this.directivesStorage.Add(info, ())
            
    member this.RemoveDirective(info: string) =
        if this.directivesStorage.ContainsKey(info) then
            // Remove connections
            for kernel in this.kernelStorage do
                kernel.Value.Directives.Remove(info) |> ignore
            for f in this.functionStorage do
                f.Value.Directives.Remove(info) |> ignore
            // Remove the item
            this.directivesStorage.Remove(info) |> ignore

    // Kernel-related methods
    member this.HasKernel(info: MethodInfo) =
        this.kernelStorage.ContainsKey(info)
            
    member this.GetKernel(info: MethodInfo) =
        if this.kernelStorage.ContainsKey(info) then
            this.kernelStorage.[info].Content :?> KernelInfo
        else
            null

    member this.AddKernel(info: KernelInfo) =
        if not (this.kernelStorage.ContainsKey(info.ID)) then
            this.kernelStorage.Add(info.ID, new CallGraphNode(info))
            this.RecomputeEntryEndPoints()
            
    member this.RemoveKernel(info: MethodInfo) =
        if this.kernelStorage.ContainsKey(info) then
            // Remove connections
            for kernel in this.kernelStorage do
                kernel.Value.OutputKernels.Remove(info) |> ignore
            // Remove the item
            this.kernelStorage.Remove(info) |> ignore
            this.RecomputeEntryEndPoints()

    // Functions-related methods
    member this.HasFunction(info: MethodInfo) =
        this.functionStorage.ContainsKey(info)
                        
    member this.GetFunction(info: MethodInfo) =
        if this.functionStorage.ContainsKey(info) then
            this.functionStorage.[info].Content
        else
            null

    member this.AddFunction(info: FunctionInfo) =
        if not (this.functionStorage.ContainsKey(info.ID)) then
            this.functionStorage.Add(info.ID, new CallGraphNode(info))
            
    member this.RemoveFunction(info: MethodInfo) =
        if this.functionStorage.ContainsKey(info) then
            // Remove connections
            for kernel in this.kernelStorage do
                kernel.Value.Functions.Remove(info) |> ignore
            for func in this.functionStorage do
                func.Value.Functions.Remove(info) |> ignore
            // Remove the item
            this.functionStorage.Remove(info) |> ignore

    ///
    ///<summary>
    /// The set of utility functions (i.e. functions called somewere in one or more kernels)
    ///</summary>
    ///
    member val CallGraph = new ModuleCallGraph() with get

    member this.Kernels 
        with get() =
            List.ofSeq(this.kernelStorage.Values)
        
    member this.Functions
        with get() =
            this.functionStorage.Values |> List.ofSeq
        
    member this.GlobalTypes 
        with get() =
            List.ofSeq(this.globalTypesStorage.Keys)
                        
    member this.Directives 
        with get() =
            List.ofSeq(this.directivesStorage.Keys)
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
    
