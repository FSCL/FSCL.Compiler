namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultSequentialProcessor() =   
    interface SequentialProcessor with
        member this.Handle(expr, e1, e2, engine:KernelBodyTransformationStage) =
                (true, Some(engine.Process(e1) + "\n" + engine.Process(e2)))