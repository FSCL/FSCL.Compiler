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
