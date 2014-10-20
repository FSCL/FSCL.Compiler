namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL.Compiler.Util
open FSCL.Language
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

[<StepProcessor("FSCL_FUNCTIONS_DISCOVERY_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP")>] 
type FunctionReferenceDiscover() =      
    inherit ModulePreprocessingProcessor()

    let DiscoverFunctionRef(k:FunctionInfo) =
        let foundFunctions = Dictionary<MethodInfo, FunctionInfo>()

        let rec DiscoverFunctionRefInner(expr) =
            match expr with
            | Patterns.Call(o, mi, args) ->
                List.iter (fun el -> DiscoverFunctionRefInner(el)) args
                try
                    match mi with
                    | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                        if not (foundFunctions.ContainsKey(mi)) then     
                            match GetCurriedOrTupledArgs(b) with
                            | Some(paramV) ->       
                                // Check if one of the params is a WorkItemInfo
                                let methodParams = mi.GetParameters()
                                let paramInfos = new List<ParameterInfo>()
                                let paramVars = new List<Var>()
                                let workItemInfo = ref None 
                                paramV |> List.iteri(fun i p ->
                                                        if p.Type <> typeof<WorkItemInfo> then
                                                            paramVars.Add(p)
                                                            paramInfos.Add(methodParams.[i])
                                                            workItemInfo := Some(args.[i]))
                                foundFunctions.Add(mi, new FunctionInfo(mi, 
                                                                        paramInfos |> List.ofSeq, 
                                                                        paramVars |> List.ofSeq,
                                                                        workItemInfo.Value,
                                                                        b, false))
                            | _ ->
                                ()
                    | _ ->
                        ()
                with
                    :? NullReferenceException -> ()
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
        // Discover functions referenced from kernel
        let mutable functionsToAnalyse = DiscoverFunctionRef(m.Kernel)
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



            