namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Core.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_LAMBDA_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP", Dependencies = [| "FSCL_REFERENCE_PARSING_PROCESSOR" |])>]
type KernelLambdaParser() =      
    inherit ModuleParsingProcessor()
        
    override this.Run(mi, en) =
        let engine = en :?> ModuleParsingStep
        if (mi :? Expr) then
            match QuotationAnalysis.LambdaToMethod(mi :?> Expr) with
            | Some(mi, b) -> 
                // Create signleton kernel call graph
                let kernelModule = new KernelModule()
                let kInfo = new KernelInfo(mi, b)
                kInfo.IsLambda <- true
                kernelModule.AddKernel(kInfo)
                Some(kernelModule)
            | _ ->
                None
        else
            None
            