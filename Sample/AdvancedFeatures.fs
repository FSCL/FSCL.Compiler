module AdvancedFeatures

open FSCL
open FSCL.Compiler.KernelLanguage

// Demonstrate the usage of ref variables as arrays of 1 element
[<ReflectedDefinition>]
let SingleReturn(a: float32[], b: float32[], c: float32 ref) =
    let gid = get_global_id(0)
    for i = 0 to a.Length - 1 do
        c := !c + a.[gid] + b.[gid]

// Demonstrate the usage of generic types (Only for primitive types)
[<ReflectedDefinition>]
let inline VectorAddGeneric (a: ^T[]) (b: ^T[]) (c: ^T[]) =
    let gid = get_global_id(0)
    c.[gid] <- a.[gid] + b.[gid]
