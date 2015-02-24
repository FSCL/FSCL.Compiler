namespace FSCL.Compiler.ModulePostprocessing

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_MODULE_POSTPROCESSING_STEP",
       Dependencies = [| "FSCL_FUNCTION_TRANSFORMATION_STEP";
                         "FSCL_FUNCTION_POSTPROCESSING_STEP";
                         "FSCL_FUNCTION_PREPROCESSING_STEP";
                         "FSCL_MODULE_PREPROCESSING_STEP";
                         "FSCL_MODULE_PARSING_STEP" |])>]
type ModulePostprocessingStep(tm: TypeManager, 
                              processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<ComputingExpressionModule, ComputingExpressionModule>(tm, processors)
        
    member private this.Process(km, opts) =
        for p in processors do
            p.Execute(km, this, opts) |> ignore

    override this.Run(cem, opts) =
        for km in cem.KernelModulesToCompile do
            this.Process(km, opts)
        ContinueCompilation(cem)