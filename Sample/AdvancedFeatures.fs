module AdvancedFeatures

open FSCL
open FSCL.Compiler.KernelLanguage

// Demonstrate the usage of math functions
[<ReflectedDefinition>]
let Fmad(a: float32[], b: float32[], c: float32[]) =
    let gid = get_global_id(0)
    c.[gid] <- (float32) (fmax((float)a.[gid], (float)b.[gid]))

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
    
// Demonstrate the usage of struct types
type PairStruct =
    struct
        val mutable x: float
        val mutable y: float
    end
[<ReflectedDefinition>]
let inline VectorAddStruct (a: PairStruct[], b: PairStruct[], c: PairStruct[]) =
    let gid = get_global_id(0)
    c.[gid].x <- a.[gid].x + b.[gid].x
    c.[gid].y <- a.[gid].y + b.[gid].y
    
// Demonstrate the usage of record types
type PairRecord = {
     mutable x: float
     mutable y: float
}
[<ReflectedDefinition>]
let inline VectorAddRecord (a: PairRecord[], b: PairRecord[], c: PairRecord[]) =
    let gid = get_global_id(0)
    c.[gid].x <- a.[gid].x + b.[gid].x
    c.[gid].y <- a.[gid].y + b.[gid].y
        
//Demonstrate the usage of utility functions
[<ReflectedDefinition>]
let sum(a:float32, b:float32) =
    a + b

[<ReflectedDefinition>]
let VectorAddWithUtility(a: float32[], b: float32[], c: float32[]) =
    let gid = get_global_id(0)
    c.[gid] <- sum(a.[gid], b.[gid])

// Demonstrate the usage of an Object method as a kernel 
type KernelContainer() =
    [<ReflectedDefinition>]
    member this.VectorAdd(a: float32[], b: float32[], c: float32[]) =
        let gid = get_global_id(0)
        c.[gid] <- a.[gid] + b.[gid]
