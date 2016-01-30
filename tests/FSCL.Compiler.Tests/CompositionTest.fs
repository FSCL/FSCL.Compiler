module FSCL.Compiler.CompositionTest

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Runtime.InteropServices

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
    
let GetData() =
    let compiler = new Compiler()
    let matA = Array2D.create 64 64 1.0f
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
     
    let size = new WorkSize([| 64L; 64L |], [| 16L; 16L |]) :> WorkItemInfo
    compiler, matA, normalization, size    

[<Test>]
let ``Can compile kernel composition for Newton's approximation of matrix inverse`` () =
    let compiler, matA, normalization, ws = GetData()
    
    let result = 
        compiler.Compile<IKernelExpression>
           <@ [| 0 .. 9 |] |>
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
            @> 
    Assert.NotNull(result)
    // Check root is a composition
    Assert.IsTrue(result.KFGRoot :? KFGCollectionCompositionNode)
    // Check 5 compiled kernels
    Assert.AreEqual(result.KernelNodes.Count, 6)

    let mutable ok = true
    let mutable i = 0
    while ok && i < result.KernelNodes.Count do
        let log, success = TestUtil.TryCompileOpenCL((result.KernelNodes.[i] :?> KFGKernelNode).Module)
        if not success then
            Assert.Fail(log)
            ok <- false
        i <- i + 1
    