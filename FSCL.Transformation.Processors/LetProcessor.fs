namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultLetProcessor() =   
    interface LetProcessor with
        member this.Handle(expr, v, value, body, engine:KernelBodyTransformationStage) =
            (true, Some(engine.Process(v.Type) + " " + v.Name + " = " + engine.Process(value) + ";\n" + engine.Process(body)))