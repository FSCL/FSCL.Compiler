namespace FSCL.Transformation

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type TransformationGlobalState = Dictionary<string, obj>

type TransformationStageBase() =
    let transformationData = TransformationGlobalState()

    member this.SetTransformationGlobalState(t:TransformationGlobalState) =
        transformationData.Clear()
        for pair in t do
            transformationData.Add(pair.Key, pair.Value)

    member this.AddTransformationData(s, d) =
        if transformationData.ContainsKey(s) then
            transformationData.[s] <- d
        else
            transformationData.Add(s, d)

    member this.RemoveTransformationData(s) =
        if transformationData.ContainsKey(s) then
            transformationData.Remove(s) |> ignore

    member this.TransformationData(s) =
        if transformationData.ContainsKey(s) then
            Some(transformationData.[s])
        else
            None

    member this.TransformationDataCopy
        with get() =
            new TransformationGlobalState(transformationData)

[<AbstractClass>]
type TransformationStage<'T,'U>() =
    inherit TransformationStageBase()

    abstract member Run: 'T -> 'U

    static member (-->) (s1:TransformationStage<'T,'U>, s2:TransformationStage<'U,'W>) =
        new SequentialTransformationStage<'T,'U,'W>(s1, s2)

    static member (+) (s1:TransformationStage<'T,'U>, s2:TransformationStage<'S,'U>) =
        new ChoiceTransformationStage<'T,'S,'U>(s1, s2)


and SequentialTransformationStage<'T,'U,'W>(s1:TransformationStage<'T,'U>, s2:TransformationStage<'U,'W>) =
    inherit TransformationStage<'T,'W>()
    override this.Run(el) =
        let result1 = el |> s1.Run
        s2.SetTransformationGlobalState(s1.TransformationDataCopy)
        let result2 = s2.Run(result1)
        this.SetTransformationGlobalState(s2.TransformationDataCopy)
        result2

and ChoiceTransformationStage<'T,'U,'W> (s1:TransformationStage<'T,'W>, s2:TransformationStage<'U,'W>) =
    inherit TransformationStage<'T option * 'U option,'W>()
    override this.Run((e1,e2)) =
        if e1.IsSome then
            let result = s1.Run(e1.Value)
            this.SetTransformationGlobalState(s1.TransformationDataCopy)
            result
        else
            let result = s2.Run(e2.Value)
            this.SetTransformationGlobalState(s2.TransformationDataCopy)
            result
    
