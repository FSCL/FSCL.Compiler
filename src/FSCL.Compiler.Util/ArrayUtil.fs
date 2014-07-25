namespace FSCL.Compiler.Util

open System.Reflection
open Microsoft.FSharp.Quotations
open System.Runtime.InteropServices
open System

type ArrayUtil() =
    static member GetArrayDimensions (t:System.Type) =
        // If not array return 0
        if t.IsArray then
            // Any better way to do this?
            let dimensionsString = t.FullName.Split([| '[' ; ']' |]).[1]
            let dimensions = ref 1
            String.iter (fun c -> if (c = ',' )then dimensions := !dimensions + 1) dimensionsString
            !dimensions
        else
            0
            
    static member GetArrayAllocationSize (o) =
        // If not array return -1
        if o.GetType().IsArray then
            let elementsCount = o.GetType().GetProperty("LongLength").GetValue(o) :?> int64
            elementsCount * (int64)(Marshal.SizeOf(o.GetType().GetElementType()))
        else
            -1L

    static member GetArrayLength (o) =
        if o.GetType().IsArray then
            o.GetType().GetProperty("Length").GetValue(o) :?> int32
        else
            -1
            
    static member GetArrayLengths (o) =
        if o.GetType().IsArray then     
            // Any better way to do this?
            let rank = o.GetType().GetProperty("Rank").GetValue(o) :?> int
            (seq {
                for i = 0 to rank - 1 do
                    yield o.GetType().GetMethod("GetLongLength").Invoke(o, [| i |]) :?> int64
                    }) |> Seq.toArray
        else
            [||]
            
    static member GetArrayOrBufferLengths (o) =
        // Any better way to do this?
        let rank = o.GetType().GetProperty("Rank").GetValue(o) :?> int
        (seq {
            for i = 0 to rank - 1 do
                yield o.GetType().GetMethod("GetLongLength").Invoke(o, [| i |]) :?> int64
                }) |> Seq.toArray
