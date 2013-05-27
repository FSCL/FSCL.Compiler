// Compiler user interface
open FSCL.Compiler
// Kernel language library
open FSCL.Compiler.KernelLanguage
    
// Vector addition
[<ReflectedDefinition>]
let VectorAdd(a: float32[], b: float32[], c: float32[]) =
    let gid = get_global_id(0)
    c.[gid] <- a.[gid] + b.[gid]

[<EntryPoint>]
let main argv =
    let compiler = new Compiler()
    compiler.Compile(<@@ VectorAdd @@>)


