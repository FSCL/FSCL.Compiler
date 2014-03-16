namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_LAMBDA_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP", Dependencies = [| "FSCL_REFERENCE_PARSING_PROCESSOR" |])>]
type KernelLambdaParser() =      
    inherit ModuleParsingProcessor()
        
    override this.Run(mi, en, opts) =
        let engine = en :?> ModuleParsingStep
        if (mi :? Expr) then
            match QuotationAnalysis.LambdaToMethod(mi :?> Expr) with
            | Some(mi, b, kernelAttributes) -> 
                // Create signleton kernel call graph
                let kernelModule = new KernelModule(new KernelInfo(mi, b, null, true))
                
                // Process each parameter
                for p in mi.GetParameters() do
                    // Create parameter info
                    let parameterEntry = new KernelParameterInfo(p.Name, p.ParameterType, p, None, null)
                    // Set var to be used in kernel body
                    parameterEntry.Placeholder <- Some(Quotations.Var(p.Name, p.ParameterType, false))         
                    // Add the parameter to the list of kernel params
                    kernelModule.Kernel.Info.Parameters.Add(parameterEntry)

                Some(kernelModule)
            | _ ->
                None
        else
            None
            