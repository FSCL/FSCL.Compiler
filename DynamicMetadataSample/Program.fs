open System
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Quotations
open System.Diagnostics
open System.Reflection
    
// Kernel with no meta
[<ReflectedDefinition>]
let VectorAdd(a: float32[], b: float32[], c: float32[]) =
    let i = get_global_id(0)
    c.[i] <- a.[i] + b.[i]
    
// Kernel with set of kernel/parameters meta that are invariant to the compilation result
// Their value is not affecting the compiler
[<ReflectedDefinition>]
let VectorAddWithMetaInvariant([<BufferWriteMode(BufferWriteMode.MapBuffer)>]
                                  a: float32[],
                                  b: float32[],
                                  c: float32[]) =
    let i = get_global_id(0)
    c.[i] <- a.[i] + b.[i]
        
// Kernel with set of kernel/parameters meta that are NOT invariant to the compilation result
// Their value is affecting the compiler result (address space affects the modifier of the parameter when code is generated)
[<ReflectedDefinition>]
let VectorAddWithMetaNotInvariant([<AddressSpace(AddressSpace.Constant)>]
                                  a: float32[],
                                  b: float32[],
                                  c: float32[]) =
    let i = get_global_id(0)
    c.[i] <- a.[i] + b.[i]
    
// Kernel with set of kernel/parameters meta that are NOT invariant to the compilation result, but whose values are equal to default one
// AddressSpace.Global is the default value of AddressSpace. Therefore, the compilation result is the same of the one produced
// with no address space meta
[<ReflectedDefinition>]
let VectorAddWithMetaNotInvariantButEqual([<AddressSpace(AddressSpace.Global)>]
                                          a: float32[],
                                          b: float32[],
                                          c: float32[]) =
    let i = get_global_id(0)
    c.[i] <- a.[i] + b.[i]
        
// Kernel with kernel meta (device type) and parameters meta (buffer write mode, memory flags)
[<ReflectedDefinition>]
[<DeviceType(DeviceType.Gpu)>]
let VectorAddWithMeta([<BufferWriteMode(BufferWriteMode.MapBuffer); 
                        MemoryFlags(MemoryFlags.AllocHostPointer ||| MemoryFlags.ReadOnly)>]
                      a: float32[], 
                      [<BufferWriteMode(BufferWriteMode.MapBuffer); 
                        MemoryFlags(MemoryFlags.AllocHostPointer ||| MemoryFlags.ReadOnly)>]
                      b: float32[], 
                      [<BufferReadMode(BufferReadMode.EnqueueReadBuffer); 
                        MemoryFlags(MemoryFlags.AllocHostPointer ||| MemoryFlags.WriteOnly)>]
                      c: float32[]) =
    let i = get_global_id(0)
    c.[i] <- a.[i] + b.[i]

let printMeta(km: KernelModule) =
    Console.WriteLine("  --- KERNEL META --- ")
    for item in (km.Kernel.Meta.KernelMeta :?> KernelMetaCollection).Collection do
        Console.WriteLine(item.Key.ToString() + " = " + item.Value.ToString())
    Console.WriteLine("  --- RETURN META --- ")
    for item in (km.Kernel.Meta.ReturnMeta).Collection do
        Console.WriteLine(item.Key.ToString() + " = " + item.Value.ToString())
    Console.WriteLine("  --- PARAMS META --- ")
    for i = 0 to km.Kernel.OriginalParameters.Length - 1 do
        Console.WriteLine("  --- PARAM " + km.Kernel.OriginalParameters.[i].Name)    
        for item in km.Kernel.Meta.ParamMeta.[i].Collection do
            Console.WriteLine(item.Key.ToString() + " = " + item.Value.ToString())
    
[<EntryPoint>]
let main argv =
    let compiler = new Compiler()
    let a = Array.create 10 2.0f
    let b = Array.create 10 2.0f
    let c = Array.zeroCreate<float32> 10

    // Kernel reference with no metadata associated
    let mutable index = 1
    Console.WriteLine(index.ToString() + ") By default, the set of meta of a kernel resulting from compilation is empty.")
    Console.WriteLine("  Compiling VectorAdd")
    let nometakmodule = compiler.Compile(<@@ VectorAdd @@>) :?> KernelModule
    printMeta(nometakmodule)
    Console.WriteLine("  This means that every possible meta has its default value resulting from invoking its parameterless constructor")
    Console.WriteLine("  e.g: DeviceType meta for VectorAdd = " + (nometakmodule.Kernel.Meta.KernelMeta.Get<DeviceTypeAttribute>()).Type.ToString())
    Console.WriteLine()
    index <- index + 1

    // Kernel reference with invariant metadata associated
    Console.WriteLine(index.ToString() + ") Compilation-invariant meta are not affecting the result of the compilation.")
    Console.WriteLine("  Compiling VectorAddWithMetaInvariant")
    let kmodule = compiler.Compile(<@@ VectorAddWithMetaInvariant @@>) :?> KernelModule
    printMeta(kmodule)
    Console.WriteLine("  Checking if the set of metadata values doesn't affect the default (no meta) compilation result: " + (compiler.IsInvariantToMetaCollection(nometakmodule.Kernel.Meta, kmodule.Kernel.Meta).ToString()))
    Console.WriteLine()
    index <- index + 1

    // Kernel reference with non-invariant metadata associated
    Console.WriteLine(index.ToString() + ") Meta that are not compilation-invariant may affect the result of the compilation.")
    Console.WriteLine("  Compiling VectorAddWithMetaNotInvariant")
    let kmodule = compiler.Compile(<@@ VectorAddWithMetaNotInvariant @@>) :?> KernelModule
    printMeta(kmodule)
    Console.WriteLine("  Checking if the set of metadata values doesn't affect the default (no meta) compilation result: " + (compiler.IsInvariantToMetaCollection(nometakmodule.Kernel.Meta, kmodule.Kernel.Meta).ToString()))
    Console.WriteLine()
    index <- index + 1
    
    // Kernel reference with non-invariant metadata associated whose
    Console.WriteLine(index.ToString() + ") Meta that are not compilation-invariant may affect the result of the compilation.")
    Console.WriteLine("  e.g. AddressSpace is used to generate the kernel signature. No AddressSpace meta, AddressSpace.Global and AddressSpace.Auto are considered equal, which means that you can interchange these values wihout affecting the result of the compilation")
    Console.WriteLine("  Compiling VectorAddWithMetaNotInvariantButEqual")
    let kmodule = compiler.Compile(<@@ VectorAddWithMetaNotInvariantButEqual @@>) :?> KernelModule
    printMeta(kmodule)
    Console.WriteLine("  Checking if the set of metadata values doesn't affect the default (no meta) compilation result: " + (compiler.IsInvariantToMetaCollection(nometakmodule.Kernel.Meta, kmodule.Kernel.Meta).ToString()))
    Console.WriteLine()
    index <- index + 1

    // Kernel call with buch of metadata associated
    Console.WriteLine(index.ToString() + ") Meta can be associated to Kernel, to the Return value and to the kernel Parameters.")
    Console.WriteLine("  Compiling VectorAddWithMeta")
    let kmodule = compiler.Compile(<@@ VectorAddWithMeta @@>) :?> KernelModule
    printMeta(kmodule)
    Console.WriteLine("  Checking if the set of metadata values doesn't affect the default (no meta) compilation result: " + (compiler.IsInvariantToMetaCollection(nometakmodule.Kernel.Meta, kmodule.Kernel.Meta).ToString()))
    Console.WriteLine()
    index <- index + 1
    
    // Kernel call with both static and dynamic (functions) metadata associated    
    Console.WriteLine(index.ToString() + ") Meta can be static or dynamic.")
    Console.WriteLine("  A static kernel meta is an attribute marking part of the kernel definition. Each static meta comes together with the dynamic counterpart, which is a function that can be used to set the value of the meta at kernel-compile time")
    Console.WriteLine("  Compiling VectorAddWithMeta using dynamic meta")
    let kmodule = compiler.Compile(<@@ DEVICE_TYPE(DeviceType.Cpu, 
                                        VectorAddWithMeta(
                                            BUFFER_WRITE_MODE(BufferWriteMode.EnqueueWriteBuffer,
                                                    a), 
                                            BUFFER_WRITE_MODE(BufferWriteMode.EnqueueWriteBuffer,
                                                MEMORY_FLAGS(MemoryFlags.UseHostPointer,
                                                    b)), 
                                                    c)) @@>) :?> KernelModule
    printMeta(kmodule)
    Console.WriteLine("  Checking if the set of metadata values doesn't affect the default (no meta) compilation result: " + (compiler.IsInvariantToMetaCollection(nometakmodule.Kernel.Meta, kmodule.Kernel.Meta).ToString()))
    Console.WriteLine()
    index <- index + 1

    Console.WriteLine("Press enter to exit")
    Console.Read() |> ignore
    0