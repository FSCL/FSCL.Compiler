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
type KernelModule(k: KernelInfo) =  
    member val internal functionStorage = new Dictionary<FunctionInfoID, FunctionEnvironment>()  

    member val Kernel = new KernelEnvironment(k) with get
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the module
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    
    member this.AddFunction(f: FunctionInfo) =
        if not (this.functionStorage.ContainsKey(f.ID)) then
            this.functionStorage.Add(f.ID, new FunctionEnvironment(f))

    member this.GetFunctions() =
        List.ofSeq(this.functionStorage.Values)        
    
    member this.GetFlattenRequiredFunctions(id: FunctionInfoID) =
        let depStack = new Stack<FunctionEnvironment>()
        let requiredFunctionsFlatten = new HashSet<FunctionEnvironment>()
        let functionEnvironment = 
            if id = this.Kernel.Info.ID then
                this.Kernel :> FunctionEnvironment
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
        for t in this.Kernel.RequiredGlobalTypes do
            requiredTypesFlatten.Add(t) |> ignore
        for k in this.functionStorage do
            for t in k.Value.RequiredGlobalTypes do
                requiredTypesFlatten.Add(t) |> ignore
        List.ofSeq(requiredTypesFlatten)
        
    member this.GetFlattenRequiredGlobalTypes(id: FunctionInfoID) =
        let requiredTypesFlatten = new HashSet<Type>()
        let functionEnvironment = 
            if id = this.Kernel.Info.ID then
                this.Kernel :> FunctionEnvironment
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
        for t in this.Kernel.RequiredDirectives do
            requiredDirectivesFlatten.Add(t) |> ignore
        for k in this.functionStorage do
            for t in k.Value.RequiredDirectives do
                requiredDirectivesFlatten.Add(t) |> ignore
        List.ofSeq(requiredDirectivesFlatten)

    member this.GetFlattenRequiredDirectives(id: FunctionInfoID) =
        let requiredDirectivesFlatten = new HashSet<String>()
        let functionEnvironment = 
            if id = this.Kernel.Info.ID then
                this.Kernel :> FunctionEnvironment
            else
                this.functionStorage.[id]
        for t in functionEnvironment.RequiredDirectives do
            requiredDirectivesFlatten.Add(t) |> ignore
        for df in this.GetFlattenRequiredFunctions(id) do
            for t in df.RequiredDirectives do
                requiredDirectivesFlatten.Add(t) |> ignore
        List.ofSeq(requiredDirectivesFlatten)
                             
