namespace FSCL.Compiler

open System
open System.IO
open System.Reflection
open FSCL.Compiler
open FSCL.Compiler.Configuration
open FSCL.Compiler.ModuleParsing
open FSCL.Compiler.ModulePreprocessing
open FSCL.Compiler.ModuleCodegen
open FSCL.Compiler.FunctionPreprocessing
open FSCL.Compiler.FunctionCodegen
open FSCL.Compiler.FunctionTransformation
open FSCL.Compiler.FunctionPostprocessing
open FSCL.Compiler.AcceleratedCollections
open FSCL.Compiler.Types
open System.Collections.Generic
open Microsoft.FSharp.Quotations

///
///<summary>
///The FSCL compiler
///</summary>
/////
//type KernelCacheItem(info, code, defines) =
//    member val Kernel:IKernelInfo = info with get 
//    member val OpenCLCode:String = code with get, set
//    member val DynamicDefines: IReadOnlyDictionary<string, Var option * Expr option * obj> = defines with get
//    // List of devices and kernel instances potentially executing the kernel
//    
//type KernelCache() =
//    member val Kernels = Dictionary<FunctionInfoID, List<ReadOnlyMetaCollection * KernelCacheItem>>() 
//        with get    
//    member this.TryFindCompatibleOpenCLCachedKernel(id: FunctionInfoID, 
//                                                    meta: ReadOnlyMetaCollection,
//                                                    openCLMetadataVerifier: ReadOnlyMetaCollection * ReadOnlyMetaCollection -> bool) =
//        if this.Kernels.ContainsKey(id) then
//            let potentialKernels = this.Kernels.[id]
//            // Check if compatible kernel meta in cached kernels
//            let item = Seq.tryFind(fun (cachedMeta: ReadOnlyMetaCollection, cachedKernel: KernelCacheItem) ->
//                                        openCLMetadataVerifier(cachedMeta, meta)) potentialKernels
//            match item with
//            | Some(m, k) ->
//                Some(k)
//            | _ ->
//                None
//        else
//            None                  
  
[<AllowNullLiteral>]
type Compiler = 
    inherit Pipeline
    
    static member DefaultConfigurationRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FSCL.Compiler")
    static member DefaultConfigurationComponentsFolder = "Components"
    static member DefaultConfigurationComponentsRoot = Path.Combine(Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder)

    static member private defComponentsAssemply = 
        [| typeof<FunctionPreprocessingStep> 
           typeof<FunctionTransformationStep> 
           typeof<FunctionCodegenStep> 
           typeof<ModuleParsingStep> 
           typeof<ModulePreprocessingStep> 
           typeof<ModuleCodegenStep>
           typeof<FunctionPostprocessingStep>
           typeof<AcceleratedArrayParser>
           typeof<DefaultTypeHandler> |]
    
    val mutable cache: KernelCache 
    val cacheEntryCreator: IKernelModule -> KernelCacheEntry
    //val cache: KernelCache
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
    new (entryCreator: IKernelModule -> KernelCacheEntry) as this = 
        { inherit Pipeline (Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder, Compiler.defComponentsAssemply)
          cache = null
          cacheEntryCreator = entryCreator
        }
        then
            this.cache <- KernelCache(this.IsInvariantToMetaCollection, entryCreator)
    new () = 
        Compiler (fun m -> KernelCacheEntry m)

    ///
    ///<summary>
    ///The constructor to instantiate a compiler with a file-based configuration
    ///</summary>
    ///<param name="file">The absolute or relative path of the configuration file</param>
    ///<returns>
    ///An instance of the compiler configured with the input file
    ///</returns>
    ///
    new (file: string, entryCreator) as this = 
        { inherit Pipeline(Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder, Compiler.defComponentsAssemply, file)
          cache = null
          cacheEntryCreator = entryCreator
        }
        then
            this.cache <- KernelCache(this.IsInvariantToMetaCollection, entryCreator)
    new(file: string) = 
        Compiler (file, fun m -> KernelCacheEntry m)
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with an object-based configuration
    ///</summary>
    ///<param name="conf">The configuration object</param>
    ///<returns>
    ///An instance of the compiler configured with the input configuration object
    ///</returns>
    ///
    new (conf: PipelineConfiguration, entryCreator) as this =
        { inherit Pipeline(Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder, Compiler.defComponentsAssemply, conf)
          cache = null
          cacheEntryCreator = entryCreator
        }
        then
            this.cache <- KernelCache(this.IsInvariantToMetaCollection, entryCreator)
    new (conf: PipelineConfiguration) =
        Compiler(conf, fun m -> KernelCacheEntry m)
        
    member this.Compile(input, 
                        opts:Map<string,obj>) =                        
        this.Run ((box input, this.cache), opts)
                
    member this.Compile input =
        this.Compile (input, Map.empty)

    member this.Compile<'T> input =
        this.Compile (input, Map.empty) :?> 'T

    member this.CacheEntryCreator 
        with get() =
            this.cacheEntryCreator
        
        
    