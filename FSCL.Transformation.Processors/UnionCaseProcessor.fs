namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type DefaultUnionCaseProcessor() =   
    interface UnionCaseProcessor with
        member this.Handle(e, ucInfo, args, engine:KernelBodyTransformationStage) =
            if ucInfo.DeclaringType.DeclaringType.Name = "fscl" then
                (true, Some(ucInfo.Name))
            else
                (false, None)