namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL
open FSCL.Compiler.ModulePreprocessing
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System
open FSCL.Compiler.AcceleratedCollections
open FSCL.Compiler.Util
    
[<StepProcessor("FSCL_ENV_REF_TO_PARAMETER_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP",
                Dependencies = [| "FSCL_FUNCTIONS_DISCOVERY_PROCESSOR";
                                  "FSCL_GLOBAL_DATA_DISCOVERY_PROCESSOR";
                                  "FSCL_STRUCT_DISCOVERY_PROCESSOR" 
                               |])>] 
type EnvRefAndOutValToParameterProcessor() = 
    inherit ModulePreprocessingProcessor()

    member this.Process(k:FunctionInfo) =
        let mutable nameIndex = 0
        // Create a parameter for each env var and out val
        for v in k.EnvVarsUsed do
            let newVar = Quotations.Var("envVarParam_" + nameIndex.ToString(), v.Type)
            let p = new FunctionParameter(newVar.Name, newVar, FunctionParameterType.EnvVarParameter(v), None)
            k.GeneratedParameters.Add(p)
            nameIndex <- nameIndex + 1
            // Replace this var in the body with the ref to new var
            k.Body <- QuotationAnalysis.FunctionsManipulation.ReplaceVar(k.Body, v, newVar)
        nameIndex <- 0
        for e in k.OutValsUsed do
            let newVar = Quotations.Var("outValParam_" + nameIndex.ToString(), e.Type)
            let p = new FunctionParameter(newVar.Name, newVar, FunctionParameterType.OutValParameter(e), None)
            k.GeneratedParameters.Add(p)
            nameIndex <- nameIndex + 1
            // Replace this value in the body with the ref to new var
            k.Body <- QuotationAnalysis.FunctionsManipulation.ReplaceExprWithVar(k.Body, e, newVar)

    override this.Run(km, st, opts) =
        let step = st :?> ModulePreprocessingStep        
        if not (km.Kernel :? AcceleratedKernelInfo) then        
            this.Process(km.Kernel)
        else
            let ak = km.Kernel :?> AcceleratedKernelInfo
            if ak.AppliedFunction.IsSome then
                let appliedFunction = ak.AppliedFunction .Value:?> FunctionInfo
                this.Process(appliedFunction)
                // Add the generated parameters to the kernel
                for p in appliedFunction.GeneratedParameters do
                    km.Kernel.GeneratedParameters.Add(new FunctionParameter(p.Name, p.OriginalPlaceholder, p.ParameterType, None))