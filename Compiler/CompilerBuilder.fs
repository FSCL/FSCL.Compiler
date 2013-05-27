namespace FSCL.Compiler.Configuration

open System
open System.Collections.Generic
open FSCL.Compiler
open FSCL.Compiler.Tools.GraphUtil
open FSCL.Compiler
open System.Reflection

exception CompilerBuildException of string

    
type internal CompilerBuilder() =
    static member Build(conf: CompilerConfiguration) =
        let typeHandlers = new Dictionary<string, TypeHandlerConfiguration>()
        let steps = new Dictionary<string, StepConfiguration>()
        let processors = new Dictionary<string, StepProcessorConfiguration>()
        
        // Explode sources and group by component type (conf must be explicit)
        for s in conf.Sources do
            for th in s.TypeHandlers do
                typeHandlers.Add(th.ID, th)
            for st in s.Steps do
                steps.Add(st.ID, st)
            for sp in s.StepProcessors do
                processors.Add(sp.ID, sp)

        // Build type handlers and type manager
        let th = seq { 
                        for t in typeHandlers do 
                            yield t.Value.Type.GetConstructor([||]).Invoke([||]) :?> TypeHandler 
                     }
        let tm = new TypeManager(List.ofSeq th)
                    
        // Check that each step has required steps
        for s in steps do
            for rs in s.Value.Dependencies do
                if not (steps.ContainsKey(rs)) then
                    raise (CompilerBuildException("The compiler step processor " + s.Key + " requires step " + rs + " but this step has not been found"))
        
        // Check that each processors has and owner step and a before/after processor
        for p in processors do
            if not (steps.ContainsKey(p.Value.Step)) then
                raise (CompilerBuildException("The compiler step processor " + p.Key + " belongs to the step " + p.Value.Step + " but this step has not been found"))
            for dep in p.Value.Dependencies do
                if not (processors.ContainsKey(dep)) then
                    raise (CompilerBuildException("The compiler step processor " + p.Key + " requires processor " + dep + " but this step has not been found"))
       
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
        let steps = seq {
            if sorted.IsNone then
                raise (CompilerBuildException("Cannot build a pipeline using the specified steps since there is a cycle in steps dependencies"))
            for (id, data) in sorted.Value do
                let procSorted = procGraphs.[id].Sorted
                if procSorted.IsNone then
                    raise (CompilerBuildException("Cannot build the step " + id + " using the specified processors since there is a cycle in processors dependencies"))
                
                // Determine the type of processors accepted by the step
                let procType = ref typeof<Object>
                let constructors = data.Type.GetConstructors()
                for c in constructors do
                    if (c.GetParameters().Length > 0 && c.GetParameters().[0].ParameterType = typeof<TypeManager>) then
                        let procArg = c.GetParameters().[1]
                        procType := procArg.ParameterType.GetGenericArguments().[0]
                        
                // Instantiate a list of proper processors via reflection
                let processors = typeof<List<_>>.GetGenericTypeDefinition().MakeGenericType([| !procType |]).GetConstructor([||]).Invoke([||])
                for (id, procNode) in procSorted.Value do
                    processors.GetType().GetMethod("Add").Invoke(processors, [| procNode.Type.GetConstructor([||]).Invoke([||]) |]) |> ignore
                        
                let flattener = typeof<ConfigurationUtil>.GetMethod("FlattenList").GetGenericMethodDefinition().MakeGenericMethod([| !procType |])
                let flatProcessors = flattener.Invoke(null, [| processors |])
                yield data.Type.GetConstructors().[0].Invoke([| tm; flatProcessors |]) :?> ICompilerStep
        }
        let flatSteps = List.ofSeq(steps)
        flatSteps
      
            
        
        




