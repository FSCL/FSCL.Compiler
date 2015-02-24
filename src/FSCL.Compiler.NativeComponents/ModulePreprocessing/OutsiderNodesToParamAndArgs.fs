namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL.Compiler.Util
open FSCL.Language
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open FSCL.Compiler.AcceleratedCollections
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

[<StepProcessor("FSCL_OUTSIDER_NODES_DISCOVERY_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP")>] 
type OutsiderNodesToParamAndArgs() =      
    inherit ModulePreprocessingProcessor()

    let rec FixSignature(k:FunctionInfo) =
        let outsiderIndex = ref 0
        match k.Type with
        | KFGNodeType.KernelNode ->
            let n = k :?> KFGKernelNode
            for el in n.Input do
                match el.Type with
                | KFGNodeType.OutsiderDataNode ->
                    let d = el :?> KFGOutsiderDataNode
                    let ov, oe = 
                        match d.Outsider with
                        | VarRef(v) ->
                            v, Expr.Var(v)
                        | DataRef(e) ->
                            let v = Quotations.Var("outsider_val_" + outsiderIndex.Value.ToString(), e.Type)
                            outsiderIndex := !outsiderIndex + 1
                            v, e
                    (n.Module.Kernel :?> KernelInfo).GeneratedParameters.Add(
                        new FunctionParameter(ov.Name, ov, FunctionParameterType.OutsiderParameter, None))
                    (n.Module :?> KernelModule).OutsiderArgs.Add(oe)
                | _ ->
                    ()
        | _ ->
            for el in k.Input do
                FixSignature(el)
                
    override this.Run(m, en, opts) =
        let engine = en :?> ModulePreprocessingStep
        // Do not do this if the kernel is an accelerated kernel (the function applied to the collection has been already generated)
        
        // Discover functions referenced from kernel
        let mutable functionsToAnalyse = 
            if not (m.Kernel :? AcceleratedKernelInfo) then        
                DiscoverFunctionRef(m.Kernel)
            else
                DiscoverFunctionRef(m.Functions.Values |> Seq.toList |> List.head :?> FunctionInfo)
           
        let mutable newFunctionsFound = new Dictionary<MethodInfo, FunctionInfo>()
        let mutable foundSomethingNew = true
        while foundSomethingNew do
            foundSomethingNew <- false
            // Discover functions referenced from other functions (prepending)
            for item in functionsToAnalyse do
                let found = DiscoverFunctionRef(item.Value)
                for it in found do
                    if not (functionsToAnalyse.ContainsKey(it.Key)) && not (newFunctionsFound.ContainsKey(it.Key)) then
                        newFunctionsFound.Add(it.Key, it.Value)
                        foundSomethingNew <- true
            for item in functionsToAnalyse do
                m.Functions.Add(item.Value.ID, item.Value)
            functionsToAnalyse <- newFunctionsFound
            newFunctionsFound <- new Dictionary<MethodInfo, FunctionInfo>()



            