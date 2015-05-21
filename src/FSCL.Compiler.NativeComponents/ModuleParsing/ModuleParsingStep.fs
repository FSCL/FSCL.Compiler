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
                       
    member this.Process(e:obj, env: Var list, opts) =
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
            
    member this.ProcessMeta(kmeta: KernelMetaCollection, rmeta: ParamMetaCollection, pmeta: List<ParamMetaCollection>, parsingInfo: Dictionary<string, obj>, opts:Map<string, obj>) =
        let mutable output = (kmeta, rmeta, pmeta)
        for p in metadataProcessors do
            output <- p.Execute((kmeta, rmeta, pmeta, parsingInfo), this, opts) :?> KernelMetaCollection * ParamMetaCollection * List<ParamMetaCollection>
        ReadOnlyMetaCollection(kmeta, rmeta, pmeta)

    override this.Run((e, cache), opts) =
        // Normalize expressione first    
        let norm = 
            if e :? Expr then
                let e1 = CompositionToCallOrApplication(e :?> Expr)
                e1 |> box 
            else
                e
        let r =
            let procResult = this.Process(norm, [], opts)
            let expr = new KernelExpression(procResult)

            if opts.ContainsKey(CompilerOptions.ParseOnly) then
                StopCompilation(expr)
            else
                if procResult = null then
                    StopCompilation(expr)
                else
                    // Inspect cache to check which kernel modules have to be compiled 
                    // and which ones can be copied from cache
                    for km in expr.KernelNodes do
                        match cache.TryGet(km.Module.Kernel.ID, km.Module.Kernel.Meta) with
                        | Some(entry) ->
                            if (entry.Module :?> KernelModule).Code.IsNone then                        
                                // If entry has no Code, then there's a kernel occurring twice or more
                                // in the input program
                                (km :?> KFGKernelNode).CacheEntry <- entry                            
                                expr.KernelModulesRequiringLazyCloning.Add(km)
                            else
                                (entry.Module :?> KernelModule).CloneTo(km.Module)
                                // Associate the new cache entry to the KFG node (used by the runtime)
                                (km :?> KFGKernelNode).CacheEntry <- entry    
                        | None ->
                            // Add this module to the cache
                            let entry = cache.Put(km.Module)
                            // Associate the new cache entry to the KFG node (used by the runtime)
                            (km :?> KFGKernelNode).CacheEntry <- entry
                            // This module must be fully compiled
                            expr.KernelModulesRequiringCompilation.Add(km.Module :?> KernelModule)
                     
                    if expr.KernelModulesRequiringCompilation.Count = 0 then
                        StopCompilation(expr)
                    else
                        ContinueCompilation(expr)
        r

        

