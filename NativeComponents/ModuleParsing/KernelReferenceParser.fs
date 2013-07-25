namespace FSCL.Compiler.ModuleParsing

open System
open FSCL.Compiler
open FSCL.Compiler.Core.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<assembly:DefaultComponentAssembly>]
do()

[<StepProcessor("FSCL_REFERENCE_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP", Dependencies = [| "FSCL_CALL_EXPRESSION_PARSING_PROCESSOR" |])>]
type KernelReferenceParser() =      
    inherit ModuleParsingProcessor()
    
    override this.Run(expr, en) =
        let engine = en :?> ModuleParsingStep
        if (expr :? Expr) then
            match QuotationAnalysis.GetKernelFromName(expr :?> Expr) with
            | Some(mi, b) ->                 
                // Create signleton kernel call graph
                let kcg = new ModuleCallGraph()
                kcg.AddKernel(new KernelInfo(mi, b))

                // Detect is device attribute set
                let device = mi.GetCustomAttribute(typeof<DeviceAttribute>)
                if device <> null then
                    kcg.Kernels.[0].Device <- device :?> DeviceAttribute

                // Create module
                Some(kcg)
            | _ ->
                None
        else
            None
            