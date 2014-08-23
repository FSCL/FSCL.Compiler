namespace FSCL

open System 
open System.Runtime.InteropServices
open System.Threading

module Language = 
    open FSCL.Compiler
    ///
    ///<summary>
    /// Enumeration describing the address spaces exposed by OpenCL
    ///</summary>
    ///
    [<Flags>]
    type AddressSpace =
    | Global = 1
    | Constant = 2
    | Local = 4
    | Private = 8
    | Auto = 0

    ///
    ///<summary>
    /// Enumerationdescribing the transfer contraints to a kernel parameter (NoTransfer, NoTransferBack, Transfer)
    ///</summary>
    ///
    type TransferMode =
    | TransferIfNeeded = 0
    | NoTransfer = 1
    | ForceTransfer = 2

    type AccessMode =
    | ReadOnly = 0
    | WriteOnly = 1
    | ReadWrite = 2    
    
    ///
    ///<summary>
    /// Enumeration describing the strategy to read from a buffer associated to an array parameter
    ///</summary>
    ///
    [<Flags>]
    type BufferReadMode =
    | Auto = 0
    | MapBuffer = 1
    | EnqueueReadBuffer = 2
   
    ///
    ///<summary>
    /// Enumeration describing the strategy to write to a buffer associated to an array parameter
    ///</summary>
    ///
    [<Flags>]
    type BufferWriteMode =
    | Auto = 0
    | MapBuffer = 1
    | EnqueueWriteBuffer = 2
    
    ///
    ///<summary>
    /// OpenCL memory flags
    ///</summary>
    ///
    [<Flags>]
    type MemoryFlags =
    // None has a different value compared to OpenCL one, cause
    // in OpenCL it's 0 but this way we couldn't say if the user specifies it or not
    | Auto = 0L
    | None = 8192L
    | ReadWrite = 1L
    | WriteOnly = 2L
    | ReadOnly = 4L
    | UseHostPointer = 8L
    | AllocHostPointer = 16L
    | CopyHostPointer = 32L
    | UsePersistentMemAMD = 64L
    | HostWriteOnly = 128L
    | HostReadOnly = 256L
    | HostNoAccess = 512L
    
    [<Flags>]
    type DeviceType =
    | Default = 1
    | Cpu = 2
    | Gpu = 4
    | Accelerator = 8
    | Custom = 16
    | All = 0xFFFFFFFF
    
    ///
    ///<summary>
    ///Memory fance modes
    ///</summary>
    ///
    type MemFenceMode =
    | CLK_LOCAL_MEM_FENCE
    | CLK_GLOBAL_MEM_FENCE  

    [<AllowNullLiteral>]
    type KernelMetadataFunctionAttribute(t: Type) =
        inherit Attribute()
        member val Metadata = t with get

    [<AllowNullLiteral>]
    type ParameterMetadataFunctionAttribute(t: Type) =
        inherit Attribute()
        member val Metadata = t with get
        
    [<AllowNullLiteral>]
    type ReturnMetadataFunctionAttribute(t: Type) =
        inherit Attribute()
        member val Metadata = t with get
        
    ///
    ///<summary>
    ///The attribute to mark a parameter to be allocated in a particular address space (global, constant, local)
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type AddressSpaceAttribute(space: AddressSpace) =
        inherit ParameterMetadataAttribute()
        member val AddressSpace = space
        new() =
            AddressSpaceAttribute(AddressSpace.Auto)
        override this.ToString() =
            this.AddressSpace.ToString()
        
    ///
    ///<summary>
    ///The attribute to specify the transfer mode for a kernel paramater
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type TransferModeAttribute(hostToDevice: TransferMode, deviceToHost: TransferMode) =
        inherit ParameterMetadataAttribute()
        member val HostToDeviceMode = hostToDevice with get
        member val DeviceToHostMode = deviceToHost with get
        new() =
            TransferModeAttribute(TransferMode.TransferIfNeeded, TransferMode.TransferIfNeeded)
         
    ///
    ///<summary>
    ///The attribute to specify the memory flags for a kernel paramater
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type MemoryFlagsAttribute(flags: MemoryFlags) =
        inherit ParameterMetadataAttribute()
        member val Flags = flags with get
        new() =
            MemoryFlagsAttribute(MemoryFlags.Auto)
        override this.ToString() =
            this.Flags.ToString()
        
    ///
    ///<summary>
    ///The attribute to specify the approach to read from a buffer
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type BufferReadModeAttribute(mode: BufferReadMode) =
        inherit ParameterMetadataAttribute()
        member val Mode = mode with get
        new() =
            BufferReadModeAttribute(BufferReadMode.Auto)
        override this.ToString() =
            this.Mode.ToString()
        
    ///
    ///<summary>
    ///The attribute to specify the approach to write a buffer
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type BufferWriteModeAttribute(mode: BufferWriteMode) =
        inherit ParameterMetadataAttribute()
        member val Mode = mode with get
        new() =
            BufferWriteModeAttribute(BufferWriteMode.Auto)    
        override this.ToString() =
            this.Mode.ToString()
        
    ///
    ///<summary>
    ///The attribute to specify a devic type
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type DeviceTypeAttribute(t: DeviceType) =
        inherit KernelMetadataAttribute()
        member val Type = t with get
        new() =
            DeviceTypeAttribute(DeviceType.Gpu)   
        override this.ToString() =
            this.Type.ToString()
            
        
    ///
    ///<summary>
    ///The attribute to specify a minimum reduction array size before going to cpu
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type MinReduceArrayLengthAttribute(t: int64) =
        inherit KernelMetadataAttribute()
        member val Length = t with get
        new() =
            MinReduceArrayLengthAttribute(1L)   
        override this.ToString() =
            this.Length.ToString()

    // Functions matching attributes for dynamic marking of parameters
    [<ParameterMetadataFunction(typeof<AddressSpaceAttribute>)>]
    let ADDRESS_SPACE(m: AddressSpace, a) = 
        a
    [<ParameterMetadataFunction(typeof<TransferModeAttribute>)>]
    let TRANSFER_MODE(htd: TransferMode, dth: TransferMode, a) = 
        a
    [<ParameterMetadataFunction(typeof<MemoryFlagsAttribute>)>]
    let MEMORY_FLAGS(m: MemoryFlags, a) = 
        a     
    [<ParameterMetadataFunction(typeof<BufferReadModeAttribute>)>]
    let BUFFER_READ_MODE(m: BufferReadMode, a) = 
        a     
    [<ParameterMetadataFunction(typeof<BufferWriteModeAttribute>)>]
    let BUFFER_WRITE_MODE(m: BufferWriteMode, a) = 
        a     
        
    // Functions matching attributes for dynamic marking of kernels
    [<KernelMetadataFunction(typeof<DeviceTypeAttribute>)>]
    let DEVICE_TYPE(t: DeviceType, a) =
        a
    [<KernelMetadataFunction(typeof<MinReduceArrayLengthAttribute>)>]
    let MIN_REDUCE_ARRAY_LENGTH(l:int64, a) = 
        a     
        
    // Functions matching attributes for dynamic marking of return buffers
    [<ReturnMetadataFunction(typeof<AddressSpaceAttribute>)>]
    let RETURN_ADDRESS_SPACE(m: AddressSpace, a) = 
        a
    [<ReturnMetadataFunction(typeof<TransferModeAttribute>)>]
    let RETURN_TRANSFER_MODE(m: TransferMode, a) = 
        a
    [<ReturnMetadataFunction(typeof<MemoryFlagsAttribute>)>]
    let RETURN_MEMORY_FLAGS(m: MemoryFlags, a) = 
        a 
    [<ReturnMetadataFunction(typeof<BufferReadModeAttribute>)>]
    let RETURN_BUFFER_READ_MODE(m: BufferReadMode, a) = 
        a         
    [<ReturnMetadataFunction(typeof<BufferWriteModeAttribute>)>]
    let RETURN_BUFFER_WRITE_MODE(m: BufferWriteMode, a) = 
        a     
            
    // Dynamic (passed when compiling OpenCL kernel code) define
    type DynamicConstantDefineAttribute() =
        inherit Attribute()
            
    /// Operator overloading for pointer arithmetic inside kernels    
    type 'T ``[]`` with
        member this.pasum(y: int) =
            this
        member this.pasub(y: int) =     
            this
    ///
    ///<summary>
    /// OpenCL modifier for local memory declaration
    ///</summary>
    ///
    let local(a) =
        a   
        
    (*
    ///
    ///<summary>
    ///OpenCL barrier function
    ///</summary>
    ///<param name="fenceMode">The memory fence mode</param>
    ///
    let barrier(fenceMode:MemFenceMode) =
        ()  
    ///
    ///<summary>
    ///OpenCL get_global_id function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The work-item global id relative to the input dimension</returns>
    ///
    let get_global_id(dim:int) = 
        0
    ///
    ///<summary>
    ///OpenCL get_local_id function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The work-item local id relative to the input dimension</returns>
    ///
    let get_local_id(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_global_size function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The workspace global size relative to the input dimension</returns>
    ///
    let get_global_size(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_local_size function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The workspace local size relative to the input dimension</returns>
    ///
    let get_local_size(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_num_groups function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The number of groups relative to the input dimension</returns>
    ///
    let get_num_groups(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_group_id function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The group id of the work-item relative to the input dimension</returns>
    ///
    let get_group_id(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_global_offset function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The global_offset relative to the input dimension</returns>
    ///
    let get_global_offset(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_work_idm function
    ///</summary>
    ///<returns>The number workspace dimensions</returns>
    ///
    let get_work_dim() =
        0
           
        *)
    // Math functions
    let acosh(x) =
        Math.Log(x + Math.Sqrt(Math.Pow(x, 2.0) - 1.0))

    let acospi(x) =
        Math.Acos(x) / Math.PI
                
    let asinh(x) =
        Math.Log(x + Math.Sqrt(Math.Pow(x, 2.0) + 1.0))

    let asinpi(x) =
        Math.Asin(x) / Math.PI
        
    let atanh(x) =
        1.0/2.0 * Math.Log((1.0 + x) / (1.0 - x))

    let atanpi(x) =
        atan(x) / Math.PI
        
    let atan2pi(y, x) =
        atan2 y x / Math.PI

    let cbrt(x) =
        Math.Pow(x, 1.0/3.0)
        
    let copysign(x, y) =
        if (x / y < 0) then
            -x
        else
            x

    let cospi(x) =
        Math.Cos(x) / Math.PI
        
    let erfc(x: float) = 
        let a1 = -1.26551223
        let a2 = 1.00002368
        let a3 = 0.37409196
        let a4 = 0.09678418
        let a5 = -0.18628806
        let a6 = 0.27886807
        let a7 = -1.13520398
        let a8 = 1.48851587
        let a9 = -0.8221522
        let a10 = 0.17087277
        
        let z = if x < 0.0 then -x else x
        if (z < 0.0) then
            1.0
        else
            let t = 1.0/(1.0 + 0.5 * z)
            let mutable v = t * Math.Exp((-z*z)+a1+t*(a2+t*(a3+t*(a4+t*(a5+t*(a6+t*(a7+t*(a8+t*(a9+t*a10)))))))))
            if (x < 0.0) then
                v <- 2.0 - v
            v

    let erf(x) =
        1.0 - erfc(x)
        
    let exp2(x) =
        Math.Pow(2.0, x)
        
    let exp10(x) =
        Math.Pow(10.0, x)

    let expm1(x) =
        Math.Exp(x) - 1.0

    let fabs(x) =
        if(x < 0.0) then
            x
        else
            -x

    let fdim(x, y) =
        if x > y then
            x - y
        else
            0.0

    let fma(a, b, c) =
        c + (a * b)

    let fmax(x, y) =
        if Double.IsNaN(x) && Double.IsNaN(y) then
            Double.NaN
        else if Double.IsNaN(x) then
            y
        else if Double.IsNaN(y) then
            x
        else
            Math.Max(x, y)
            
    let fmin(x, y) =
        if Double.IsNaN(x) && Double.IsNaN(y) then
            Double.NaN
        else if Double.IsNaN(x) then
            y
        else if Double.IsNaN(y) then
            x
        else
            Math.Min(x, y)

    let fmod(x: float, y: float) =
        x - y * (Math.Truncate(x / y))

    let inline mul24(x:^T, y:^T) =
        x * y

    let inline mad24(x:^T, y:^T, z:^T) =
        x * y + z
        
    // Array -> Vector Array markers
    // Array -> Vector Array markers
    [<VectorTypeArrayReinterpret>]
    let AsFloat2(arr: float32[]) =
        let varr = Array.zeroCreate<float2>(arr.Length / 2)
        let varrH = GCHandle.Alloc(varr, GCHandleType.Pinned)
        Marshal.Copy(arr, 0, varrH.AddrOfPinnedObject(), arr.Length)
        varrH.Free()
        varr
    [<VectorTypeArrayReinterpret>]
    let AsFloat3(arr: float32[]) =
        let varr = Array.zeroCreate<float3>(arr.Length / 3)
        let varrH = GCHandle.Alloc(varr, GCHandleType.Pinned)
        Marshal.Copy(arr, 0, varrH.AddrOfPinnedObject(), arr.Length)
        varrH.Free()
        varr
    [<VectorTypeArrayReinterpret>]
    let AsFloat4(arr: float32[]) =
        let varr = Array.zeroCreate<float4>(arr.Length / 4)
        let varrH = GCHandle.Alloc(varr, GCHandleType.Pinned)
        Marshal.Copy(arr, 0, varrH.AddrOfPinnedObject(), arr.Length)
        varrH.Free()
        varr
    [<VectorTypeArrayReinterpret>]
    let AsInt2(arr: int32[]) =
        let varr = Array.zeroCreate<int2>(arr.Length / 2)
        let varrH = GCHandle.Alloc(varr, GCHandleType.Pinned)
        Marshal.Copy(arr, 0, varrH.AddrOfPinnedObject(), arr.Length)
        varrH.Free()
        varr
    [<VectorTypeArrayReinterpret>]
    let AsInt3(arr: int32[]) =
        let varr = Array.zeroCreate<int3>(arr.Length / 3)
        let varrH = GCHandle.Alloc(varr, GCHandleType.Pinned)
        Marshal.Copy(arr, 0, varrH.AddrOfPinnedObject(), arr.Length)
        varrH.Free()
        varr
    [<VectorTypeArrayReinterpret>]
    let AsInt4(arr: int32[]) =
        let varr = Array.zeroCreate<int4>(arr.Length / 4)
        let varrH = GCHandle.Alloc(varr, GCHandleType.Pinned)
        Marshal.Copy(arr, 0, varrH.AddrOfPinnedObject(), arr.Length)
        varrH.Free()
        varr

    // Conversions
    type VectorTypeConversionRoundingMode =
    | rte = 0
    | rtz = 1
    | rtp = 2
    | rtn = 3

    type int2 with
        [<VectorTypeConversion>]
        member this.ToFloat2(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat2Sat(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2Sat(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
    type int3 with
        [<VectorTypeConversion>]
        member this.ToFloat3(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToSByte3(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat3Sat(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3Sat(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToSByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
    type int4 with
        [<VectorTypeConversion>]
        member this.ToFloat4(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToSByte4(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat4Sat(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4Sat(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToSByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)

    type uint2 with
        [<VectorTypeConversion>]
        member this.ToFloat2(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToInt2(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int32, this.y |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat2Sat(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2Sat(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int32, this.y |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
    type uint3 with
        [<VectorTypeConversion>]
        member this.ToFloat3(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToInt3(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int32, this.y |> int32, this.z |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte3(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)

        [<VectorTypeConversion>]
        member this.ToFloat3Sat(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3Sat(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int32, this.y |> int32, this.z |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
    type uint4 with
        [<VectorTypeConversion>]
        member this.ToFloat4(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToInt4(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int32, this.y |> int32, this.z |> int32, this.w |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte4(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat4Sat(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4Sat(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int32, this.y |> int32, this.z |> int32, this.w |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)

    type sbyte2 with
        [<VectorTypeConversion>]
        member this.ToFloat2(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToByte2(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.Components)
        [<VectorTypeConversion>]
        member this.ToChar2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat2Sat(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2Sat(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.Components)
        [<VectorTypeConversion>]
        member this.ToChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
    type sbyte3 with
        [<VectorTypeConversion>]
        member this.ToFloat3(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToByte3(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat3Sat(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3Sat(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
    type sbyte4 with
        [<VectorTypeConversion>]
        member this.ToFloat4(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToByte4(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat4Sat(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4Sat(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)

    type byte2 with
        [<VectorTypeConversion>]
        member this.ToFloat2(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)

        [<VectorTypeConversion>]
        member this.ToFloat2Sat(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2Sat(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
    type byte3 with
        [<VectorTypeConversion>]
        member this.ToFloat3(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToSByte3(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar3(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat3Sat(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3Sat(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToSByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
    type byte4 with
        [<VectorTypeConversion>]
        member this.ToFloat4(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToSByte4(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar4(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat4Sat(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4Sat(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToSByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)

    type float2 with
        [<VectorTypeConversion>]
        member this.ToUint2(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToDouble2(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToInt2(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int32, this.y |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToUint2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToDouble2Sat(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int32, this.y |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
    type float3 with
        [<VectorTypeConversion>]
        member this.ToUint3(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToDouble3(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToInt3(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int32, this.y |> int32, this.z |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte3(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToUint3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToDouble3Sat(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int32, this.y |> int32, this.z |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
    type float4 with
        [<VectorTypeConversion>]
        member this.ToUint4(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToDouble4(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToInt4(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int32, this.y |> int32, this.z |> int32, this.w |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte4(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToUint4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToDouble4Sat(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int32, this.y |> int32, this.z |> int32, this.w |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
           
    type double2 with
        [<VectorTypeConversion>]
        member this.ToUint2(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToFloat2(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToInt2(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int32, this.y |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToUint2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToFloat2Sat(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int32, this.y |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToUChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
    type double3 with
        [<VectorTypeConversion>]
        member this.ToUint3(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToFloat3(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToInt3(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int32, this.y |> int32, this.z |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte3(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToUint3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToFloat3Sat(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int32, this.y |> int32, this.z |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToUChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
    type double4 with
        [<VectorTypeConversion>]
        member this.ToUint4(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToFloat4(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToInt4(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int32, this.y |> int32, this.z |> int32, this.w |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte4(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToUint4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToFloat4Sat(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int32, this.y |> int32, this.z |> int32, this.w |> int32)
        [<VectorTypeConversion>]
        member this.ToSByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToUChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)

    type uchar2 with
        [<VectorTypeConversion>]
        member this.ToFloat2(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToByte2(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar2(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
        [<VectorTypeConversion>]
        member this.ToFloat2Sat(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2Sat(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            char2(this.x |> char, this.y |> char)
    type uchar3 with
        [<VectorTypeConversion>]
        member this.ToFloat3(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToByte3(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte3(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar3(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
        [<VectorTypeConversion>]
        member this.ToFloat3Sat(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3Sat(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            char3(this.x |> char, this.y |> char, this.z |> char)
    type uchar4 with
        [<VectorTypeConversion>]
        member this.ToFloat4(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToByte4(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte4(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar4(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)
        [<VectorTypeConversion>]
        member this.ToFloat4Sat(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4Sat(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            char4(this.x |> char, this.y |> char, this.z |> char, this.w |> char)

    type char2 with
        member this.ToFloat2(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToByte2(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte2(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToUChar2(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
        member this.ToFloat2Sat(?rounding: VectorTypeConversionRoundingMode) =
            float2(this.x |> float32, this.y |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble2Sat(?rounding: VectorTypeConversionRoundingMode) =
            double2(this.x |> float, this.y |> float)
        [<VectorTypeConversion>]
        member this.ToUInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint2(this.x |> uint32, this.y |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt2Sat(?rounding: VectorTypeConversionRoundingMode) =
            int2(this.x |> int, this.y |> int)
        [<VectorTypeConversion>]
        member this.ToByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte2(this.x |> byte, this.y |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte2Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte2(this.x |> sbyte, this.y |> sbyte)
        [<VectorTypeConversion>]
        member this.ToUChar2Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar2(this.x |> byte, this.y |> byte)
    type char3 with
        [<VectorTypeConversion>]
        member this.ToFloat3(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToByte3(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte3(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToUChar3(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToFloat3Sat(?rounding: VectorTypeConversionRoundingMode) =
            float3(this.x |> float32, this.y |> float32, this.z |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble3Sat(?rounding: VectorTypeConversionRoundingMode) =
            double3(this.x |> float, this.y |> float, this.z |> float)
        [<VectorTypeConversion>]
        member this.ToUInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint3(this.x |> uint32, this.y |> uint32, this.z |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt3Sat(?rounding: VectorTypeConversionRoundingMode) =
            int3(this.x |> int, this.y |> int, this.z |> int)
        [<VectorTypeConversion>]
        member this.ToByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte3(this.x |> byte, this.y |> byte, this.z |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte3Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte3(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte)
        [<VectorTypeConversion>]
        member this.ToUChar3Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar3(this.x |> byte, this.y |> byte, this.z |> byte)
    type char4 with
        [<VectorTypeConversion>]
        member this.ToFloat4(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToByte4(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte4(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToUChar4(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        member this.ToFloat4Sat(?rounding: VectorTypeConversionRoundingMode) =
            float4(this.x |> float32, this.y |> float32, this.z |> float32, this.w |> float32)
        [<VectorTypeConversion>]
        member this.ToDouble4Sat(?rounding: VectorTypeConversionRoundingMode) =
            double4(this.x |> float, this.y |> float, this.z |> float, this.w |> float)
        [<VectorTypeConversion>]
        member this.ToUInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uint4(this.x |> uint32, this.y |> uint32, this.z |> uint32, this.w |> uint32)
        [<VectorTypeConversion>]
        member this.ToInt4Sat(?rounding: VectorTypeConversionRoundingMode) =
            int4(this.x |> int, this.y |> int, this.z |> int, this.w |> int)
        [<VectorTypeConversion>]
        member this.ToByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            byte4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
        [<VectorTypeConversion>]
        member this.ToSByte4Sat(?rounding: VectorTypeConversionRoundingMode) =
            sbyte4(this.x |> sbyte, this.y |> sbyte, this.z |> sbyte, this.w |> sbyte)
        [<VectorTypeConversion>]
        member this.ToUChar4Sat(?rounding: VectorTypeConversionRoundingMode) =
            uchar4(this.x |> byte, this.y |> byte, this.z |> byte, this.w |> byte)
    (*
    let fract(x, iptr) =
      

gentype fract (gentype x, 
__global gentype *iptr)
49
gentype fract (gentype x, 
__local gentype *iptr)
gentype fract (gentype x, 
__private gentype *iptr)
Returns fmin( x – floor (x), 0x1.fffffep-1f ).
floor(x) is returned in iptr.
floatn frexp (floatn x, 
__global intn *exp)
floatn frexp (floatn x, 
__local intn *exp)
Extract mantissa and exponent from x. For each 
component the mantissa returned is a float with 
magnitude in the interval [1/2, 1) or 0. Each 
component of x equals mantissa returned * 2exp *)

    let clamp(a, min, max) =
        if a < min then
            min
        elif a > max then
            max
        else
            a

    ///////////////////////////////////////////////////
    // Load and Store functions
    ///////////////////////////////////////////////////
    (*
    let vload2<'T> (offset: int, p: T'[]) =
        int2(p.[offset], p.[offset + 1]) 
    let vload3 (offset: int, p: int[]) =
        int3(p.[offset], p.[offset + 1], p.[offset + 2]) 
    let vload4 (offset: int, p: int[]) =
        int4(p.[offset], p.[offset + 1], p.[offset + 2], p.[offset + 3]) 
    let vstore2 (data: int2, offset: int, p: int[]) =
        p.[offset] <- data.x
        p.[offset + 1] <- data.y
    let vstore3 (data: int3, offset: int, p: int[]) =
        p.[offset] <- data.x
        p.[offset + 1] <- data.y
        p.[offset + 2] <- data.z
    let vstore4 (data: int4, offset: int, p: int[]) =
        p.[offset] <- data.x
        p.[offset + 1] <- data.y
        p.[offset + 2] <- data.z
        p.[offset + 3] <- data.w
      *)  

    type MemoryFlagsUtil() =
        static member OnlyAccessFlags(f: MemoryFlags) =
            f &&& (~~~ (MemoryFlags.None ||| MemoryFlags.CopyHostPointer ||| MemoryFlags.UseHostPointer ||| MemoryFlags.AllocHostPointer ||| MemoryFlags.UsePersistentMemAMD))
        static member OnlyKernelAccessFlags(f: MemoryFlags) =
            MemoryFlagsUtil.WithNoHostAccessFlags(MemoryFlagsUtil.OnlyAccessFlags(f))
        static member OnlyHostAccessFlags(f: MemoryFlags) =
            MemoryFlagsUtil.WithNoKernelAccessFlags(MemoryFlagsUtil.OnlyAccessFlags(f))
        static member WithNoKernelAccessFlags(f: MemoryFlags) =
            f &&& (~~~ (MemoryFlags.ReadOnly ||| MemoryFlags.WriteOnly ||| MemoryFlags.ReadWrite))
        static member WithNoHostAccessFlags(f: MemoryFlags) =
            f &&& (~~~ (MemoryFlags.HostReadOnly ||| MemoryFlags.HostWriteOnly ||| MemoryFlags.HostNoAccess))        
        static member WithNoAccessFlags(f: MemoryFlags) =
            MemoryFlagsUtil.WithNoHostAccessFlags(MemoryFlagsUtil.WithNoKernelAccessFlags(f))
        static member HasKernelAccessFlags(f: MemoryFlags) =
            f &&& (MemoryFlags.ReadOnly ||| MemoryFlags.WriteOnly ||| MemoryFlags.ReadWrite) |> int > 0
        static member HasHostAccessFlags(f: MemoryFlags) =
            f &&& (MemoryFlags.HostReadOnly ||| MemoryFlags.HostWriteOnly ||| MemoryFlags.HostNoAccess) |> int > 0
        static member HasAccessFlags(f: MemoryFlags) =
            MemoryFlagsUtil.HasHostAccessFlags(f) || MemoryFlagsUtil.HasHostAccessFlags(f)
        static member CanKernelWrite(f:MemoryFlags) =
            f &&& MemoryFlags.ReadOnly |> int = 0
        static member CanKernelReadAndWrite(f:MemoryFlags) =
            f &&& (MemoryFlags.ReadOnly ||| MemoryFlags.WriteOnly) |> int = 0
        static member CanKernelRead(f:MemoryFlags) =
            f &&& MemoryFlags.WriteOnly |> int > 0
        static member CanHostWrite(f:MemoryFlags) =
            f &&& (MemoryFlags.HostReadOnly ||| MemoryFlags.HostNoAccess) |> int = 0
        static member CanHostRead(f:MemoryFlags) =
            f &&& (MemoryFlags.HostWriteOnly ||| MemoryFlags.HostNoAccess) |> int = 0
        static member CanHostReadAndWrite(f:MemoryFlags) =
            f &&& MemoryFlags.HostNoAccess |> int = 0
            
            

    ///
    ///<summary>
    ///The container of workspace related functions
    ///</summary>
    ///<remarks>
    ///This is not generally used directly since the runtime-support (i.e. the semantic) of workspace functions is given by the 
    ///OpenCL-to-device code compiler (e.g. Intel/AMD OpenCL compiler). Nevertheless this can help the FSCL runtime to produce
    ///debuggable multithrad implementation that programmers can use to test their kernels
    ///</remarks>
    ///
    type WorkItemInfo = 
        interface
            abstract member GlobalID: int -> int
            abstract member LocalID: int -> int
            abstract member GlobalSize: int -> int
            abstract member LocalSize: int -> int
            abstract member NumGroups: int -> int
            abstract member GroupID: int -> int
            abstract member GlobalOffset: int -> int
            abstract member WorkDim: unit -> int
            abstract member Barrier: MemFenceMode -> unit
        end
        
    type WorkSize(global_size: int64[], local_size: int64[], global_offset: int64[]) = 
        new (globalSize: int64[]) =
            WorkSize(globalSize, null, null)
        new (globalSize: int64[], localSize: int64[]) =
            WorkSize(globalSize, localSize, null)
        new (globalSize: int64, localSize: int64, globalOffset: int64) =
            WorkSize([| globalSize |], [| localSize |], [| globalOffset |])
        new (globalSize: int64, localSize: int64) =
            WorkSize([| globalSize |], [| localSize |], null)
        new (globalSize: int64) =
            WorkSize([| globalSize |], null, null)
        
        member this.GlobalSize() =
            global_size
            
        member this.LocalSize() =
            local_size
            
        member this.GlobalOffset() =
            global_offset
            
        ///
        ///<summary>
        ///OpenCL get_global_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The work-item global id relative to the input dimension</returns>
        ///
        abstract member GlobalID: int -> int
        default this.GlobalID(dim) =
            0
        ///
        ///<summary>
        ///OpenCL get_local_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The work-item local id relative to the input dimension</returns>
        ///
        abstract member LocalID: int -> int
        default this.LocalID(dim) =
            0
        ///
        ///<summary>
        ///OpenCL get_global_size function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The workspace global size relative to the input dimension</returns>
        ///
        member this.GlobalSize(dim) =
            global_size.[dim] |> int
        ///
        ///<summary>
        ///OpenCL get_local_size function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The workspace local size relative to the input dimension</returns>
        ///
        member this.LocalSize(dim) =
            local_size.[dim] |> int
        ///
        ///<summary>
        ///OpenCL get_num_groups function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The number of groups relative to the input dimension</returns>
        ///
        member this.NumGroups(dim) =
            (int)(Math.Ceiling((float)global_size.[dim] / (float)local_size.[dim]))
        ///
        ///<summary>
        ///OpenCL get_group_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The group id of the work-item relative to the input dimension</returns>
        ///
        member this.GroupID(dim) =
            (int)(Math.Floor((float)(this.GlobalID(dim)) / (float)(this.GlobalSize(dim))))
        ///
        ///<summary>
        ///OpenCL get_global_offset function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The global_offset relative to the input dimension</returns>
        ///
        member this.GlobalOffset(dim) =
            global_offset.[dim] |> int
        ///
        ///<summary>
        ///OpenCL get_work_idm function
        ///</summary>
        ///<returns>The number workspace dimensions</returns>
        ///
        abstract member WorkDim: unit -> int
        default this.WorkDim() =
            global_size.Length
            
        abstract member Barrier: MemFenceMode -> unit
        default this.Barrier(fenceMode:MemFenceMode) =
            ()

        interface WorkItemInfo with
            member this.GlobalSize(dim) =
                this.GlobalSize(dim)
            member this.LocalSize(dim) =
                this.LocalSize(dim)
            member this.GlobalID(dim) =
                this.GlobalID(dim)
            member this.LocalID(dim) =
                this.LocalID(dim)
            member this.GroupID(dim) =
                this.GroupID(dim)
            member this.NumGroups(dim) =
                this.NumGroups(dim)
            member this.GlobalOffset(dim) =
                this.GlobalOffset(dim)
            member this.WorkDim() =
                this.WorkDim()
            member this.Barrier(mfm) =
                this.Barrier(mfm)

    // Datatype mapping      
    ///
    ///<summary>
    ///Alias of <see cref="System.Byte"/>
    ///</summary>
    ///
    type uchar = byte  
    ///
    ///<summary>
    ///Alias of <see cref="System.UInt32"/>
    ///</summary>
    ///
    type uint = uint32  
    ///
    ///<summary>
    ///Alias of <see cref="System.UInt32"/>
    ///</summary>
    ///
    type size_t = uint32    
    ///
    ///<summary>
    ///Alias of <see cref="System.Float16"/>
    ///</summary>
    ///
    type half = Float16
    ///
    ///<summary>
    ///Alias of <see cref="System.Long"/>
    ///</summary>
    ///
    type long = int64  
    ///
    ///<summary>
    ///Alias of <see cref="System.ULong"/>
    ///</summary>
    ///
    type ulong = uint64  
    





