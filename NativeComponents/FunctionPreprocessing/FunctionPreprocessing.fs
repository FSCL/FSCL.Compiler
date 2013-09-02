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
    member val FlowGraph = null with get, set

    member this.FunctionInfo 
        with get() =
            this.currentFunction
        and private set(v) =
            this.currentFunction <- v

    member private this.Process(k) =
        this.FunctionInfo <- k
        for p in processors do
            p.Execute(k, this) |> ignore
               
    override this.Run(km: KernelModule) =
        this.FlowGraph <- km.FlowGraph
        for k in km.GetKernels() do
            if not (k.Info.Skip) then
                this.Process(k.Info)
        for f in km.GetFunctions() do
            if not (f.Info.Skip) then
                this.Process(f.Info)
        km


