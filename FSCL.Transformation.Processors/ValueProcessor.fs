namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultValueProcessor() =   
    interface ValueProcessor with
        member this.Handle(v, t, engine:KernelBodyTransformationStage) =
            engine.Process(t) |> ignore
            (true, Some(v.ToString()))