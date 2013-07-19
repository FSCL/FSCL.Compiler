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
    member val private currentModule:KernelModule = null with get, set

    member this.RemoveInConnection(conn) =
        let conns = this.currentModule.CallGraph.GetInputConnections(this.currentFunction.ID)
        for caller in conns do
            for c in caller.Value do
                if c.Value = conn then
                    this.currentModule.CallGraph.RemoveConnection(caller.Key, this.currentFunction.ID, c.Key)
                    
    member this.RemoveOutConnection(conn) =
        let conns = this.currentModule.CallGraph.GetOutputConnections(this.currentFunction.ID)
        for connection in conns do
            for c in connection.Value do
                if c.Key = conn then
                    this.currentModule.CallGraph.RemoveConnection(this.currentFunction.ID, connection.Key, c.Key)
                    
    member this.ChangeInConnection(before, after) =
        let conns = this.currentModule.CallGraph.GetInputConnections(this.currentFunction.ID)
        for caller in conns do
            for c in caller.Value do
                if c.Value = before then
                    this.currentModule.CallGraph.ChangeConnection(caller.Key, this.currentFunction.ID, c.Key, after)
                    
    member this.ChangeOutConnection(before, after) =
        let conns = this.currentModule.CallGraph.GetOutputConnections(this.currentFunction.ID)
        for connection in conns do
            for c in connection.Value do
                if c.Key = before then
                    this.currentModule.CallGraph.ChangeConnection(this.currentFunction.ID, connection.Key, after, c.Value)

    member private this.FunctionInfo 
        with get() =
            this.currentFunction
        and set(v) =
            this.currentFunction <- v

    member private this.Process(k) =
        this.FunctionInfo <- k
        for p in processors do
            p.Execute(k, this) |> ignore
               
    override this.Run(km: KernelModule) =
        this.currentModule <- km
        for k in km.CallGraph.KernelIDs do
            this.Process(km.CallGraph.GetKernel(k))
        for f in km.CallGraph.FunctionIDs do
            this.Process(km.CallGraph.GetFunction(f))
        km


