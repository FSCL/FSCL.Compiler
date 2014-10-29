module FSCL.Compiler.AcceleratedCollectionContextTest

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Runtime.InteropServices

module KernelModule =            
    [<ReflectedDefinition>] 
    let UtilFunction a b =
        a + b
        
    let Compile(compiler: Compiler, size, a, b) =
        compiler.Compile(<@ Array.map2 UtilFunction @>) :?> IKernelModule
            
type KernelWrapper() =        
    [<ReflectedDefinition>] 
    let UtilFunction a b =
        a + b
                        
    [<ReflectedDefinition>] 
    member this.UtilFunctionMember a b =
        a + b
               
    [<ReflectedDefinition>] 
    static member UtilFunctionStatic a b =
        a + b
        
    member this.CompileAccelCollField(compiler: Compiler, size, a, b) =        
        compiler.Compile(<@ Array.map2 UtilFunction @>) :?> IKernelModule
                
    member this.CompileAccelCollMember(compiler: Compiler, size, a, b) =        
        compiler.Compile(<@ Array.map2 this.UtilFunctionMember @>) :?> IKernelModule
        
    member this.CompileAccelCollStatic(compiler: Compiler, size, a, b) =  
        compiler.Compile(<@ Array.map2 KernelWrapper.UtilFunctionStatic @>) :?> IKernelModule
        
    member this.CompileAccelCollModule(compiler: Compiler, size, a, b) =  
        compiler.Compile(<@ Array.map2 KernelModule.UtilFunction @>) :?> IKernelModule
        
let GetData() =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    compiler, a, b, size, wrapper    

[<Test>]
let ``Can compile module collection from inside and outside module`` () =
    let compiler, a, b, size, wrapper = GetData()

    let insideResult = KernelModule.Compile(compiler, size, a, b)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, insideResult.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ Array.map2 KernelModule.UtilFunction @>) :?> IKernelModule
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, outsideResult.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile instance field collection from inside instance`` () =
    let compiler, a, b, size, wrapper = GetData()

    let insideResult = wrapper.CompileAccelCollField(compiler, size, a, b)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, insideResult.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile instance member collection from inside and outside instance`` () =
    let compiler, a, b, size, wrapper = GetData()

    let insideResult = wrapper.CompileAccelCollMember(compiler, size, a, b)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, insideResult.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ Array.map2 wrapper.UtilFunctionMember @>) :?> IKernelModule
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, outsideResult.Kernel.InstanceExpr)
        
[<Test>]
let ``Can compile static member collection from inside and outside instance`` () =
    let compiler, a, b, size, wrapper = GetData()

    let insideResult = wrapper.CompileAccelCollStatic(compiler, size, a, b)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, insideResult.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ Array.map2 KernelWrapper.UtilFunctionStatic @>) :?> IKernelModule
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, outsideResult.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile module collection called from instance member from inside instance`` () =
    let compiler, a, b, size, wrapper = GetData()

    let insideResult = wrapper.CompileAccelCollModule(compiler, size, a, b)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, insideResult.Kernel.InstanceExpr)
    