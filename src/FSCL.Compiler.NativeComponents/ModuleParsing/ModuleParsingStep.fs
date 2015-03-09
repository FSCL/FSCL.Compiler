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
    inherit CompilerStep<obj * KernelCache, KernelExpression>(tm, procs)
    
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

    override this.Run((e, cache), opt) =
        // Normalize expressione first    
        let norm = 
            if e :? Expr then
                let e1 = CompositionToCallOrApplication(e :?> Expr)
                e1 |> box 
            else
                e
        opts <- opt
        let r =
            let procResult = this.Process(norm, [])
            let expr = new KernelExpression(procResult)

            if opts.ContainsKey(CompilerOptions.ParseOnly) then
                StopCompilation(expr)
            else
                if procResult = null then
                    StopCompilation(expr)
                else
                    // Inspect cache to check which kernel modules have to be compiled 
                    // and which ones can be copied from cache
                    for km in expr.KernelModules do
                        match cache.TryGet(km.Kernel.ID, km.Kernel.Meta) with
                        | Some(entry) ->
                            (entry.Module :?> KernelModule).CloneTo(km)
                        | None ->
                            // This must be fully compiled
                            expr.KernelModulesRequiringCompilation.Add(km :?> KernelModule)
                     
                    if expr.KernelModulesRequiringCompilation.Count = 0 then
                        StopCompilation(expr)
                    else
                        ContinueCompilation(expr)
        r

        

