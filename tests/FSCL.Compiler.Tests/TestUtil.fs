module TestUtil

open OpenCL
open FSCL.Compiler
open FSCL.Language
open System
open System.Collections.Generic
open Microsoft.FSharp.Linq.RuntimeHelpers

let GetVectorSampleData() =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.create 64 0.0f
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    compiler, a, b, c, size

let EvalDynamicDefines(kernelModule: IKernelModule) =
    let dynDefineValues = new Dictionary<string, string>()   
    let dynDefValuesChain = ref ""             
    for item in kernelModule.ConstantDefines do
        let tv, _, evaluator = item.Value
        let dynDefVal = evaluator.GetType().GetMethod("Invoke").Invoke(evaluator, 
                            if tv.IsSome then 
                                let thisObj = LeafExpressionConverter.EvaluateQuotation(kernelModule.InstanceExpr.Value)
                                [| thisObj |]
                            else
                                [|()|])
        dynDefValuesChain := !dynDefValuesChain + dynDefVal.ToString()
        dynDefineValues.Add(item.Key, dynDefVal.ToString())
    
    let mutable definesOption = ""
    for d in dynDefineValues do
        definesOption <- definesOption + "-D " + d.Key + "=" + d.Value + " "
    definesOption

let TryCompileOpenCL(m:IKernelModule) =
    if OpenCL.OpenCLPlatform.Platforms.Count > 0 then
        let code = m.Code.Value
        let dynDef = EvalDynamicDefines(m)

        let platform = OpenCL.OpenCLPlatform.Platforms.[0]
        let device = platform.Devices.[0]
        let contextProperties = new OpenCLContextPropertyList(platform)
        let computeContext = new OpenCLContext(new List<OpenCLDevice>([| device |]), contextProperties, null, System.IntPtr.Zero) 
        let computeProgram = new OpenCLProgram(computeContext, code)
        // Generate define options
        let log, success =
            try
                computeProgram.Build([| device |], dynDef, null, System.IntPtr.Zero)
                null, true
            with
            | ex -> 
                let log = computeProgram.GetBuildLog(device)
                log, false
        log, success
    else
        "No OpenCL device found to test backend compilation", true