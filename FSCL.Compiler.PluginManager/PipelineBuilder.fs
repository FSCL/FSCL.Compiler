namespace FSCL.Compiler.Plugin

open System
open System.Collections.Generic
open FSCL.Compiler
open GraphUtil
open FSCL.Compiler
open System.Reflection

exception CompilerPipelineBuildException of string

type internal CompilerPluginStepGraphNode(id:string, t:Type) =
    member val ID = id with get
    member val Type = t with get
    member val Processors = new Graph<CompilerPluginStepProcessorGraphNode, string>() with get

and internal CompilerPluginStepProcessorGraphNode(id:string, t:Type) =
    member val ID = id with get
    member val Type = t with get
    
type internal CompilerPipelineBuilder() =
    let typeHandlers = new Dictionary<string, TypeHandlerAttribute * Type>()
    let steps = new Dictionary<string, StepAttribute * Type>()
    let stepReplacements = new Dictionary<string, StepAttribute * Type>()
    let processors = new Dictionary<string, StepProcessorAttribute * Type>()
    let processorReplacements = new Dictionary<string, StepProcessorAttribute * Type>()

    member this.AddTypeHandler(info: TypeHandlerAttribute, t: Type) =
        if typeHandlers.ContainsKey(info.Id) then
            raise (CompilerPipelineBuildException("A type handler with ID " + info.Id + " has already been added"))
        else
            typeHandlers.Add(info.Id, (info, t))
            
    member this.AddCompilerStep(info: StepAttribute, t: Type) =
        if info.Replace.IsSome then
            stepReplacements.Add(info.Replace.Value, (info, t))
        else 
            if steps.ContainsKey(info.Id) then
                raise (CompilerPipelineBuildException("A compiler step with ID " + info.Id + " has already been added"))
            else
                steps.Add(info.Id, (info, t))
            
    member this.AddCompilerStepProcessor(info: StepProcessorAttribute, t: Type) =
        if info.Replace.IsSome then
            processorReplacements.Add(info.Replace.Value, (info, t))
        else 
            if processors.ContainsKey(info.Id) then
                raise (CompilerPipelineBuildException("A compiler step processor with ID " + info.Id + " has already been added"))
            else
                processors.Add(info.Id, (info, t))

    member this.RemoveTypeHandler(id: string) =
        typeHandlers.Remove(id)
        
    member this.RemoveCompilerStep(id: string) =
        steps.Remove(id)
        
    member this.RemoveCompilerStepProcessor(id: string) =
        processors.Remove(id)

    member this.Build() =
        let th = seq { 
                        for t in typeHandlers do 
                            match t.Value with
                            | id, typ ->
                                yield typ.GetConstructor([||]).Invoke([||]) :?> TypeHandler 
                     }
        let tm = new TypeManager(List.ofSeq th)
                    
        // Check that each step has required steps
        for s in steps do
            match s.Value with
            | attr, typ ->
                for rs in attr.Dependencies do
                    if not (steps.ContainsKey(rs)) then
                        raise (CompilerPipelineBuildException("The compiler step processor " + s.Key + " requires step " + rs + " but this step has not been found"))
        for s in stepReplacements do
            match s.Value with
            | attr, typ ->
                for rs in attr.Dependencies do
                    if not (steps.ContainsKey(attr.Replace.Value)) then
                        raise (CompilerPipelineBuildException("The compiler step processor " + s.Key + " should replace the step " + attr.Replace.Value + " but this step has not been found"))
        // Inject replacements
        for s in stepReplacements do
            match s.Value with
            | attr, typ ->
                let replaced = steps.[attr.Replace.Value]
                match replaced with
                | r_attr, _ ->
                    steps.[attr.Replace.Value] <- (new StepAttribute(attr.Replace.Value, 
                                                                     Array.append (attr.Dependencies) (r_attr.Dependencies),
                                                                     Array.append (attr.Before) (r_attr.Before)), typ)
                       
        // Check that each processors has and owner step and a before/after processor
        for p in processors do
            match p.Value with
            | attr, typ ->
                if not (steps.ContainsKey(attr.Step)) then
                    raise (CompilerPipelineBuildException("The compiler step processor " + p.Key + " belongs to the step " + attr.Step + " but this step has not been found"))
                for dep in attr.Dependencies do
                    if not (processors.ContainsKey(dep)) then
                        raise (CompilerPipelineBuildException("The compiler step processor " + p.Key + " requires processor " + dep + " but this step has not been found"))
        for p in processorReplacements do
            match p.Value with
            | attr, typ ->
                for rs in attr.Dependencies do
                    if not (steps.ContainsKey(attr.Replace.Value)) then
                        raise (CompilerPipelineBuildException("The compiler step processor " + p.Key + " should replace the processor " + attr.Replace.Value + " but this processor has not been found"))
        // Inject replacements
        for p in processorReplacements do
            match p.Value with
            | attr, typ ->
                let replaced = processors.[attr.Replace.Value]
                match replaced with
                | r_attr, _ ->
                    processors.[attr.Replace.Value] <-  (new StepProcessorAttribute(attr.Replace.Value,
                                                                                    attr.Step,
                                                                                    Array.append (attr.Dependencies) (r_attr.Dependencies),
                                                                                    Array.append (attr.Before) (r_attr.Before)), typ)
        // Create graph of steps
        let graph = new Graph<CompilerPluginStepGraphNode, string>()
        for s in steps do
            match s.Value with
            | attr, typ ->
                graph.Add(s.Key, CompilerPluginStepGraphNode(attr.Id, typ)) |> ignore
        for s in steps do
            match s.Value with
            | attr, typ ->                
                for d in attr.Dependencies do
                    graph.Connect(d, attr.Id)
                for d in attr.Before do
                    if steps.ContainsKey(d) then
                        graph.Connect(attr.Id, d)
                        
        // Foreach step, create graph of processors
        for p in processors do
            match p.Value with
            | attr, typ ->
                graph.Get(attr.Step).Value.Processors.Add(p.Key, CompilerPluginStepProcessorGraphNode(p.Key, typ)) |> ignore
        for p in processors do
            match p.Value with
            | attr, typ ->
                let step = graph.Get(attr.Step).Value
                for d in attr.Dependencies do
                    step.Processors.Connect(d, attr.Id)
                for d in attr.Before do
                    if processors.ContainsKey(d) then
                        step.Processors.Connect(attr.Id, d)

        // Topological sort of steps
        let sorted = graph.Sorted
        let steps = seq {
            if sorted.IsNone then
                raise (CompilerPipelineBuildException("Cannot build a pipeline using the specified steps since there is a cycle in steps dependencies"))
            for (id, data) in sorted.Value do
                let procSorted = data.Processors.Sorted
                if procSorted.IsNone then
                    raise (CompilerPipelineBuildException("Cannot build the step " + id + " using the specified processors since there is a cycle in processors dependencies"))
                
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
                        
                let flattener = typeof<PluginUtil>.GetMethod("FlattenList").GetGenericMethodDefinition().MakeGenericMethod([| !procType |])
                let flatProcessors = flattener.Invoke(this, [| processors |])
                yield data.Type.GetConstructors().[0].Invoke([| tm; flatProcessors |]) :?> ICompilerStep
        }
        let flatSteps = List.ofSeq(steps)
        flatSteps
      
            
        
        




