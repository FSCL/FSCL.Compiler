module TestUtil

open OpenCL
open FSCL.Compiler
open FSCL.Language
open System
open System.Collections.Generic

let GetVectorSampleData() =
    let compiler = new Compiler()
    let a = Array.create 64 1.0f
    let b = Array.create 64 2.0f
    let c = Array.create 64 0.0f
    let size = new WorkSize(64L, 64L) :> WorkItemInfo
    compiler, a, b, c, size

let TryCompileOpenCL(code:String) =
    if OpenCL.OpenCLPlatform.Platforms.Count > 0 then
        let platform = OpenCL.OpenCLPlatform.Platforms.[0]
        let device = platform.Devices.[0]
        let contextProperties = new OpenCLContextPropertyList(platform)
        let computeContext = new OpenCLContext(new List<OpenCLDevice>([| device |]), contextProperties, null, System.IntPtr.Zero) 
        let computeProgram = new OpenCLProgram(computeContext, code)
        // Generate define options
        let log, success =
            try
                computeProgram.Build([| device |], "", null, System.IntPtr.Zero)
                null, true
            with
            | ex -> 
                let log = computeProgram.GetBuildLog(device)
                log, false
        log, success
    else
        "No OpenCL device found to test backend compilation", true