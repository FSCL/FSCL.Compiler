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
| HostEndpointNode
| KernelNode of FlowGraphKernelNode

and [<AllowNullLiteral>] FlowGraphKernelNode(kernelId: MethodInfo) =
    member val KernelID = kernelId with get
    member val internal Input = new Dictionary<string, FlowGraphNode * FlowGraphNodeBindingPoint>() with get
    member val internal Output = new Dictionary<FlowGraphNodeBindingPoint, HashSet<FlowGraphNode * FlowGraphNodeBindingPoint>>() with get
  
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
            (Seq.tryFind(
                fun(k: KeyValuePair<FlowGraphNodeBindingPoint, HashSet<FlowGraphNode * FlowGraphNodeBindingPoint>>) ->
                    (Seq.tryFind(
                        fun(k: FlowGraphNode * FlowGraphNodeBindingPoint) ->
                            match k with
                            | (node, point) ->
                                match node with
                                | KernelNode(n) ->
                                    true
                                | _ ->
                                    false) (k.Value)).IsSome
                ) (this.Output)).IsNone

type FlowGraph() =
    let kernelNodes = new Dictionary<MethodInfo, List<FlowGraphKernelNode>>()
    let nodes = new List<FlowGraphNode>()

    member this.AddNode(node: FlowGraphNode) =
        match node with
        | KernelNode(cgn) ->
            if not (kernelNodes.ContainsKey(cgn.KernelID)) then
                kernelNodes.Add(cgn.KernelID, new List<FlowGraphKernelNode>())
            kernelNodes.[cgn.KernelID].Add(cgn)
        | _ ->
            ()
        nodes.Add(node)
        
    member this.GetKernelNodes(id: MethodInfo) =
        if kernelNodes.ContainsKey(id) then
            List.ofSeq(kernelNodes.[id])
        else
            []

    member this.SetNodeConnection(before: FlowGraphNode,
                                  after: FlowGraphNode,
                                  beforePoint: FlowGraphNodeBindingPoint,
                                  afterPoint: FlowGraphNodeBindingPoint) =
        match before, after with
        | RuntimeValueNode(v), KernelNode(n) ->
            match afterPoint with
            | ParameterBindingPoint(p) ->
                if n.Input.ContainsKey(p) then
                    n.Input.[p] <- (before, ImplicitBindingPoint)
                else
                    n.Input.Add(p, (before, ImplicitBindingPoint))
            | _ ->
                raise (FlowGraphException("Cannot set a connection to a kernel node with a binding point different from ParameterBindingPoint"))
        | StaticValueNode(v), KernelNode(n) ->
            match afterPoint with
            | ParameterBindingPoint(p) ->
                if n.Input.ContainsKey(p) then
                    n.Input.[p] <- (before, ImplicitBindingPoint)
                else
                    n.Input.Add(p, (before, ImplicitBindingPoint))
            | _ ->
                raise (FlowGraphException("Cannot set a connection to a kernel node with a binding point different from ParameterBindingPoint"))
        | KernelNode(n1), KernelNode(n2) ->
            match beforePoint, afterPoint with
            | ParameterBindingPoint(p1), ParameterBindingPoint(p2) ->
                if n2.Input.ContainsKey(p2) then
                    n2.Input.[p2] <- (before, beforePoint)
                else
                    n2.Input.Add(p2, (before, beforePoint))
                if not (n1.Output.ContainsKey(beforePoint)) then
                    n1.Output.Add(beforePoint, new HashSet<FlowGraphNode * FlowGraphNodeBindingPoint>())
                n1.Output.[beforePoint].Add(after, afterPoint) |> ignore                
            | ReturnValueBindingPoint(index), ParameterBindingPoint(p2) ->
                if n2.Input.ContainsKey(p2) then
                    n2.Input.[p2] <- (before, beforePoint)
                else
                    n2.Input.Add(p2, (before, beforePoint))
                if not (n1.Output.ContainsKey(beforePoint)) then
                    n1.Output.Add(beforePoint, new HashSet<FlowGraphNode * FlowGraphNodeBindingPoint>())
                n1.Output.[beforePoint].Add(after, afterPoint) |> ignore
            | _, _ ->
                raise (FlowGraphException("Two kernel calls can be connected only in a parameter-parameter or in a returnValue-parameter mode"))
        | KernelNode(n1), HostEndpointNode ->
            if not (n1.Output.ContainsKey(beforePoint)) then
                n1.Output.Add(beforePoint, new HashSet<FlowGraphNode * FlowGraphNodeBindingPoint>())
            n1.Output.[beforePoint].Add(after, ImplicitBindingPoint) |> ignore           
        | _, _ ->
            raise (FlowGraphException("In a call graph it is possible to connect only runtime values to kernel, static values to kernel, kernels to kernels and kernels to host endpoints"))
                
                

