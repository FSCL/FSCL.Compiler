// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Language
open System.Runtime.InteropServices
open System.Runtime.CompilerServices
open System.Diagnostics
open FSCL
open System    
//open OpenCL
open System.Collections.Generic
// Vector addition with utility function    
//
//let TryCompileOpenCL(code:String) =
//    if OpenCL.OpenCLPlatform.Platforms.Count > 0 then
//        let platform = OpenCL.OpenCLPlatform.Platforms.[0]
//        let device = platform.Devices.[0]
//        let contextProperties = new OpenCLContextPropertyList(platform)
//        let computeContext = new OpenCLContext(new List<OpenCLDevice>([| device |]), contextProperties, null, System.IntPtr.Zero) 
//        let computeProgram = new OpenCLProgram(computeContext, code)
//        // Generate define options
//        let log, success =
//            try
//                computeProgram.Build([| device |], "", null, System.IntPtr.Zero)
//                null, true
//            with
//            | ex -> 
//                let log = computeProgram.GetBuildLog(device)
//                log, false
//        log, success
//    else
//        "No OpenCL device found to test backend compilation", true

[<ReflectedDefinition>]
let sumElementsArrays(a: float32[], b:float32[], wi:WorkItemInfo) =    
    let gid = wi.GlobalID(0) 
    a.[gid] + b.[gid]


[<ReflectedDefinition; Kernel>]
let VectorAddWithUtility(a: float32[], b:float32[], wi:WorkItemInfo) =
    let c = Array.zeroCreate<float32> (a.GetLength(0))
    let s = sumElementsArrays(a, b, wi)
    c.[wi.GlobalID(0)] <- s
    c
    
[<ReflectedDefinition;Kernel>]
let MatMul (wi: WorkItemInfo) (matA: float32[,]) (matB: float32[,]) =
    let matC = Array2D.zeroCreate<float32> (matA.GetLength(0)) (matA.GetLength(1))
    let r = wi.GlobalID(1)
    let c = wi.GlobalID(0)
    
    // Unroll 8
    let mutable accum = 0.0f
    if r < matA.GetLength(0) && c < matB.GetLength(1) then
        for i = 0 to matA.GetLength(1) - 1 do
            accum <- accum + matA.[r, i] * matB.[i, c]
        matC.[r, c] <- accum
    matC
        
// Matrix multiplication
[<ReflectedDefinition;Kernel>]
let Map2 (wi: WorkItemInfo) (matA: float32[,]) (matB: float32[,]) =
    let matC = Array2D.zeroCreate<float32> (matA.GetLength(0)) (matA.GetLength(1))
    let r = wi.GlobalID(1)
    let c = wi.GlobalID(0)
    
    // Unroll 8
    let mutable accum = 0.0f
    matC.[r, c] <- matA.[r,c] - matB.[r,c]
    matC
        
[<ReflectedDefinition;Kernel>]
let MatTransp (wi: WorkItemInfo) (matA: float32[,]) =
    let matC = Array2D.zeroCreate<float32> (matA.GetLength(1)) (matA.GetLength(0))

    let xIndex = wi.GlobalID(0)
    let yIndex = wi.GlobalID(1)
    matC.[yIndex, xIndex] <- matA.[xIndex, yIndex]
    matC
    
[<EntryPoint>]
let main argv =     
    //let result = compiler.Compile(<@ Array.map2 FloatSum a b @>) :?> IKernelExpression

    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.zeroCreate<float32> 64
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
     
    let rnd = System.Random()
    // Matrices    
    let count = 10
    let outputSize = 128
    let inputSize = outputSize
    let matA = Array2D.create inputSize inputSize (rnd.NextDouble() |> float32) 
    let mutable norm1 = 0.0f
    for r = 0 to matA.GetLength(0) - 1 do
        let mutable s = 0.0f
        for c = 0 to matA.GetLength(1) - 1 do
            s <- s + matA.[r,c]
        if s > norm1 then
            norm1 <- s
    let mutable normInf = 0.0f
    for c = 0 to matA.GetLength(1) - 1 do
        let mutable s = 0.0f
        for r = 0 to matA.GetLength(0) - 1 do
            s <- s + matA.[r,c]
        if s > normInf then
            normInf <- s
    let normalization = 1.0f / (norm1 * normInf)
     
    let ws = new WorkSize([| inputSize |> int64; inputSize |> int64 |], [| 16L; 16L |])  
    let c = <@ [| 0 .. 9 |] |>
                Array.fold(fun X it ->
                                Map2 ws 
                                    (X |> 
                                    Array2D.map(fun el -> el * 2.0f))  
                                    (MatMul ws 
                                            (MatMul ws X matA) 
                                            X)) 
                            (matA |> 
                            MatTransp ws |> 
                            Array2D.map(fun it -> it * normalization)) 
            @> |> compiler.Compile :?> IKernelExpression

            

    let result0 = compiler.Compile(<@ Array.reduce(fun a b -> a + b + 1) @>) :?> IKernelExpression
    let result01 = compiler.Compile(<@ Array.reduce(fun a b -> a + b + 1) @>) :?> IKernelExpression
    let result1 = compiler.Compile(<@ Array.reduce(fun a b -> a + b + 1.0) @>) :?> IKernelExpression
    let result2 = compiler.Compile(<@ VectorAddWithUtility(a, b, size) @>) :?> IKernelExpression

    0 // return an integer exit code
