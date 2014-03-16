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
open FSCL.Compiler.Types
open System.Collections.ObjectModel
open System.Collections.Generic
///
///<summary>
///The FSCL compiler
///</summary>
///
[<AllowNullLiteral>]
type Compiler =  
    static member private defConfRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FSCL.Compiler")
    static member private defConfCompRoot = "Components"

    static member private defComponentsAssemply = 
        [| typeof<FunctionPreprocessingStep>; 
           typeof<FunctionTransformationStep>; 
           typeof<FunctionCodegenStep>; 
           typeof<ModuleParsingStep>; 
           typeof<ModulePreprocessingStep>; 
           typeof<ModuleCodegenStep>; 
           typeof<DefaultTypeHandler> |]

    val mutable private steps : ICompilerStep array
    val mutable private configuration: PipelineConfiguration
    val mutable private configurationManager: PipelineConfigurationManager

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
    new() as this = { steps = [||]; 
                      configurationManager = new PipelineConfigurationManager(Compiler.defComponentsAssemply, Compiler.defConfRoot, Compiler.defConfCompRoot); 
                      configuration = null }   
                    then
                        this.configuration <- this.configurationManager.DefaultConfiguration()
                        this.steps <- this.configurationManager.Build(this.configuration)
    
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with a file-based configuration
    ///</summary>
    ///<param name="file">The absolute or relative path of the configuration file</param>
    ///<returns>
    ///An instance of the compiler configured with the input file
    ///</returns>
    ///
    new(file: string) as this = { steps = [||]; 
                                  configurationManager = new PipelineConfigurationManager(Compiler.defComponentsAssemply, Compiler.defConfRoot, Compiler.defConfCompRoot); 
                                  configuration = null }   
                                then
                                    this.configuration <- this.configurationManager.LoadConfiguration(file)
                                    this.steps <- this.configurationManager.Build(this.configuration)
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with an object-based configuration
    ///</summary>
    ///<param name="conf">The configuration object</param>
    ///<returns>
    ///An instance of the compiler configured with the input configuration object
    ///</returns>
    ///
    new(conf: PipelineConfiguration) as this = { steps = [||]; 
                                                 configurationManager = new PipelineConfigurationManager(Compiler.defComponentsAssemply, Compiler.defConfRoot, Compiler.defConfCompRoot); 
                                                 configuration = conf }   
                                               then
                                                   this.steps <- this.configurationManager.Build(this.configuration)
    ///
    ///<summary>
    ///The compiler configuration
    ///</summary>
    ///                                                
    member this.Configuration 
        with get() =
            this.configuration

    ///
    ///<summary>
    ///The method to be invoke to compile a managed kernel
    ///</summary>
    ///  
    member this.Compile(input, opts) =
        let mutable state = input
        let mutable stopCompilation = false
        let mutable i = 0
        while not(stopCompilation) && i < this.steps.Length do
            match this.steps.[i].Execute(state, opts) with
            | StopCompilation(r) ->
                state <- r
                stopCompilation <- true
            | ValidResult(o) ->
                state <- o
                i <- i + 1
        state
        
    member this.Compile(input) =
        this.Compile(input, new ReadOnlyDictionary<string, obj>(new Dictionary<string, obj>()))
        
    member this.CollectMetadataComparisonFunctions =
        let comp = new List<DynamicKernelMetadataCollection * DynamicKernelMetadataCollection -> bool>()
        for s in this.steps do
            match s.BehaveDifferentlyWithKernelMetadata with
            | Some(f) ->
                comp.Add(f)
            | _ ->
                ()
            for p in s.Processors do
                match p.BehaveDifferentlyWithKernelMetadata with
                | Some(f) ->
                    comp.Add(f)
                | _ ->
                    ()                
        comp
          
    static member DefaultConfigurationRoot() =
        Compiler.defConfCompRoot

    static member DefaultConfigurationComponentsRoot() =
        Path.Combine(Compiler.defConfCompRoot, Compiler.defConfCompRoot)

    static member DefaultComponents() =
        Compiler.defComponentsAssemply
    (*
        let typeManager = new TypeManager([ new DefaultTypeHandler();
                                            new RefVariableTypeHandler()])
                               
        let parser = new ModuleParsingStep(typeManager, [ new KernelReferenceParser();
                                                          new KernelMethodInfoParser() ])

        let moduleBuilder = new ModulePreprocessingStep(typeManager, [ new GenericInstantiator();
                                                                       new FunctionReferenceDiscover() ])

        let preprocessor = new FunctionPreprocessingStep(typeManager, [ new SignaturePreprocessor();
                                                                        new RefVariablePreprocessor() ])
        
        let transformation = new FunctionTransformationStep(typeManager, [ new ReturnTypeTransformation();
                                                                           new GlobalVarRefTransformation();
                                                                           new ConditionalAssignmentTransformation();
                                                                           new ArrayAccessTransformation();
                                                                           new RefVariableTransformationProcessor();
                                                                           new ReturnLifting() ])

        let printer = new FunctionPrettyPrintingStep(typeManager, [ new SignaturePrinter();
                                                                  
        // ArrayAccess -> ArithmeticOperation -> Call order is important (to be fixed)
                                                                    new ArrayAccessPrinter();
                                                                    new ArithmeticOperationPrinter();
                                                                    new ForInPrinter();
                                                                    new CallPrinter();
                                                                    new ValuePrinter();
                                                                    new VarPrinter();
                                                                    new IfThenElsePrinter();
                                                                    new WhileLoopPrinter();
                                                                    new VarSetPrinter();
                                                                    new UnionCasePrinter();
                                                                    new DeclarationPrinter();
                                                                    new SequentialPrinter();
                                                                    new IntegerRangeLoopPrinter() ])
                                                                    
        let finalizer = new ModulePrettyPrintingStep(typeManager, [ new ModulePrettyPrinter() ])

        // Run pipeline
        let pipeline = new CompilerPipeline(typeManager, [ parser;
                                                           moduleBuilder;
                                                           preprocessor;
                                                           transformation;
                                                           printer;
                                                           finalizer ]);
        pipeline
        *)
