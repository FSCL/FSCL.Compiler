namespace FSCL.Compiler.ModuleParsing

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler
open FSCL.Compiler.Util.QuotationAnalysis.KernelParsing

[<Step("FSCL_MODULE_PARSING_STEP")>] 
type ModuleParsingStep(tm: TypeManager,
                       procs: ICompilerStepProcessor list) = 
    inherit CompilerStep<obj, KernelModule>(tm, procs)
    
    let parsingProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> ModuleParsingProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                                 typeof<ModuleParsingProcessor>.IsAssignableFrom(p.GetType())) procs)
    let metadataProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> MetadataFinalizerProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                            typeof<MetadataFinalizerProcessor>.IsAssignableFrom(p.GetType())) procs)

    let mutable opts = null
                 
    member private this.Process(expr:obj) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < parsingProcessors.Length) do
            output <- parsingProcessors.[index].Execute(expr, this, opts) :?> KernelModule option
            index <- index + 1
        if output.IsNone then
            null
        else//raise (CompilerException("The engine is not able to parse a kernel inside the expression [" + expr.ToString() + "]"))
            output.Value
            
    member this.ProcessMeta(kmeta: KernelMetaCollection, rmeta: ParamMetaCollection, pmeta: List<ParamMetaCollection>, parsingInfo: Dictionary<string, obj>) =
        let mutable output = (kmeta, rmeta, pmeta)
        for p in metadataProcessors do
            output <- p.Execute((kmeta, rmeta, pmeta, parsingInfo), this, opts) :?> KernelMetaCollection * ParamMetaCollection * List<ParamMetaCollection>
        ReadOnlyMetaCollection(kmeta, rmeta, pmeta)

    override this.Run(e, opt) =
        // Normalize expressione first
        let norm = 
            if e :? Expr then
                CompositionToCallOrApplication(e :?> Expr, None) :> obj
            else
                e
        opts <- opt
        let r =
            if opts.ContainsKey(CompilerOptions.ParseOnly) then
                StopCompilation(this.Process(norm))
            else
                let parsingResult = this.Process(norm)
                if parsingResult = null then
                    StopCompilation(parsingResult)
                else
                    ContinueCompilation(parsingResult)
        r

        

