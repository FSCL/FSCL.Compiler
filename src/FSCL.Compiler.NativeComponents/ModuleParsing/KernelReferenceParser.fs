namespace FSCL.Compiler.ModuleParsing

open System
open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open FSCL.Language
open FSCL
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

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
            match GetKernelFromName(expr :?> Expr) with
            | Some(mi, paramInfo, paramVars, b, kMeta, rMeta, pMeta) ->              
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, null)

                let kernelModule = new KernelModule(new KernelInfo(mi, paramInfo, paramVars, None, b, finalMeta, false))
                
                // Create module
                Some(kernelModule)
            | _ ->
                None
        else
            None
            