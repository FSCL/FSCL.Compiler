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
    
    member private this.ChangeKernelOutputPoint(oldPoint: KernelOutputPoint,
                                                newPoint: KernelOutputPoint,
                                                currentNode: CallGraphNode) =
        for arg in List.ofSeq(currentNode.Arguments.Keys) do
            match currentNode.Arguments.[arg] with
            | KernelOutput(node, point) ->
                if node.KernelID = this.currentFunction.ID && point = oldPoint then
                    currentNode.Arguments.[arg] <- KernelOutput(node, newPoint)
                // Recursive search
                this.ChangeKernelOutputPoint(oldPoint, newPoint, node)
            | _ ->
                ()
                
    member private this.SetArgument(argument: string,
                                    value: CallGraphNodeArgument,
                                    currentNode: CallGraphNode) =
        if currentNode.KernelID = this.currentFunction.ID then
            if currentNode.Arguments.ContainsKey(argument) then
                currentNode.Arguments.[argument] <- value
            else
                currentNode.Arguments.Add(argument, value)
        for arg in List.ofSeq(currentNode.Arguments.Keys) do 
            match currentNode.Arguments.[arg] with
            | KernelOutput(node, point) ->
                this.SetArgument(argument, value, node)
            | _ ->
                ()

    member this.ChangeKernelOutputPoint(oldPoint: KernelOutputPoint,
                                        newPoint: KernelOutputPoint) =
        this.ChangeKernelOutputPoint(oldPoint, newPoint, this.currentModule.CallGraph)
        
    member this.SetArgument(argument: string,
                            value: CallGraphNodeArgument) =
        this.SetArgument(argument, value, this.currentModule.CallGraph)

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
        for k in km.GetKernels() do
            if not (k.Info.Skip) then
                this.Process(k.Info)
        for f in km.GetFunctions() do
            if not (f.Info.Skip) then
                this.Process(f.Info)
        km


