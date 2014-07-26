(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
FSCL Kernel Programming
========================

With the compiler and the object-model provided by the FSCL.Compiler project you can write OpenCL kernels
as F# functions, static/instance methods and lambdas. This page gives an overview on kernel programming in FSCL.

###Basic example: Vector Addition

The most simple example of kernel programming is very likely parallel vector addition, where each thread (known as work-item in OpenCL)
sums the matching elements of the two input vectors whose index is determined by the thread id.
*)
(*** hide ***)
#r "FSCL.Compiler.dll"
#r "FSCL.Compiler.Core.dll"
#r "FSCL.Compiler.Language.dll"
(**
*)
open FSCL
open FSCL.Compiler
open FSCL.Language

[<ReflectedDefinition>]
let VectorAdd(a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    c.[myId] <- a.[myId] + b.[myId]

(**
Every FSCL kernel is characterized by an additional parameter of type `WorkItemInfo` (not necessarily the last one) that is used from within the kernel body
to retrieve all the information related to the work-items space (global/local id of the thread, global/local thread count, work-items space rank, etc.).
In addition, every kernel must be marked with `[<ReflectedDefinition>]` attribute to enable the compiler to inspect the AST of its body.

###A more complex example: Sobel Filter

The FSCL compiler library exposes an object-model that allows to write every possible OpenCL kernel in F#. In particular,
all the OpenCL built-in math/vector data/geometric functions are available to be used inside kernels, as like as vector data-types (e.g. float4, int3)
and parameter qualifiers (e.g. \_\_local_, \_\_constant). The *Image* subset of OpenCL has not been ported to FSCL yet, but it will be very soon.

The following example shows some of these features applied to the Sobel filter algorithm optimised for GPU execution.
In particular, we use vector data-types (`float4`, `uchar4`), we perform vector-types conversions (`ToFloat4()`, `ToUChar4()`) 
and we use built-in OpenCL math functions, such as `float4.hypot()`.
An important aspect to note is that the function input are 2D arrays. 
In fact, while in OpenCL C, kernel inputs are restricted to flat 1D arrays, FSCL allows to work with data of type `Array2D` and `Array3D`. The compiler
will automatically flat every istance of those types and appropriately manipulate the indexes used to access it.
*)
[<ReflectedDefinition>]
let SobelFilter2D(inputImage: uchar4[,], outputImage: uchar4[,], wi: WorkItemInfo) =
    let x = wi.GlobalID(0)
    let y = wi.GlobalID(1)

    let width = wi.GlobalSize(0)
    let height = wi.GlobalSize(1)

    let mutable Gx = float4(0.0f)
    let mutable Gy = Gx

    // Read each texel component and calculate the filtered value using neighbouring texel components 
    let i00 = (inputImage.[y, x]).ToFloat4()
    let i10 = (inputImage.[y, x + 1]).ToFloat4()
    let i20 = (inputImage.[y, x + 2]).ToFloat4()
    let i01 = (inputImage.[y + 1, x]).ToFloat4()
    let i11 = (inputImage.[y + 1, x + 1]).ToFloat4()
    let i21 = (inputImage.[y + 1, x + 2]).ToFloat4()
    let i02 = (inputImage.[y + 2, x]).ToFloat4()
    let i12 = (inputImage.[y + 2, x + 1]).ToFloat4()
    let i22 = (inputImage.[y + 2, x + 2]).ToFloat4()

    Gx <- i00 + float4(2.0f) * i10 + i20 - i02  - float4(2.0f) * i12 - i22
    Gy <- i00 - i20  + float4(2.0f) * i01 - float4(2.0f) * i21 + i02 - i22

    outputImage.[y, x] <- (float4.hypot(Gx, Gy)/float4(2.0f)).ToUChar4()

(**
###Another complex example: Matrix Multiplication

Matrix multiplication optimised for GPU execution is another example that shows how OpenCL programming
constructs and built-in functions are mapped into F#.
Here, the kernel uses a global property `BLOCK_SIZE` marked with `[<ReflectedDefinition>]`. Whenever a kernel
references a reflected property, the compiler produces an appropriate `#define` in the OpenCL source (in the particular case `#define BLOCK_SIZE 16`).

The example also shows how to declare *local* variables (i.e. data shares among the threads in a work group) inside kernels, that is
by wrapping `Array.zeroCreate` calls in the function `local()`.
*)
[<ReflectedDefinition>]
let BLOCK_SIZE = 16
[<ReflectedDefinition>]
let MatrixMult(matA: float32[,], matB: float32[,], matC: float32[,], wi: WorkItemInfo) =
    let bx = wi.GroupID(0)
    let by = wi.GroupID(1)
    let tx = wi.LocalID(0)
    let ty = wi.LocalID(1)
    let wa = matA.GetLength(0)
    let wb = matB.GetLength(0)

    let bCol = bx * BLOCK_SIZE
    let bBeginRow = 0
    let bStep  = BLOCK_SIZE
    let mutable bRow = bBeginRow
    let mutable Csub = 0.0f
 
    let As = local(Array2D.zeroCreate<float32> BLOCK_SIZE BLOCK_SIZE)
    let Bs = local(Array2D.zeroCreate<float32> BLOCK_SIZE BLOCK_SIZE)

    for aCol in 0 .. BLOCK_SIZE .. (wa - 1) do
        As.[ty, tx] <- matA.[by * BLOCK_SIZE, aCol]
        Bs.[ty, tx] <- matB.[bRow, bCol]
        wi.Barrier(CLK_LOCAL_MEM_FENCE)
 
        for k = 0 to BLOCK_SIZE - 1 do
            Csub <- Csub + (As.[ty,k] * Bs.[k,tx])
        wi.Barrier(CLK_LOCAL_MEM_FENCE)

        bRow <- bRow + bStep
    matC.[by * BLOCK_SIZE + ty, bx * BLOCK_SIZE + tx] <- Csub

(**
In OpenCL there are two ways to share data among the threads in a group. The first is by declaring
local variables inside the kernel body as shown in the previous example, the second is by using parameters 
qualified with *__local*. This last way allows to establish the size of the local data dynamically.

Parameter qualifiers are mapped to FSCL as .NET custom attributes. Given this, we may rewrite the example above,
lifting the local declarations from the kernel body and adding two local parameters as follows:
*)
[<ReflectedDefinition>]
let MatrixMultWithLocalParam(matA: float32[,], 
                             matB: float32[,], 
                             matC: float32[,], 
                             [<AddressSpace(AddressSpace.Local)>]
                             As: float32[,],
                             [<AddressSpace(AddressSpace.Local)>]
                             Bs: float32[,],
                             wi: WorkItemInfo) =
    let bx = wi.GroupID(0)
    let by = wi.GroupID(1)
    let tx = wi.LocalID(0)
    let ty = wi.LocalID(1)
    let wa = matA.GetLength(0)
    let wb = matB.GetLength(0)

    let bCol = bx * BLOCK_SIZE
    let bBeginRow = 0
    let bStep  = BLOCK_SIZE
    let mutable bRow = bBeginRow
    let mutable Csub = 0.0f

    for aCol in 0 .. BLOCK_SIZE .. (wa - 1) do
        As.[ty, tx] <- matA.[by * BLOCK_SIZE, aCol]
        Bs.[ty, tx] <- matB.[bRow, bCol]
        wi.Barrier(CLK_LOCAL_MEM_FENCE)
 
        for k = 0 to BLOCK_SIZE - 1 do
            Csub <- Csub + (As.[ty,k] * Bs.[k,tx])
        wi.Barrier(CLK_LOCAL_MEM_FENCE)

        bRow <- bRow + bStep
    matC.[by * BLOCK_SIZE + ty, bx * BLOCK_SIZE + tx] <- Csub

(**
The attribute `AddressSpace` is only one of the many provided by FSCL to add meta-information to kernels and kernel parameters.
For additional information about them, see [Dynamic Metadata Tutorial](dynamicMetadataTutorial.html).

###High-level constructs example: Vector Addition with return type

While enabling to code "classic" OpenCL kernels in F#, FSCL gives the chance to employ additional .NET/F# programming
constructs and data-types that are generally not supported in OpenCL.

The most important, especially from the [kernel composition](http://nuget.org/FSCL.Runtime) point of view, is the 
ability for and FSCL kernel to return a value.
In OpenCL, kernels are forced to return *void*, which is a constrain respected in all the previous examples. 
Nevertheless, the FSCL compiler is able to transform kernel returning non-void values into legal OpenCL kernels, replacing the returned
variable (it must be a variable) with an additional kernel parameter whose purpose is to be a container for the output data.

We can exploit this feature and rewrite our first example. The following code shows the two versions of the vector addition kernel, semantically equivalent, 
where the second is employing the FSCL kernel-return-types feature.

*)
// Classic OpenCL vector addition
[<ReflectedDefinition>]
let VectorAddNoReturn(a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    c.[myId] <- a.[myId] + b.[myId]

// Vector addition with return type
[<ReflectedDefinition>]
let VectorAddWithReturn(a: float32[], b:float32[], wi: WorkItemInfo) =
    let c = Array.zeroCreate<float32> a.Length

    let myId = wi.GlobalID(0)
    c.[myId] <- a.[myId] + b.[myId]

    c

(**
FSCL is supporting other kind of high-level constructs and data-types, such as *structs*, *records* and *reference cells*.
Reference cells are particularly interesting (and expressive) whenever a kernel output is a singleton (1-element) array.
For example, consider a kernel that executes a computation and eventually produces a scalar value as output. Generally,
the task of writing this value to the output buffer is performed by a specific thread (often the first one), so that
the kernel code looks like the following.
*)
[<ReflectedDefinition>]
let MyKernelWithArray(input: float32[], output:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    let mutable resultToWrite = 0.0f
    // Do some calculation and compute resultToWrite
    // ...

    // If I'm the first thread then write to the output
    if myId = 0 then
        output.[0] <- resultToWrite 

(**
In such a case, you can use a reference cell in place of the output array, as shown below.
*)
[<ReflectedDefinition>]
let MyKernelWithRefVar(input: float32[], output:float32 ref, wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    let mutable resultToWrite = 0.0f
    // Do some calculation and compute resultToWrite
    // ...

    // If I'm the first thread then write to the output
    if myId = 0 then
        output := resultToWrite 
(**
###Utility functions

When programming a kernel, you're not fored to encapsulate the whole code in a single kernel. Kernels can leverage on 
utility functions to performs some computations or well defined tasks.
For example, in the vector addition sample we may put the operation to be applied to the matching elements of the two input arrays in a separate function.
The OpenCL source produced will contain both the kernel and the utility function definitions.
*)
[<ReflectedDefinition>]
let op a b =
    a + b

[<ReflectedDefinition>]
let VectorAddWithUtilityFunction(a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    c.[myId] <- op a.[myId] b.[myId]

(**
###FSCL kernels as lambdas

FSCL kernels can be also expressed using the lambdas. For example, instad of defining a function, we may write the vector addition kernel as follows.
While the FSCL Compiler is still able to produce the appropriate kernel code in such a case, this time the name of the kernel in the OpenCL source produced
is automatically generated (it is no more *VectorAdd* ).
*)
fun (a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) ->
    let myId = wi.GlobalID(0)
    c.[myId] <- op a.[myId] b.[myId]

(**
###Using collection functions

In addition to custom FSCL kernels, programmers can compile to OpenCL references and calls to `Array` collection functions, such as `Array.sum`, `Array.map2` and `Array.reduce`. 
In such a case, the kernel code is not specified by the programmer but produced automatically by the compiler given the intrinsic semantic of those functions. 
For more information about the kernel source produced from collection functions, see [Compiler Interface Tutorial](compilerInterfaceTutorial.html).
*)