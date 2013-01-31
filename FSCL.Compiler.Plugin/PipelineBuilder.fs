namespace FSCL.Compiler.Plugin

open System
open System.Collections.Generic
open FSCL.Compiler
open GraphUtil

exception CompilerPipelineBuildException of string

type CompilerPluginStepInfo(id: string, t: Type, before: string, after: string, requires: string list) =
    member val ID = id with get
    member val Type = t with get
    member val Before = before with get
    member val After = after with get
    member val Requires = requires with get
    
type CompilerPluginStepProcessorInfo(id: string, stepId: string, t: Type, before: string, after: string, requires: (string * string) list) =
    member val ID = id with get
    member val StepID = stepId with get
    member val Type = t with get
    member val Before = before with get
    member val After = after with get
    member val Requires = requires with get

type CompilerPluginStepGraphNode(id:string, t:Type) =
    member val ID = id with get
    member val Type = t with get
    member val Processors = new Graph<CompilerPluginStepProcessorGraphNode, string>() with get

and CompilerPluginStepProcessorGraphNode(id:string, t:Type) =
    member val ID = id with get
    member val Type = t with get
    
type CompilerPipelineBuilder(typeManager: TypeManager,
                             kernelModuleType: Type) =
    let typeHandlers = new Dictionary<string, TypeHandler>()
    let steps = new Dictionary<string, CompilerPluginStepInfo>()
    let processors = new Dictionary<string, CompilerPluginStepProcessorInfo>()
    
    member this.Add(id: string, th: TypeHandler) =
        if typeHandlers.ContainsKey(id) then
            raise (CompilerPipelineBuildException("A type handler with ID " + id + " has already been added"))
        else
            typeHandlers.Add(id, th)
            
    member this.Add(info: CompilerPluginStepInfo) =
        if steps.ContainsKey(info.ID) then
            raise (CompilerPipelineBuildException("A compiler step with ID " + info.ID + " has already been added"))
        else
            steps.Add(info.ID, info)
            
    member this.Add(info: CompilerPluginStepProcessorInfo) =
        if processors.ContainsKey(info.ID) then
            raise (CompilerPipelineBuildException("A compiler step processor with ID " + info.ID + " has already been added"))
        else
            processors.Add(info.ID, info)

    member this.RemoveTypeHandler(id: string) =
        typeHandlers.Remove(id)

    member this.RemoveCompilerStep(id: string) =
        steps.Remove(id)
        
    member this.RemoveCompilerStepProcessors(id: string) =
        processors.Remove(id)

    member this.Build() =
        // Check that each step has required steps
        for s in steps do
            for rs in s.Value.Requires do
                if not (steps.ContainsKey(rs)) then
                    raise (CompilerPipelineBuildException("The compiler step processor " + s.Key + " requires step " + rs + " but this step has not been found"))
        // Check that each processors has and owner step and a before/after processor
        for p in processors do
            if not (steps.ContainsKey(p.Value.StepID)) then
                raise (CompilerPipelineBuildException("The compiler step processor " + p.Key + " belongs to the step " + p.Value.StepID + " but this step has not been found"))
            for (step, processor) in p.Value.Requires do
                if not (steps.ContainsKey(step)) then
                    raise (CompilerPipelineBuildException("The compiler step processor " + p.Key + " requires step " + step + " but this step has not been found"))
                if not (processors.ContainsKey(processor)) then
                    raise (CompilerPipelineBuildException("The compiler step processor " + p.Key + " requires processor " + processor + " but this step has not been found"))

        // Create graph of steps
        let graph = new Graph<CompilerPluginStepGraphNode, string>()
        for s in steps do
            graph.Add(s.Key, CompilerPluginStepGraphNode(s.Value.ID, s.Value.Type)) |> ignore
        for s in steps do
            graph.Connect(s.Value.After, s.Value.ID)
            graph.Connect(s.Value.ID, s.Value.Before)
        // Foreach step, create graph of processors
        for p in processors do
            graph.Get(p.Value.StepID).Value.Processors.Add(p.Key, CompilerPluginStepProcessorGraphNode(p.Key, p.Value.Type)) |> ignore
        for p in processors do
            let step = graph.Get(p.Value.StepID).Value
            step.Processors.Connect(p.Key, p.Value.Before) |> ignore
            step.Processors.Connect(p.Value.After, p.Key) |> ignore

        // Topological sort of steps
        if graph.Sorted.IsNone then
            raise (CompilerPipelineBuildException("Cannot build a pipeline using the specified steps since there is a cycle in steps dependencies"))
        for (id, data) in graph.Sorted.Value do
            if data.Processors.Sorted.IsNone then
                raise (CompilerPipelineBuildException("Cannot build the step " + id + " using the specified processors since there is a cycle in processors dependencies"))
          
        




