open System
// Compiler user interface
open FSCL.Compiler
// Kernel language library
open FSCL.Compiler.KernelLanguage
    
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
    let compiler = new Compiler()
    // Declare input matrices 
    let a = Array2D.zeroCreate<float32> 1 1 
    let b = Array2D.zeroCreate<float32> 1 1 
    let c = Array2D.zeroCreate<float32> 1 1 
    let d = Array2D.zeroCreate<float32> 1 1 

    // Setup matrices content
    //...

    // #1a: Simplest kernel expression: single method/function reference
    let mutable result = compiler.Compile(<@@ MatrixMult @@>)
    
    // #1b: Kernels can also be functions with tupled arguments
    result <- compiler.Compile(<@@ MatrixMultTupled @@>)

    // #2: Compiler can compile also method/function calls, lifting the parameters
    result <- compiler.Compile(<@@ MatrixMult a b c @@>) 
        
    // #3: Lambda expressions (with tupled arguments)
    result <- compiler.Compile(<@@ 
                                    fun(a: float32[,], b: float32[,], c: float32[,]) ->
                                        let x = get_global_id(0)
                                        let y = get_global_id(1)

                                        let mutable accum = 0.0f
                                        for k = 0 to a.GetLength(1) - 1 do
                                            accum <- accum + (a.[x,k] * b.[k,y])
                                        c.[x,y] <- accum 
                                @@>) 
                      
    // #4: Instance or static methods
    let kc = new KernelContainer()
    result <- compiler.Compile(<@@ kc.MatrixMult @@>)

    // #5: Multiple kernels
    // This will compile into an OpenCL source containing both the kernels (add and multiply) 
    // together with data-structures reporting the relation between the two kernels
    result <- compiler.Compile(<@@ MatrixMult(MatrixAdd a b) c d @@>)

    // #6: A kernel returning a tuple can be the input of a kernel accepting a tupled set of parameters
    result <- compiler.Compile(<@@ MatrixAddOver(MatrixAddSub a b) @@>) 
    0


