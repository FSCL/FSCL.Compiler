namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type ModulePreprocessingProcessor =
    abstract member Handle : KernelModule * ModulePreprocessingStep -> unit

and [<Step("FSCL_MODULE_PREPROCESSING_STEP", 
           [| "FSCL_MODULE_PARSING_STEP" |])>]
    ModulePreprocessingStep(tm: TypeManager,
                            processors: ModulePreprocessingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm)
           
    member private this.Process(km) =
        for p in processors do
            p.Handle(km, this)
        km

    override this.Run(data) =
        this.Process(data)

        

