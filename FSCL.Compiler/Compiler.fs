namespace FSCL.Compiler

open System
open System.IO
open System.Reflection
open FSCL.Compiler
open FSCL.Compiler.Configuration

type Compiler =  
    val mutable private pluginManager : CompilerConfigurationManager
    val mutable private steps : ICompilerStep list
        
    new() as this = { pluginManager = new CompilerConfigurationManager(); steps = []; }   
                    then
                        this.pluginManager.FromStorage()
                        this.steps <- this.pluginManager.Build()
    
    new(file: string) as this = { pluginManager = new CompilerConfigurationManager(); steps = []; }   
                                then
                                    this.pluginManager.FromFile(file)
                                    this.steps <- this.pluginManager.Build()
    
    new(conf: CompilerConfiguration) as this = { pluginManager = new CompilerConfigurationManager(); steps = []; }   
                                                then
                                                    this.pluginManager.FromConfiguration(conf)
                                                    this.steps <- this.pluginManager.Build()
                                                    
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
