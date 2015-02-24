module FSCL.Compiler.KernelCompilationTests

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers

// Simple vector addition
[<ReflectedDefinition; Kernel>]
let VectorAdd(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    
// Simple vector addition (curried form)
[<ReflectedDefinition; Kernel>]
let VectorAddCurried(a: float32[]) (b:float32[]) (c:float32[]) (wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    
// Vector addition with return type
[<ReflectedDefinition; Kernel>]
let VectorAddWithReturn(a: float32[], b:float32[], wi:WorkItemInfo) =
    let c = Array.zeroCreate<float32> (a.Length)
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    c
        
// Vector addition with return parameter
[<ReflectedDefinition; Kernel>]
let VectorAddWithReturnParameter(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    c
    
// Vector addition with ref cell
[<ReflectedDefinition; Kernel>]
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
    
[<ReflectedDefinition>][<Inline>]
let inline sumElementsNested(a: float32[], b:float32[], gid: int) =    
    sumElements(a.[gid], b.[gid])

[<ReflectedDefinition; Kernel>]
let VectorAddWithUtility(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let s = sumElementsArrays(a, b, wi)
    setElement(c, s, wi.GlobalID(0))

[<ReflectedDefinition; Kernel>]
let VectorAddWithNestedUtility(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =   
    let gid = wi.GlobalID(0)
    c.[gid] <- sumElementsNested(a, b, gid)

//[<Test>]
let ``Can compile tupled kernel reference`` () =
    let compiler = new Compiler()
    let result = compiler.Compile(<@ VectorAdd @>) :?> IComputingExpressionModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual((result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize, None)
    // Work item info parameter should be lifted
    Assert.AreEqual((result.KFGRoot :?> KFGKernelNode).Module.Kernel.OriginalParameters.Count, 3)
    
//[<Test>]
let ``Can compile curried kernel reference`` () =
    let compiler = new Compiler()
    let result = compiler.Compile(<@ VectorAddCurried @>) :?> IComputingExpressionModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(None, (result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, (result.KFGRoot :?> KFGKernelNode).Module.Kernel.OriginalParameters.Count)
    
[<Test>]
let ``Can compile tupled kernel call`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAdd(a, b, c, size) @>) :?> IComputingExpressionModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation((result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // Work item info parameter should be lifted
    Assert.AreEqual(3, (result.KFGRoot :?> KFGKernelNode).Module.Kernel.OriginalParameters.Count)
    
[<Test>]
let ``Can compile curried kernel call`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddCurried a b c size @>) :?> IComputingExpressionModule
    let wInfo = LeafExpressionConverter.EvaluateQuotation((result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    
[<Test>]
let ``Can compile kernel with utility functions`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithUtility(a, b, c, size) @>) :?> IComputingExpressionModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation((result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // We should have two functions
    Assert.AreEqual(2, (result.KFGRoot :?> KFGKernelNode).Module.Functions.Count)
    // Work item info parameter should be lifted from first function
    for f in (result.KFGRoot :?> KFGKernelNode).Module.Functions do
        if (f.Value.ParsedSignature.Name = "sumElementsArrays") then
            Assert.AreEqual(4, f.Value.Parameters.Count)
        else
            Assert.AreEqual(4, f.Value.Parameters.Count)
    // Call to first function should not have the last argument
    let firstCut = (result.KFGRoot :?> KFGKernelNode).Module.Code.Value.Substring((result.KFGRoot :?> KFGKernelNode).Module.Code.Value.IndexOf("sumElements(") + "sumElements(".Length)
    let secondCut = firstCut.Substring(0, firstCut.IndexOf(")"))
    let split = secondCut.Split(',')
    Assert.AreEqual(4, split.Length)
    
[<Test>]
let ``Can compile kernel with inline and nested utility functions`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithNestedUtility(a, b, c, size) @>) :?> IComputingExpressionModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation((result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // We should have two functions
    Assert.AreEqual(2, (result.KFGRoot :?> KFGKernelNode).Module.Functions.Count)
    // Work item info parameter should be lifted from first function
    for f in (result.KFGRoot :?> KFGKernelNode).Module.Functions do
        if (f.Value.ParsedSignature.Name = "sumElementsNested") then
            Assert.AreEqual(5, f.Value.Parameters.Count)
            Assert.IsTrue(f.Value.Code.StartsWith("inline "))
        else
            Assert.AreEqual(2, f.Value.Parameters.Count)
            
[<Test>]
let ``Can compile kernel returning a parameter`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithReturnParameter(a, b, c, size) @>) :?> IComputingExpressionModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation((result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize.Value)
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
    let result = compiler.Compile(<@ VectorAddWithRefCell(a, b, c, r, size) @>) :?> IComputingExpressionModule
    //printf "%s\n" (result.Code.Value.ToString())
    let wInfo = LeafExpressionConverter.EvaluateQuotation((result.KFGRoot :?> KFGKernelNode).Module.Kernel.WorkSize.Value)
    // Work item info should be stored
    Assert.AreEqual(size, wInfo)
    // 4th paramter should be an array 
    Assert.AreEqual(typeof<float32[]>, (result.KFGRoot :?> KFGKernelNode).Module.Kernel.OriginalParameters.[3].DataType)
    // There should be two accesses to element 0 of the ref cell array
    let firstIndex = (result.KFGRoot :?> KFGKernelNode).Module.Code.Value.IndexOf("r[0]")
    let lastIndex = (result.KFGRoot :?> KFGKernelNode).Module.Code.Value.LastIndexOf("r[0]")
    Assert.Greater(firstIndex, 0)
    Assert.Greater(lastIndex, 0)
    Assert.AreNotEqual(firstIndex, lastIndex)
   