namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultWhileLoopProcessor() =   
    interface WhileLoopProcessor with
        member this.Handle(expr, cond, body, engine:KernelBodyTransformationStage) =
            (true, Some("while(" + engine.Process(cond) + ") {\n" + engine.Process(body) + "\n}\n"))