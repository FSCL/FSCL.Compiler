namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_LAMBDA_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP", Dependencies = [| "FSCL_REFERENCE_PARSING_PROCESSOR" |])>]
type KernelLambdaParser() =      
    inherit ModuleParsingProcessor()
        
    override this.Run(mi, s, opts) =
        let step = s :?> ModuleParsingStep
        if (mi :? Expr) then
            match QuotationAnalysis.LambdaToMethod(mi :?> Expr, true) with
            | Some(mi, paramInfo, paramVars, b, kMeta, rMeta, pMeta) -> 
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, null)

                // Create signleton kernel call graph
                let kernelModule = new KernelModule(new KernelInfo(mi, paramInfo, paramVars, None, b, finalMeta, true))
                
                Some(kernelModule)
            | _ ->
                None
        else
            None
            