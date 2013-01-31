namespace FSCL.Transformation

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type ParameterProcessor =
    abstract member Handle : ParameterInfo * KernelSignatureTransformationStage -> bool * String option

and SignatureProcessor =
    abstract member Handle : MethodInfo * KernelSignatureTransformationStage -> bool * ((Expr * String) option)

and KernelSignatureTransformationStage() = 
    inherit TransformationStage<MethodInfo, (Expr * String)>()

    member val SignatureProcessors = new List<SignatureProcessor>() with get   
    member val ParameterProcessors = new List<ParameterProcessor>() with get      
           
    member this.Process(methodInfo:MethodInfo) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < this.SignatureProcessors.Count) do
            match this.SignatureProcessors.[index].Handle(methodInfo, this) with
            | (true, s) ->
                output <- s
            | (false, _) ->
                ()
        if output.IsNone then
            raise (new KernelTransformationException("The engine found a method info that cannot be handled [" + methodInfo.Name + "]"))
        output.Value
        
    member this.Process(p:ParameterInfo) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < this.ParameterProcessors.Count) do
            match this.ParameterProcessors.[index].Handle(p, this) with
            | (true, s) ->
                output <- s
            | (false, _) ->
                ()
        if output.IsNone then
            raise (new KernelTransformationException("The engine found a parameter that cannot be handled [" + p.Name + "]"))
        output.Value

    override this.Run(methodInfo:MethodInfo) =
        this.Process(methodInfo)
               


