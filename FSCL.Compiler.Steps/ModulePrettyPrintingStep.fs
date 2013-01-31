namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type ModulePrettyPrintingProcessor =    
    abstract member Handle: KernelModule * String * ModulePrettyPrintingStep -> String

and ModulePrettyPrintingStep(pipeline: ICompilerPipeline, processors: ModulePrettyPrintingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule * String>(pipeline)
        
    override this.Run(k) =
        let state = ref ""
        for p in processors do
            state := p.Handle((k, !state, this))
        (k, !state)