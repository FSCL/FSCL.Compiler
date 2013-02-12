namespace FSCL.Compiler.Configuration

open System
open System.IO
open System.Reflection
open System.Collections.Generic
open FSCL.Compiler
open FSCL.Compiler.FunctionPreprocessing
open FSCL.Compiler.ModuleParsing
open FSCL.Compiler.FunctionPrettyPrinting
open FSCL.Compiler.FunctionTransformation
open FSCL.Compiler.ModulePreprocessing
open FSCL.Compiler.Types
open FSCL.Compiler.ModulePrettyPrinting
open System.Xml
open System.Xml.Linq

exception CompilerConfigurationException of string

type internal CompilerConfigurationManager() =      
    let mutable compilerKernelModuleType = typeof<KernelModule>
    let pipelineBuilder = new CompilerBuilder()
    // Trick to guarantee the default components assemblies are loaded
    let defAssemblyComponents = [
                                    typeof<SignaturePreprocessor>;
                                    typeof<SignaturePrinter>;
                                    typeof<ReturnLifting>;
                                    typeof<KernelReferenceParser>;
                                    typeof<GenericInstantiator>;
                                    typeof<ModulePrettyPrinter>;
                                    typeof<DefaultTypeHandler>;
                                    typeof<ModuleParsingStep>;
                                ]

    // The root where to place configuration file
    static member ConfigurationRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FSCL.Compiler")
    // The root where to place plugins and configuration file
    static member PluginRoot = Path.Combine(CompilerConfigurationManager.ConfigurationRoot, "Plugins")
      
    // Load from configuration object      
    member this.FromConfiguration(cf: CompilerConfiguration) =
        if cf.LoadDefaultSteps then
            this.ProcessDefault()
        for source in cf.Sources do
            match source with
            | FileSource(f) ->
                if not (File.Exists(f)) then
                    raise (CompilerConfigurationException("Compiler source file " + f + " doesn't exists or is unreadable"))
                let assembly = Assembly.LoadFile(f)
                this.Process(assembly)        
        // Once loaded sources, analyze overridden configuration
        for t in cf.OverrideTypeHandlers do
            if pipelineBuilder.HasTypeHandler(t.ID) then
                if t.OverrideMode = Replace then
                    pipelineBuilder.ReplaceTypeHandler(t.ID, new TypeHandlerAttribute(t.ID, Before = Array.ofSeq(t.Before)))
                else if t.OverrideMode = Merge then
                    let merged = new List<string>()
                    for item in t.Before do
                        merged.Add(item)
                    match pipelineBuilder.TypeHandler(t.ID) with
                    | a, _ ->
                        for item in a.Before do
                            merged.Add(item)
                    // Remove duplicates
                    let m = merged |> Set.ofSeq |> Set.toList
                    pipelineBuilder.ReplaceTypeHandler(t.ID, new TypeHandlerAttribute(t.ID, Before = Array.ofSeq(m)))
                else
                    pipelineBuilder.RemoveTypeHandler(t.ID) |> ignore
        // Overridden steps
        for s in cf.OverrideSteps do
            if pipelineBuilder.HasCompilerStep(s.ID) then
                if s.OverrideMode = Replace then
                    pipelineBuilder.ReplaceCompilerStep(s.ID, new StepAttribute(s.ID, Dependencies = Array.ofSeq(s.Dependencies), Before = Array.ofSeq(s.Before)))
                else if s.OverrideMode = Merge then
                    let temp = new List<string>()
                    for item in s.Before do
                        temp.Add(item)
                    match pipelineBuilder.CompilerStep(s.ID) with
                    | a, _ ->
                        for item in a.Before do
                            temp.Add(item)
                    // Remove duplicates
                    let beforeMerged = temp |> Set.ofSeq |> Set.toList
                    temp.Clear()
                    for item in s.Dependencies do
                        temp.Add(item)
                    match pipelineBuilder.CompilerStep(s.ID) with
                    | a, _ ->
                        for item in a.Dependencies do
                            temp.Add(item)
                    // Remove duplicates
                    let depMerged = temp |> Set.ofSeq |> Set.toList
                    pipelineBuilder.ReplaceCompilerStep(s.ID, new StepAttribute(s.ID, Before = Array.ofSeq(beforeMerged), Dependencies = Array.ofSeq(depMerged)))
                else
                    pipelineBuilder.RemoveCompilerStep(s.ID) |> ignore
        // Overridden processors
        for p in cf.OverrideProcessors do
            if pipelineBuilder.HasCompilerStepProcessor(p.ID) then
                let (oldProc, oldProcType) = pipelineBuilder.CompilerStepProcessor(p.ID)
                if p.OverrideMode = Replace then
                    pipelineBuilder.ReplaceCompilerStepProcessor(p.ID, new StepProcessorAttribute(p.ID, oldProc.Step, Dependencies = Array.ofSeq(p.Dependencies), Before = Array.ofSeq(p.Before)))
                else if p.OverrideMode = Merge then
                    let temp = new List<string>()
                    for item in p.Before do
                        temp.Add(item)
                    match pipelineBuilder.CompilerStepProcessor(p.ID) with
                    | a, _ ->
                        for item in a.Before do
                            temp.Add(item)
                    // Remove duplicates
                    let beforeMerged = temp |> Set.ofSeq |> Set.toList
                    temp.Clear()
                    for item in p.Dependencies do
                        temp.Add(item)
                    match pipelineBuilder.CompilerStepProcessor(p.ID) with
                    | a, _ ->
                        for item in a.Dependencies do
                            temp.Add(item)
                    // Remove duplicates
                    let depMerged = temp |> Set.ofSeq |> Set.toList
                    pipelineBuilder.ReplaceCompilerStepProcessor(p.ID, new StepProcessorAttribute(p.ID, oldProc.Step, Before = Array.ofSeq(beforeMerged), Dependencies = Array.ofSeq(depMerged)))
                else
                    pipelineBuilder.RemoveCompilerStepProcessor(p.ID) |> ignore
    
    // Load from configuration file    
    member this.FromFile(cf: string) =
        let document = XDocument.Load(cf)
        let conf = CompilerConfiguration.FromXml(document)
        this.FromConfiguration(conf)
        
    // Load from configuration file    
    member this.FromStorage() =
        let conf = Path.Combine(CompilerConfigurationManager.ConfigurationRoot, "conf.xml")
        if not (File.Exists(conf)) then
            this.ProcessDefault()
            let pluginFolder = CompilerConfigurationManager.PluginRoot
            if Directory.Exists(pluginFolder) then
                let dlls = Directory.GetFiles(pluginFolder, "*.dll")
                for f in dlls do
                    this.Process(Assembly.LoadFile(f))
        else
            this.FromFile(conf)

    member this.Process(assembly: Assembly) =
        // Analyze assembly content
        for t in assembly.GetTypes() do
            let thAttribute = t.GetCustomAttribute<TypeHandlerAttribute>()
            if thAttribute <> null then
                pipelineBuilder.AddTypeHandler(thAttribute, t)

            let stepAttribute = t.GetCustomAttribute<StepAttribute>()
            if stepAttribute <> null then
                pipelineBuilder.AddCompilerStep(stepAttribute, t)

            let procAttribute = t.GetCustomAttribute<StepProcessorAttribute>()
            if procAttribute <> null then
                pipelineBuilder.AddCompilerStepProcessor(procAttribute, t)
                
    member this.ProcessDefault() =
        // Analyze assembly content
        let sources = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
        for s in sources do
            let assembly = Assembly.Load(s)
            for t in assembly.GetTypes() do
                let thAttribute = t.GetCustomAttribute<TypeHandlerAttribute>()
                if thAttribute <> null then
                    pipelineBuilder.AddTypeHandler(thAttribute, t)

                let stepAttribute = t.GetCustomAttribute<StepAttribute>()
                if stepAttribute <> null then
                    pipelineBuilder.AddCompilerStep(stepAttribute, t)

                let procAttribute = t.GetCustomAttribute<StepProcessorAttribute>()
                if procAttribute <> null then
                    pipelineBuilder.AddCompilerStepProcessor(procAttribute, t)

    member this.Build() =
        pipelineBuilder.Build()