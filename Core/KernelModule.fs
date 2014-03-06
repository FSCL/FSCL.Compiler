namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel
    
[<AllowNullLiteral>]
type FunctionEnvironment(f: FunctionInfo) =
    member val Info = f with get
    member val RequiredFunctions = new HashSet<FunctionInfoID>() with get
    member val RequiredGlobalTypes = new HashSet<Type>() with get
    member val RequiredDirectives = new HashSet<String>() with get

    override this.Equals(o) =
        if not (o :? FunctionEnvironment) then
            false
        else
            (o :?> FunctionEnvironment).Info.ID = this.Info.ID
    override this.GetHashCode() =
        this.Info.ID.GetHashCode()
    
[<AllowNullLiteral>]
type KernelEnvironment(f: KernelInfo) =
    inherit FunctionEnvironment(f)
    member val Info = f with get
///
///<summary>
/// The main information built and returned by the compiler, represeting the result of the compilation process together with a set of meta-information generated during the compilation itself
///</summary>
///
[<AllowNullLiteral>]
type KernelModule() =  
    member val internal kernelStorage = new Dictionary<FunctionInfoID, KernelEnvironment>()  
    member val internal functionStorage = new Dictionary<FunctionInfoID, FunctionEnvironment>()  
    (*
    // Global-types-related methods
    member this.AddRequiredGlobalType(f: MethodInfo, 
                                      info: Type) =
        if this.kernelStorage.ContainsKey(f) then
            this.kernelStorage.[f].RequiredGlobalTypes.Add(info) |> ignore
        else
            this.functionStorage.[f].RequiredGlobalTypes.Add(info) |> ignore
            
    member this.RemoveRequiredGlobalType(f: MethodInfo,
                                         info: Type) =
        if this.kernelStorage.ContainsKey(f) then
            this.kernelStorage.[f].RequiredGlobalTypes.Remove(info) |> ignore
        else
            this.functionStorage.[f].RequiredGlobalTypes.Remove(info) |> ignore
            
    member this.RemoveRequiredGlobalType(info: Type) =
        for k in this.kernelStorage do
            k.Value.RequiredGlobalTypes.Remove(info) |> ignore
        for k in this.functionStorage do
            k.Value.RequiredGlobalTypes.Remove(info) |> ignore
            
    // Directives-related methods
    member this.AddRequiredDirective(f: MethodInfo, 
                                     info: String) =
        if this.kernelStorage.ContainsKey(f) then
            this.kernelStorage.[f].RequiredDirectives.Add(info) |> ignore
        else
            this.functionStorage.[f].RequiredDirectives.Add(info) |> ignore
            
    member this.RemoveRequiredDirective(f: MethodInfo,
                                        info: String) =
        if this.kernelStorage.ContainsKey(f) then
            this.kernelStorage.[f].RequiredDirectives.Remove(info) |> ignore
        else
            this.functionStorage.[f].RequiredDirectives.Remove(info) |> ignore
            
    member this.RemoveRequiredDirective(info: String) =
        for k in this.kernelStorage do
            k.Value.RequiredDirectives.Remove(info) |> ignore
        for k in this.functionStorage do
            k.Value.RequiredDirectives.Remove(info) |> ignore
    *)
    // Kernel-related methods
    member this.HasKernel(id: FunctionInfoID) =
        this.kernelStorage.ContainsKey(id)
            
    member this.GetKernel(id: FunctionInfoID) =
        if this.kernelStorage.ContainsKey(id) then
            this.kernelStorage.[id]
        else
            null

    member this.AddKernel(info: KernelInfo) =
        if not (this.kernelStorage.ContainsKey(info.ID)) then
            this.kernelStorage.Add(info.ID, new KernelEnvironment(info))
            
    member this.RemoveKernel(id: FunctionInfoID) =
        if this.kernelStorage.ContainsKey(id) then
            this.kernelStorage.Remove(id) |> ignore

    // Functions-related methods
    member this.HasFunction(id: FunctionInfoID) =
        this.functionStorage.ContainsKey(id)
            
    member this.GetFunction(id: FunctionInfoID) =
        if this.functionStorage.ContainsKey(id) then
            this.functionStorage.[id]
        else
            null

    member this.AddFunction(info: FunctionInfo) =
        if not (this.functionStorage.ContainsKey(info.ID)) then
            this.functionStorage.Add(info.ID, new FunctionEnvironment(info))
            
    member this.RemoveFunction(id: FunctionInfoID) =
        if this.functionStorage.ContainsKey(id) then
            this.functionStorage.Remove(id) |> ignore
            
    ///
    ///<summary>
    /// The set of utility functions (i.e. functions called somewere in one or more kernels)
    ///</summary>
    ///
    member val FlowGraph:FlowGraphNode = null with get, set
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the module
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    

    member this.GetKernels() =
        List.ofSeq(this.kernelStorage.Values)

    member this.GetFunctions() =
        List.ofSeq(this.functionStorage.Values)        
        
    member this.GetFlattenRequiredFunctions() =
        this.GetFunctions()

    member this.GetFlattenRequiredFunctions(id: FunctionInfoID) =
        let depStack = new Stack<FunctionEnvironment>()
        let requiredFunctionsFlatten = new HashSet<FunctionEnvironment>()
        let functionEnvironment = 
            if this.kernelStorage.ContainsKey(id) then
                this.kernelStorage.[id] :> FunctionEnvironment
            else
                this.functionStorage.[id]

        for f in functionEnvironment.RequiredFunctions do
            depStack.Push(this.functionStorage.[f])
            requiredFunctionsFlatten.Add(this.functionStorage.[f]) |> ignore

        while depStack.Count > 0 do
            let current = depStack.Pop()
            for f in current.RequiredFunctions do
                if not (requiredFunctionsFlatten.Add(this.functionStorage.[f])) then
                    depStack.Push(this.functionStorage.[f])
        List.ofSeq(requiredFunctionsFlatten)
                
    member this.GetFlattenRequiredGlobalTypes() =
        let requiredTypesFlatten = new HashSet<Type>()
        for k in this.kernelStorage do
            for t in k.Value.RequiredGlobalTypes do
                requiredTypesFlatten.Add(t) |> ignore
        for k in this.functionStorage do
            for t in k.Value.RequiredGlobalTypes do
                requiredTypesFlatten.Add(t) |> ignore
        List.ofSeq(requiredTypesFlatten)
        
    member this.GetFlattenRequiredGlobalTypes(id: FunctionInfoID) =
        let requiredTypesFlatten = new HashSet<Type>()
        let functionEnvironment = 
            if this.kernelStorage.ContainsKey(id) then
                this.kernelStorage.[id] :> FunctionEnvironment
            else
                this.functionStorage.[id]
        for t in functionEnvironment.RequiredGlobalTypes do
            requiredTypesFlatten.Add(t) |> ignore
        for df in this.GetFlattenRequiredFunctions(id) do
            for t in df.RequiredGlobalTypes do
                requiredTypesFlatten.Add(t) |> ignore
        List.ofSeq(requiredTypesFlatten)
        
    member this.GetFlattenRequiredDirectives() =
        let requiredDirectivesFlatten = new HashSet<String>()
        for k in this.kernelStorage do
            for t in k.Value.RequiredDirectives do
                requiredDirectivesFlatten.Add(t) |> ignore
        for k in this.functionStorage do
            for t in k.Value.RequiredDirectives do
                requiredDirectivesFlatten.Add(t) |> ignore
        List.ofSeq(requiredDirectivesFlatten)

    member this.GetFlattenRequiredDirectives(id: FunctionInfoID) =
        let requiredDirectivesFlatten = new HashSet<String>()
        let functionEnvironment = 
            if this.kernelStorage.ContainsKey(id) then
                this.kernelStorage.[id] :> FunctionEnvironment
            else
                this.functionStorage.[id]
        for t in functionEnvironment.RequiredDirectives do
            requiredDirectivesFlatten.Add(t) |> ignore
        for df in this.GetFlattenRequiredFunctions(id) do
            for t in df.RequiredDirectives do
                requiredDirectivesFlatten.Add(t) |> ignore
        List.ofSeq(requiredDirectivesFlatten)
                             
    // Other methods
    member this.MergeWith(m: KernelModule) =
        // Add kernels
        for k in m.GetKernels() do
            this.AddKernel(k.Info)
            for f in k.RequiredFunctions do
                this.GetKernel(k.Info.ID).RequiredFunctions.Add(f) |> ignore
            for t in k.RequiredGlobalTypes do
                this.GetKernel(k.Info.ID).RequiredGlobalTypes.Add(t) |> ignore
            for d in k.RequiredDirectives do
                this.GetKernel(k.Info.ID).RequiredDirectives.Add(d) |> ignore
        // Add functions
        for k in m.GetFunctions() do
            this.AddFunction(k.Info)
            for f in k.RequiredFunctions do
                this.GetKernel(k.Info.ID).RequiredFunctions.Add(f) |> ignore
            for t in k.RequiredGlobalTypes do
                this.GetKernel(k.Info.ID).RequiredGlobalTypes.Add(t) |> ignore
            for d in k.RequiredDirectives do
                this.GetKernel(k.Info.ID).RequiredDirectives.Add(d) |> ignore
    
