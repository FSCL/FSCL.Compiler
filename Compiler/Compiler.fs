namespace FSCL.Compiler

open System
open System.IO
open System.Reflection
open FSCL.Compiler
open FSCL.Compiler.Configuration
  
///
///<summary>
///The FSCL compiler
///</summary>
///
type Compiler =  
    val mutable private steps : ICompilerStep list
    val mutable private configuration: CompilerConfiguration
    
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
    new() as this = { steps = []; configuration = CompilerConfigurationManager.LoadConfiguration() }   
                    then
                        this.steps <- CompilerConfigurationManager.Build()
    
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with a file-based configuration
    ///</summary>
    ///<param name="file">The absolute or relative path of the configuration file</param>
    ///<returns>
    ///An instance of the compiler configured with the input file
    ///</returns>
    ///
    new(file: string) as this = { steps = []; configuration = CompilerConfigurationManager.LoadConfiguration(file) }   
                                then
                                    this.steps <- CompilerConfigurationManager.Build(file)
    ///
    ///<summary>
    ///The constructor to instantiate a compiler with an object-based configuration
    ///</summary>
    ///<param name="conf">The configuration object</param>
    ///<returns>
    ///An instance of the compiler configured with the input configuration object
    ///</returns>
    ///
    new(conf: CompilerConfiguration) as this = { steps = []; configuration = conf }   
                                                then
                                                    this.steps <- CompilerConfigurationManager.Build(conf)
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
    member this.Compile(input) =
        let mutable state = (input :> obj, new KernelModule()) :> obj
        //let timer = new System.Diagnostics.Stopwatch()
        for step in this.steps do
            //timer.Reset()
            //timer.Start()
            state <- step.Execute(state)
            //timer.Stop()
            //Console.WriteLine("Step " + (step.GetType().ToString()) + ": " + timer.ElapsedMilliseconds.ToString() + "ms")
        state
          
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
