module FSCL.Compiler.KernelContextTest

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Runtime.InteropServices

module KernelModule =
    [<ReflectedDefinition; Kernel>] 
    let VectorAddTupledInModule (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] 

    [<ReflectedDefinition; Kernel>] 
    let VectorAddCurriedInModule (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]
        
    let CompileVectorAddTupled(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ VectorAddTupledInModule(size, a, b, c) @>) :?> IKernelExpression
        
    let CompileVectorAddCurried(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ VectorAddCurriedInModule size a b c @>) :?> IKernelExpression
    
[<AbstractClass>]
type Base() =
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddTupledMemberBase (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddCurriedMemberBase (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]

type KernelWrapper() =
    inherit Base()

    [<ReflectedDefinition; Kernel>] 
    let VectorAddTupled (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] 

    [<ReflectedDefinition; Kernel>] 
    let VectorAddCurried (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddTupledMember (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddCurriedMember (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    static member VectorAddTupledMemberStatic (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    static member VectorAddCurriedMemberStatic (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid]

    member this.CompileVectorAddTupled(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ VectorAddTupled(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurried(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ VectorAddCurried size a b c @>) :?> IKernelExpression
                
    member this.CompileVectorAddTupledMember(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddTupledMember(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedMember(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddCurriedMember size a b c @>) :?> IKernelExpression
        
    member this.CompileVectorAddTupledMemberStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ KernelWrapper.VectorAddTupledMemberStatic(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedMemberStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ KernelWrapper.VectorAddCurriedMemberStatic size a b c @>) :?> IKernelExpression
                
    member this.CompileVectorAddTupledMemberBase(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddTupledMemberBase(size, a, b, c) @>) :?> IKernelExpression

    member this.CompileVectorAddCurriedMemberBase(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddCurriedMemberBase size a b c @>) :?> IKernelExpression
        
[<Test>]
let ``Can compile tupled module field kernel from inside and outside the module`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let insideResult = KernelModule.CompileVectorAddTupled(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelModule.VectorAddTupledInModule(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile curried module field kernel from inside and outside the module`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let insideResult = KernelModule.CompileVectorAddCurried(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelModule.VectorAddCurriedInModule size a b c @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)

[<Test>]
let ``Can compile tupled instance field kernel from inside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupled(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile curried instance field kernel from inside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurried(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile tupled instance member kernel from inside and outside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupledMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddTupledMember(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile curried instance member kernel from inside and outside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurriedMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddCurriedMember size a b c @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile tupled static member kernel from inside and outside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupledMemberStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelWrapper.VectorAddTupledMemberStatic(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile curried static member kernel from inside and outside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurriedMemberStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelWrapper.VectorAddCurriedMemberStatic size a b c @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile tupled inherited instance member kernel from inside and outside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupledMemberBase(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddTupledMemberBase(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    
[<Test>]
let ``Can compile curried inherited instance member kernel from inside and outside the instance`` () =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurriedMemberBase(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddCurriedMemberBase size a b c @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.Kernel.InstanceExpr)