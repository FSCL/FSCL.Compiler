namespace FSCL.Compiler.ModuleParsing

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler
open FSCL.Compiler.Util.QuotationAnalysis.KernelParsing

type MetadataVerifier = ReadOnlyMetaCollection * ReadOnlyMetaCollection -> bool

[<Step("FSCL_MODULE_PARSING_STEP")>] 
type ModuleParsingStep(tm: TypeManager,
                       procs: ICompilerStepProcessor list) = 
    inherit CompilerStep<obj * MetadataVerifier, ComputingExpressionModule>(tm, procs)
    
    let parsingProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> ModuleParsingProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                                 typeof<ModuleParsingProcessor>.IsAssignableFrom(p.GetType())) procs)
    let metadataProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> MetadataFinalizerProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                            typeof<MetadataFinalizerProcessor>.IsAssignableFrom(p.GetType())) procs)

    //let kernelModules = new List<IKernelModule>() 
    let mutable opts = null
                       
    member this.Process(e:obj, env: Var list) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < parsingProcessors.Length) do
            output <- parsingProcessors.[index].Execute((e, env), this, opts) :?> IKFGNode option
            index <- index + 1
        if output.IsNone then
            if e :? Expr then
                // Data node
                new KFGDataNode(e :?> Expr) :> IKFGNode
            else
                null
        else
            //raise (CompilerException("The engine is not able to parse a kernel inside the expression [" + expr.ToString() + "]"))
            // Check if a kernel
            //if output.Value :? KFGKernelNode then
               // kernelModules.Add((output.Value :?> KFGKernelNode).Module)
            output.Value
            
    member this.ProcessMeta(kmeta: KernelMetaCollection, rmeta: ParamMetaCollection, pmeta: List<ParamMetaCollection>, parsingInfo: Dictionary<string, obj>) =
        let mutable output = (kmeta, rmeta, pmeta)
        for p in metadataProcessors do
            output <- p.Execute((kmeta, rmeta, pmeta, parsingInfo), this, opts) :?> KernelMetaCollection * ParamMetaCollection * List<ParamMetaCollection>
        ReadOnlyMetaCollection(kmeta, rmeta, pmeta)

    override this.Run((e, verifier), opt) =
        // Normalize expressione first    
        let norm = 
            if e :? Expr then
                let e1 = CompositionToCallOrApplication(e :?> Expr)
                e1 |> box 
            else
                e
        opts <- opt
        let r =
            if opts.ContainsKey(CompilerOptions.ParseOnly) then
                StopCompilation(new ComputingExpressionModule(this.Process(norm, []), verifier))
            else
                let parsingResult = new ComputingExpressionModule(this.Process(norm, []), verifier)
                if parsingResult = null then
                    StopCompilation(parsingResult)
                else
                    ContinueCompilation(parsingResult)
        r

        

