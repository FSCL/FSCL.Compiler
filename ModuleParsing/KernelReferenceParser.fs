namespace FSCL.Compiler.ModuleParsing

open System
open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open FSCL.Compiler.Language

[<assembly:DefaultComponentAssembly>]
do()

[<StepProcessor("FSCL_REFERENCE_PARSING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP", 
                Dependencies = [| "FSCL_CALL_EXPRESSION_PARSING_PROCESSOR" |])>]
type KernelReferenceParser() =      
    inherit ModuleParsingProcessor()
    
    override this.Run(expr, en, opts) =
        let engine = en :?> ModuleParsingStep
        if (expr :? Expr) then
            match QuotationAnalysis.GetKernelFromName(expr :?> Expr) with
            | Some(mi, b, kernelAttributes) ->                 
                // Create signleton kernel call graph
                let kernelModule = new KernelModule(new KernelInfo(mi, b, None, null, false))
                
                // Process each parameter
                for p in mi.GetParameters() do
                    // Create parameter info
                    let parameterEntry = new KernelParameterInfo(p.Name, p.ParameterType, p, null)
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
            