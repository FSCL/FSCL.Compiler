namespace FSCL

open Cloo
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System.Reflection
open Microsoft.FSharp.Linq.QuotationEvaluation
open FSCL.Compiler
        

// Wrappers just for syntax purpose. You can run your kernel by typing f.WithSize(sizes).Run(params)
type KernelSizeWrapper<'T,'U>(f: Expr<'T -> 'U>, k:KernelRunner) =
    member this.WithSize(globalSize: int array, localSize: int array) =
        new KernelCallWrapper<'T,'U>(f, k, globalSize, localSize)
        
and KernelCallWrapper<'T,'U>(f: Expr<'T -> 'U>, k:KernelRunner, globalSize: int array, localSize: int array) =
    member this.Run(p:'T) =
        k.Run(f, p, globalSize, localSize)

// The Kernel runner
and KernelRunner =    
    val compiler : KernelCompiler
        
    member this.Compiler 
        with get() = 
            this.compiler

    member this.Kernel(f: Expr<'T -> 'U>) =
        new KernelSizeWrapper<'T, 'U>(f, this) 
        
    member this.Run(kernelInfo: MethodInfo, arguments: obj[], argumentsInfo: (ParameterInfo * int * Expr)[], globalSize: int array, localSize: int array) =
        let globalDataStorage = this.compiler.GlobalDataStorage

        // Found a kernel in global data matching the call
        let matchingKernel = ref (this.compiler.FindMatchingKernelInfo(kernelInfo, Array.map (fun a -> a.GetType()) arguments))
        if (!matchingKernel).IsNone then
            // Try add it to the compiler
            this.compiler.Add(kernelInfo) |> ignore
            matchingKernel := this.compiler.FindMatchingKernelInfo(kernelInfo, Array.map (fun a -> a.GetType()) arguments)
            
        // Fix: here to be called INSTANTIATE on a metric to get the device to use
        let kernelInstance = (!matchingKernel).Value.Instances.[0]
        let queue = globalDataStorage.Devices.[kernelInstance.DeviceIndex.Value].Queue
        let context = globalDataStorage.Devices.[kernelInstance.DeviceIndex.Value].Context
        // FIX: determine best read/write strategy

        // For each parameter, create buffer (if array), write it and set kernel arg   
        let additionalArgCount = ref 0     
        let paramObjectBufferMap = new System.Collections.Generic.Dictionary<string, (System.Object * ComputeMemory)>()

        let argIndex = ref 0
        Array.iteri (fun pIndex (par:ParameterInfo, dim:int, a:Expr) ->
            if par.ParameterType.IsArray then
                let o = arguments.[pIndex]
                // Check if constant buffer. In this case we pass the dimension (sizeof) the array and not a real buffer
                if (!matchingKernel).Value.Info.ParameterInfo.[par.Name].AddressSpace = KernelParameterAddressSpace.LocalSpace then
                    let size = (o.GetType().GetProperty("LongLength").GetValue(o) :?> int64) * 
                               (int64 (System.Runtime.InteropServices.Marshal.SizeOf(o.GetType().GetElementType())))
                    // Set kernel arg
                    kernelInstance.Kernel.SetLocalArgument(!argIndex, size) 
                else
                    // Check if read or read_write mode
                    let matchingParameter = (!matchingKernel).Value.Info.ParameterInfo.[par.Name]
                    let access = matchingParameter.Access
                    let mustInitBuffer =
                        ((matchingParameter.AddressSpace = KernelParameterAddressSpace.GlobalSpace) ||
                         (matchingParameter.AddressSpace = KernelParameterAddressSpace.ConstantSpace)) &&
                        ((access = KernelParameterAccessMode.ReadOnly) || 
                         (access = KernelParameterAccessMode.ReadWrite))

                    // Create buffer and eventually init it
                    let t = par.ParameterType.GetElementType()
                    let mutable buffer = None
                    if (t = typeof<uint32>) then
                        buffer <- Some(KernelRunnerTools.WriteBuffer<uint32>(context, queue, o, dim, mustInitBuffer))
                    elif (t = typeof<uint64>) then
                        buffer <- Some(KernelRunnerTools.WriteBuffer<uint64>(context, queue, o, dim ,mustInitBuffer))
                    elif (t = typeof<int64>) then
                        buffer <- Some(KernelRunnerTools.WriteBuffer<int64>(context, queue, o, dim, mustInitBuffer))
                    elif (t = typeof<int>) then
                        buffer <- Some(KernelRunnerTools.WriteBuffer<int>(context, queue, o, dim, mustInitBuffer))
                    elif (t = typeof<double>) then
                        buffer <- Some(KernelRunnerTools.WriteBuffer<double>(context, queue, o, dim, mustInitBuffer))
                    elif (t = typeof<float32>) then
                        buffer <- Some(KernelRunnerTools.WriteBuffer<float32>(context, queue, o, dim, mustInitBuffer))
                    elif (t = typeof<bool>) then
                        buffer <- Some(KernelRunnerTools.WriteBuffer<int>(context, queue, o, dim, mustInitBuffer))
                 
                    // Stor association between parameter, array and buffer object
                    paramObjectBufferMap.Add(par.Name, (o, buffer.Value))

                    // Set kernel arg
                    kernelInstance.Kernel.SetMemoryArgument(!argIndex, buffer.Value)  

                // Set additional args for array params (dimensions) 
                for dimension = 0 to dim - 1 do
                    let sizeOfDim = o.GetType().GetMethod("GetLength").Invoke(o, [| dimension |]) :?> int
                    kernelInstance.Kernel.SetValueArgument<int>(argumentsInfo.Length + !additionalArgCount + dimension, sizeOfDim)
                additionalArgCount := !additionalArgCount + dim
            else
                let t = par.ParameterType
                if (t = typeof<uint32>) then
                    kernelInstance.Kernel.SetValueArgument<uint32>(!argIndex, arguments.[pIndex] :?> uint32)
                elif (t = typeof<uint64>) then
                    kernelInstance.Kernel.SetValueArgument<uint64>(!argIndex, arguments.[pIndex] :?> uint64)
                elif (t = typeof<int64>) then
                    kernelInstance.Kernel.SetValueArgument<int64>(!argIndex, arguments.[pIndex] :?> int64)
                elif (t = typeof<int>) then
                    kernelInstance.Kernel.SetValueArgument<int>(!argIndex, arguments.[pIndex] :?> int)
                elif (t = typeof<double>) then
                    kernelInstance.Kernel.SetValueArgument<double>(!argIndex, arguments.[pIndex] :?> double)
                elif (t = typeof<float32>) then
                    kernelInstance.Kernel.SetValueArgument<float32>(!argIndex, arguments.[pIndex] :?> float32)
                elif (t = typeof<bool>) then
                    kernelInstance.Kernel.SetValueArgument<bool>(!argIndex, arguments.[pIndex] :?> bool)
            
            argIndex := !argIndex + 1) (argumentsInfo)

        // Run kernel
        let offset = Array.zeroCreate<int64>(globalSize.Length)
        // 32 bit enought for size_t. Kernel uses size_t like int withour cast. We cannot put case into F# kernels each time the user does operations with get_global_id and similar!
        queue.Execute(kernelInstance.Kernel, offset, Array.map(fun el -> int64(el)) globalSize, Array.map(fun el -> int64(el)) localSize, null)

        // Read result if needed
        Array.iteri (fun index (par:ParameterInfo, dim:int, arg:Expr) ->
            if par.ParameterType.IsArray then
                if (!matchingKernel).Value.Info.ParameterInfo.[par.Name].AddressSpace <> KernelParameterAddressSpace.LocalSpace then
                    // Get association between parameter, array and buffer object
                    let (o, buffer) = paramObjectBufferMap.[par.Name]

                    // Check if write or read_write mode
                    let mutable mustReadBuffer = false
                    let matchingParameter = (!matchingKernel).Value.Info.ParameterInfo.[par.Name]
                    let access = matchingParameter.Access
                    mustReadBuffer <-                     
                        ((matchingParameter.AddressSpace = KernelParameterAddressSpace.GlobalSpace)) &&
                        ((access = KernelParameterAccessMode.WriteOnly) || 
                         (access = KernelParameterAccessMode.ReadWrite))

                    if(mustReadBuffer) then
                        // Create buffer and eventually init it
                        let t = par.ParameterType.GetElementType()
                        if (t = typeof<uint32>) then
                            KernelRunnerTools.ReadBuffer<uint32>(context, queue, o, dim, buffer :?> ComputeBuffer<uint32>) 
                        elif (t = typeof<uint64>) then
                            KernelRunnerTools.ReadBuffer<uint64>(context, queue, o, dim, buffer :?> ComputeBuffer<uint64>) 
                        elif (t = typeof<int64>) then
                            KernelRunnerTools.ReadBuffer<int64>(context, queue, o, dim, buffer :?> ComputeBuffer<int64>) 
                        elif (t = typeof<int>) then
                            KernelRunnerTools.ReadBuffer<int>(context, queue, o, dim, buffer :?> ComputeBuffer<int>) 
                        elif (t = typeof<double>) then
                            KernelRunnerTools.ReadBuffer<double>(context, queue, o, dim, buffer :?> ComputeBuffer<double>) 
                        elif (t = typeof<float32>) then
                            KernelRunnerTools.ReadBuffer<float32>(context, queue, o, dim, buffer :?> ComputeBuffer<float32>) 
                        elif (t = typeof<bool>) then
                            KernelRunnerTools.ReadBuffer<bool>(context, queue, o, dim, buffer :?> ComputeBuffer<bool>)) argumentsInfo 


    // Run a kernel through a quoted kernel call
    member this.Run(expr: Expr, arg: obj, globalSize: int array, localSize: int array) =
        let (c, kernelInfo, args) = KernelCompilerTools.ExtractMethodInfo(expr)
        let arguments = FSharpValue.GetTupleFields(arg)
        this.Run(kernelInfo, arguments, args, globalSize, localSize)

    member this.Run(expr: Expr, globalSize: int array, localSize: int array) =                     
        let (c, kernelInfo, args) = KernelCompilerTools.ExtractMethodInfo(expr)
        let arguments = Array.map (fun (p, d, e:Expr) -> e.EvalUntyped()) args
        this.Run(kernelInfo, arguments, args, globalSize, localSize)
                 
    new () = {
        compiler = new KernelCompiler()
    }
    new (comp) = {
        compiler = comp
    }
      
module KernelExtension =
    let ker(f:'T -> unit) =
        fun (t:'T) -> (fun(r:KernelRunner, gSize, lSize) -> r.Run(<@ f @>, gSize, lSize))

    let kernel(f:'T -> 'U) =
        fun (t:'T, globalSize: int array, localSize: int array, runner: KernelRunner) ->
            runner.Run(<@ f @>, globalSize, localSize)
  
            
            

