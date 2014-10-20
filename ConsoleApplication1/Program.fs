// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Language
open System.Runtime.InteropServices
open System.Runtime.CompilerServices
open System.Diagnostics

type MyStruct =
    struct
        val mutable x: int
        val mutable y: int
        new(a: int, b: int) = { x = a; y = b }
    end
    
[<StructLayout(LayoutKind.Sequential)>]
type MyRecord = {
    x: int;
    y: int
}

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


[<EntryPoint>]
let main argv =     
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    (*
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
                                   
    let result10 = compiler.Compile(<@ (size, VectorAddCurried size a b c) ||> 
                                       fun (wi:WorkItemInfo) (a: float32[])  ->
                                            let gid = wi.GlobalID(0)
                                            a.[gid] <- a.[gid] * a.[gid]  @>) :?> IKernelModule
                                            
    let result11 = compiler.Compile(<@ (b, c) ||> 
                                       (fun (wi:WorkItemInfo) (a:float32[]) (b: float32[]) (c: float32[])  ->
                                            let gid = wi.GlobalID(0)
                                            b.[gid] <- a.[gid] * a.[gid]) size a @>) :?> IKernelModule
                                            
    // Composition with accel collections
    let sum a b = a + b
    let result12 = compiler.Compile(<@ VectorAddCurried size a b c |> Array.map (fun a -> a * 2.0f)  @>) :?> IKernelModule
    let result13 = compiler.Compile(<@ VectorAddCurried size a b c |> Array.map sum  @>) :?> IKernelModule
    *)

    let quot = <@ VectorAddTupled(size, a,b,c)  @>

    // Parsing performance
    let mutable r = null
    let watch = new Stopwatch()
    watch.Start()
    for i = 1 to 50000 do
        r <- compiler.Compile(quot, (CompilerOptions.ParseOnly, box()))
    watch.Stop()
    System.Console.WriteLine("Parsing time " + (((double)watch.ElapsedMilliseconds) / 50000.0).ToString() + " ms")     


    0 // return an integer exit code
