namespace FSCL.Compiler.ModulePrettyPrinting

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_MODULE_PRETTY_PRINTING_PROCESSOR", "FSCL_MODULE_PRETTY_PRINTING_STEP")>]
type ModulePrettyPrinter() =      
    interface ModulePrettyPrintingProcessor with
        member this.Process((km, currOut), en) =
            let directives = String.concat "\n\n" km.Directives
            let functions = String.concat "\n\n" (List.map (fun (f: FunctionInfo) -> f.PrettyPrinting) km.Functions) 
            let kernels = String.concat "\n\n" (List.map (fun (f: KernelInfo) -> f.PrettyPrinting) km.Kernels)
            String.concat "\n\n" [directives; functions; kernels]
             
            