namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler.Processors

type CompilerPipeline(typeManager: TypeManager,
                      kernelModuleType: Type) =
    inherit ICompilerPipeline(typeManager, kernelModuleType)

    let SetGlobalData(d:Dictionary<string, obj>, s:ICompilerStep) =
        s.GlobalData.Clear()
        for k in d do
            s.GlobalData.Add(k.Key, k.Value)

    member val Steps = [] with get, set

    member this.Run(input) =
        let mutable state = input
        let mutable globalData = Dictionary<string, obj>()
        for step in this.Steps do
            SetGlobalData(globalData, step)
            state <- step.Run(state)
            globalData <- step.GlobalData
        state
        
    static member Default() =  
        let typeManager = new TypeManager([ new DefaultTypeHandler();
                                            new RefVariableTypeHandler()])
                               
        let pipeline = new CompilerPipeline(typeManager, typeof<KernelModule>)           

        let parser = new ModuleParsingStep(pipeline, [ new KernelReferenceParser();
                                                          new KernelMethodInfoParser() ])

        let moduleBuilder = new ModulePreprocessingStep(pipeline, [ new GenericInstantiator();
                                                                       new FunctionReferenceDiscover() ])

        let preprocessor = new FunctionPreprocessingStep(pipeline, [ new SignaturePreprocessor();
                                                                        new RefVariablePreprocessor() ])
        
        let transformation = new FunctionTransformationStep(pipeline, [ new ReturnTypeTransformation();
                                                                           new GlobalVarRefTransformation();
                                                                           new ConditionalAssignmentTransformation();
                                                                           new ArrayAccessTransformation();
                                                                           new RefVariableTransformationProcessor();
                                                                           new ReturnLifting() ])

        let printer = new FunctionPrettyPrintingStep(pipeline, [ new SignaturePrinter();
                                                                  
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
                                                                    
        let finalizer = new ModulePrettyPrintingStep(pipeline, [ new ModulePrettyPrinter() ])

        // Run pipeline
        pipeline
        
