module FSCL.Compiler.FunctionContextTest

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Runtime.InteropServices

module KernelModule =    
    [<ReflectedDefinition>] 
    let UtilFunctionTupled(a, b) =
        a + b
        
    [<ReflectedDefinition>] 
    let UtilFunctionCurried a b =
        a + b

    [<ReflectedDefinition; Kernel>] 
    let VectorAddWithModuleTupledFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- UtilFunctionTupled(a.[gid], b.[gid]) 
        
    [<ReflectedDefinition; Kernel>] 
    let VectorAddWithModuleCurriedFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- UtilFunctionCurried a.[gid] b.[gid]
        
    let CompileVectorAddTupled(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ VectorAddWithModuleTupledFunction(size, a, b, c) @>) :?> IKernelExpression
        
    let CompileVectorAddCurried(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ VectorAddWithModuleCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
    
type KernelWrapper() =
    [<ReflectedDefinition>] 
    let UtilFunctionTupled(a, b) =
        a + b
        
    [<ReflectedDefinition>] 
    let UtilFunctionCurried a b =
        a + b
        
    [<ReflectedDefinition; Kernel>] 
    let VectorAddFieldTupledFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- UtilFunctionTupled(a.[gid], b.[gid]) 
        
    [<ReflectedDefinition; Kernel>] 
    let VectorAddFieldCurriedFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- UtilFunctionCurried a.[gid] b.[gid]

    [<ReflectedDefinition>] 
    member this.UtilFunctionTupledMember(a, b) =
        a + b
        
    [<ReflectedDefinition>] 
    member this.UtilFunctionCurriedMember a b =
        a + b
        
    [<ReflectedDefinition>] 
    static member UtilFunctionTupledStatic(a, b) =
        a + b
        
    [<ReflectedDefinition>] 
    static member UtilFunctionCurriedStatic a b =
        a + b
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddMemberTupledFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- this.UtilFunctionTupledMember(a.[gid], b.[gid])
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddMemberCurriedFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- this.UtilFunctionCurriedMember a.[gid] b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddFieldTupledFunctionFromMember (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- UtilFunctionTupled(a.[gid], b.[gid])
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddFieldCurriedFunctionFromMember (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- UtilFunctionCurried a.[gid] b.[gid]
                
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddStaticTupledFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- KernelWrapper.UtilFunctionTupledStatic(a.[gid], b.[gid])
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddStaticCurriedFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- KernelWrapper.UtilFunctionCurriedStatic a.[gid] b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddModuleTupledFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- KernelModule.UtilFunctionTupled(a.[gid], b.[gid])
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddModuleCurriedFunction (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- KernelModule.UtilFunctionCurried a.[gid] b.[gid]
        
    [<ReflectedDefinition; Kernel>] 
    static member VectorAddStaticTupledFunctionFromStatic (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- KernelWrapper.UtilFunctionTupledStatic(a.[gid], b.[gid])
        
    [<ReflectedDefinition; Kernel>] 
    static member VectorAddStaticCurriedFunctionFromStatic (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- KernelWrapper.UtilFunctionCurriedStatic a.[gid] b.[gid]
        
    member this.CompileVectorAddTupledField(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ VectorAddFieldTupledFunction(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedField(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ VectorAddFieldCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddTupledFieldFromMember(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddFieldTupledFunctionFromMember(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedFieldFromMember(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddFieldCurriedFunctionFromMember(size, a, b, c) @>) :?> IKernelExpression
                
    member this.CompileVectorAddTupledMember(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddMemberTupledFunction(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedMember(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddMemberCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddTupledMemberStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddStaticTupledFunction(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedMemberStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddStaticCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
                
    member this.CompileVectorAddTupledMemberStaticFromStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ KernelWrapper.VectorAddStaticTupledFunctionFromStatic(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedMemberStaticFromStatic(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ KernelWrapper.VectorAddStaticCurriedFunctionFromStatic(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddTupledModule(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddModuleTupledFunction(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddCurriedModule(compiler: Compiler, size, a, b, c) =
        compiler.Compile(<@ this.VectorAddModuleCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
    
let GetData() =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper()
    compiler, a, b, c, size, wrapper    

[<Test>]
let ``Can compile kernel with tupled module function from inside and outside module`` () =
    let compiler, a, b, c, size, wrapper = GetData()

    let insideResult = KernelModule.CompileVectorAddTupled(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelModule.VectorAddWithModuleTupledFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with curried module function from inside and outside module`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = KernelModule.CompileVectorAddCurried(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelModule.VectorAddWithModuleCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with tupled module function called from instance kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddTupledModule(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddModuleCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with curried module function called from instance kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddCurriedModule(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddModuleCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  

[<Test>]
let ``Can compile kernel with tupled instance field function from inside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddTupledField(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with curried instance field function from inside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddCurriedField(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with tupled instance field function called from member kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddTupledFieldFromMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddFieldTupledFunctionFromMember(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with curried instance field function called from member kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddCurriedFieldFromMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddFieldCurriedFunctionFromMember(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with tupled instance member function from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddTupledMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddMemberTupledFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with curried instance member from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddCurriedMember(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddMemberCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with tupled static member function called from instance kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddTupledMemberStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddStaticTupledFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with curried static member function called from instance kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddCurriedMemberStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreNotSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddStaticCurriedFunction(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreNotSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with tupled static member function called from static kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddTupledMemberStaticFromStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelWrapper.VectorAddStaticTupledFunctionFromStatic(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    
[<Test>]
let ``Can compile kernel with curried static member function called from static kernel from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let insideResult = wrapper.CompileVectorAddCurriedMemberStaticFromStatic(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.AreSame(None, (insideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)
    let outsideResult = compiler.Compile(<@ KernelWrapper.VectorAddStaticCurriedFunctionFromStatic(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.AreSame(None, (outsideResult.KFGRoot :?> KFGKernelNode).Module.InstanceExpr)

    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module)
    if not success then
        Assert.Fail(log)  
    