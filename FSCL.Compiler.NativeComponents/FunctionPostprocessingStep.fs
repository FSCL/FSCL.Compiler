namespace FSCL.Compiler.FunctionPostprocessing

open FSCL.Compiler
open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler.Util.VerboseCompilationUtil

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_FUNCTION_POSTPROCESSING_STEP",
      Dependencies = [| "FSCL_FUNCTION_TRANSFORMATION_STEP";
                        "FSCL_FUNCTION_PREPROCESSING_STEP";
                        "FSCL_MODULE_PREPROCESSING_STEP";
                        "FSCL_MODULE_PARSING_STEP" |])>]
type FunctionPostprocessingStep(tm: TypeManager, 
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
        let verb = StartVerboseStep(this, opts)

        for f in km.Functions do
            this.Process(f.Value :?> FunctionInfo, opts)
        this.Process(km.Kernel, opts)
        let r = ContinueCompilation(km)
        
        StopVerboseStep(verb)
        r


