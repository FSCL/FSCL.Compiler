### 2.0 - 24 February 2015
* New FSCL language. Collection functions can be used of any order. Collection functions of lowest order are translated into OpenCL kernels, the ones of higher orders encodes multi-thread coordination of (sub)kernels. Kernel-FlowGraph creation inside the compiler (previously was built from within the FSCL.Runtime).

### 1.5.5 - 29 October 2014
* Bug fix in parsing compositions

### 1.5.4 - 29 October 2014
* Support for F# two-components tuple

### 1.5.3 - 29 October 2014
* Big fix for on-the-fly anonymous structs and options

### 1.5.2 - 29 October 2014
* Support for F# options

### 1.5.1 - 28 October 2014
* Bug fix

### 1.4.9 - 27 October 2014
* Added tests to validate various possibilities of declaring an utility function (static/instance method, lambda, field)

### 1.4.8 - 26 October 2014
* Added tests to validate various possibilities of declaring a kernel (static/instance method, lambda, field)

### 1.4.7 - 25 October 2014
* Enabled class fields to represent code-time or opencl-compile-time macros

### 1.4.6 - 23 October 2014
* Bug ix in parsing static and instance methods

### 1.4.5 - 23 October 2014
* Handling parsing of closures derived from slicing and using class members

### 1.4.4 - 22 October 2014
* Bug fix

### 1.4.3 - 22 October 2014
* For Array.reduce and Array.sum the output array in kernel is forced not to be transferred back to the host. This fixes a bug that causes the final value computed on the CPU
to be overwritten by the content of the (first item of the) output array when transferred back to the host

### 1.4.2 - 21 October 2014
* Bug fix

### 1.4.1 - 21 October 2014
* Referencing let-variables or properties declared outside kernels turn into -define- macros only if ReflectedDefinitionAttribute is associated to the variable/property 
* No need to use DynamicDefine attribute anymore. Immutable variables/propeties are replaced with the corresponding value (expression) when OpenCL source code is generated. OpenCL-target-code-generation-time defines are generated (by the runtime) for mutable variables/properties

### 1.4.0 - 20 October 2014
* Automatic characterization of lambda functions (if a lambda has a WorkItemInfo param it's turn into a kernel, otherwise it's applied by reflection on the CPU)
* Optimisation of parsing step to speed up the runtime. Approximatively 80 microsecs on macbook pro i5.
* Bug fix

### 1.3.9 - 16 October 2014
* Added support for pipelining operators (|>, ||> and |||>)
* Added support for partial functions (curried form)
* Added support for lambda application

### 1.3.8 - 11 September 2014
* Fixed flattening of multi-dimensional arrays

### 1.3.7 - 7 September 2014
* Added inline attribute for utility functions. Lambdas are inlined by default

### 1.3.6 - 28 August 2014
* Fixed reduce code generation to handle records and structs

### 1.3.5 - 28 August 2014
* Fixed conflict between new struct creation construct (NewObject()) and vectorised data-types

### 1.3.4 - 28 August 2014
* Enabled utility functions chain calls of arbitrary length (utility functions can call other utility functions)
* Enabled passing arrays to utility functions
* Inserted kernel/function declaration before definition
* Added some tests

### 1.3.3 - 23 August 2014
* Fixed bug in struct type codegen

### 1.3.2 - 23 August 2014
* Fixed bug and extended support for structs and records. Now you can use both custom F# records and structs (and arrays of records and structs) as parameters of kernels and functions. Also, you can declare private/local structs and records using record initialisation construct, struct parameterless constructor and "special" struct constructor (a constructor taking N arguments, each of one matching one of the N fields, in the order).
- Valid record decl: let myRec = { field1 = val1; ... fieldN = valN }
- Valid default struct decl: let myStruct = new MyStruct()
- Valid "special constructor" struct decl: let myStruct = new MyStruct(valForField1, valForField2, ... valForFieldN)
- NOT valid struct decl: let myStruct = new MyStruct(<Args where the i-TH is not a value assigned to the i-TH field>)

### 1.3.1 - 20 August 2014
* Fixed bug in char and uchar types handling in codegen

### 1.3 - 25 July 2014
* Restructured project according to F# Project Scaffold
* Iterative reduction execution on CPU
* Work size specification as part of kernel signature
