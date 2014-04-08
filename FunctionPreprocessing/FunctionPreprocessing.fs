namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_FUNCTION_PREPROCESSING_STEP", 
       Dependencies = [| "FSCL_MODULE_PREPROCESSING_STEP"; "FSCL_MODULE_PARSING_STEP" |])>]
type FunctionPreprocessingStep(tm: TypeManager, 
                               processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm, processors)
    
    member val private currentFunction:FunctionInfo = null with get, set
   
    member this.FunctionInfo 
        with get() =
            this.currentFunction
        and private set(v) =
            this.currentFunction <- v

    member private this.Process(k, opts) =
        this.FunctionInfo <- k
        for p in processors do
            p.Execute(k, this, opts) |> ignore
               
    override this.Run(km: KernelModule, opts) =
        for f in km.Functions do
            this.Process(f.Value :?> FunctionInfo, opts)
        this.Process(km.Kernel, opts)
        ContinueCompilation(km)


