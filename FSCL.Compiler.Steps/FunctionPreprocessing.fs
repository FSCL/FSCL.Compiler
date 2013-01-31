namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type FunctionPreprocessingProcessor =    
    abstract member Handle: FunctionInfo * FunctionPreprocessingStep -> unit

and FunctionPreprocessingStep(pipeline: ICompilerPipeline, 
                              processors: FunctionPreprocessingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(pipeline)

    member private this.Process(k) =
        for p in processors do
            p.Handle(k ,this) 
               
    override this.Run(km: KernelModule) =
        for kernel in km.Kernels do
            this.Process(kernel)
        for f in km.Functions do
            this.Process(f)
        km


