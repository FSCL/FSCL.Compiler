open System
open FSCL.Compiler
open FSCL.Compiler.Language
open Microsoft.FSharp.Quotations
open System.Diagnostics
open Cloo
    
[<ReflectedDefinition>]
let VectorAdd(a: float32[], b: float32[], c: float32[]) =
    let i = get_global_id(0)
    c.[i] <- a.[i] + b.[i]
        
[<ReflectedDefinition>]
[<Device(0, 0)>]
let VectorAddWithMeta([<BufferWriteMode(BufferWriteMode.MapBuffer); 
                        MemoryFlags(ComputeMemoryFlags.AllocateHostPointer ||| ComputeMemoryFlags.ReadOnly)>]
                      a: float32[], 
                      [<BufferWriteMode(BufferWriteMode.MapBuffer); 
                        MemoryFlags(ComputeMemoryFlags.AllocateHostPointer ||| ComputeMemoryFlags.ReadOnly)>]
                      b: float32[], 
                      [<BufferReadMode(BufferReadMode.EnqueueReadBuffer); 
                        MemoryFlags(ComputeMemoryFlags.AllocateHostPointer ||| ComputeMemoryFlags.WriteOnly)>]
                      c: float32[]) =
    let i = get_global_id(0)
    c.[i] <- a.[i] + b.[i]

[<EntryPoint>]
let main argv =
    let compiler = new Compiler()
    let a = Array.create 10 2.0f
    let b = Array.create 10 2.0f
    let c = Array.zeroCreate<float32> 10

    // Kernel reference with no metadata associated
    let mutable result = compiler.Compile(<@@ VectorAdd @@>)

    // Kernel reference with metadata associated
    result <- compiler.Compile(<@@ VectorAddWithMeta @@>)
    
    // Kernel call with metadata associated
    result <- compiler.Compile(<@@ VectorAddWithMeta(a, b, c) @@>)
    
    // Kernel call with both static and dynamic (functions) metadata associated
    result <- compiler.Compile(<@@ DEVICE(0, 1, 
                                        VectorAddWithMeta(
                                            BUFFER_WRITE_MODE(BufferWriteMode.EnqueueWriteBuffer,
                                                    a), 
                                            BUFFER_WRITE_MODE(BufferWriteMode.EnqueueWriteBuffer,
                                                MEMORY_FLAGS(ComputeMemoryFlags.UseHostPointer,
                                                    b)), 
                                                    c)) @@>)
    0