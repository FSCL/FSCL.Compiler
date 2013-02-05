namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

[<Step("FSCL_FUNCTION_PREPROCESSING_STEP",
       [| "FSCL_MODULE_PREPROCESSING_STEP"; 
          "FSCL_MODULE_PARSING_STEP" |])>]
type FunctionPreprocessingStep(tm: TypeManager, 
                               processors: FunctionPreprocessingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm)

    member private this.Process(k) =
        for p in processors do
            p.Process(k ,this) 
               
    override this.Run(km: KernelModule) =
        for kernel in km.Kernels do
            this.Process(kernel)
        for f in km.Functions do
            this.Process(f)
        km


