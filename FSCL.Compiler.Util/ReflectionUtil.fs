namespace FSCL.Compiler.Util

open System.Reflection
open Microsoft.FSharp.Quotations
open System.Runtime.InteropServices
open System

module ReflectionUtil =
    type MethodInfo with
        member this.TryGetGenericMethodDefinition() =
            if this.IsGenericMethod then
                this.GetGenericMethodDefinition()
            else
                this
