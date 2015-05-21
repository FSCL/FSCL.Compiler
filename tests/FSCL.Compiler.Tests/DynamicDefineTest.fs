module FSCL.Compiler.DynamicDefineTest

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Runtime.InteropServices

module KernelModule =     
    [<ConstantDefine>] 
    let DataFieldModule =
        10.0f
    
    [<ReflectedDefinition; Kernel>] 
    let VectorAddModule (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * DataFieldModule
        
    let Compile(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ VectorAddModule(size, a, b, c) @>) :?> IKernelExpression
            
type KernelWrapper(data: float32) =        
    [<ConstantDefine>] 
    let DataField =
        10.0f
             
    [<ConstantDefine>] 
    let mutable DataFieldMutable =
        10.0f
        
    [<ConstantDefine>] 
    static let mutable DataFieldMutableStatic =
        10.0f
            
    [<ConstantDefine>] 
    let DataFieldFromConstructor =
        data
                        
    [<ConstantDefine>] 
    member this.DataPropertyWithGet 
        with get() =
            10.0f    
               
    [<ConstantDefine>] 
    member this.DataPropertyWithGetFromConstructor
        with get() =
            data

    [<ConstantDefine>] 
    member this.DataPropertyWithGetSet
        with get() =
            DataFieldMutable
        and set v =
            DataFieldMutable <- v 
            
    [<ConstantDefine>] 
    static member StaticDataField = 10.0f   
    
    static member StaticDataPropertyWithGetSet
        with get() =
            DataFieldMutableStatic
        and set v =
            DataFieldMutableStatic <- v               
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingField (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * DataField
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingFieldFromConstructor (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * DataFieldFromConstructor
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingMutableField (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * DataFieldMutable
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingPropertyWithGet (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * this.DataPropertyWithGet
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingPropertyWithGetSet (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * this.DataPropertyWithGetSet
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingPropertyWithGetFromConstructor (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * this.DataPropertyWithGetFromConstructor
        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingStaticField (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * KernelWrapper.StaticDataField
                        
    [<ReflectedDefinition; Kernel>] 
    member this.VectorAddUsingModuleField (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * KernelModule.DataFieldModule
                        
    member this.CompileVectorAddUsingField(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingField(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddUsingMutableField(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingMutableField(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddUsingFieldFromConstructor(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingFieldFromConstructor(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddUsingPropertyWithGet(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingPropertyWithGet(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddUsingPropertyWithGetSet(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingPropertyWithGetSet(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddUsingPropertyWithGetFromConstructor(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingPropertyWithGetFromConstructor(size, a, b, c) @>) :?> IKernelExpression
        
    member this.CompileVectorAddUsingStaticField(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingStaticField(size, a, b, c) @>) :?> IKernelExpression
                        
    member this.CompileVectorAddUsingModuleField(compiler: Compiler, size, a, b, c) =        
        compiler.Compile(<@ this.VectorAddUsingModuleField(size, a, b, c) @>) :?> IKernelExpression
        
let GetData() =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.create 64 0.0f
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    let wrapper = new KernelWrapper(10.0f)
    compiler, a, b, c, size, wrapper    

let FirstConstDefineValue(m: IKernelExpression, inst:KernelWrapper option) =
    let thisVar, _, f = (m.KFGRoot :?> KFGKernelNode).Module.ConstantDefines.Values |> List.ofSeq |> List.head
    if thisVar.IsSome then
        f.GetType().GetMethod("Invoke").Invoke(f, [| inst.Value |]) :?> float32
    else
        f.GetType().GetMethod("Invoke").Invoke(f, [|()|]) :?> float32
        
[<Test>]
let ``Can compile module kernel using module field from inside and outside module`` () =
    let compiler, a, b, c, size, wrapper = GetData()

    let insideResult = KernelModule.Compile(compiler, size, a, b, c)
    Assert.NotNull(insideResult)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, None), 10.0f)
    let outsideResult = compiler.Compile(<@ KernelModule.VectorAddModule(size, a, b, c) @>) :?> IKernelExpression
    Assert.NotNull(outsideResult)
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, None), 10.0f)

    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile kernel using instance field from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()

    let insideResult = wrapper.CompileVectorAddUsingField(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingField(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)

    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile kernel using mutable instance field from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()

    let insideResult = wrapper.CompileVectorAddUsingMutableField(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)
    wrapper.DataPropertyWithGetSet <- 5.0f    
    let insideResult2 = wrapper.CompileVectorAddUsingMutableField(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult2, Some(wrapper)), 5.0f)
    
    wrapper.DataPropertyWithGetSet <- 10.0f    
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingMutableField(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)
    wrapper.DataPropertyWithGetSet <- 5.0f    
    let outsideResult2 = compiler.Compile(<@ wrapper.VectorAddUsingMutableField(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult2, Some(wrapper)), 5.0f)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    let log, success = TestUtil.TryCompileOpenCL((insideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile kernel using field set from constructor from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let wrapper2 = new KernelWrapper(2.0f)    

    let insideResult = wrapper.CompileVectorAddUsingFieldFromConstructor(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)
    let insideResult2 = wrapper2.CompileVectorAddUsingFieldFromConstructor(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult2, Some(wrapper2)), 2.0f)
        
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingFieldFromConstructor(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)
    let outsideResult2 = compiler.Compile(<@ wrapper2.VectorAddUsingFieldFromConstructor(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult2, Some(wrapper2)), 2.0f)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    let log, success = TestUtil.TryCompileOpenCL((insideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile kernel using getter property from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()

    let insideResult = wrapper.CompileVectorAddUsingPropertyWithGet(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)        
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingPropertyWithGet(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
            
[<Test>]
let ``Can compile kernel using getter property from constructor from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    let wrapper2 = new KernelWrapper(2.0f)    

    let insideResult = wrapper.CompileVectorAddUsingPropertyWithGetFromConstructor(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)
    let insideResult2 = wrapper2.CompileVectorAddUsingPropertyWithGetFromConstructor(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult2, Some(wrapper2)), 2.0f)
        
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingPropertyWithGetFromConstructor(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)
    let outsideResult2 = compiler.Compile(<@ wrapper2.VectorAddUsingPropertyWithGetFromConstructor(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult2, Some(wrapper2)), 2.0f)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    let log, success = TestUtil.TryCompileOpenCL((insideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile kernel using getter-setter property from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()

    let insideResult = wrapper.CompileVectorAddUsingPropertyWithGetSet(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)
    wrapper.DataPropertyWithGetSet <- 3.0f
    let insideResult2 = wrapper.CompileVectorAddUsingPropertyWithGetSet(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult2, Some(wrapper)), 3.0f)
        
    wrapper.DataPropertyWithGetSet <- 10.0f
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingPropertyWithGetSet(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)
    wrapper.DataPropertyWithGetSet <- 3.0f
    let outsideResult2 = compiler.Compile(<@ wrapper.VectorAddUsingPropertyWithGetSet(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult2.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult2, Some(wrapper)), 3.0f)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    let log, success = TestUtil.TryCompileOpenCL((insideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult2.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    
[<Test>]
let ``Can compile kernel using static field from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()

    let insideResult = wrapper.CompileVectorAddUsingStaticField(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)        
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingStaticField(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
                
[<Test>]
let ``Can compile instance kernel using module field from inside and outside instance`` () =
    let compiler, a, b, c, size, wrapper = GetData()
    
    let insideResult = wrapper.CompileVectorAddUsingModuleField(compiler, size, a, b, c)
    Assert.IsNotEmpty((insideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(insideResult, Some(wrapper)), 10.0f)
    let outsideResult = compiler.Compile(<@ wrapper.VectorAddUsingModuleField(size, a, b, c) @>) :?> IKernelExpression
    Assert.IsNotEmpty((outsideResult.KFGRoot :?> KFGKernelNode).Module.ConstantDefines)
    Assert.AreEqual(FirstConstDefineValue(outsideResult, Some(wrapper)), 10.0f)
    
    let log, success = TestUtil.TryCompileOpenCL((insideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)        
    let log, success = TestUtil.TryCompileOpenCL((outsideResult.KFGRoot :?> KFGKernelNode).Module.Code.Value)
    if not success then
        Assert.Fail(log)
    