namespace FSCL.Compiler
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open Microsoft.FSharp.Quotations
open System

type KernelConnection =
| ParameterConnection of string
| ReturnValueConnection of int

[<AllowNullLiteral>]
type CallGraphNode(content: FunctionInfo) =
    member val Content = content with get
    member val OutputKernels = new Dictionary<MethodInfo, Dictionary<KernelConnection, KernelConnection>>() with get
    member val Functions = new Dictionary<MethodInfo, unit>() with get    
    member val Directives = new Dictionary<string, unit>() with get
    member val GlobalTypes = new Dictionary<Type, unit>() with get

[<AllowNullLiteral>]
type ModuleCallGraph() =
    member val internal functionStorage = new Dictionary<MethodInfo, CallGraphNode>()    
    member val internal kernelStorage = new Dictionary<MethodInfo, CallGraphNode>()  
    member val internal globalTypesStorage = new Dictionary<Type, unit>()   
    member val internal directivesStorage = new Dictionary<string, unit>() 
            
    member private this.RecomputeEntryEndPoints() =
        for k in this.kernelStorage do
            (k.Value.Content :?> KernelInfo).IsEntryPoint <- true
            (k.Value.Content :?> KernelInfo).IsEndPoint <- (k.Value.OutputKernels.Count = 0) 
        for k in this.kernelStorage do
            for k2 in k.Value.OutputKernels do
                (this.kernelStorage.[k2.Key].Content :?> KernelInfo).IsEntryPoint <- false
                
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
            
    // Type-usage-related methods
    member this.AddTypeUsage(src: MethodInfo, t: Type) =
        if this.kernelStorage.ContainsKey(src) then
            if this.globalTypesStorage.ContainsKey(t) then
                this.kernelStorage.[src].GlobalTypes.Add(t, ())
        else 
            if this.globalTypesStorage.ContainsKey(t) then
                this.functionStorage.[src].GlobalTypes.Add(t, ())   
                
    member this.RemoveTypeUsage(src: MethodInfo, t: Type) =
        if this.kernelStorage.ContainsKey(src) then
            if this.globalTypesStorage.ContainsKey(t) then
                this.kernelStorage.[src].GlobalTypes.Remove(t) |> ignore
        else 
            if this.globalTypesStorage.ContainsKey(t) then
                this.functionStorage.[src].GlobalTypes.Remove(t) |> ignore  
                
    member this.ClearTypeUsage(src: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) then
            this.kernelStorage.[src].GlobalTypes.Clear()
        else 
            this.functionStorage.[src].GlobalTypes.Clear()

    member this.GetTypesUsage(src: MethodInfo) =
        List.ofSeq(this.kernelStorage.[src].GlobalTypes.Keys)
            
    // Type-usage-related methods
    member this.AddRequireDirective(src: MethodInfo, directive: String) =
        if this.kernelStorage.ContainsKey(src) then
            if this.directivesStorage.ContainsKey(directive) then
                this.kernelStorage.[src].Directives.Add(directive, ())
        else 
            if this.directivesStorage.ContainsKey(directive) then
                this.functionStorage.[src].Directives.Add(directive, ())   
                
    member this.RemoveRequireDirective(src: MethodInfo, directive: String) =
        if this.kernelStorage.ContainsKey(src) then
            if this.directivesStorage.ContainsKey(directive) then
                this.kernelStorage.[src].Directives.Remove(directive) |> ignore
        else 
            if this.directivesStorage.ContainsKey(directive) then
                this.functionStorage.[src].Directives.Remove(directive) |> ignore  
                
    member this.ClearRequireDirective(src: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) then
            this.kernelStorage.[src].Directives.Clear()
        else 
            this.functionStorage.[src].Directives.Clear()
            
    member this.GetRequireDirectives(src: MethodInfo) =
        List.ofSeq(this.kernelStorage.[src].Directives.Keys)

    // Connection-related methods
    member this.AddConnection(src: MethodInfo, dst: MethodInfo, inBinding: KernelConnection, outBinding: KernelConnection) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            if not (this.kernelStorage.[src].OutputKernels.ContainsKey(dst)) then
                this.kernelStorage.[src].OutputKernels.Add(dst, new Dictionary<KernelConnection, KernelConnection>()) 
            this.kernelStorage.[src].OutputKernels.[dst].Add(inBinding, outBinding)
            this.RecomputeEntryEndPoints()

    member this.ChangeConnection(src: MethodInfo, dst: MethodInfo, inBinding: KernelConnection, outBinding: KernelConnection) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            if this.kernelStorage.[src].OutputKernels.ContainsKey(dst) then
                if this.kernelStorage.[src].OutputKernels.[dst].ContainsKey(inBinding) then
                    this.kernelStorage.[src].OutputKernels.[dst].[inBinding] <- outBinding
            this.RecomputeEntryEndPoints()
            
    member this.RemoveConnection(src: MethodInfo, dst: MethodInfo, connection: KernelConnection) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            if this.kernelStorage.[src].OutputKernels.ContainsKey(dst) then
                this.kernelStorage.[src].OutputKernels.[dst].Remove(connection) |> ignore
            this.RecomputeEntryEndPoints()
                
    member this.RemoveConnection(src: MethodInfo, dst: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) && this.kernelStorage.ContainsKey(dst) then
            this.kernelStorage.[src].OutputKernels.Remove(dst) |> ignore
            this.RecomputeEntryEndPoints()
            
    member this.ClearConnection(src: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) then
            this.kernelStorage.[src].OutputKernels.Clear()
            this.RecomputeEntryEndPoints()
            
    member this.GetOutputConnections(info: MethodInfo) =
        let outputConnections = new Dictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>()
        for k in this.kernelStorage.[info].OutputKernels do
            outputConnections.Add(k.Key, new ReadOnlyDictionary<KernelConnection, KernelConnection>(k.Value))
        new ReadOnlyDictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>(outputConnections)

    member this.GetInputConnections(info: MethodInfo) =        
        let inputConnections = new Dictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>()
        for k in this.kernelStorage do
            for next in k.Value.OutputKernels do
                if next.Key = info then
                    inputConnections.Add(k.Key, new ReadOnlyDictionary<KernelConnection, KernelConnection>(next.Value))
        new ReadOnlyDictionary<MethodInfo, ReadOnlyDictionary<KernelConnection, KernelConnection>>(inputConnections)
                
    // Call-related methods
    member this.AddCall(src: MethodInfo, dst: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) && this.functionStorage.ContainsKey(dst) then
            if not (this.kernelStorage.[src].Functions.ContainsKey(dst)) then
                this.kernelStorage.[src].Functions.Add(dst, ()) 
            
    member this.RemoveCall(src: MethodInfo, dst: MethodInfo) =
        if this.kernelStorage.ContainsKey(src) && this.functionStorage.ContainsKey(dst) then
            this.kernelStorage.[src].Functions.Remove(dst) |> ignore 
            
    member this.ClearCall(src: MethodInfo) =
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
            for k in this.kernelStorage do
                for next in k.Value.Functions do
                    if next.Key = info then
                        inputConnections.Add(k.Key, ())      
            for k in this.functionStorage do
                for next in k.Value.Functions do
                    if next.Key = info then
                        inputConnections.Add(k.Key, ())   
            new ReadOnlyDictionary<MethodInfo, unit>(inputConnections)
        else
            new ReadOnlyDictionary<MethodInfo, unit>(new Dictionary<MethodInfo, unit>())

    // Other methods
    member this.MergeWith(kcg: ModuleCallGraph) =
        for k in kcg.Kernels do
            this.AddKernel(k)
        for f in kcg.Functions do
            this.AddFunction(f)
        for k in kcg.Kernels do
            for connSet in this.kernelStorage.[k.ID].OutputKernels do
                for conn in connSet.Value do
                    this.AddConnection(k.ID, connSet.Key, conn.Key, conn.Value)
            for connSet in this.kernelStorage.[k.ID].Functions do
                this.AddCall(k.ID, connSet.Key)
        for f in kcg.Functions do
            for connSet in this.functionStorage.[f.ID].Functions do
                this.AddCall(f.ID, connSet.Key)
        this.RecomputeEntryEndPoints()
        
    member this.EntyPoints = 
        this.kernelStorage.Values |> (Seq.filter(fun (n: CallGraphNode) -> (n.Content :?> KernelInfo).IsEntryPoint) >> Seq.map(fun(n: CallGraphNode) -> n.Content :?> KernelInfo) >> List.ofSeq) 
    member this.EndPoints =  
        this.kernelStorage.Values |> (Seq.filter(fun (n: CallGraphNode) -> (n.Content :?> KernelInfo).IsEndPoint) >> Seq.map(fun(n: CallGraphNode) -> n.Content :?> KernelInfo) >> List.ofSeq)
      
    // Return in a breath-first mode
    member this.Kernels 
        with get() =
            let l = new List<KernelInfo>()
            l.AddRange(this.EntyPoints)
            for i = 0 to l.Count - 1 do
                for next in this.kernelStorage.[l.[i].ID].OutputKernels.Keys do
                    l.Add(this.kernelStorage.[next].Content :?> KernelInfo)
            List.ofSeq(l)
        
    member this.Functions
        with get() =
            this.functionStorage.Values |> (Seq.map(fun (n: CallGraphNode) -> n.Content)) |> List.ofSeq
        
    member this.GlobalTypes 
        with get() =
            List.ofSeq(this.globalTypesStorage.Keys)
                        
    member this.Directives 
        with get() =
            List.ofSeq(this.directivesStorage.Keys)
        
