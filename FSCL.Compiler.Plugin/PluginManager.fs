namespace FSCL.Compiler.Plugin

open System
open System.Reflection
open System.Collections.Generic
open FSCL.Compiler
open FSCL.Compiler.Processors
open System.Xml
open System.Xml.Linq

exception CompilerPluginException of string

type PluginManager() =      
    let mutable compilerKernelModuleType = typeof<KernelModule>
    let mutable defaultPipeline = None
       
    let pipelineBuilder = new CompilerPipelineBuilder()

    member this.Load(file: string) =
        let document = XDocument.Load(file)

        // Check if compiler pipeline should be init to default or not
        let compilerInit = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerPipelineInit")))
        let mutable shouldInitPipelineToDefault = true
        if compilerInit.[0].Value = "None" then
            shouldInitPipelineToDefault <- false
        match defaultPipeline with
        | None ->
            defaultPipeline <- Some(shouldInitPipelineToDefault)
        | Some(v) ->
            if v <> shouldInitPipelineToDefault then
                raise (CompilerPluginException("Cannot load " + file + " because it requires a pipeline initialization mode that differs from the one set by a plugin previously loaded"))

        // Load kernel module is any
        let kmodule = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerKernelModule")))
        if not kmodule.IsEmpty then
            let kSource = kmodule.[0].Attribute(XName.Get("source"))
            let kModuleClass = kmodule.[0].Attribute(XName.Get("class"))
            
            // Load dll and instantiate factory
            let dll = Assembly.LoadFile(kSource.Value)
            let kModuleType = dll.GetType(kModuleClass.Value)
            compilerKernelModuleType <- kModuleType
        else
            compilerKernelModuleType <- typeof<KernelModule>
            
        // Load steps if any
        let steps = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerStep")))
        for step in steps do
            let stepClass = step.Attribute(XName.Get("class"))
            let stepSource = step.Attribute(XName.Get("source"))
            let stepID = step.Attribute(XName.Get("id"))
            
            // Load dll and store processor instance
            let dll = Assembly.LoadFile(stepSource.Value)
            let stepInstance = dll.CreateInstance(stepClass.Value) :?> ICompilerStep
            compilerSteps.Add(stepID.Value, stepInstance)

        // Load processors if any
        let processors = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerStepProcessor")))
        for processor in processors do
            let processorsClass = processor.Attribute(XName.Get("class"))
            let processorSource = processor.Attribute(XName.Get("source"))
            let processorStepID = processor.Attribute(XName.Get("stepId"))
            let processorID = processor.Attribute(XName.Get("id"))
            
            // Load dll and store processor instance
            let dll = Assembly.LoadFile(processorSource.Value)
            let procInstance = dll.CreateInstance(processorsClass.Value)
            compilerStepProcessors.Add(processorID.Value, (procInstance, processorStepID.Value))

    member private this.DefaultPipeline() =
        let compilerSteps = new Dictionary<string, ICompilerStep>()
        let compilerStepProcessors = new Dictionary<string, obj * string>()        
        let parser = new ModuleParsingStep(typeManager, [ new KernelReferenceParser();
                                                            new KernelMethodInfoParser() ], typeof<KernelModule>)

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

        let printer = new FunctionPrettyPrintingStep(typeManager, [ new SignaturePrinter() ],
                                                                    [
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
        ()
        
    member this.GetPipeline() =
        

         

        // Build steps dependency graph


        
        (*


    member this.Run(input) =
        if this.Steps.IsEmpty then
            raise (CompilerException("Compiler pipeline is empty"))
        let result = List.fold(fun input (step:CompilerStepBase) -> 
                                let runMethod = step.GetType().GetMethod("Run")
                                runMethod.Invoke(step, [| input |])) input (this.Steps)
        result :?> (KernelModule * String)

    member this.AddStep(step: CompilerStepBase, position) =
        // Check it is compatible with the last step
        let nextStep = if position < this.Steps.Length then Some(this.Steps.[position]) else None
        let prevStep = if position >= 0 then Some(this.Steps.[position]) else None

        let inputType = if prevStep.IsNone then typeof<obj> else prevStep.Value.GetType().GetMethod("Run").GetGenericArguments().[1]
        let outputType = if nextStep.IsNone then typeof<KernelModule * String> else nextStep.Value.GetType().GetMethod("Run").GetGenericArguments().[0]

        let runMethod = step.GetType().GetMethod("Run")
        let ioParams = runMethod.GetGenericArguments()
        if not (ioParams.[0].IsAssignableFrom(inputType)) then
            raise (CompilerException("Compiler step of type " + step.GetType().ToString() + " can't be inserted in position " + position.ToString() + ": the input type is " + inputType.GetType().ToString() + " but the step accepts " + ioParams.[0].ToString()))
        if not (outputType.IsAssignableFrom(ioParams.[1])) then        
            raise (CompilerException("Compiler step of type " + step.GetType().ToString() + " can't be inserted in position " + position.ToString() + ": the output type is " + outputType.GetType().ToString() + " but the step accepts " + ioParams.[1].ToString()))

        // Add step
        this.Steps <- Util.insert step position (this.Steps)
        
    member this.AddStep(step: CompilerStepBase) =
        this.AddStep(step, this.Steps.Length)

    member this.RemoveStep(position) =
        // Check it is compatible with the last step
        let nextStep = if position < this.Steps.Length - 1 then Some(this.Steps.[position]) else None
        let prevStep = if position > 0 then Some(this.Steps.[position - 1]) else None

        let inputType = if prevStep.IsNone then typeof<obj> else prevStep.Value.GetType().GetMethod("Run").GetGenericArguments().[1]
        let outputType = if nextStep.IsNone then typeof<KernelModule * String> else nextStep.Value.GetType().GetMethod("Run").GetGenericArguments().[0]

        if outputType <> inputType then
            raise (CompilerException("Compiler step of index " + position.ToString() + " can't be removed, since the previous step output is of type " + inputType.ToString() + " and the next step input is of type " + outputType.GetType().ToString()))
        
        // Remove step
        this.Steps <- Util.remove position (this.Steps)

    member this.ReplaceStep(step: CompilerStepBase, position) =
        // Check it is compatible with the last step
        let currStep = this.Steps.[position]

        let currStepIO = currStep.GetType().GetMethod("Run").GetGenericArguments()
        let stepIO = step.GetType().GetMethod("Run").GetGenericArguments()

        Array.iteri(fun i (t:Type) ->
            if t <> currStepIO.[i] then
                raise (CompilerException("Compiler step of index " + position.ToString() + " can't be replaces with step of type " + step.GetType().ToString() + " because input/output do not match"))) stepIO
        
        // Remove step
        this.Steps <- Util.remove position (this.Steps)
        this.Steps <- Util.insert step position (this.Steps)
        *)