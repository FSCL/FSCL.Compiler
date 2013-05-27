namespace FSCL.Compiler.ModulePreprocessing

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_MODULE_PREPROCESSING_STEP", 
      Dependencies = [| "FSCL_MODULE_PARSING_STEP" |])>]
type ModulePreprocessingStep(tm: TypeManager,
                             processors: ModulePreprocessingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm)
           
    member private this.Process(km) =
        for p in processors do
            p.Process(km, this)
        km

    override this.Run(data) =
        this.Process(data)

        

