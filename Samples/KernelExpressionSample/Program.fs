open System
// Compiler user interface
open FSCL.Compiler
// Kernel language library
open FSCL.Compiler.KernelLanguage
open Microsoft.FSharp.Quotations
open System.Diagnostics
    
// Matrix addition (exploits FSCL return type for kernels)
[<ReflectedDefinition>]
let MatrixAdd (a: float32[,]) (b: float32[,]) =
    // Return type
    let c = Array2D.zeroCreate (a.GetLength(0)) (a.GetLength(1))
    // Computation
    let y = get_global_id(1)
    for i = 0 to a.GetLength(1) do
        c.[y, i] <- a.[y, i] + b.[y, i]
    c
    
// Matrix multiplication
[<ReflectedDefinition>]
let MatrixMult (a: float32[,]) (b: float32[,]) (c: float32[,]) =
    let x = get_global_id(0)
    let y = get_global_id(1)

    let mutable accum = 0.0f
    for k = 0 to a.GetLength(1) - 1 do
        accum <- accum + (a.[x,k] * b.[k,y])
    c.[x,y] <- accum
    
// Matrix multiplication (tupled arguments)
[<ReflectedDefinition>]
let MatrixMultTupled(a: float32[,], b: float32[,], c: float32[,]) =
    let x = get_global_id(0)
    let y = get_global_id(1)

    let mutable accum = 0.0f
    for k = 0 to a.GetLength(1) - 1 do
        accum <- accum + (a.[x,k] * b.[k,y])
    c.[x,y] <- accum
    
// Demonstrate the usage of an Object method as a kernel 
type KernelContainer() =
    [<ReflectedDefinition>]
    member this.MatrixMult(a: float32[,], b: float32[,], c: float32[,]) =
        let x = get_global_id(0)
        let y = get_global_id(1)

        let mutable accum = 0.0f
        for k = 0 to a.GetLength(1) - 1 do
            accum <- accum + (a.[x,k] * b.[k,y])
        c.[x,y] <- accum
        
// Matrix addition and subtraction (exploits FSCL tupled return types)
[<ReflectedDefinition>]
let MatrixAddSub (a: float32[,]) (b: float32[,]) =
    // Return types
    let add = Array2D.zeroCreate (a.GetLength(0)) (a.GetLength(1))
    let sub = Array2D.zeroCreate (a.GetLength(0)) (a.GetLength(1))
    // Computation
    let y = get_global_id(1)
    for i = 0 to a.GetLength(1) do
        add.[y, i] <- a.[y, i] + b.[y, i]
        sub.[y, i] <- a.[y, i] - b.[y, i]
    // Tupled return
    (add, sub)
    
// Matrix sum ovewriting the second matrix
[<ReflectedDefinition>]
let MatrixAddOver (a: float32[,], b: float32[,]) =
    // Computation
    let y = get_global_id(1)
    for i = 0 to a.GetLength(1) do
        b.[y, i] <- a.[y, i] + b.[y, i]

[<EntryPoint>]
let main argv =
    let timer = new Stopwatch()
    timer.Start()
    let compiler = new Compiler()
    timer.Stop()
    Console.WriteLine("Compiler instantiation: " + timer.ElapsedMilliseconds.ToString() + "ms")
    timer.Start()
    // Declare input matrices 
    let a = Array2D.zeroCreate<float32> 1000 1000 
    let b = Array2D.zeroCreate<float32> 1000 1000 
    let c = Array2D.zeroCreate<float32> 1000 1000 
    let d = Array2D.zeroCreate<float32> 1000 1000
    timer.Stop()
    Console.WriteLine("Input instantiation: " + timer.ElapsedMilliseconds.ToString() + "ms")

    // Setup matrices content
    //...

    // #1a: Simplest kernel expression: single method/function reference
    timer.Reset()
    timer.Start()
    let mutable result = compiler.Compile(<@@ MatrixMult @@>)
    timer.Stop()
    Console.WriteLine("First kernel: " + timer.ElapsedMilliseconds.ToString() + "ms")
    
    // #1b: Kernels can also be functions with tupled arguments
    timer.Reset()
    timer.Start()
    result <- compiler.Compile(<@@ MatrixMultTupled @@>)
    timer.Stop()
    Console.WriteLine("Second kernel: " + timer.ElapsedMilliseconds.ToString() + "ms")

    // #2: Compiler can compile also method/function calls, lifting the parameters
    timer.Reset()
    timer.Start()
    result <- compiler.Compile(<@@ MatrixMult a b c @@>) 
    timer.Stop()
    Console.WriteLine("Third kernel: " + timer.ElapsedMilliseconds.ToString() + "ms")
        
    // #3: Lambda expressions (with tupled arguments)
    timer.Reset()
    timer.Start()
    result <- compiler.Compile(<@@ 
                                    fun(a: float32[,], b: float32[,], c: float32[,]) ->
                                        let x = get_global_id(0)
                                        let y = get_global_id(1)

                                        let mutable accum = 0.0f
                                        for k = 0 to a.GetLength(1) - 1 do
                                            accum <- accum + (a.[x,k] * b.[k,y])
                                        c.[x,y] <- accum 
                                @@>) 
    timer.Stop()
    Console.WriteLine("Fourth kernel: " + timer.ElapsedMilliseconds.ToString() + "ms")
                      
    // #4: Instance or static methods
    timer.Reset()
    timer.Start()
    let kc = new KernelContainer()
    result <- compiler.Compile(<@@ kc.MatrixMult @@>)
    timer.Stop()
    Console.WriteLine("Fifth kernel: " + timer.ElapsedMilliseconds.ToString() + "ms")

    // #5: Multiple kernels
    // This will compile into an OpenCL source containing both the kernels (add and multiply) 
    // together with data-structures reporting the relation between the two kernels
    timer.Reset()
    timer.Start()
    result <- compiler.Compile(<@@ MatrixMult(MatrixAdd a b) c d @@>)
    timer.Stop()
    Console.WriteLine("Sixth kernel: " + timer.ElapsedMilliseconds.ToString() + "ms")

    // #6: A kernel returning a tuple can be the input of a kernel accepting a tupled set of parameters
    timer.Reset()
    timer.Start()
    result <- compiler.Compile(<@@ MatrixAddOver(MatrixAddSub a b) @@>) 
    timer.Stop()
    Console.WriteLine("Seventh kernel: " + timer.ElapsedMilliseconds.ToString() + "ms")
    0


