namespace FSCL.Compiler
open System.Collections.Generic
open System.Collections.ObjectModel
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.ObjectModel
open System.Collections.Generic

type FlowGraphNodeBindingPoint =
| ParameterBindingPoint of string
| ReturnValueBindingPoint of int
| ImplicitBindingPoint

type FlowGraphNode =
| RuntimeValueNode of Expr
| StaticValueNode of obj
| KernelNode of FlowGraphKernelNode

and [<AllowNullLiteral>] FlowGraphKernelNode(kernelId: MethodInfo) =
    member val KernelID = kernelId with get
    member val internal Input = new Dictionary<string, FlowGraphNode * FlowGraphNodeBindingPoint>() with get
    member val internal Output = new Dictionary<FlowGraphNodeBindingPoint, HashSet<FlowGraphKernelNode * FlowGraphNodeBindingPoint>>() with get
  
    member this.IsEntryPoint 

        with get() =
            (Seq.tryFind(
                fun(k: KeyValuePair<string, FlowGraphNode * FlowGraphNodeBindingPoint>) ->
                    match k.Value with 
                    |(node, point) ->
                        match node with
                        | KernelNode(n) ->
                            true
                        | _ ->
                            false
                ) (this.Input)).IsNone
            
    member this.IsEndPoint 
        with get() =
            this.Output.Count = 0

[<AllowNullLiteral>]
type FlowGraph(mainEndPoint: FlowGraphKernelNode) =
    let kernelNodes = new Dictionary<MethodInfo, List<FlowGraphKernelNode>>()
    let nodes = new List<FlowGraphNode>()
    
    member val MainEndPoint = mainEndPoint with get

    member this.GetNodes() =
        List.ofSeq(nodes)

    member this.GetKernelNodes(id: MethodInfo) =
        if kernelNodes.ContainsKey(id) then
            List.ofSeq(kernelNodes.[id])
        else
            []
            
    member this.AppendNode(node: FlowGraphNode) =
        match node with
        | KernelNode(cgn) ->
            if not (kernelNodes.ContainsKey(cgn.KernelID)) then
                kernelNodes.Add(cgn.KernelID, new List<FlowGraphKernelNode>())
            kernelNodes.[cgn.KernelID].Add(cgn)
        | _ ->
            ()
        nodes.Add(node)
        
    member this.GetNodeInputConnection(node: FlowGraphKernelNode,
                                       par: string) =
        if node.Input.ContainsKey(par) then
            Some(node.Input.[par])
        else
            None
            
    member this.GetNodeOutputConnections(node: FlowGraphKernelNode,
                                         point: FlowGraphNodeBindingPoint) =
        if node.Output.ContainsKey(point) then
            Some(List.ofSeq(node.Output.[point]))
        else
            None

    member this.GetNodeInputConnections(node: FlowGraphKernelNode) =
        ReadOnlyDictionary<string, FlowGraphNode * FlowGraphNodeBindingPoint>(node.Input)
        
    member this.GetNodeOutputConnections(node: FlowGraphKernelNode) =
        let d = new Dictionary<FlowGraphNodeBindingPoint, (FlowGraphKernelNode * FlowGraphNodeBindingPoint) list>()
        for item in node.Output do            
            d.Add(item.Key, List.ofSeq(item.Value))
        ReadOnlyDictionary<FlowGraphNodeBindingPoint, (FlowGraphKernelNode * FlowGraphNodeBindingPoint) list>(d)

    member this.SetNodeConnection(before: FlowGraphNode,
                                  after: FlowGraphKernelNode,
                                  beforePoint: FlowGraphNodeBindingPoint,
                                  afterPoint: FlowGraphNodeBindingPoint) =
        match before, after with
        | RuntimeValueNode(v), n ->
            match afterPoint with
            | ParameterBindingPoint(p) ->
                if n.Input.ContainsKey(p) then
                    n.Input.[p] <- (before, ImplicitBindingPoint)
                else
                    n.Input.Add(p, (before, ImplicitBindingPoint))
            | _ ->
                raise (FlowGraphException("Cannot set a connection to a kernel node with a binding point different from ParameterBindingPoint"))
        | StaticValueNode(v), n ->
            match afterPoint with
            | ParameterBindingPoint(p) ->
                if n.Input.ContainsKey(p) then
                    n.Input.[p] <- (before, ImplicitBindingPoint)
                else
                    n.Input.Add(p, (before, ImplicitBindingPoint))
            | _ ->
                raise (FlowGraphException("Cannot set a connection to a kernel node with a binding point different from ParameterBindingPoint"))
        | KernelNode(n1), n2 ->
            match beforePoint, afterPoint with
            | ParameterBindingPoint(p1), ParameterBindingPoint(p2) ->
                if n2.Input.ContainsKey(p2) then
                    n2.Input.[p2] <- (before, beforePoint)
                else
                    n2.Input.Add(p2, (before, beforePoint))
                if not (n1.Output.ContainsKey(beforePoint)) then
                    n1.Output.Add(beforePoint, new HashSet<FlowGraphKernelNode * FlowGraphNodeBindingPoint>())
                n1.Output.[beforePoint].Add(after, afterPoint) |> ignore                
            | ReturnValueBindingPoint(index), ParameterBindingPoint(p2) ->
                if n2.Input.ContainsKey(p2) then
                    n2.Input.[p2] <- (before, beforePoint)
                else
                    n2.Input.Add(p2, (before, beforePoint))
                if not (n1.Output.ContainsKey(beforePoint)) then
                    n1.Output.Add(beforePoint, new HashSet<FlowGraphKernelNode * FlowGraphNodeBindingPoint>())
                n1.Output.[beforePoint].Add(after, afterPoint) |> ignore
            | _, _ ->
                raise (FlowGraphException("Two kernel calls can be connected only in a parameter-parameter or in a returnValue-parameter mode"))  
        
    member this.MergeWith(fg: FlowGraph) =
        for item in fg.GetNodes() do
            this.AppendNode(item)
        
                

