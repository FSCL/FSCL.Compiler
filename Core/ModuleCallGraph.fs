namespace FSCL.Compiler
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.ObjectModel
open System.Collections.Generic
(*
type KernelBindingPointType =
| OutParameter of string
| ReturnItem of int

type KernelBindingPoint(id: MethodInfo,
                        callId: int,
                        point: KernelBindingPointType) =
    let mutable call = callId
    member val KernelID = id with get
    member val Point = point with get
    member this.CallID 
        with get() = 
            call
        and internal set(v) =
            call <- v
    override this.Equals(o) =
        if not (o :? KernelBindingPoint) then
            false
        else
            let ke = o :?> KernelBindingPoint
            (this.KernelID = ke.KernelID) && (this.CallID = ke.CallID) && (this.Point = ke.Point)
    override this.GetHashCode() =
        (this.KernelID, this.CallID, this.Point).GetHashCode()
            
type ArgumentBinding =
| KernelOutput of KernelBindingPoint
| ValueExpr of Expr
| ImplicitValue of obj

[<AllowNullLiteral>]
type KernelCallInfo(kernelId: MethodInfo,
                    callId: int) =
    member val KernelID = kernelId with get
    member val CallID = callId with get
    member val Arguments = new Dictionary<string, ArgumentBinding>() with get

    member this.IsEntryPoint 
        with get() =
            let isEntry = ref true
            for arg in this.Arguments do
                match arg.Value with
                | KernelOutput(k) ->
                    isEntry := false
                | _ ->
                    ()
            !isEntry
    override this.Equals(o) =
        if not (o :? KernelCallInfo) then
            false
        else
            let kci = o :?> KernelCallInfo
            kci.KernelID = this.KernelID && kci.CallID = this.CallID
    override this.GetHashCode() =
        (this.KernelID, this.CallID).GetHashCode()

[<AllowNullLiteral>]
type ModuleCallGraph() = 
    member val internal kernelStorage = new Dictionary<MethodInfo, Dictionary<int, KernelCallInfo>>()
    member val internal kernelCallNextId = new Dictionary<MethodInfo, int>()    

    member this.AddKernelCall(id: MethodInfo) =
        // Check id exists
        if not (this.kernelCallNextId.ContainsKey(id)) then
            this.kernelCallNextId.Add(id, 0)
        let call = KernelCallInfo(id, this.kernelCallNextId.[id])
        // Store the call
        if not (this.kernelStorage.ContainsKey(id)) then
            this.kernelStorage.Add(id, new Dictionary<int, KernelCallInfo>())
        this.kernelStorage.[id].Add(call.CallID, call)
        // Update call id
        this.kernelCallNextId.[id] <- call.CallID + 1
        // Return id
        call.CallID
        
    member this.RemoveKernelCall(id: MethodInfo,
                                 callId: int) =
        if this.kernelStorage.ContainsKey(id) then
            this.kernelStorage.[id].Remove(callId) |> ignore
            
    member this.GetKernelCall(id: MethodInfo,
                              callId: int) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then
            this.kernelStorage.[id].[callId]
        else
            null

    member this.GetLastKernelCall(id: MethodInfo) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].Count > 0 then
            this.kernelStorage.[id].[Seq.max(this.kernelStorage.[id].Keys)]
        else
            null        

    member this.GetKernelCalls(id: MethodInfo) =
        if this.kernelStorage.ContainsKey(id) then
            List.ofSeq(this.kernelStorage.[id])
        else
            []
            
    member this.GetKernels() =
        List.ofSeq(this.kernelStorage.Keys)

    member this.SetArgument(id: MethodInfo, 
                            callId: int,
                            argument: string,
                            binding: ArgumentBinding) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            if not (kernelCall.Arguments.ContainsKey(argument)) then
                kernelCall.Arguments.Add(argument, binding)
            else
                kernelCall.Arguments.[argument] <- binding
                
    member this.RemoveArgument(id: MethodInfo, 
                               callId: int,
                               argument: string) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            kernelCall.Arguments.Remove(argument) |> ignore
                    
    member this.GetArgument(id: MethodInfo, 
                            callId: int,
                            argument: string) =
        if this.kernelStorage.ContainsKey(id) && this.kernelStorage.[id].ContainsKey(callId) then         
            let kernelCall = this.kernelStorage.[id].[callId]
            if kernelCall.Arguments.ContainsKey(argument) then
                Some(kernelCall.Arguments.[argument])
            else
                None
        else
            None
            
    member this.GetEntryPoints() =
        let entryPoints = new List<KernelCallInfo>()
        for k in this.kernelStorage do
            for call in k.Value do
                if call.Value.IsEntryPoint then
                    entryPoints.Add(call.Value)
        List.ofSeq(entryPoints)
        
    // Restriction: only one endpoint
    member this.GetEndPoint() =
        // TODO: make this more efficient
        let endPoints = new List<KernelCallInfo>()
        for k1 in this.kernelStorage do
            for c1 in k1.Value do
                let isInput = (Seq.tryFind(fun(k2: KeyValuePair<MethodInfo, Dictionary<int, KernelCallInfo>>) ->
                                                (Seq.tryFind(fun(c2: KeyValuePair<int, KernelCallInfo>) ->
                                                                (Seq.tryFind(fun(arg: KeyValuePair<string, ArgumentBinding>) ->
                                                                                 match arg.Value with 
                                                                                 | KernelOutput(kbp) ->
                                                                                     if kbp.KernelID = k1.Key && kbp.CallID = c1.Key then
                                                                                         true
                                                                                     else
                                                                                         false
                                                                                 | _ ->
                                                                                     false) (c2.Value.Arguments)).IsSome) (k2.Value)).IsSome) (this.kernelStorage)).IsSome
                if not isInput then
                    endPoints.Add(c1.Value)            
        List.ofSeq(endPoints)

    member private this.ChangeCallIDs(id: MethodInfo,
                                      callIdStart: int) =
        if this.kernelCallNextId.ContainsKey(id) then
            this.kernelCallNextId.[id] <- callIdStart
        // Reset the call ids
        for oldCallId in this.kernelStorage.[id].Keys do
            if oldCallId < callIdStart then
                // Add a copy of the call and remove the old one
                this.kernelStorage.[id].Add(this.kernelCallNextId.[id], this.kernelStorage.[id].[oldCallId])
                this.kernelStorage.[id].Remove(oldCallId) |> ignore
                // Fix calls whose output is this call
                for kernel in this.kernelStorage do
                    if kernel.Key <> id then
                        for call in kernel.Value do
                            for arg in call.Value.Arguments do
                                match arg.Value with
                                | KernelOutput(k) ->
                                    if k.KernelID = id && k.CallID = oldCallId then
                                        k.CallID <- this.kernelCallNextId.[id]
                                | _ ->
                                    ()
                // Update next id
                this.kernelCallNextId.[id] <- this.kernelCallNextId.[id] + 1

    // Other methods
    member this.MergeWith(kcg: ModuleCallGraph) =
        // Change kcg call ids to not interfere with the ones in this
        for kernel in this.GetKernels() do
            kcg.ChangeCallIDs(kernel, this.kernelCallNextId.[kernel])
        // Merbge
        for k in kcg.GetKernels() do            
            for call in kcg.GetKernelCalls(k) do
                let newCallId = this.AddKernelCall(k)
                for arg in call.Value.Arguments do
                    this.SetArgument(call.Value.KernelID, newCallId, arg.Key, arg.Value)
        *)