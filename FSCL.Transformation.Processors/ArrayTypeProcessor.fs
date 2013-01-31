namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open System

type ArrayTypeProcessor() =         
    let GetArrayDimensions (t:Type) =
        let dimensionsString = t.FullName.Split([| '['; ']' |]).[1]
        let dimensions = ref 1
        String.iter (fun c -> if (c = ',') then dimensions := !dimensions + 1) dimensionsString
        !dimensions
             
    interface TypeProcessor with
        member this.Handle(t, engine:KernelBodyTransformationStage) =
            if (t.IsArray) then
                let dimensions = GetArrayDimensions(t)
                (true, Some(engine.Process(t.GetElementType()) + "*"))
            else
                (false, None)