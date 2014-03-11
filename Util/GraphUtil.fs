namespace FSCL.Compiler.Util

open System.Collections.Generic

module GraphUtil =
    type Graph<'T,'K when 'K: equality> =
        val internalData: Dictionary<'K, 'T>
        val internalOutgoing: Dictionary<'K, List<'K>>
        val internalIngoing: Dictionary<'K, List<'K>>
        val mutable sorted: (('K * 'T) list option) option
    
        new () = {
            internalData = Dictionary<'K, 'T>()
            internalOutgoing = Dictionary<'K, List<'K>>()
            internalIngoing = Dictionary<'K, List<'K>>()
            sorted = None
        }
    
        new (id, ii, io) = {
            internalData = id
            internalOutgoing = io
            internalIngoing = ii
            sorted = None
        }
    
        member this.Nodes
            with get() =
                seq {
                    for item in this.internalOutgoing do
                        yield item.Key
                }

        member this.StartNodes 
            with get() =
                seq {
                    for item in this.internalIngoing do
                        if item.Value.Count = 0 then
                            yield item.Key
                }
            
        member this.NodeCount 
            with get() =
                this.internalData.Count
            
        member this.EdgeCount 
            with get() =
                let mutable c = 0
                for item in this.internalOutgoing do
                    c <- c + item.Value.Count
                c
            
        member this.Add(key:'K, value: 'T) =
            if this.internalData.ContainsKey(key) then
                false
            else
                this.internalData.Add(key, value)
                this.internalOutgoing.Add(key, new List<'K>())
                this.internalIngoing.Add(key, new List<'K>())
                this.sorted <- None
                true

        member this.Get(id) =
            if this.internalData.ContainsKey(id) then
                Some(this.internalData.[id])
            else
                None
            
        member this.Outgoing(id) =
            if this.internalOutgoing.ContainsKey(id) then
                Some(this.internalOutgoing.[id])
            else
                None
            
        member this.Ingoing(id) =
            if this.internalIngoing.ContainsKey(id) then
                Some(this.internalIngoing.[id])
            else
                None

        member this.Connect(f: 'K, s: 'K) =
            if not (this.internalOutgoing.[f].Contains(s)) then
                this.internalOutgoing.[f].Add(s)
                this.sorted <- None
            if not (this.internalIngoing.[s].Contains(f)) then
                this.internalIngoing.[s].Add(f)
                this.sorted <- None
            
        member this.Disconnect(f: 'K, s: 'K) =
            if (this.internalOutgoing.[f].Contains(s)) then
                this.internalOutgoing.[f].Remove(s) |> ignore
                this.sorted <- None
            if (this.internalIngoing.[s].Contains(f)) then
                this.internalIngoing.[s].Remove(f) |> ignore
                this.sorted <- None
            
        member this.Sorted 
            with get() =
                if this.sorted.IsSome then
                    this.sorted.Value
                else
                    let graph = new Graph<'T,'K>(this.internalData, this.internalIngoing, this.internalOutgoing)

                    let endNodes = new List<'K>()
                    let startNodes = new List<'K>()
                    for item in graph.StartNodes do
                        startNodes.Add(item)

                    // Process
                    while startNodes.Count > 0 do
                        let node = startNodes.[0]
                        startNodes.RemoveAt(0)
                        endNodes.Add(node)
                        while(graph.Outgoing(node).Value.Count > 0) do
                            let target = graph.Outgoing(node).Value.[0]
                            graph.Disconnect(node, target)
                            if graph.Ingoing(target).Value.Count = 0 then
                                startNodes.Add(target)
                    // Return
                    if graph.EdgeCount > 0 then
                        this.sorted <- Some(None)
                    else
                        this.sorted <- Some(Some(List.ofSeq(seq { for item in endNodes do yield (item, graph.Get(item).Value) })))
                    this.sorted.Value




                
