// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Language
open System.Runtime.InteropServices
open System.Runtime.CompilerServices
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
let inline sumElementsNested(a: float32[], b:float32[], gid: int) =    
    sumElements(a.[gid], b.[gid])

[<ReflectedDefinition>]
let VectorAddWithUtility(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
    let s = sumElementsArrays(a, b, wi)
    setElement(c, s, wi.GlobalID(0))

[<ReflectedDefinition>]
let VectorAddWithNestedUtility(a: float32[], b:float32[], c:float32[], wi:WorkItemInfo) =   
    let gid = wi.GlobalID(0)
    c.[gid] <- sumElementsNested(a, b, gid)

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithNestedUtility(a, b, c, size) @>) :?> IKernelModule
    //printf "%s\n" (result.Code.Value.ToString())
    // Work item info should be stored
    
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ VectorAddWithUtility(a, b, c, size) @>) :?> IKernelModule

    let a = Array.create 64 1.0f
    let b = Array.create 64 1.0f
    let result = compiler.Compile(<@ Array.reduce (fun e1 e2 -> e1 + e2) a @>) :?> IKernelModule
    
    let a = Array.create 64 ({ x = 1; y = 1 })
    let c = Array.zeroCreate<MyRecord> 64
    let size = new WorkSize(64L, 64L)
    let result = compiler.Compile(<@ Array.reduce (fun e1 e2 -> 
                                                        let ret = { x = e1.x + e2.x; y = e1.y + e2.y }
                                                        ret) a @>) :?> IKernelModule
    0 // return an integer exit code
