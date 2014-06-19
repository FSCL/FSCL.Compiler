namespace FSCL.Compiler.Configuration

open System
open System.Collections.Generic
open System.Collections.ObjectModel
open FSCL.Compiler
open System.Reflection
open GraphUtil

exception PipelineBuildException of string
    
type internal PipelineBuilder() =
    static member Build(conf: PipelineConfiguration) =
        let typeHandlers = new Dictionary<string, TypeHandlerConfiguration>()
        let steps = new Dictionary<string, StepConfiguration>()
        let processors = new Dictionary<string, StepProcessorConfiguration>()
        
        // Explode sources and group by component type (conf must be explicit)
        for s in conf.Sources do
            for th in s.TypeHandlers do
                if not (typeHandlers.ContainsKey(th.ID)) then
                    typeHandlers.Add(th.ID, th)
            for st in s.Steps do
                if not (steps.ContainsKey(st.ID)) then
                    steps.Add(st.ID, st)
            for sp in s.StepProcessors do
                if not (processors.ContainsKey(sp.ID)) then
                    processors.Add(sp.ID, sp)
                    
        // Create graph of type handlers
        let thGraph = new Graph<TypeHandlerConfiguration, string>()
        for t in typeHandlers do 
            thGraph.Add(t.Key, t.Value) |> ignore
        for t in typeHandlers do 
            for d in t.Value.Dependencies do
                thGraph.Connect(d, t.Value.ID)
            for d in t.Value.Before do
                if typeHandlers.ContainsKey(d) then
                    thGraph.Connect(t.Value.ID, d)
        let sortedTypeHandlers = thGraph.Sorted.Value

        // Build type handlers and type manager
        let th = seq { 
                        for s,t in sortedTypeHandlers do 
                            yield t.Type.GetConstructor([||]).Invoke([||]) :?> TypeHandler 
                     }
        let tm = new TypeManager(List.ofSeq th)
                    
        // Collect metadata affecting compilation
        let usedMetadata = new Dictionary<Type, MetadataComparer list>()

        // Check that each step has required steps
        for s in steps do
            for ty, comp in s.Value.UsedMetadata do
                if not (usedMetadata.ContainsKey(ty)) then
                    usedMetadata.Add(ty, [ comp ])
                else
                    usedMetadata.[ty] <- usedMetadata.[ty] @ [ comp ]

            for rs in s.Value.Dependencies do
                if not (steps.ContainsKey(rs)) then
                    raise (PipelineBuildException("The step processor " + s.Key + " requires step " + rs + " but this step has not been found"))
        
        // Check that each processors has and owner step and a before/after processor
        for p in processors do
            for ty, comp in p.Value.UsedMetadata do
                if not (usedMetadata.ContainsKey(ty)) then
                    usedMetadata.Add(ty, [ comp ])
                else
                    usedMetadata.[ty] <- usedMetadata.[ty] @ [ comp ]

            if not (steps.ContainsKey(p.Value.Step)) then
                raise (PipelineBuildException("The step processor " + p.Key + " belongs to the step " + p.Value.Step + " but this step has not been found"))
            for dep in p.Value.Dependencies do
                if not (processors.ContainsKey(dep)) then
                    raise (PipelineBuildException("The step processor " + p.Key + " requires processor " + dep + " but this step has not been found"))
       
        // Create graph of steps
        let graph = new Graph<StepConfiguration, string>()
        for s in steps do
            graph.Add(s.Key, s.Value) |> ignore
        for s in steps do              
            for d in s.Value.Dependencies do
                graph.Connect(d, s.Value.ID)
            for d in s.Value.Before do
                if steps.ContainsKey(d) then
                    graph.Connect(s.Value.ID, d)
                        
        // Foreach step, create graph of processors
        let procGraphs = new Dictionary<string, Graph<StepProcessorConfiguration, string>>()
        for s in steps do
            procGraphs.Add(s.Key, new Graph<StepProcessorConfiguration, string>())
        for p in processors do
            procGraphs.[p.Value.Step].Add(p.Key, p.Value) |> ignore
        for p in processors do
            let step = procGraphs.[p.Value.Step].Get(p.Value.ID).Value
            for d in p.Value.Dependencies do
                procGraphs.[p.Value.Step].Connect(d, p.Value.ID)
            for d in p.Value.Before do
                if processors.ContainsKey(d) then
                    procGraphs.[p.Value.Step].Connect(p.Value.ID, d)

        // Topological sort of steps
        let sorted = graph.Sorted
        let i = 0

        let steps = seq {
            if sorted.IsNone then
                raise (PipelineBuildException("Cannot build a pipeline using the specified steps since there is a cycle in steps dependencies"))
            for (id, data) in sorted.Value do
                let procSorted = procGraphs.[id].Sorted
                if procSorted.IsNone then
                    raise (PipelineBuildException("Cannot build the step " + id + " using the specified processors since there is a cycle in processors dependencies"))
                        
                // Instantiate a list of proper processors via reflection
                let processors = new List<ICompilerStepProcessor>()
                for (id, procNode) in procSorted.Value do
                    processors.Add(procNode.Type.GetConstructor([||]).Invoke([||]) :?> ICompilerStepProcessor) |> ignore
                        
                yield data.Type.GetConstructors().[0].Invoke([| tm; List.ofSeq(processors) |]) :?> ICompilerStep
        }
        let flatSteps = Array.ofSeq(steps)
        flatSteps, usedMetadata
      
            
        
        




