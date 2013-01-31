namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type ModulePreprocessingProcessor =
    abstract member Handle : KernelModule * ModulePreprocessingStep -> unit

and ModulePreprocessingStep(pipeline: ICompilerPipeline,
                            processors: ModulePreprocessingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(pipeline)
           
    member private this.Process(km) =
        for p in processors do
            p.Handle(km, this)
        km

    override this.Run(data) =
        this.Process(data)

        

