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
    
    override this.Run(expr, s, opts) =
        let step = s :?> ModuleParsingStep
        if (expr :? Expr) then
            match QuotationAnalysis.GetKernelFromName(expr :?> Expr) with
            | Some(mi, paramVars, b, kMeta, rMeta, pMeta) ->              
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, null)

                let kernelModule = new KernelModule(new KernelInfo(mi, paramVars, b, finalMeta, false))
                
                // Create module
                Some(kernelModule)
            | _ ->
                None
        else
            None
            