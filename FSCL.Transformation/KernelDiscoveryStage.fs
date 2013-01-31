namespace FSCL.Transformation

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type DiscoveryProcessor =
    abstract member Handle : Expr * KernelDiscoveryStage -> bool * MethodInfo option

and KernelDiscoveryStage() = 
    inherit TransformationStage<Expr, MethodInfo>()

    member val DiscoveryProcessors = new List<DiscoveryProcessor>() with get     
           
    member this.Process(expr:Expr) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < this.DiscoveryProcessors.Count) do
            match this.DiscoveryProcessors.[index].Handle(expr, this) with
            | (true, s) ->
                output <- s
            | (false, _) ->
                ()
        if output.IsNone then
            raise (new KernelTransformationException("The engine is not able to discover a kernel inside the expression [" + expr.ToString() + "]"))
        output.Value

    override this.Run(expr) =
        this.Process(expr)
        

