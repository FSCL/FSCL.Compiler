namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open FSCL.Compiler.Language

[<StepProcessor("FSCL_METHOD_INFO_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
type KernelMethodInfoParser() =      
    inherit ModuleParsingProcessor() 
        
    override this.Run(mi, s, opts) =
        let step = s :?> ModuleParsingStep
        if (mi :? MethodInfo) then
            match QuotationAnalysis.GetKernelFromMethodInfo(mi :?> MethodInfo) with
            | Some(mi, paramVars, b, kMeta, rMeta, pMeta) -> 
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, null)

                // Create singleton kernel call graph
                let kernelModule = new KernelModule(new KernelInfo(mi, paramVars, b, finalMeta, false))
                
                // Create module
                Some(kernelModule)
            | _ ->
                None
        else
            None
            