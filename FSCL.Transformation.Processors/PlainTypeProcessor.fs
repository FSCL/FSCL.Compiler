namespace FSCL.Transformation.Processors

open FSCL.Transformation
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

type PlainTypeProcessor() =          
    interface TypeProcessor with
        member this.Handle(t, engine:KernelBodyTransformationStage) =
            if (t = typeof<uint32>) then
                (true, Some("unsigned int"))            
            elif (t = typeof<uint64>) then
                (true, Some("unsigned long"))
            elif (t = typeof<int64>) then
                (true, Some("long"))
            elif (t = typeof<int>) then
                (true, Some("int"))
            elif (t = typeof<double>) then
                (true, Some("double"))
            elif (t = typeof<float32>) then
                (true, Some("float"))
            elif (t = typeof<bool>) then
                (true, Some("int"))
            elif (t = typeof<float>) then
                (true, Some("float"))
            else
                (false, None)