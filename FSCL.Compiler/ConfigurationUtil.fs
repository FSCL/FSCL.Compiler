namespace FSCL.Compiler.Configuration

open System
open System.Collections.Generic
open Microsoft.FSharp.Reflection

type ConfigurationUtil() =
    static member FlattenList<'T>(t: obj) =
        List.ofSeq(t :?> IEnumerable<'T>)

    static member ParseUnion<'T>(s) = 
        match FSharpType.GetUnionCases (typeof<'T>) |> Array.filter (fun case -> case.Name = s) with
        | [|case|] -> Some(FSharpValue.MakeUnion(case,[||]) :?> 'T)
        |_ -> None
