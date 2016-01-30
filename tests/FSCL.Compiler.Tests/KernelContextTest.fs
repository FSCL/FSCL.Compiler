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
        compiler.Compile<IKernelExpression>(<@ VectorAddTupledInModule(size, a, b, c) @>) 
        
    let CompileVectorAddCurried(compiler: Compiler, size, a, b, c) =        
        compiler.Compile<IKernelExpression>(<@ VectorAddCurriedInModule size a b c @>) 
    
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
        compiler.Compile<IKernelExpression>(<@ VectorAddTupled(size, a, b, c) @>) 
        
    member this.CompileVectorAddCurried(compiler: Compiler, size, a, b, c) =        
        compiler.Compile<IKernelExpression>(<@ VectorAddCurried size a b c @>) 
                
    member this.CompileVectorAddTupledMember(compiler: Compiler, size, a, b, c) =
        compiler.Compile<IKernelExpression>(<@ this.VectorAddTupledMember(size, a, b, c) @>) 
        
    member this.CompileVectorAddCurriedMember(compiler: Compiler, size, a, b, c) =
        compiler.Compile<IKernelExpression>(<@ this.VectorAddCurriedMember size a b c @>) 
        
    member this.CompileVectorAddTupledMemberStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile<IKernelExpression>(<@ KernelWrapper.VectorAddTupledMemberStatic(size, a, b, c) @>) 
        
    member this.CompileVectorAddCurriedMemberStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile<IKernelExpression>(<@ KernelWrapper.VectorAddCurriedMemberStatic size a b c @>) 
                
    member this.CompileVectorAddTupledMemberBase(compiler: Compiler, size, a, b, c) =
        compiler.Compile<IKernelExpression>(<@ this.VectorAddTupledMemberBase(size, a, b, c) @>) 

    member this.CompileVectorAddCurriedMemberBase(compiler: Compiler, size, a, b, c) =
        compiler.Compile<IKernelExpression>(<@ this.VectorAddCurriedMemberBase size a b c @>) 
        
[<Test>]
let ``Can compile tupled module field kernel from inside and outside the module`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let insideResult = KernelModule.CompileVectorAddTupled(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ KernelModule.VectorAddTupledInModule(size, a, b, c) @>) 
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile curried module field kernel from inside and outside the module`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let insideResult = KernelModule.CompileVectorAddCurried(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ KernelModule.VectorAddCurriedInModule size a b c @>) 
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  

[<Test>]
let ``Can compile tupled instance field kernel from inside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupled(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
[<Test>]
let ``Can compile curried instance field kernel from inside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurried(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)        
    
[<Test>]
let ``Can compile tupled instance member kernel from inside and outside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupledMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ wrapper.VectorAddTupledMember(size, a, b, c) @>) 
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile curried instance member kernel from inside and outside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurriedMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ wrapper.VectorAddCurriedMember size a b c @>) 
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile tupled static member kernel from inside and outside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupledMemberStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ KernelWrapper.VectorAddTupledMemberStatic(size, a, b, c) @>) 
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile curried static member kernel from inside and outside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurriedMemberStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ KernelWrapper.VectorAddCurriedMemberStatic size a b c @>) 
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile tupled inherited instance member kernel from inside and outside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddTupledMemberBase(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ wrapper.VectorAddTupledMemberBase(size, a, b, c) @>) 
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile curried inherited instance member kernel from inside and outside the instance`` () =
    let compiler, a, b, c, size = TestUtil.GetVectorSampleData()
    let wrapper = new KernelWrapper()
    let insideResult = wrapper.CompileVectorAddCurriedMemberBase(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile<IKernelExpression>(<@ wrapper.VectorAddCurriedMemberBase size a b c @>) 
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)