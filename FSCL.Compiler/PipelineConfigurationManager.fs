namespace FSCL.Compiler.Configuration

open System
open System.IO
open System.Reflection
open System.Collections.Generic
open System.Xml
open System.Xml.Linq

exception PipelineConfigurationException of string

type PipelineConfigurationManager(defAssemblyComp:Type array, confRoot, compRoot) = 
    do
        if not (Directory.Exists(Path.Combine(confRoot, compRoot))) then
            Directory.CreateDirectory(Path.Combine(confRoot, compRoot)) |> ignore

    // Trick to guarantee the default components assemblies are loaded
    member val private defAssemblyComponents = defAssemblyComp
    // The root where to place configuration file
    member val ConfigurationRoot = confRoot
    //Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FSCL.Compiler")
    // The root where to place plugins and configuration file
    member val ComponentsRoot = Path.Combine(confRoot, compRoot)
    //Path.Combine(PipelineConfigurationManager.ConfigurationRoot, "Components")
    
        
    // Default configuration
    member this.DefaultConfiguration() =
        let sources = List<SourceConfiguration>()
        // Create configuration from assembly
        for item in this.defAssemblyComponents do
            let assembly = item.Assembly
            // Make configuration explicit
            sources.Add(SourceConfiguration(AssemblySource(assembly)))
        PipelineConfiguration(false, Array.ofSeq sources)

    // Load from configuration file    
    member this.LoadConfiguration(cf:string) =
        let document = XDocument.Load(cf)
        let conf = PipelineConfiguration.FromXml(document, Path.GetDirectoryName(Path.GetFullPath(cf)))
        conf
        
    // Load from configuration file    
    member this.LoadConfiguration() =
        let conf = Path.Combine(this.ConfigurationRoot, "FSCL.Config.xml")
        if not (File.Exists(conf)) then
            let sources = List<SourceConfiguration>()
            let pluginFolder = this.ComponentsRoot
            if Directory.Exists(pluginFolder) then
                let dlls = Directory.GetFiles(pluginFolder, "*.dll")
                for f in dlls do
                    sources.Add(SourceConfiguration(FileSource(f)))
            PipelineConfiguration(true, Array.ofSeq sources)
        else
            PipelineConfiguration(true)
            
    member this.StoreConfiguration(conf: PipelineConfiguration, f: string) =
        conf.MakeExplicit().ToXml().Save(f)
              
    member this.Build() =
        let conf = this.LoadConfiguration()
        this.Build(conf)


    // Build from file
    member this.Build(cf: string) =
        let conf = this.LoadConfiguration(cf)
        this.Build(conf)
        
    member this.Build(conf: PipelineConfiguration) =
        let explicitConf = conf.MergeDefault(this.DefaultConfiguration())
        PipelineBuilder.Build(explicitConf)

