module FSCL.Compiler.DataTypeTests

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers

type MyStruct =
    struct
        val mutable x: int
        val mutable y: int
        new(a: int, b: int) = { x = a; y = b }
    end
    
type MyRecord = {
    x: int;
    y: int
}

// Simple vector addition char
[<ReflectedDefinition>]
let VectorAddChar(a: char[], b:char[], c:char[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    
// Simple vector addition uchar
[<ReflectedDefinition>]
let VectorAddUchar(a: uchar[], b:uchar[], c:uchar[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    
// Simple vector addition with struct
[<ReflectedDefinition>]
let VectorAddStruct(a: MyStruct[], b:MyStruct[], c:MyStruct[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    // A little verbose just to test correct codegen of different constructs
    let mutable newStruct = new MyStruct()
    newStruct.x <- a.[gid].x + b.[gid].x
    newStruct.y <- a.[gid].y + b.[gid].y
    c.[gid] <- newStruct
    
// Simple vector addition with struct constructor
[<ReflectedDefinition>]
let VectorAddStructWithConstructor(a: MyStruct[], b:MyStruct[], c:MyStruct[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    // A little verbose just to test correct codegen of different constructs
    let mutable newStruct = new MyStruct(a.[gid].x + b.[gid].x, a.[gid].y + b.[gid].y)
    c.[gid] <- newStruct
    
// Simple vector addition with record
[<ReflectedDefinition>]
let VectorAddRecord(a: MyRecord[], b:MyRecord[], c:MyRecord[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    // A little verbose just to test correct codegen of different constructs
    let newRecord = { x = a.[gid].x + b.[gid].x;  y = a.[gid].y + b.[gid].y }
    c.[gid] <- newRecord
    
[<Test>]
let ``Can compile char vector add`` () =
    let compiler = new Compiler()
    let a = Array.create 64 ((char)1)
    let b = Array.create 64 ((char)2)
    let c = Array.zeroCreate<char> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddChar(a, b, c, size) @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, result.Kernel.OriginalParameters.Count)
    
[<Test>]
let ``Can compile uchar vector add`` () =
    let compiler = new Compiler()
    let a = Array.create 64 ((byte)0)
    let b = Array.create 64 ((byte)2)
    let c = Array.zeroCreate<byte> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddUchar(a, b, c, size) @>) :?> IKernelModule
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, result.Kernel.OriginalParameters.Count)
    
[<Test>]
let ``Can compile custom struct vector add`` () =
    let compiler = new Compiler()
    let a = Array.create 64 (new MyStruct(1, 2))
    let b = Array.create 64 (new MyStruct(2, 3))
    let c = Array.zeroCreate<MyStruct> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddStruct(a, b, c, size) @>) :?> IKernelModule
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, result.Kernel.OriginalParameters.Count)
    // A struct type should be added to the global types
    Assert.AreNotEqual(None, result.GlobalTypes |> List.tryFind(fun t -> t = typeof<MyStruct>))
        
[<Test>]
let ``Can compile custom struct with custom constructor vector add`` () =
    let compiler = new Compiler()
    let a = Array.create 64 (new MyStruct(1, 2))
    let b = Array.create 64 (new MyStruct(2, 3))
    let c = Array.zeroCreate<MyStruct> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddStructWithConstructor(a, b, c, size) @>) :?> IKernelModule
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, result.Kernel.OriginalParameters.Count)
    // A struct type should be added to the global types
    Assert.AreNotEqual(None, result.GlobalTypes |> List.tryFind(fun t -> t = typeof<MyStruct>))
    
[<Test>]
let ``Can compile custom record vector add`` () =
    let compiler = new Compiler()
    let a = Array.create 64 ({ x = 1; y = 1 })
    let b = Array.create 64 ({ x = 2; y = 2 })
    let c = Array.zeroCreate<MyRecord> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddRecord(a, b, c, size) @>) :?> IKernelModule
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, result.Kernel.OriginalParameters.Count)
    // A struct type should be added to the global types
    Assert.AreNotEqual(None, result.GlobalTypes |> List.tryFind(fun t -> t = typeof<MyRecord>))
    