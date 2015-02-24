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
///
type KernelCacheItem(info, code, defines) =
    member val Kernel:IKernelInfo = info with get 
    member val OpenCLCode:String = code with get, set
    member val DynamicDefines: IReadOnlyDictionary<string, Var option * Expr option * obj> = defines with get
    // List of devices and kernel instances potentially executing the kernel
    
type KernelCache() =
    member val Kernels = Dictionary<FunctionInfoID, List<ReadOnlyMetaCollection * KernelCacheItem>>() 
        with get    
    member this.TryFindCompatibleOpenCLCachedKernel(id: FunctionInfoID, 
                                                    meta: ReadOnlyMetaCollection,
                                                    openCLMetadataVerifier: ReadOnlyMetaCollection * ReadOnlyMetaCollection -> bool) =
        if this.Kernels.ContainsKey(id) then
            let potentialKernels = this.Kernels.[id]
            // Check if compatible kernel meta in cached kernels
            let item = Seq.tryFind(fun (cachedMeta: ReadOnlyMetaCollection, cachedKernel: KernelCacheItem) ->
                                        openCLMetadataVerifier(cachedMeta, meta)) potentialKernels
            match item with
            | Some(m, k) ->
                Some(k)
            | _ ->
                None
        else
            None                  
  
[<AllowNullLiteral>]
type Compiler = 
    inherit Pipeline
    
    static member DefaultConfigurationRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FSCL.Compiler")
    static member DefaultConfigurationComponentsFolder = "Components"
    static member DefaultConfigurationComponentsRoot = Path.Combine(Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder)

    static member private defComponentsAssemply = 
        [| typeof<FunctionPreprocessingStep>; 
           typeof<FunctionTransformationStep>; 
           typeof<FunctionCodegenStep>; 
           typeof<ModuleParsingStep>; 
           typeof<ModulePreprocessingStep>; 
           typeof<ModuleCodegenStep>;
           typeof<FunctionPostprocessingStep>;
           typeof<AcceleratedArrayParser>;
           typeof<DefaultTypeHandler> |]
     
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
    new() = 
        { inherit Pipeline(Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder, Compiler.defComponentsAssemply) }
    
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with a file-based configuration
    ///</summary>
    ///<param name="file">The absolute or relative path of the configuration file</param>
    ///<returns>
    ///An instance of the compiler configured with the input file
    ///</returns>
    ///
    new(file: string) = 
        { inherit Pipeline(Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder, Compiler.defComponentsAssemply, file) }
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with an object-based configuration
    ///</summary>
    ///<param name="conf">The configuration object</param>
    ///<returns>
    ///An instance of the compiler configured with the input configuration object
    ///</returns>
    ///
    new(conf: PipelineConfiguration) =
        { inherit Pipeline(Compiler.DefaultConfigurationRoot, Compiler.DefaultConfigurationComponentsFolder, Compiler.defComponentsAssemply, conf) }

    member this.Compile(input, opts) =
        this.Run((box input, this.IsInvariantToMetaCollection), opts)
        
    member this.Compile(input, [<ParamArray>] args: (string * obj)[]) =
        let opts = new Dictionary<string, obj>()
        for key, value in args do
            if not (opts.ContainsKey(key)) then
                opts.Add(key, value)
            else
                opts.[key] <- value
        this.Run(input, opts)
        
    member this.Compile(input) =
        this.Compile(input, new Dictionary<string, obj>())
    