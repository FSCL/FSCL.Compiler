namespace FSCL.Compiler.ModulePreprocessing

//open FSCL.Compiler
//open FSCL.Compiler.Util
//open FSCL.Language
//open System.Collections.Generic
//open System.Reflection
//open Microsoft.FSharp.Quotations
//open System
//open FSCL.Compiler.AcceleratedCollections
//open QuotationAnalysis.FunctionsManipulation
//open QuotationAnalysis.KernelParsing
//open QuotationAnalysis.MetadataExtraction
//
//[<StepProcessor("FSCL_FUNCTIONS_DISCOVERY_PROCESSOR", 
//                "FSCL_MODULE_PREPROCESSING_STEP")>] 
//type FunctionReferenceDiscover() =      
//    inherit ModulePreprocessingProcessor()
//
//    let DiscoverFunctionRef(k:FunctionInfo) =
//        let foundFunctions = Dictionary<MethodInfo, FunctionInfo>()
//
//        let rec DiscoverFunctionRefInner(expr, env) =
//            match expr with
//            | UtilityFunctionCall(obv, ob, mi, parameters, paramVars, body, args, workItemInfo, _, _, _) ->
//                let fi = new FunctionInfo(obv,
//                                          ob,
//                                          mi, 
//                                          parameters |> List.ofArray, 
//                                          paramVars,
//                                          env,
//                                          workItemInfo,
//                                          body, false)
//                foundFunctions.Add(mi, fi)
//                k.CalledFunctions.Add(fi.ID)
//            | _ ->
//                match expr with 
//                | ExprShape.ShapeLambda(v, a) ->
//                    DiscoverFunctionRefInner(a, env)
//                | ExprShape.ShapeCombination(o, list) ->
//                    List.iter (fun el -> DiscoverFunctionRefInner(el, env)) list
//                | _ ->
//                    ()
//
//        DiscoverFunctionRefInner(k.Body, k.EnvVars)
//        foundFunctions
//
//    override this.Run(m, en, opts) =
//        let engine = en :?> ModulePreprocessingStep
//        // Do not do this if the kernel is an accelerated kernel (the function applied to the collection has been already generated)
//        
//        // Discover functions referenced from kernel
//        let mutable functionsToAnalyse = 
//            if not (m.Kernel :? AcceleratedKernelInfo) then        
//                DiscoverFunctionRef(m.Kernel)
//            else
//                DiscoverFunctionRef(m.Functions.Values |> Seq.toList |> List.head :?> FunctionInfo)
//           
//        let mutable newFunctionsFound = new Dictionary<MethodInfo, FunctionInfo>()
//        let mutable foundSomethingNew = true
//        while foundSomethingNew do
//            foundSomethingNew <- false
//            // Discover functions referenced from other functions (prepending)
//            for item in functionsToAnalyse do
//                let found = DiscoverFunctionRef(item.Value)
//                for it in found do
//                    if not (functionsToAnalyse.ContainsKey(it.Key)) && not (newFunctionsFound.ContainsKey(it.Key)) then
//                        newFunctionsFound.Add(it.Key, it.Value)
//                        foundSomethingNew <- true
//            for item in functionsToAnalyse do
//                m.Functions.Add(item.Value.ID, item.Value)
//            functionsToAnalyse <- newFunctionsFound
//            newFunctionsFound <- new Dictionary<MethodInfo, FunctionInfo>()
//
//
//
//            