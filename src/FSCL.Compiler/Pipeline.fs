namespace FSCL.Compiler.Configuration

open System
open System.IO
open System.Reflection
open FSCL.Compiler
open FSCL.Language
open FSCL
open System.Collections.Generic
///
///<summary>
/// A step-processor pipeline
///</summary>
///
[<AllowNullLiteral>]
type Pipeline =  
    val mutable internal steps : ICompilerStep array
    val mutable internal usedMetadata : IReadOnlyDictionary<Type, MetadataComparer list>
    val mutable internal configuration: PipelineConfiguration
    val mutable internal configurationManager: PipelineConfigurationManager


    ///
    ///<summary>
    ///The default constructor of the compiler
    ///</summary>
    ///<returns>
    ///An instance of the compiler with the default configuration
    ///</returns>
    ///<remarks>
    ///Defalt configuration means that the FSCL.Compiler configuration folder is checked for the presence of a configuration file.
    ///If no configuration file is found, the Plugins subfolder is scanned looking for the components of the compiler to load.
    ///In addition of the 0 or more components found, the native components are always loaded (if no configuration file is found in the first step)
    ///</remarks>
    ///
    new (confRoot, compRoot, assemblies) as this = 
        {   steps = [||]
            usedMetadata = null
            configurationManager = PipelineConfigurationManager (assemblies, confRoot, compRoot)
            configuration = null 
        } then
            this.configuration <- this.configurationManager.DefaultConfiguration ()
            let s, m = this.configurationManager.Build this.configuration
            this.steps <- s
            this.usedMetadata <- Dictionary<Type, MetadataComparer list> m
    
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with a file-based configuration
    ///</summary>
    ///<param name="file">The absolute or relative path of the configuration file</param>
    ///<returns>
    ///An instance of the compiler configured with the input file
    ///</returns>
    ///
    new (confRoot, compRoot, assemblies, file:string) as this = 
        {   steps = [||]
            usedMetadata = null
            configurationManager = PipelineConfigurationManager (assemblies, confRoot, compRoot)
            configuration = null 
        }   
        then
            this.configuration <- this.configurationManager.LoadConfiguration file
            let s,m = this.configurationManager.Build this.configuration
            this.steps <- s
            this.usedMetadata <- Dictionary<Type, MetadataComparer list> m
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with an object-based configuration
    ///</summary>
    ///<param name="conf">The configuration object</param>
    ///<returns>
    ///An instance of the compiler configured with the input configuration object
    ///</returns>
    ///
    new (confRoot, compRoot, assemblies, conf: PipelineConfiguration) as this = 
        {   steps = [||]
            usedMetadata = null
            configurationManager = PipelineConfigurationManager (assemblies, confRoot, compRoot)
            configuration = conf 
        } then
            let s, m = this.configurationManager.Build this.configuration
            this.steps <- s
            this.usedMetadata <- Dictionary<Type, MetadataComparer list> m
    
    ///
    ///<summary>
    /// The compiler configuration
    ///</summary>
    ///                                                
    member this.Configuration with get () = this.configuration

    ///
    ///<summary>
    /// The set of metadata affecting compilation result
    ///</summary>
    ///                                                
    member this.UsedMetadata with get () = this.usedMetadata
            

    ///
    ///<summary>
    /// The steps count
    ///</summary>
    ///                                                
    member this.StepsCount with get () = this.steps.Length
            
    ///
    ///<summary>
    /// The set of metadata affecting compilation result
    ///</summary>
    ///                                                
    member private this.IsInvariantToKernelMeta (meta1: IKernelMetaCollection, meta2: IKernelMetaCollection) =
        // Check all the meta types effectively used
        let firstMetaNotInvariant = 
            this.UsedMetadata 
            |> Seq.tryFind (fun (metaItem: KeyValuePair<Type, MetadataComparer list>) -> 
                if not (typeof<KernelMetadataAttribute>.IsAssignableFrom metaItem.Key) then false else
                // Check if some steps or processors use this meta
                let item1 = meta1.Get metaItem.Key
                let item2 = meta2.Get metaItem.Key
                // Now compare the two values of meta against all the comparers
                let firstFalseComparer = 
                    metaItem.Value 
                    |> List.tryFind (fun(comp: MetadataComparer) -> not (comp.MetaEquals (item1, item2)))
                firstFalseComparer.IsSome
            )
        firstMetaNotInvariant.IsNone
                          
    member private this.IsInvariantToParamMeta (meta1: IParamMetaCollection, meta2: IParamMetaCollection) =
        // Check all the meta types effectively used
        let firstMetaNotInvariant = 
            this.UsedMetadata 
            |> Seq.tryFind (fun (metaItem: KeyValuePair<Type, MetadataComparer list>) -> 
                if not (typeof<ParameterMetadataAttribute>.IsAssignableFrom metaItem.Key) then false else
                // Check if some steps or processors use this meta
                let item1 = meta1.Get metaItem.Key
                let item2 = meta2.Get metaItem.Key
                // Now compare the two values of meta against all the comparers
                let firstFalseComparer = 
                    metaItem.Value 
                    |> List.tryFind (fun (comp: MetadataComparer) -> not (comp.MetaEquals (item1, item2)))
                firstFalseComparer.IsSome
            )
        firstMetaNotInvariant.IsNone
        
    member this.IsInvariantToMetaCollection (meta1: ReadOnlyMetaCollection, meta2: ReadOnlyMetaCollection) =
        // Check all the meta types effectively used
        if not (this.IsInvariantToKernelMeta (meta1.KernelMeta, meta2.KernelMeta)) then false
        elif not (this.IsInvariantToParamMeta (meta1.ReturnMeta, meta2.ReturnMeta)) then false
        elif meta1.ParamMeta.Count <> meta2.ParamMeta.Count then false
        else 
            let mutable found = false
            let mutable i = 0
            while (not found && i < meta1.ParamMeta.Count) do
                if not (this.IsInvariantToParamMeta (meta1.ParamMeta.[i], meta2.ParamMeta.[i])) 
                then found <- true
                else i <- i + 1
            not found

    ///
    ///<summary>
    ///The method to be invoke to compile a managed kernel
    ///</summary>
    ///  
    member this.Run (input, opts: Map<string, obj>) =
        let mutable state = input
        let mutable stopCompilation = false
        let mutable i = 0
        while not stopCompilation && i < this.steps.Length do
            match this.steps.[i].Execute (state, opts) with
            | StopCompilation r ->
                state <- r
                stopCompilation <- true
            | ContinueCompilation o ->
                state <- o
                i <- i + 1
        state
        
    member this.Run input = this.Run (input, Map.empty)