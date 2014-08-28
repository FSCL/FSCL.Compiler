module FSCL.Compiler.AcceleratedCollectionTests

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Runtime.InteropServices

[<StructLayout(LayoutKind.Sequential)>]
type MyRecord = {
    x: float32;
    y: float32
}

[<ReflectedDefinition>]
let FloatSum(a: float32) (b:float32) =
    a + b
    
[<ReflectedDefinition>]
let RecordSum(a: MyRecord) (b: MyRecord) =
    let s = { x = a.x + b.x; y = a.y + b.y }
    s
    
[<Test>]
let ``Can compile array.map2 collection function`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let result = compiler.Compile(<@ Array.map2 FloatSum a b @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(result.Kernel.WorkSize, None)
    Assert.AreEqual(1, result.Functions.Count)
    
[<Test>]
let ``Can compile array.map2 collection function with lambda`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let result = compiler.Compile(<@ Array.map2 (fun e1 e2 -> e1 + e2) a b @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(None, result.Kernel.WorkSize)
    Assert.AreEqual(1, result.Functions.Count)
    
[<Test>]
let ``Can compile array.reduce lambda`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let result = compiler.Compile(<@ Array.reduce (fun e1 e2 -> e1 + e2) a @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(None, result.Kernel.WorkSize)
    Assert.AreEqual(1, result.Functions.Count)
    
[<Test>]
let ``Can compile array.reduce with record data-type`` () =
    let compiler = new Compiler()
    let a = Array.create 64 { x = 1.0f; y = 2.0f }
    let result = compiler.Compile(<@ Array.reduce RecordSum a @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(None, result.Kernel.WorkSize)
    Assert.AreEqual(1, result.Functions.Count)
    
[<Test>]
let ``Can compile array.reduce lambda with record data-type`` () =
    let compiler = new Compiler()
    let a = Array.create 64 { x = 1.0f; y = 2.0f }
    let result = compiler.Compile(<@ Array.reduce (fun el1 el2 -> 
                                                        let v = { x = el1.x + el2.x; y = el1.y + el2.y }
                                                        v) a @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // No work item info should be stored
    Assert.AreEqual(None, result.Kernel.WorkSize)
    Assert.AreEqual(1, result.Functions.Count)
    