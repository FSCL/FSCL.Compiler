// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Language
open System.Runtime.InteropServices
open System.Runtime.CompilerServices
open System.Diagnostics
open FSCL
open System

[<ReflectedDefinition>]
let myConstProp = 2.0f

[<ReflectedDefinition>]
let myConstComplexProp = float4(0.0f, 1.0f, 2.0f, 3.0f)

module Prova =
    [<ReflectedDefinition>]
    let mutable myDynamicComplexProp = float4(0.0f, 1.0f, 2.0f, 3.0f)

type MyStruct =
    struct
        val mutable x: int
        val mutable y: int
        new(a: int, b: int) = { x = a; y = b }
    end
    

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
let VectorAddTupled (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    c

[<ReflectedDefinition>] 
let VectorAddCurried (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =    
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    c

[<ReflectedDefinition>] 
let VectorMulCurried (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =    
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] * b.[gid]
    c
    
[<ReflectedDefinition>] 
let VectorNop (a: float32[]) =   
    let gid = 0
    a.[gid] <- a.[gid] * a.[gid]
    
[<ReflectedDefinition>] 
let VectorMulCurried2 (a: float32[]) (b:float32[]) (c:float32[]) =
    let gid = 0
    c.[gid] <- a.[gid] * b.[gid]

[<ReflectedDefinition>] 
let VectorAddUsingConstProp (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + (b.[gid] * myConstProp)
    
[<ReflectedDefinition>] 
let VectorAddUsingConstComplexProp (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + (b.[gid] * myConstComplexProp.x)
    
[<ReflectedDefinition>] 
let VectorAddUsingDynamicComplexProp (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + (b.[gid] * Prova.myDynamicComplexProp.x)
    
[<ReflectedDefinition>] 
let VectorReturnScalar (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) =   
    if a.[0] > 0.f then
        2.0f * a.[0]
    else
        2.0f + a.[0]
        
[<ReflectedDefinition>] 
let VectorReturnScalarParam (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) (l: int) =   
    let gid = wi.GlobalID(0)
    a.[gid] <- a.[gid] * a.[gid]
    l
    
[<ReflectedDefinition>] 
let VectorAddOption (wi:WorkItemInfo, a: float32 option[], b:float32 option[], c:float32 option[]) =   
    let gid = wi.GlobalID(0)
    let addit = Some(3.0f)
    let mutable n = None
    if a.[gid].IsSome then
        c.[gid] <- Some(a.[gid].Value + b.[gid].Value)
        n <- a.[gid]
        
[<ReflectedDefinition>] 
let VectorAddTuple (wi:WorkItemInfo, a: (float32 * int)[], b:(float32 * int)[], c:(float32 * int)[]) =   
    let gid = wi.GlobalID(0)
    let plus = (2.0f, 3)
    c.[gid] <- (fst(a.[gid]) + fst(b.[gid]), snd(a.[gid]) + snd(b.[gid]))
        
type KernelWrapper0() =
    let immutablePropField = 20.0f
    
    [<ReflectedDefinition>]
    member this.SumFun a b =
        a + b + immutablePropField

    [<ReflectedDefinition>]
    member this.ImmutableProp 
        with get() =
            immutablePropField
    
module KernelModule =     
    [<ReflectedDefinition>] 
    let DataFieldModule =
        10.0f
    
    [<ReflectedDefinition>] 
    let VectorAddModule (wi:WorkItemInfo, a: float32[], b:float32[], c:float32[]) =    
        let gid = wi.GlobalID(0)
        c.[gid] <- a.[gid] + b.[gid] * DataFieldModule
        
    let Compile(compiler: Compiler, size, a, b, c) =        
        let r = compiler.Compile(VectorAddModule(size, a, b, c)) :?> IKernelExpression
        r


[<StructLayout(LayoutKind.Sequential)>]
type MyRecord = {
    mutable x: float32;
    mutable y: float32    
}

[<ReflectedDefinition>]
let sumCurried (a: MyRecord) (b: MyRecord)  =
    let v = { x = a.x + b.x; y = a.y + b.y }
    v
    
// Simple vector addition int4
[<ReflectedDefinition; Kernel>]
let VectorAddInt4(a: int4[], b:int4[], c:int4[], wi:WorkItemInfo) =
    let gid = wi.GlobalID(0)
    c.[gid] <- a.[gid] + b.[gid]
    
[<ReflectedDefinition; Kernel>]
let VectorAddWithUtility(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let s = sumElementsArrays(a, b, wi)
    setElement(c, s, wi.GlobalID(0))
    
[<ReflectedDefinition>]
let FloatSum(a: float32) (b:float32) =
    a + b

[<ReflectedDefinition>] 
let sum a = a *2.0f
[<EntryPoint>]
let main argv =     
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    //let result = compiler.Compile(<@ Array.map2 FloatSum a b @>) :?> IKernelExpression

    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    

    //KMeans.Run()
    Test.Test1()
    //Test.Test2()
    //Test.Test3()
    //Test.Test8()
    //Test.Test9()
    //Test.Test10()
    
    let result1 = compiler.Compile(<@ VectorMulCurried size (VectorAddCurried size a b c) b c @>) :?> IKernelModule
    let result2 = compiler.Compile(<@ VectorAddCurried size a b c |> VectorMulCurried size a b  @>) :?> IKernelModule    
    let result3 = compiler.Compile(<@ VectorAddCurried size a b c |> VectorNop  @>) :?> IKernelModule
    let result4 = compiler.Compile(<@ (VectorAddCurried size a b c, a) ||> VectorMulCurried size a  @>) :?> IKernelModule
    let result5 = compiler.Compile(<@ (b, c) ||> VectorMulCurried size a  @>) :?> IKernelModule
    let result6 = compiler.Compile(<@ (a, b, c) |||> VectorMulCurried size  @>) :?> IKernelModule
    let result7 = compiler.Compile(<@ (a, b, c) |||> VectorMulCurried size  @>) :?> IKernelModule
    
    let result71 = compiler.Compile(<@ (fun (wi:WorkItemInfo) (a: float32[]) (b:float32[]) (c:float32[]) ->
                                            let gid = wi.GlobalID(0)
                                            c.[gid] <- a.[gid] * b.[gid]) size a b c @>) :?> IKernelModule
    // Auto lambda (result8 should be null cause no WorkItemInfo param)
    let result8 = compiler.Compile(<@ fun (a: float32[]) (b:float32[]) (c:float32[]) ->
                                            let gid = 0
                                            c.[gid] <- a.[gid] * b.[gid]
                                   @>) :?> IKernelModule
    let result81 = compiler.Compile(<@ (a, b, c) |||> VectorMulCurried2  @>) :?> IKernelModule
                                   
    let result10 = compiler.Compile(<@ (size, VectorAddCurried size a b c) ||> 
                                       fun (wi:WorkItemInfo) (a: float32[])  ->
                                            let gid = wi.GlobalID(0)
                                            a.[gid] <- a.[gid] * a.[gid]  @>) :?> IKernelModule
                                            
    let result11 = compiler.Compile(<@ (b, c) ||> 
                                       (fun (wi:WorkItemInfo) (a:float32[]) (b: float32[]) (c: float32[])  ->
                                            let gid = wi.GlobalID(0)
                                            b.[gid] <- a.[gid] * a.[gid]) size a @>) :?> IKernelModule
                                            
    // Composition with accel collections
    
    
    //let result13 = compiler.Compile(<@ VectorAddCurried size a b c |> Array.map sum  @>) :?> IKernelModule
    let s = 64
    let a = Array.create s (Some(2.0f))
    let b = Array.create s (Some(3.0f))
    let mutable c = Array.zeroCreate<float32 option> s
    
    //let res = compiler.Compile(<@ VectorAddOption(size, a, b, c) @>) :?> IKernelModule
    
    let at = Array.create s (2.0f, 3)
    let bt = Array.create s (3.0f, 2)
    let mutable ct = Array.zeroCreate<float32 * int> s
    
    let rest = compiler.Compile(<@ VectorAddTuple(size, at, bt, ct) @>) :?> IKernelModule

    // Parsing performance
    let mutable r = null
    let watch = new Stopwatch()
    watch.Start()
    for i = 1 to 50000 do
        ()//r <- compiler.Compile(quot, (CompilerOptions.ParseOnly, box()))
    watch.Stop()
    System.Console.WriteLine("Parsing time " + (((double)watch.ElapsedMilliseconds) / 50000.0).ToString() + " ms")     


    0 // return an integer exit code
