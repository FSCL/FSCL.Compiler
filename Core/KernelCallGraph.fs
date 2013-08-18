namespace FSCL.Compiler
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.ObjectModel
open System.Collections.Generic

type KernelEndpoint(id: MethodInfo,
                    callId: int,
                    parameter: string) =
    let mutable call = callId
    member val ID = id with get
    member val Parameter = parameter with get
    member this.CallID 
        with get() = 
            call
        and internal set(v) =
            call <- v
    override this.Equals(o) =
        if not (o :? KernelEndpoint) then
            false
        else
            let ke = o :?> KernelEndpoint
            (this.ID = ke.ID) && (this.CallID = ke.CallID) && (this.Parameter = ke.Parameter)
            
type KernelConnectionSource =
| OutParameter of string
| ReturnItem of int

[<AllowNullLiteral>]
type KernelCallInfo(id: MethodInfo) =
    member val ID = id with get
    
    member val Output = new Dictionary<KernelConnectionSource, HashSet<KernelEndpoint>>() with get

    member this.IsEndPoint 
        with get() =
            let isEnd = ref true
            for arg in this.Output do
                if arg.Value <> null && arg.Value.Count > 0 then
                    isEnd := false
            !isEnd

[<AllowNullLiteral>]
type ModuleCallGraph() = 
    member val internal kernelStorage = new Dictionary<MethodInfo, Dictionary<int, KernelCallInfo>>()
    member val internal kernelCallNextId = new Dictionary<MethodInfo, int>()
        
    member this.AddKernelCall(id: MethodInfo) =
        // Update id
        if this.kernelCallNextId.ContainsKey(id) then
            this.kernelCallNextId.[id] <- this.kernelCallNextId.[id] + 1
        else
            this.kernelCallNextId.Add(id, 0)
        // Store the call
        if not (this.kernelStorage.ContainsKey(id)) then
            this.kernelStorage.Add(id, new Dictionary<int, KernelCallInfo>())
        this.kernelStorage.[id].Add(this.kernelCallNextId.[id], KernelCallInfo(id))

    member this.RemoveKernelCall(id: MethodInfo,
                                 callId: int) =
        if this.kernelStorage.ContainsKey(id) then
            this.kernelStorage.[id].Remove(callId) |> ignore

    member this.GetKernelCalls(id: MethodInfo) =
        if this.kernelStorage.ContainsKey(id) then
            List.ofSeq(this.kernelStorage.[id])
        else
            []
            
    member this.GetKernels() =
        List.ofSeq(this.kernelStorage.Keys)

    member this.SetOutput(id: MethodInfo, 
                          callId: int,
                          argument: KernelConnectionSource,
                          output: KernelEndpoint) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            if not (kernelCall.Output.ContainsKey(argument)) then
                kernelCall.Output.Add(argument, new HashSet<KernelEndpoint>())
            
            if not (kernelCall.Output.[argument].Contains(output)) then
                kernelCall.Output.[argument].Add(output) |> ignore
                
    member this.RemoveOutput(id: MethodInfo, 
                             callId: int,
                             argument: KernelConnectionSource,
                             output: KernelEndpoint) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            if kernelCall.Output.ContainsKey(argument) then
                kernelCall.Output.[argument].Remove(output) |> ignore
                    
    member this.RemoveOutput(id: MethodInfo, 
                             callId: int,
                             argument: KernelConnectionSource) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            if kernelCall.Output.ContainsKey(argument) then
                kernelCall.Output.[argument].Clear()
                
    member this.GetOutput(id: MethodInfo, 
                          callId: int,
                          argument: KernelConnectionSource) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            if kernelCall.Output.ContainsKey(argument) then
                List.ofSeq(kernelCall.Output.[argument])
            else
                []
        else
            []
            
    member this.GetOutputSources(id: MethodInfo, 
                                 callId: int) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            List.ofSeq(kernelCall.Output.Keys)            
        else
            []

    member this.GetEndPoints() =
        let endPoints = new List<KernelCallInfo>()
        for k in this.kernelStorage do
            for call in k.Value do
                if call.Value.IsEndPoint then
                    endPoints.Add(call.Value)
        List.ofSeq(endPoints)

    member this.RefactorCallIDs(id: MethodInfo,
                                callIdStart: int) =
        if this.kernelCallNextId.ContainsKey(id) then
            this.kernelCallNextId.[id] <- callIdStart
        // Reset thte call ids
        for oldCallId in this.kernelStorage.[id].Keys do
            if oldCallId < callIdStart then
                // Add a copy of the call and remove the old one
                this.kernelStorage.[id].Add(this.kernelCallNextId.[id], this.kernelStorage.[id].[oldCallId])
                this.kernelStorage.[id].Remove(oldCallId) |> ignore
                // Fix calls whose output is this call
                for kernel in this.kernelStorage do
                    if kernel.Key <> id then
                        for call in kernel.Value do
                            for conn in call.Value.Output do
                                for kc in conn.Value do
                                    if kc.ID = id && kc.CallID = oldCallId then
                                       kc.CallID <- this.kernelCallNextId.[id]
                // Update next id
                this.kernelCallNextId.[id] <- this.kernelCallNextId.[id] + 1

    // Other methods
    member this.MergeWith(kcg: ModuleCallGraph) =
        for k in kcg.GetKernels() do
            for 
            this.(k)
        for f in kcg.Functions do
            this.AddFunction(f)
        for k in kcg.Kernels do
            for connSet in kcg.GetOutputConnections(k.ID) do
                for conn in connSet.Value do
                    this.AddConnection(k.ID, connSet.Key, conn.Key, conn.Value)
            for connSet in kcg.GetOutputCalls(k.ID) do
                this.AddCall(k.ID, connSet.Key)
        for f in kcg.Functions do
            for connSet in kcg.GetOutputCalls(f.ID) do
                this.AddCall(f.ID, connSet.Key)
        this.RecomputeEntryEndPoints()
        