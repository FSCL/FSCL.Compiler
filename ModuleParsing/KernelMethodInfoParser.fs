namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Core.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_METHOD_INFO_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
type KernelMethodInfoParser() =      
    inherit ModuleParsingProcessor() 
        
    override this.Run(mi, en, opts) =
        let engine = en :?> ModuleParsingStep
        if (mi :? MethodInfo) then
            match QuotationAnalysis.GetKernelFromMethodInfo(mi :?> MethodInfo) with
            | Some(mi, b) -> 
                // Create signleton kernel call graph
                let kernelModule = new KernelModule()
                kernelModule.AddKernel(new KernelInfo(mi, b, false))
                
                // Detect is device attribute set
                let device = mi.GetCustomAttribute(typeof<DeviceAttribute>)
                if device <> null then
                    kernelModule.GetKernel(MethodID(mi)).Info.Device <- device :?> DeviceAttribute

                // Create module
                Some(kernelModule)
            | _ ->
                None
        else
            None
            