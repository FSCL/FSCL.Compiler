// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Language
open FSCL
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System

// Vector addition
[<ReflectedDefinition>]
let Vector4DAdd(a: float4[], b: float4[], c: float4[]) =
    let gid = get_global_id(0)
    c.[gid] <- a.[gid] + b.[gid]
    
[<ReflectedDefinition>]
let Vector4DInvert(a: float4[], b: float4[], c: float4[]) =
    let gid = get_global_id(0)
    c.[gid].zwyx <- a.[gid].zwyx + b.[gid].zwyw
    
[<ReflectedDefinition>]
let Vector4DManipulation(a: float4[], b: float4[], c: float2[]) =
    let gid = get_global_id(0)
    let first = (a.[gid].xy + b.[gid].xy)
    let second = (a.[gid].wz + b.[gid].wz)
    c.[gid].x <- first.x + first.y
    c.[gid].y <- second.x + second.y

[<EntryPoint>]
let main argv =
    let compiler = Compiler()
    let result = compiler.Compile(<@ Vector4DAdd @>)
    let result = compiler.Compile(<@ Vector4DManipulation @>) :?> KernelModule
    Console.WriteLine(result.Code.Value.ToString())
    0
