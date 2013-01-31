namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultIntegerRangeLoopProcessor() =   
    interface IntegerRangeLoopProcessor with
        member this.Handle(expr, var, startexpr, endexp, body, engine:KernelBodyTransformationStage) =
            (true, Some("for(" + engine.Process(var.Type) + " " + var.Name + " = " + engine.Process(startexpr) + "; " + var.Name + " <= " + engine.Process(endexp) + ";" + var.Name + "++) {\n" + engine.Process(body) + "\n}\n"))
           