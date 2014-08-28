module FSCL.Compiler.KernelCompilationTests

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers

// Simple vector addition
[<ReflectedDefinition>]
let VectorAdd(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    
// Simple vector addition (curried form)
[<ReflectedDefinition>]
let VectorAddCurried(a: float32[]) (b:float32[]) (c:float32[]) (wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    
// Vector addition with return type
[<ReflectedDefinition>]
let VectorAddWithReturn(a: float32[], b:float32[], wi:WorkItemInfo) =
    let c = Array.zeroCreate<float32> (a.Length)
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    c
        
// Vector addition with return parameter
[<ReflectedDefinition>]
let VectorAddWithReturnParameter(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    c
    
// Vector addition with ref cell
[<ReflectedDefinition>]
let VectorAddWithRefCell(a: float32[], b:float32[], c:float32[], r: float32 ref, wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid] + !r
    if wi.LocalID(0) = 0 then
        r := 0.0f

// Vector addition with utility function    
[<ReflectedDefinition>]
let sumElements(a: float32, b:float32) =    
    a + b

[<ReflectedDefinition>]
let sumElementsArrays(a: float32[], b:float32[], wi:WorkItemInfo) =    
    let gid = wi.GlobalID(0)
    a.[gid] + b.[gid]

[<ReflectedDefinition>]
let setElement(c: float32[], value: float32, index: int) =    
    c.[index] <- value
    
[<ReflectedDefinition>]
let sumElementsNested(a: float32[], b:float32[], gid: int) =    
    sumElements(a.[gid], b.[gid])

[<ReflectedDefinition>]
let VectorAddWithUtility(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let s = sumElementsArrays(a, b, wi)
    setElement(c, s, wi.GlobalID(0))

[<ReflectedDefinition>]
let VectorAddWithNestedUtility(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =   
    let gid = wi.GlobalID(0)
    c.[gid] <- sumElementsNested(a, b, gid)

[<Test>]
let ``Can compile tupled kernel reference`` () =
    let compiler = new Compiler()
    let result = compiler.Compile(<@ VectorAdd @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(result.Kernel.WorkSize, None)
    // Work item info parameter should be lifted
    Assert.AreEqual(result.Kernel.OriginalParameters.Count, 3)
    
[<Test>]
let ``Can compile curried kernel reference`` () =
    let compiler = new Compiler()
    let result = compiler.Compile(<@ VectorAddCurried @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(None, result.Kernel.WorkSize)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, result.Kernel.OriginalParameters.Count)
    
[<Test>]
let ``Can compile tupled kernel call`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAdd(a, b, c, size) @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, result.Kernel.OriginalParameters.Count)
    
[<Test>]
let ``Can compile curried kernel call`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddCurried a b c size @>) :?> IKernelModule
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    
[<Test>]
let ``Can compile kernel with utility functions`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithUtility(a, b, c, size) @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // We should have two functions
    Assert.AreEqual(2, result.Functions.Count)
    // Work item info parameter should be lifted from first function
    for f in result.Functions do
        if (f.Value.ParsedSignature.Name = "sumElementsArrays") then
            Assert.AreEqual(4, f.Value.Parameters.Count)
        else
            Assert.AreEqual(4, f.Value.Parameters.Count)
    // Call to first function should not have the last argument
    let firstCut = result.Code.Value.Substring(result.Code.Value.IndexOf("sumElements(") + "sumElements(".Length)
    let secondCut = firstCut.Substring(0, firstCut.IndexOf(")"))
    let split = secondCut.Split(',')
    Assert.AreEqual(4, split.Length)
    
[<Test>]
let ``Can compile kernel with nested utility functions`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithNestedUtility(a, b, c, size) @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // We should have two functions
    Assert.AreEqual(2, result.Functions.Count)
    // Work item info parameter should be lifted from first function
    for f in result.Functions do
        if (f.Value.ParsedSignature.Name = "sumElementsNested") then
            Assert.AreEqual(5, f.Value.Parameters.Count)
        else
            Assert.AreEqual(2, f.Value.Parameters.Count)
    
[<Test>]
let ``Can compile kernel returning a parameter`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithReturnParameter(a, b, c, size) @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    
[<Test>]
let ``Can compile kernel with reference cells`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let r = ref 2.0f
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithRefCell(a, b, c, r, size) @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation(result.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // 4th paramter should be an array 
    Assert.AreEqual(typeof<float32[]>, result.Kernel.OriginalParameters.[3].DataType)
    // There should be two accesses to element 0 of the ref cell array
    let firstIndex = result.Code.Value.IndexOf("r[0]")
    let lastIndex = result.Code.Value.LastIndexOf("r[0]")
    Assert.Greater(firstIndex, 0)
    Assert.Greater(lastIndex, 0)
    Assert.AreNotEqual(firstIndex, lastIndex)
   