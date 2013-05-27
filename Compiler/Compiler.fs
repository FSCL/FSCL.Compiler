namespace FSCL.Compiler

open System
open System.IO
open System.Reflection
open FSCL.Compiler
open FSCL.Compiler.Configuration

type Compiler =  
    val mutable private steps : ICompilerStep list
    val mutable private configuration: CompilerConfiguration

    new() as this = { steps = []; configuration = CompilerConfigurationManager.LoadConfiguration() }   
                    then
                        this.steps <- CompilerConfigurationManager.Build()
    
    new(file: string) as this = { steps = []; configuration = CompilerConfigurationManager.LoadConfiguration(file) }   
                                then
                                    this.steps <- CompilerConfigurationManager.Build(file)
    
    new(conf: CompilerConfiguration) as this = { steps = []; configuration = conf }   
                                                then
                                                    this.steps <- CompilerConfigurationManager.Build(conf)
                                                    
    member this.Configuration 
        with get() =
            this.configuration

    member this.Compile(input) =
        let mutable state = input
        for step in this.steps do
            state <- step.Execute(state)
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
