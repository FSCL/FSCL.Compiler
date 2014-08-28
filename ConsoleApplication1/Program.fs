// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Language

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
let sumElementsNested(a: float32[], b:float32[], gid: int) =    
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

    0 // return an integer exit code
