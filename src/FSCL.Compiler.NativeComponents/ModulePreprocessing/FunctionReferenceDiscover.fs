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
open Microsoft.FSharp.Linq.RuntimeHelpers

[<StepProcessor("FSCL_FUNCTIONS_DISCOVERY_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP")>] 
type FunctionReferenceDiscover() =      
    inherit ModulePreprocessingProcessor()

    let DiscoverFunctionRef(k:FunctionInfo) =
        let foundFunctions = Dictionary<MethodInfo, FunctionInfo>()

        let rec DiscoverFunctionRefInner(expr) =
            match expr with
            | UtilityFunctionCall(obv, ob, mi, parameters, paramVars, body, args, workItemInfo) ->
                if (foundFunctions.ContainsKey(mi) |> not) then
                    //let envVars, outVals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(body)
                    // Right now utility functions cannot refer env vars or out vals
                    let fi = new FunctionInfo(mi.Name, Some(mi), 
                                              paramVars,
                                              mi.ReturnType,
                                              new List<Var>(), new List<Expr>(),
                                              body)
                    foundFunctions.Add(mi, fi)
                    k.CalledFunctions.Add(fi.ID)
            | _ ->
                match expr with 
                | ExprShape.ShapeLambda(v, a) ->
                    DiscoverFunctionRefInner(a)
                | ExprShape.ShapeCombination(o, list) ->
                    List.iter (fun el -> DiscoverFunctionRefInner(el)) list
                | _ ->
                    ()

        DiscoverFunctionRefInner(k.Body)
        foundFunctions

    override this.Run(m, en, opts) =
        let engine = en :?> ModulePreprocessingStep
        // Do not do this if the kernel is an accelerated kernel (the function applied to the collection has been already generated)
        
        // Discover functions referenced from kernel
        let mutable functionsToAnalyse = 
            match m.Kernel with
            | :? AcceleratedKernelInfo as aki when aki.AppliedFunction.IsSome -> 
                DiscoverFunctionRef(aki.AppliedFunction.Value :?> FunctionInfo)    
            | _ ->
                DiscoverFunctionRef(m.Kernel)
           
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



            