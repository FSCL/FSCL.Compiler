// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
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
    let mutable result = compiler.Compile(<@ Vector4DAdd @>)
    result <- compiler.Compile(<@ Vector4DInvert @>)
    result <- compiler.Compile(<@ Vector4DManipulation @>)
    
    let template = 
        <@
            fun(g_idata:int[], [<Local>]sdata:int[], n, g_odata:int[]) ->
                let tid = get_local_id(0)
                let i = get_group_id(0) * (get_local_size(0) * 2) + get_local_id(0)

                sdata.[tid] <- if(i < n) then g_idata.[i] else 0
                if (i + get_local_size(0) < n) then 
                    sdata.[tid] <- sdata.[tid] + g_idata.[i + get_local_size(0)]

                barrier(CLK_LOCAL_MEM_FENCE)
                // do reduction in shared mem
                let mutable s = get_local_size(0) >>> 1
                while (s > 0) do 
                    if (tid < s) then
                        sdata.[tid] <- sdata.[tid] + sdata.[tid + s]
                    barrier(CLK_LOCAL_MEM_FENCE)
                    s <- s >>> 1

                if (tid = 0) then 
                    g_odata.[get_group_id(0)] <- sdata.[0]
        @>
    0
