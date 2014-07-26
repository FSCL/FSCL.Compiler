(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
Dynamic Metadata
========================

Dynamic Metadata are a powerful tool based on custom attributes to drive compilation and execution behaviour in FSCL by associating meta-information to kernels and kernel parameters statically or at kernel execution time.
From the language point of view, a dynamic metadata is a custom attribute that comes along with a function to 
enable dynamic association of the information the custom attribute represents.
In this page we provide an overview on how to use dynamic metadata in FSCL programming and on the suggested approach to create your custom metadata.

###Built-in Dynamic Metadata

The FSCL Compiler (and the Runtime) defines a set of built-in metadata that programmers can use to associate information to kernels and parameters.
If you took a look to [Kernel Programming Tutorial](kernelProgrammingTutorial.html), you already encountered a dynamic metadata (i.e. `AddressSpace`) to specify the memory space
where the OpenCL buffer corrisponding to a parameter should be allocated.

Another interesting built-in metadata is `DeviceType`, that allows to optimise the OpenCL code generated for a specific architecture.
This is particularly true when compiling collection functions (such as `Array.reduce`), for which the FSCL compiler is in fact able to generate different, optimised OpenCL code
for CPU devices and for GPU devices.

In the following example, two identical vector addition kernels are declared, the first to be optimised for Gpu execution, the second for Cpu.
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

[<ReflectedDefinition; DeviceType(DeviceType.Gpu)>]
let VectorAddGpu(a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    c.[myId] <- a.[myId] + b.[myId]

[<ReflectedDefinition; DeviceType(DeviceType.Cpu)>]
let VectorAddCpu(a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    c.[myId] <- a.[myId] + b.[myId]

let compiler = new Compiler()
let compilationResultForGpu = compiler.Compile(<@ VectorAddGpu @>)
let compilationResultForCpu = compiler.Compile(<@ VectorAddCpu @>)

(**
In the example above we needed to define vector addition twice just to associate different instances of the `DeviceType` custom attribute to each of them.
That's why every dynamic metadata requires to define a function together with a custom attribute. In this particular case, the FSCL Compiler object model exposes
both a `DeviceType` custom attribute and a `DEVICE_TYPE` function (uppercase is suggested to distinguish dynamic metadata function from other functions used inside quotations)
that can be used at kernel compilation time.
Using this function we can define vector addition only once and produce optimised code for different devices by wrapping the kernel reference or call inside it when creating the quoted expression.
*)
[<ReflectedDefinition>]
let VectorAdd(a: float32[], b:float32[], c:float32[], wi: WorkItemInfo) =
    let myId = wi.GlobalID(0)
    c.[myId] <- a.[myId] + b.[myId]

let compiler = new Compiler()
let compilationResultForGpu = compiler.Compile(<@ DEVICE_TYPE(
                                                        DeviceType.Gpu, 
                                                        VectorAdd 
                                               ) @>)
let compilationResultForGpu = compiler.Compile(<@ DEVICE_TYPE(
                                                        DeviceType.Cpu, 
                                                        VectorAdd
                                               ) @>)

(**
###Defining Custom Metadata

Whenever you develop a custom compiler or runtime step (see [Compiler Customisation Tutorial](compilerCustomisationTutorial.html)) that may act differently depending on some user hints, I suggest to leverage on a custom metadata to encode that hint or suggestion that the user can give you.
Defining custom metadata is really easy, there are only two things you need to do:

+ Define your new metadata subclassing `KernelMetadataAttribute` or `ParameterMetadataAttribute`: 
the first is the base class of all the metadata that can be associated to kernels, while the second is the base class 
of all the metadata associated to kernel parameters or kernel return values.

+ Define a function that enables to specify the metadata value dynamically, that is whenever you execute a kernel. 
Mark this function with the attribute `KernelMetadataFunction(type)`, `ParameterMetadataFunction(type)` or `ReturnMetadataFunction(type)`, 
depending on the target you have planned for your custom metadata. 

Let's show an example. Assume we developed a compiler step processor that "flatten" vector types (such as `float4`, `int3`, `uint8`) 
into the corresponding scalar types.
We want to enable the programmer to decide whether to flatten a kernel parameter or not. 
Since FSCL kernels can return arrays we should also provide a way to tell the compiler step to flatten returned arrays (if they contain values of some vector type).

###Define dynamic metadata attribute

The first step is to define a custom attribute to statically mark kernel paramters and return type.
The only important thing to remember is to always give your custom meta a default constructor. 
In fact, whenever we define a custom parameter metadata (but the same holds for kernel metadata), every possible kernel parameter 
will have an instance of that metadata associated. 
If the programmer does not associate any instance of our metadata to a kernel parameter, the parameter will still hold a value for that metadata, which is obtained by invoking its default constructor.
In other words, if a programmer does not specify a particular parameter meta, it is exactly like he was instantiating that meta using the default constructor.
*)
type DevectorizeAttribute(enable: bool) =
    inherit ParameterMetadataAttribute()
    member val Enable = enable with get
    new() =
        DevectorizeAttribute(false)

(**
###Define metadata function

If we want to enable programmers to dynamically provide a value for your meta when a particular instance of the kernel is executed (i.e. inside quotations), 
we need to define a function that "emulates" the .NET custom attribute in a dynamic context.
*)
[<ParameterMetadataFunction(typeof<DevectorizeAttribute>)>]
let DEVECTORIZE(enable: bool, a) = a

[<ReturnMetadataFunction(typeof<DevectorizeAttribute>)>]
let DEVECTORIZE_RETURN(enable: bool, a) = a 
(**
Since the metadata can be associated to both parameters and return values, two distinct functions are requires. 
This is (unfortunately) required to enable FSCL to distinguish whether a metadata is associated to a parameter or to the return value when the metadata function wraps a subkernel, 
that is a kernel passed as parameter to another kernel.

###Employing metadata in kernel programming

Once define a custom metadata attribute and a matching metadata function, 
programmers can mark parameters and return types in kernel definitions as well as actual arguments an return values in kernel calls.
*)
// Static metadata are associated to kernel definition
[<Devectorize(true)>] 
let MyKernel(a: float4[], 
             [<Devectorize(true)>] 
             b: float4[], 
             c: float4[]) =
    let d = Array.zeroCreate<float4> a.Length
    // ...
    d
    
let a = Array.create 1024 (float4(0.0f))
let b = Array.create 1024 (float4(0.0f))
let c = Array.zeroCreate<float4> 1024

// Dynamic metadata functions override static metadata in kernel execution
// Override return
let overrideReturn = <@ DEVECTORIZE_RETURN(false, MyKernel(a, b, c)) @>

// Override devectorize metadata value for b
let overrideB = <@ MyKernel(a, DEVECTORIZE(false, b), c) @>

(**
Remember that if programmers do not specify any devectorize metadata value for a parameter or return value, 
it is exactly like they specified the default value.
*)
// This one...
let MyKernel(a: float4[], 
             [<Devectorize(true)>] 
             b: float4[], 
             c: float4[]) =
    let d = Array.zeroCreate<float4> a.Length
    // ...
    d

// ...is equivalent to (since default constructor assigns "false" to DevectorizeAttribute "Enable" property) 
[<Devectorize(false)>]
let MyKernel([<Devectorize(false)>] a: float4[],
             [<Devectorize(true)>] b: float4[], 
             [<Devectorize(false)>] c: float4[]) =
    let d = Array.zeroCreate<float4> a.Length
    // ...
    d

