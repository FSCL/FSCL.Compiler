namespace FSCL.Compiler.Util

open System.Reflection
open Microsoft.FSharp.Quotations
open System.Runtime.InteropServices
open System
open Microsoft.FSharp.Reflection

module ReflectionUtil =
    type MethodInfo with
        member this.TryGetGenericMethodDefinition() =
            if this.IsGenericMethod then
                this.GetGenericMethodDefinition()
            else
                this
    type Type with
        member t.IsStruct() =
            if (FSharpType.IsRecord(t) || (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum) && (typeof<unit> <> t) && (typeof<System.Void> <> t))) then   
                // Check this is not a ref type
                if (not(t.IsGenericType) || (t.GetGenericTypeDefinition() <> typeof<Microsoft.FSharp.Core.ref<int>>.GetGenericTypeDefinition())) then
                    true
                else
                    false
            else
                false
        member t.IsOption 
            with get() =
                t.IsGenericType && (t.GetGenericTypeDefinition() = typeof<int option>.GetGenericTypeDefinition())
        member t.OptionInnerType 
            with get() =
                t.GetGenericArguments().[0]

