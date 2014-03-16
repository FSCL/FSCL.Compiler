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
        
    override this.Run(mi, en, opts) =
        let engine = en :?> ModuleParsingStep
        if (mi :? MethodInfo) then
            match QuotationAnalysis.GetKernelFromMethodInfo(mi :?> MethodInfo) with
            | Some(mi, b) -> 
                // Create singleton kernel call graph
                let kernelModule = new KernelModule(new KernelInfo(mi, b, null, false))
                
                // Process each parameter
                for p in mi.GetParameters() do
                    // Create parameter info
                    let parameterEntry = new KernelParameterInfo(p.Name, p.ParameterType, p, None, null)
                    // Set var to be used in kernel body
                    parameterEntry.Placeholder <- Some(Quotations.Var(p.Name, p.ParameterType, false))         
                    // Add the parameter to the list of kernel params
                    kernelModule.Kernel.Info.Parameters.Add(parameterEntry)

                // Create module
                Some(kernelModule)
            | _ ->
                None
        else
            None
            