namespace FSCL.Compiler.ModulePreprocessing
//
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
//[<StepProcessor("FSCL_ENV_REFS_DISCOVERY_PROCESSOR", 
//                "FSCL_MODULE_PREPROCESSING_STEP",
//                Dependencies = [| "FSCL_FUNCTIONS_DISCOVERY_PROCESSOR" |])>] 
//type EnvRefsDiscover() =      
//    inherit ModulePreprocessingProcessor()
//             
//    let GenerateOutsiderArg (name:string, n:obj) =
//         String.Format("{0}_outsider_{1}", name, n.ToString())
//
//    let rec DiscoverRefsToEnv(f:FunctionInfo, m:KernelModule) =
//        let envVars, outVals = new List<Var>(), new List<Expr>()
//        for id in f.CalledFunctions do
//            let sf = m.Functions.[id] :?> FunctionInfo
//            let vars, vals = DiscoverRefsToEnv(sf, m) 
//            // Merge
//            for v in vars do
//                if not (envVars.Contains(v)) then
//                    envVars.Add(v)
//            for v in vals do
//                if not (outVals.Contains(v)) then
//                    outVals.Add(v)
//            
//        let vars, vals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(f)
//        // Merge
//        for v in vars do
//            if not (envVars.Contains(v)) then
//                envVars.Add(v)
//        for v in vals do
//            if not (outVals.Contains(v)) then
//                outVals.Add(v)           
//        // Update function info 
//        for v in envVars do
//            f.EnvVarsUsed.Add(v) 
//            f.GeneratedParameters.Add(new FunctionParameter(v.Name, v, FunctionParameterType.EnvVarParameter(v), None))
//        let mutable i = -1;
//        for e in outVals do
//            i <- i + 1
//            f.OutValsUsed.Add(e) 
//            let name = GenerateOutsiderArg("value", i)
//            let paramVar = Var(name, e.Type, false)
//            f.GeneratedParameters.Add(new FunctionParameter(name, paramVar, FunctionParameterType.OutValParameter(e), None))
//            // Must replace all occurrences of value ref expr with a ref to this parameter
//            f.Body <- QuotationAnalysis.FunctionsManipulation.ReplaceExprWithVar(f.Body, e, paramVar)
//                  
//        envVars, outVals
//
//    override this.Run(m, en, opts) =
//        let engine = en :?> ModulePreprocessingStep
//        // Discover functions referenced from kernel
//        DiscoverRefsToEnv(m.Kernel, m) |> ignore
//
//
//            