namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultVarSetProcessor() =   
    interface VarSetProcessor with
        member this.Handle(expr, v, e, engine:KernelBodyTransformationStage) =
            engine.Process(v.Type) |> ignore
            (true, Some(v.Name + " = " + engine.Process(e) + ";"))