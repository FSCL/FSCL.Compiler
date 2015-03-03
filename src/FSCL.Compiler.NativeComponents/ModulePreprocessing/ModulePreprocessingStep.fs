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
                             processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelExpression, KernelExpression>(tm, processors)
           
    member private this.Process(km, opts) =
        for p in processors do
            p.Execute(km, this, opts) |> ignore

    override this.Run(cem, opts) =
        for km in cem.KernelModulesRequiringCompilation do
            this.Process(km, opts)
        ContinueCompilation(cem)

        

