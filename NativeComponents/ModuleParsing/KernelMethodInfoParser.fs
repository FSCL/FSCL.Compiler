namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Core.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_METHOD_INFO_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
type KernelMethodInfoParser() =      
    inherit ModuleParsingProcessor() 
        
    override this.Run(mi, en) =
        let engine = en :?> ModuleParsingStep
        if (mi :? MethodInfo) then
            match QuotationAnalysis.GetKernelFromMethodInfo(mi :?> MethodInfo) with
            | Some(mi, b) -> 
                // Create signleton kernel call graph
                let kcg = new ModuleCallGraph()
                kcg.AddKernel(new KernelInfo(mi, b))
                // Create module
                let km = new KernelModule(kcg)
                Some(km)
            | _ ->
                None
        else
            None
            