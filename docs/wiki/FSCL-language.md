### Data types supported

Currently, the FSCL language supports computing on the following types

- _Primitive types_: int, int64, uint, uint64, float32, float (double), byte

- _Struct types_: structs where each field is of a primitive type

- _Vector types_: \<T\>\<Size\> where T is a primitive type and Size can be 2, 3 or 4 (e.g _float4_)

- _Array types_: Array<'T>, Array2D<'T>, Array3D<'T> where each element of the array is of primitive, struct or vector type

### Custom kernels definition

Custom kernels can be expressed as F# module functions, instance/static methods or lambdas.
In case of module functions and methods, the kernel definition must be marked with **ReflectedDefinition** and **Kernel**.

```F#
[<ReflectedDefinition;Kernel>]
let vecAdd(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
	// Do something
	
type KernelContainer() =
	[<ReflectedDefinition;Kernel>]
	member this.vecAdd(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
		// Do something
	
	[<ReflectedDefinition;Kernel>]
	static member vecAdd(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
		// Do something
```
			
A lambda doesn't require particular solutions to mark it as a kernel. A lambda recognized as a kernel if it contains a parameter of type **WorkItemInfo**.

```F#
// This is a kernel
let vecAddKernel = <@ 
					fun(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
						// Do something
					@>
	
// This is not a kernel (no WorkItemInfo parameter)
let vecAddNormal = <@ 
					fun(a:float32[], b:float32[], c:float32[]) =
						// Do something
					@>
```
				
Kernels can be expressed in both tupled and curried format.
	
```F#	
[<ReflectedDefinition;Kernel>]
let vecAddTupled(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
	// Do something
	
[<ReflectedDefinition;Kernel>]
let vecAddCurried(a:float32[]) (b:float32[]) (c:float32[]) (wi:WorkItemInfo) =
	// Do something
```
		
### Collection functions

Currently, we support most of the **Array/Array2D/Array3D** collection functions. We are planning to move to F# 4.0 to exploit a more extensive and homogeneous set of functions across the collection types.

### Kernel utility functions

Collection and custom kernels can call utility functions, which must be marked with **ReflectedDefinition** (but not with Kernel).

```F#
[<ReflectedDefinition;Kernel>]
let sumElem(a:float32, b:float32)
	a + b
	
[<ReflectedDefinition;Kernel>]
let vecAddWithUtil(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
	let gid = wi.GlobalID(0)
	c.[gid] <- sumElem(a.[gid], b.[gid])
```
		
Utility functions can call other utility functions as well.
The only restriction that applies is that an instance method kernel can only call utility functions relative to the same instance (i.e. _this_).

```F#	
type KernelContainer() =
	[<ReflectedDefinition;Kernel>]
	member this.vecAdd(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
	let gid = wi.GlobalID(0)
	// OK
	c.[gid] <- this.sumElem(a.[gid], b.[gid])
	
type KernelContainer() =
	[<ReflectedDefinition;Kernel>]
	member this.vecAdd(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
	let gid = wi.GlobalID(0)
	// ERROR
	c.[gid] <- otherInstance.sumElem(a.[gid], b.[gid])
```
		
### Function composition

Kernels and sequential functions can be composed each other using the "classic" syntax _g(f(x))_ or through the forward pipeline operators __|>__, __||>__, __|||>__.

```F#
let comp1 = <@ 
				Array.reduce
					(fun a b -> a + b) 
					(myKernel(a, b, ws))
	  	    @>
			
let comp2 = <@ 
				myKernel(a, b, ws) |>
				Array.reduce(fun a b -> a + b) 
	  	    @>				
```

### Access module and instance fields

Kernels and utility functions can read-only access module, instance and static fields and properties.

```F#
let multiplier = 5
	
[<ReflectedDefinition;Kernel>]
let vecAddMult(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
	let gid = wi.GlobalID(0)
	c.[gid] <- a.[gid] + multiplier * b.[gid]
```
		
Instance method kernels can only access fields and properties of _this_.

A field/property access is compiled into an additional kernel parameter. The matching argument is computed at kernel-execution time by evaluating the property-get or retrieving the value associated to the field.

In case of constant values, OpenCL programmers often encode such values via _#define_. To obtain this behaviour in FSCL, we must mark the field/property with **ConstantDefine**.

```F#
[<ContantDefine>]
let multiplier = 5
	
[<ReflectedDefinition;Kernel>]
let vecAddMult(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
	let gid = wi.GlobalID(0)
	c.[gid] <- a.[gid] + multiplier * b.[gid]
```
		
In this case the value associated to multiplier is computed at kernel-compilation time and the name _multiplier_ is introduced in the kernel code using [_clBuildProgram_	preprocessor directives](https://www.khronos.org/registry/cl/sdk/1.2/docs/man/xhtml/clBuildProgram.html).

### Access to local variables declared outside a function

In many cases, F# programmers code functions that refer to local variables defined outside the functions themeselves, such as _myNum_ in the following example, referenced by the functional argument of Array.map.

```F#
let myNum = 5
let myArr = [||]

let comp = <@ Array.map(fun it -> it * myNum) myArr @>
```
In some cases, variables are introduced _inside_ the quotation but still outside the function that refers them, such as _myArr_ in the following example.

```F#
let arrOfArrs = [| [|| |]

let comp = <@ 
			arrOfArrs |>
			Array.map(fun myArr ->
						Array.reduce (+) myArr)
```

Both cases are supported by FSCL, which recognizes references to variables which are not local variables introduced in the function body (including variables holding arguments) and generates appropriate additional parameters for the kernel/utility functions.

### OpenCL native functions

OpenCL exposes a wide set of predefined functions, such as _atan_, _mad_ and _vload_.
OpenCL functions that have a match in .NET _System.Math_ or in F# _FSharp.Core.Operators_ namespaces can be expressed using such matching functions.

```F#
[<ReflectedDefinition;Kernel>]
let vecExp(a:float32[], c:float32[], wi:WorkItemInfo) =
	let gid = wi.GlobalID(0)
	// FSharp.Core.Operators
	c.[gid] <- exp(a.[gid])
	// Equivalently, System.Math
	c.[gid] <- System.Math.Exp(a.[gid])
```

Functions with no correspondence in .NET/F# (e.g. _mul24_) are contained by the _FSCL.Language_ dll.
