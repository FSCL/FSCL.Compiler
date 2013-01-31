namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultVarProcessor() =   
    interface VarProcessor with
        member this.Handle(v, engine:KernelBodyTransformationStage) =
            engine.Process(v.Type) |> ignore
            (true, Some(v.Name))