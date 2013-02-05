#r "Y:/Documents/Projects/FSCL/FSCL/bin/Debug/FSCL.dll"
open Microsoft.FSharp.Quotations
open FSCL.KernelFunctions
    
type StructType = 
    struct
        val x: int
        val y: int
    end

let e = <@@ let t = StructType() in t.x @@>

[<ReflectedDefinition>]
let t = <@@ let VectorAdd(a: float32[], b: float32[], c: float32[]) =
                let gid = get_global_id(0)
                c.[gid] <- a.[gid] + b.[gid]
            in 
                VectorAdd @@>