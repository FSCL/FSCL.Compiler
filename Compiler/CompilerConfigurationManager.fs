namespace FSCL.Compiler.Configuration

open System
open System.IO
open System.Reflection
open System.Collections.Generic
open FSCL.Compiler
open FSCL.Compiler.FunctionPreprocessing
open FSCL.Compiler.ModuleParsing
open FSCL.Compiler.FunctionCodegen
open FSCL.Compiler.FunctionTransformation
open FSCL.Compiler.ModulePreprocessing
open FSCL.Compiler.Types
open FSCL.Compiler.ModuleCodegen
open System.Xml
open System.Xml.Linq

exception CompilerConfigurationException of string

type internal CompilerConfigurationManager() = 
    // Trick to guarantee the default components assemblies are loaded
    static member private defAssemblyComponents = [typeof<SignaturePreprocessor>; typeof<SignatureCodegen>; typeof<ReturnLifting>; typeof<KernelReferenceParser>; typeof<GenericInstantiator>; typeof<ModuleCodegen>; typeof<DefaultTypeHandler>; typeof<VectorTypeHandler>; ]

    // The root where to place configuration file
    static member ConfigurationRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FSCL.Compiler")
    // The root where to place plugins and configuration file
    static member ComponentsRoot = Path.Combine(CompilerConfigurationManager.ConfigurationRoot, "Components")
    
    // Default configuration
    static member DefaultConfiguration() =
        let sources = List<SourceConfiguration>()
        // Create configuration from assembly
        for item in CompilerConfigurationManager.defAssemblyComponents do
            let assembly = item.Assembly
            // Make configuration explicit
            sources.Add(SourceConfiguration(AssemblySource(assembly)))
        CompilerConfiguration(false, List.ofSeq sources)

    // Load from configuration file    
    static member LoadConfiguration(cf:string) =
        let document = XDocument.Load(cf)
        let conf = CompilerConfiguration.FromXml(document, Path.GetDirectoryName(Path.GetFullPath(cf)))
        conf
        
    // Load from configuration file    
    static member LoadConfiguration() =
        let conf = Path.Combine(CompilerConfigurationManager.ConfigurationRoot, "FSCL.Config.xml")
        if not (File.Exists(conf)) then
            let sources = List<SourceConfiguration>()
            let pluginFolder = CompilerConfigurationManager.ComponentsRoot
            if Directory.Exists(pluginFolder) then
                let dlls = Directory.GetFiles(pluginFolder, "*.dll")
                for f in dlls do
                    sources.Add(SourceConfiguration(FileSource(f)))
            CompilerConfiguration(true, List.ofSeq sources)
        else
            CompilerConfiguration(true)
            
    static member StoreConfiguration(conf: CompilerConfiguration, f: string) =
        conf.MakeExplicit().ToXml().Save(f)
              
    static member Build() =
        let conf = CompilerConfigurationManager.LoadConfiguration()
        CompilerConfigurationManager.Build(conf)


    // Build from file
    static member Build(cf: string) =
        let conf = CompilerConfigurationManager.LoadConfiguration(cf)
        CompilerConfigurationManager.Build(conf)
        
    static member Build(conf: CompilerConfiguration) =
        let explicitConf = conf.MergeDefault(CompilerConfigurationManager.DefaultConfiguration())
        CompilerBuilder.Build(explicitConf)

