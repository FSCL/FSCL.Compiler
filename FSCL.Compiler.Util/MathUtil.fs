namespace FSCL.Compiler.Util

open System.Reflection
open Microsoft.FSharp.Quotations
open System.Runtime.InteropServices
open System

type MathUtil() =
    static member GCD (a, b) =
        if a = b then
            a
        else if a > b then
            MathUtil.GCD(a - b, b)
        else
            MathUtil.GCD(a, b - a)

    static member GetClosestPowerOf2 (a) =
        if (a < 0) then
            0
        else
            let mutable x = a - 1 
            x <- x ||| (x >>> 1)
            x <- x ||| (x >>> 2)
            x <- x ||| (x >>> 4)
            x <- x ||| (x >>> 8)
            x <- x ||| (x >>> 16)
            x + 1

        

