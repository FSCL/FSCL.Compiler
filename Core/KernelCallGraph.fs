namespace FSCL.Compiler
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open Microsoft.FSharp.Quotations
open System

type KernelConnection =
| ParameterIndex of int
| ReturnValue of int

[<AllowNullLiteral>]
type CallGraphNode(content: FunctionInfo) =
    member val Content = content with get
    member val Next = new Dictionary<MethodInfo, Dictionary<KernelConnection, KernelConnection>>() with get
    member val Functions = new Dictionary<MethodInfo, unit>() with get
    
[<AllowNullLiteral>]
type KernelCallGraph() =
    member val internal kernelStorage = new Dictionary<MethodInfo, CallGraphNode>() 
            
    member private this.RecomputeEntryEndPoints() =
        for k in this.kernelStorage do
            (k.Value.Content :?> KernelInfo).IsEntryPoint <- true
            (k.Value.Content :?> KernelInfo).IsEndPoint <- (k.Value.Next.Count = 0) 
        for k in this.kernelStorage do
            for k2 in k.Value.Next do
                (this.kernelStorage.[k2.Key].Content :?> KernelInfo).IsEntryPoint <- false

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
                kernel.Value.Next.Remove(info) |> ignore
            // Remove the item
            this.kernelStorage.Remove(info) |> ignore
            this.RecomputeEntryEndPoints()

    member this.AddConnection(src: MethodInfo, dst: MethodInfo, inBinding: KernelConnection, outBinding: KernelConnection) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            if not (this.kernelStorage.[src].Next.ContainsKey(dst)) then
                this.kernelStorage.[src].Next.Add(dst, new Dictionary<KernelConnection, KernelConnection>()) 
            this.kernelStorage.[src].Next.[dst].Add(inBinding, outBinding)
            this.RecomputeEntryEndPoints()

    member this.ChangeConnection(src: MethodInfo, dst: MethodInfo, inBinding: KernelConnection, outBinding: KernelConnection) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            if this.kernelStorage.[src].Next.ContainsKey(dst) then
                if this.kernelStorage.[src].Next.[dst].ContainsKey(inBinding) then
                    this.kernelStorage.[src].Next.[dst].[inBinding] <- outBinding
            this.RecomputeEntryEndPoints()
            
    member this.RemoveConnection(src: MethodInfo, dst: MethodInfo, connection: KernelConnection) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            if this.kernelStorage.[src].Next.ContainsKey(dst) then
                this.kernelStorage.[src].Next.[dst].Remove(connection) |> ignore
            this.RecomputeEntryEndPoints()
                
    member this.RemoveConnections(src: MethodInfo, dst: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            this.kernelStorage.[src].Next.Remove(dst) |> ignore
            this.RecomputeEntryEndPoints()
            
    member this.ClearConnections(src: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) then
            this.kernelStorage.[src].Next.Clear()
            this.RecomputeEntryEndPoints()
            
    member this.GetOutputConnections(info: MethodInfo) =
        let outputConnections = new Dictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>()
        for k in this.kernelStorage.[info].Next do
            outputConnections.Add(k.Key, new ReadOnlyDictionary<KernelConnection, KernelConnection>(k.Value))
        new ReadOnlyDictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>(outputConnections)

    member this.GetInputConnections(info: MethodInfo) =        
        let inputConnections = new Dictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>()
        for id in this.KernelIDs do
            for next in this.kernelStorage.[id].Next do
                if next.Key = info then
                    inputConnections.Add(id, new ReadOnlyDictionary<KernelConnection, KernelConnection>(next.Value))
        new ReadOnlyDictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>(inputConnections)

    member this.MergeWith(kcg: KernelCallGraph) =
        for k in kcg.KernelIDs do
            this.AddKernel(kcg.GetKernel(k))
        for k in kcg.KernelIDs do
            for connSet in this.kernelStorage.[k].Next do
                for conn in connSet.Value do
                    this.AddConnection(k, connSet.Key, conn.Key, conn.Value)
        this.RecomputeEntryEndPoints()
        
    member this.EntyPoints = 
        List.ofSeq((Seq.map(fun (g: CallGraphNode) -> g.Content) >> Seq.filter(fun (k: FunctionInfo) -> (k :?> KernelInfo).IsEntryPoint)) (this.kernelStorage.Values))
        
    member this.EndPoints =  
        List.ofSeq((Seq.map(fun (g: CallGraphNode) -> g.Content) >> Seq.filter(fun (k: FunctionInfo) -> (k :?> KernelInfo).IsEndPoint)) (this.kernelStorage.Values))
        
    member this.Kernels = 
        List.ofSeq(Seq.map(fun (g: CallGraphNode) -> g.Content :?> KernelInfo) this.kernelStorage.Values)

    member this.KernelIDs =
        List.ofSeq(this.kernelStorage.Keys)
        
[<AllowNullLiteral>]
type ModuleCallGraph() =
    inherit KernelCallGraph()

    member val internal functionStorage = new Dictionary<MethodInfo, CallGraphNode>()
    
    member this.GetFunction(info: MethodInfo) =
        if this.functionStorage.ContainsKey(info) then
            this.functionStorage.[info].Content
        else
            null

    member this.HasFunction(info: MethodInfo) =
        this.functionStorage.ContainsKey(info)

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

    member this.AddCall(src: MethodInfo, dst: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) && this.functionStorage.ContainsKey(dst) then
            if not (this.kernelStorage.[src].Functions.ContainsKey(dst)) then
                this.kernelStorage.[src].Functions.Add(dst, ()) 
            
    member this.RemoveCall(src: MethodInfo, dst: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) && this.functionStorage.ContainsKey(dst) then
            this.kernelStorage.[src].Functions.Remove(dst) |> ignore 
            
    member this.ClearCalls(src: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) then
            this.kernelStorage.[src].Functions.Clear()
        if this.functionStorage.ContainsKey(src) then
            this.functionStorage.[src].Functions.Clear()
            
    member this.GetOutputCalls(info: MethodInfo) =
        if this.kernelStorage.ContainsKey(info) then
            new ReadOnlyDictionary<MethodInfo, unit>(this.kernelStorage.[info].Functions)
        else 
            new ReadOnlyDictionary<MethodInfo, unit>(this.functionStorage.[info].Functions)
            
    member this.GetInputCalls(info: MethodInfo) =
        if this.functionStorage.ContainsKey(info) then
            let inputConnections = new Dictionary<MethodInfo, unit>()
            for id in this.KernelIDs do
                for next in this.kernelStorage.[id].Functions do
                    if next.Key = info then
                        inputConnections.Add(id, ())      
            for id in this.FunctionIDs do
                for next in this.kernelStorage.[id].Functions do
                    if next.Key = info then
                        inputConnections.Add(id, ())   
            new ReadOnlyDictionary<MethodInfo, unit>(inputConnections)
        else
            new ReadOnlyDictionary<MethodInfo, unit>(new Dictionary<MethodInfo, unit>())
                        
    member this.Functions = 
        List.ofSeq(Seq.map(fun (g: CallGraphNode) -> g.Content) this.functionStorage.Values)

    member this.FunctionIDs =
        List.ofSeq(this.functionStorage.Keys)
        
