namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

type ModulePrettyPrinter() =      
    interface ModulePrettyPrintingProcessor with
        member this.Handle(km, currOut, engine:ModulePrettyPrintingStep) =
            let directives = String.concat "\n\n" km.Directives
            let functions = String.concat "\n\n" (List.map (fun (f: FunctionInfo) -> f.PrettyPrinting) km.Functions) 
            let kernels = String.concat "\n\n" (List.map (fun (f: KernelInfo) -> f.PrettyPrinting) km.Kernels)
            String.concat "\n\n" [directives; functions; kernels]
             
            