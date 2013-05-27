namespace FSCL.Compiler.Plugin

open System
open System.Reflection
open System.Collections.Generic
open FSCL.Compiler
open FSCL.Compiler.FunctionPreprocessing
open FSCL.Compiler.ModuleParsing
open FSCL.Compiler.FunctionPrettyPrinting
open FSCL.Compiler.FunctionTransformation
open FSCL.Compiler.ModulePreprocessing
open FSCL.Compiler.Types
open FSCL.Compiler.ModulePrettyPrinting
open System.Xml
open System.Xml.Linq

exception CompilerPluginException of string

type CompilerPluginManager() =      
    let mutable compilerKernelModuleType = typeof<KernelModule>
    let mutable defaultPipeline = None
    let pipelineBuilder = new CompilerPipelineBuilder()
    // Trick to guarantee the default components assemblies are loaded
    let defAssemblyComponents = [
                                    typeof<SignaturePreprocessor>;
                                    typeof<SignaturePrinter>;
                                    typeof<ReturnLifting>;
                                    typeof<KernelReferenceParser>;
                                    typeof<GenericInstantiator>;
                                    typeof<ModulePrettyPrinter>;
                                    typeof<DefaultTypeHandler>;
                                    typeof<ModuleParsingStep>;
                                ]
    
    member this.Load(assembly: Assembly) =
        // Analyze assembly content
        for t in assembly.GetTypes() do
            let thAttribute = t.GetCustomAttribute<TypeHandlerAttribute>()
            if thAttribute <> null then
                pipelineBuilder.AddTypeHandler(thAttribute, t)

            let stepAttribute = t.GetCustomAttribute<StepAttribute>()
            if stepAttribute <> null then
                pipelineBuilder.AddCompilerStep(stepAttribute, t)

            let procAttribute = t.GetCustomAttribute<StepProcessorAttribute>()
            if procAttribute <> null then
                pipelineBuilder.AddCompilerStepProcessor(procAttribute, t)
                
    member this.LoadDefault() =
        // Analyze assembly content
        let sources = Assembly.GetExecutingAssembly().GetReferencedAssemblies()
        for s in sources do
            let assembly = Assembly.Load(s)
            for t in assembly.GetTypes() do
                let thAttribute = t.GetCustomAttribute<TypeHandlerAttribute>()
                if thAttribute <> null then
                    pipelineBuilder.AddTypeHandler(thAttribute, t)

                let stepAttribute = t.GetCustomAttribute<StepAttribute>()
                if stepAttribute <> null then
                    pipelineBuilder.AddCompilerStep(stepAttribute, t)

                let procAttribute = t.GetCustomAttribute<StepProcessorAttribute>()
                if procAttribute <> null then
                    pipelineBuilder.AddCompilerStepProcessor(procAttribute, t)

                (*
    member this.Load(file: string) =
        let document = XDocument.Load(file)
        let source = document.Root.Attribute(XName.Get("source"))
        let dll = Assembly.LoadFile(source.Value)
        
        // Load kernel module extensions if any
        let kmodule = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerKernelModuleExtension")))
        for kextension in kmodule do
            let kModuleId = kextension.Attribute(XName.Get("id"))
            let kModuleClass = kextension.Attribute(XName.Get("class"))
            
            // Instantiate module extension
            let kModuleType = dll.GetType(kModuleClass.Value)
            pipelineBuilder.AddTypeHandler(kModuleId.Value, kModuleType)
            
        // Load type handlers if any
        let mutable typeHandlers = [];
        let handlers = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerTypeHandler")))
        for handler in handlers do
            let handlerId = handler.Attribute(XName.Get("id"))
            let handlerClass = handler.Attribute(XName.Get("class"))
            
            // Instantiate handler
            let handlerType = dll.GetType(handlerClass.Value)
            pipelineBuilder.AddTypeHandler(handlerId.Value, handlerType)
        
        // Load processors if any
        let processors = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerStepProcessor")))
        for processor in processors do
            let processorClass = processor.Attribute(XName.Get("class"))
            let processorStepID = processor.Attribute(XName.Get("stepId"))
            let processorID = processor.Attribute(XName.Get("id"))
            let before = processor.Attribute(XName.Get("before"))
            let after = processor.Attribute(XName.Get("after"))
            
            // Store processor instance
            let processorType = dll.GetType(processorClass.Value)
            let processorInfo = new CompilerPluginStepProcessorInfo(processorID.Value,
                                                                    processorStepID.Value,
                                                                    processorType,
                                                                    before.Value,
                                                                    after.Value,
                                                                    [])

            pipelineBuilder.AddCompilerStepProcessor(processorInfo)

        // Load steps if any
        let steps = List.ofSeq (document.Descendants(XName.Get("FSCLCompilerStep")))
        for step in steps do
            let stepClass = step.Attribute(XName.Get("class"))
            let stepID = step.Attribute(XName.Get("id"))
            let before = step.Attribute(XName.Get("before"))
            let after = step.Attribute(XName.Get("after"))
            
            // Load dll and store processor instance
            let stepType = dll.GetType(stepClass.Value)
            let stepInfo = new CompilerPluginStepInfo(stepID.Value,
                                                      stepType,
                                                      before.Value,
                                                      after.Value,
                                                      [])
            pipelineBuilder.AddCompilerStep(stepInfo)
            *)
    member this.Build() =
        pipelineBuilder.Build()

        
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