(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
FSCL Compiler
===================

FSCL Compiler is a source-to-source compiler that translates quoted F# function calls and other contructs into valid C99 OpenCL kernel sources, enabling
programming OpenCL-enabled parallel devices from within F#.

How to get FSCL Compiler
-------

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The FSCL Compiler library can be <a href="https://nuget.org/packages/FSCL.Compiler">installed from NuGet</a>:
      <pre>PM> Install-Package FSCL.Compiler</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>

Getting started with FSCL.Compiler
-------

FSCL Compiler is able to produce valid OpenCL source code from quoted expressions containing:

+ The name (ref) or call to an FSCL kernel
+ The name (ref) or call to a Array collection function (e.g. Array.reverse, Array.map2)
+ The name (ref) or call to a "regular" function or static/instance method

An FSCL kernel is an F# function or static/instance method marked with [<ReflectedDefinition>] attribute (to enable the compiler to inspect the AST of its body) and
resembling an OpenCL C kernel. Aside from the differences in syntax and in part of the object-model/programming constructs, every OpenCL kernel a programmer can
express in C99 can be coded in F# as well.
For example, an OpenCL C kernel to execute parallel vector addition would look like:

`__kernel void vectorAdd(__global float * a, __global const float * b, __global float * c)
{
    int myId = get_global_id(0);
    c[myId] = a[myId] + b[myId];
}`

In FSCL, the same kernel can be coded as follows:
*)
#r "FSCL.Compiler.Core.dll"
#r "FSCL.Compiler.dll"
#r "FSCL.Compiler.Language.dll"
open FSCL
open FSCL.Compiler
open FSCL.Language

[<ReflectedDefinition>]
let VectorAdd(a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    c.[myId] <- a.[myId] + b.[myId]
(**
The major difference between an OpenCL kernel written in C and the equivalent in FSCL is the additional parameter of type WorkItemInfo, which contains all the functions related to the work items domain (including barrier).
Whereas in OpenCL C programmers you use global functions like *get_global_id()* and *get_local_size()*, in FSCL you invoke matching functions exposed by the additional parameter (*wi.GlobalId()*, *wi.LocalSize()*).

To compile an FSCL kernel to OpenCL, you need to instantiate the FSCL Compiler and to pass the quoted kernel call or reference to the *Compile* method.
*)
// Instantiate the compiler
let compiler = new Compiler()
// Pass a kernel
let resultCompilingRef = compiler.Compile(<@ VectorAdd @>)
// Or a kernel call
let a = Array.create 1024 2.0f
let b = Array.create 1024 3.0f
let c = Array.zeroCreate<float32> 1024
let size = worksize([| a.Length |], [| 128 |], [| 0 |])
let resultCompilingCall = compiler.Compile(<@ VectorAdd(a, b, c, size) @>)

(**

Tutorials and Documentation
-----------------------

The FSCL Compiler API documentation is under development and will be available soon.
In the meantime, take a look to the following tutorials.

 * [Kernel Programming Tutorial](kernelProgrammingTutorial.html) introduces the object-model and the programming constructs available to write OpenCL kernels in F#.
 
 * [Compiler Interface Tutorial](compilerInterfaceTutorial.html) shows how to use the FSCL compiler to compile FSCL kernels and provides and overview on the set of information produced during compilation.

 * [Dynamic Metadata Tutorial](dynamicMetadataTutorial.html) shows how to use the FSCL Dynamic Metadata infrastructure and how to create new, custom metadata.
 
 * [Compiler Configuration Tutorial](compilerConfigurationTutorial.html) illustrates how to configure the FSCL compiler.

 * [Compiler Customisation and Extension Tutorial](compilerCustomisationTutorial.html) shows how to customise and extend the FSCL Compiler pipeline.

 * [API Reference](reference/index.html) contains automatically generated documentation for all types, modules
   and functions in the library. This includes additional brief samples on using most of the
   functions.
 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read [library design notes][readme] to understand how it works.

The library is available under Apache 2.0 license. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/GabrieleCocco/FSCL.Compiler/tree/master/docs/content
  [gh]: https://github.com/GabrieleCocco/FSCL.Compiler
  [issues]: https://github.com/GabrieleCocco/FSCL.Compiler/issues
  [readme]: https://github.com/GabrieleCocco/FSCL.Compiler/blob/master/README.md
  [license]: https://github.com/GabrieleCocco/FSCL.Compiler/blob/master/LICENSE.txt
*)
