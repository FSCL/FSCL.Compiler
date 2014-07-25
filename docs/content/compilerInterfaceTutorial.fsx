(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
Interfacing with the FSCL compiler
========================

Once you have coded your FSCL kernels, the next step is to compile them to OpenCL. Valid OpenCL C99 source code is not
the only output produced by the FSCL Compiler. Instead, the compiler produces a lot of useful information about the kernel structure,
the data-types used and the way parameters are accessed from withing the kernel body.
In this page we provide an overview on how to compile FSCL kernels and on the information produced by the compilation itself.

Default compilation
----------------------

Most of the time, to compile a kernel you simply create an instance of the FSCL `Compiler` and you pass the quoted kernel
call or kernel reference to its `Compile` method. From the compilation point of view, there is no much difference between passing the kernel 
and passing a call to the kernel. The compilation process is exactly the same as like as the OpenCL source produced.
The only difference is that in case of a call the result contains the expressions of the actual arguments. Those expressions
are currently required only if you *Run* the kernel, which is a task performed by the [FSCL Runtime](http://nuget.org/FSCL.Runtime).
Nonetheless, future developments may introduce compiler steps whose behavior is driven by the actual arguments of a kernel call (for example, merging two kernels into one
when the argument of a kernel call is the result of another kernel call). In such a case, the compiler itself would produce different results
depending on whether you pass a kernel reference of a kernel call.
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

// Instantiate the compiler
let compiler = new Compiler()
// Kernel reference
let compilationResultFromRef = compiler.Compile(<@ VectorAdd @>)
// Kernel call
let a = Array.create 1024 2.0f
let b = Array.create 1024 3.0f
let c = Array.zeroCreate<float32> 1024
let size = worksize([| a.Length |], [| 128 |], [| 0 |])
let compilationResultFromCall = compiler.Compile(<@ VectorAdd(a, b, c, size) @>)

(**
The compiler data-structure: IKernelModule
------------------------------------

The `Compile` signature declares that the method is returning an instance of `Object`. The runtime type of the returned value
depends on the configuration of the compiler pipeline (see [Compiler Customisation Tutorial](compilerCustomisationTutorial.html)).
The default pipeline is composed of built-in (on *native* ) steps and generates an instance of `IKernelModule`.
Among the other information, this instance contains the OpenCL source code produced.
*)
let compilationResult = compiler.Compile(<@ VectorAdd(a, b, c, size) @>) :?> IKernelModule
// Call arguments (if the quotation contains a kernel call)
let callArgs = compilationResult.CallArgs
// OpenCL source code
let code = compilationResult.Code
// References to global properties translated into #define(s)
let defines = compilationResult.ConstantDefines
// OpenCL-specific compiler directives (such as the one to enable double-precision)
let directives = compilationResult.Directives
// Structs or records used by the kernel or by one or more utility functions
let globalTypes = compilationResult.GlobalTypes
// Utility functions
let utilityFunctions = compilationResult.Functions
// Kernel
let kernel = compilationResult.Kernel

(**
The kernel data structure: IKernelInfo
------------------------------------

One of the most interesting data in the kernel module is provided by the `Kernel` property (of type `IKernelInfo). Among the other information,
the other information, this property holds the result of access analysis relative to each kernel parameter. 
Access analysis is performed on the kernel body for each vector (array) parameter to detect the way the parameter is accessed (ReadOnly, WriteOnly, ReadWrite, NoAccess).
This analysis is particularly useful for the FSCL Runtime to optimise buffer allocation prior to kernel execution.
*)
let firstKernelPar = compilationResult.Kernel.Parameters.[0]
let accessAnalysis = firstKernelPar.AccessAnalysis

(**
Compiler options
------------------------------------

The `Compile` method is overloaded to enable the programmer to specify some compilation options using an insteance  
of `Dictionary<string, obj>`.
The FSCL compiler library is currently declaring two built-in options, heavily used by the FSCL Runtime to speed-up kernel compilation: `"ParsOnly"` and `"NoCodegen"`. The first stops the native
pipeline after the parsing step, the second right before the latest two steps (function and module codegen). 

Compiler options are thought to be completely extensible. Programmer can define additional options to drive the behavior of custom compiler
steps. 
*)
open System.Collections.Generic

let myCustomOptions = new Dictionary<string, obj>()
myCustomOptions.Add(CompilerOptions.ParseOnly, ())
// The IKernelModule data structure is only partially filled
// It contains only data produced by the parsing step
let compilationResultOnlyParsing = compiler.Compile(<@ VectorAdd(a, b, c, size) @>) :?> IKernelModule
