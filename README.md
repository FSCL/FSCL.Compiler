FSCL.Compiler
=============

FSharp to OpenCL Compiler
-------------------------

###FSCL documentation

A draft of the FSCL compiler and FSCL runtime specification is available: 
[FSCL compiler and runtime documentation](https://github.com/GabrieleCocco/FSCL.Compiler/blob/master/FSCL%20Documentation%20v1.0.pdf)

Note that the FSCL runtime is a separate project hosted on gihub: [FSCL runtime project](https://github.com/GabrieleCocco/FSCL.Runtime)

###At a glance

FSCL.Compiler is an F# to OpenCL compiler that allows programmers to develop OpenCL kernels inside .NET, with all the benefits of 
code-completion, type-checking and many other facilities provided by the .NET visual machine environment.
FSCL.Compiler currently supports all the features of OpenCL Specification v1.2 except for the image-related processing, which is under development.

In addition to the ability to express OpenCL-C99-like kernels in F#, FSCL.Compiler introduces many higher-level features to be used in kernel coding, such as generic types, F# structs,
F# ref cells and kernel with return types, which globally allows to increase the abstraction over "standard kernel coding".

In particular, the list of enhanced features currently supported in kernel writing are:

+ *Automatic array length*: when coding OpenCL C kernels working on arrays, the length of each array often must be explicitely passed as an additional parameter. Thanks to the power of reflection and dynamic method construction of .NET, programmers can code F# kernels without the need of passing the length of each input/output array. Whenever the length of an array is required in the kernel body, programmers can use the well-known "length" property exposed by the .NET array type. The FSCL compiler is capable of genenrating additional parameters to host the length of each input/output arrays and to replace each usage of the "length" property with a reference to the appropriate additional parameter;
+ *Ref variables*: ref variables can be used as kernel parameters to used to pass information from the kernel to the host without using arrays. Referenced values can be either primitive values, records or structs. The compiler lifts ref variables replacing them with arrays of unary length;
+ *Records and structs*: records and structs containing primitive fields can be passed to F# kernels. Struct/record parameters are processed to generate C99 struct declaration in the OpenCL kernel source;
+ *Return type*: OpenCL kernels are constrained to return no value (void). The FSCL compiler removes this contraint and allows F# kernels to return array values. When a kernel returns a value, the compiler analyzes the whole kernel body and produces a valid OpenCL C99 code with an additional buffer representing the returned data. The ability to return a value allows to define kernels as overloaded operators (e.g. a multiply operator for two matrixes, which returns a matrix);
+ *Generic kernels*: the FSCL compiler allows to declare generic F# kernels. For example, programmers can write a parallel matrix multiplication once, using generic arrays, and then run it on two integer, float, double matrices. The only contraint set by the compiler infrastructure is that generic types can be instantiated only with primitive types.
When an F# kernel containing generic parameters is compiled, FSCL produces an instance of kernel for each possible combination of primitive types assigned to each generic parameter. 

###Usage

To use the FSCL compiler to write F# kernels and to compile them into OpenCL code, programmers must:

1. Link the appropriate libraries: *FSCL.Compiler.dll* and *FSCL.Compiler.Language.dll*;
2. Open the appropriate namespaces: *FSCL.Compiler* and *FSCL.Compiler.KernelLanguage*;
3. Write the F# kernel and mark it with the *ReflectedDefinition attribute*;
4. Instantiate the *Compiler* type and call the *Compile method* passing the quotation of the function/method reference (name).

The following code sample represents a template for F# kernel definition and compilation.

    // Compiler user interface
    open FSCL.Compiler
    // Kernel language library
    open FSCL.Compiler.KernelLanguage
    // Kernel properly marked with ReflectedDefinition attribute
    [<ReflectedDefinition>]
    let VectorAdd(a: float32[], b: float32[], c: float32[]) =
        let gid = get_global_id(0)
        c.[gid] <- a.[gid] + b.[gid]
    [<EntryPoint>]
    let main argv =
        // Instantiate the compiler
        let compiler = new Compiler()
        // Compile the kernel
        let compilationResult = compiler.Compile(<@@ VectorAdd @@>)
        

