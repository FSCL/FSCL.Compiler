namespace FSCL.Compiler

open System 

module Language = 
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
    /// Enumeration describing the transfer contraints to a kernel parameter (NoTransfer, NoTransferBack, Transfer)
    ///</summary>
    ///
    [<Flags>]
    type TransferMode =
    | TransferIfNeeded = 0
    | NoTransfer = 1
    | NoTransferBack = 2

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
    type TransferModeAttribute(mode: TransferMode) =
        inherit ParameterMetadataAttribute()
        member val Mode = mode with get
        new() =
            TransferModeAttribute(TransferMode.TransferIfNeeded)
        override this.ToString() =
            this.Mode.ToString()
         
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

    // Functions matching attributes for dynamic marking of parameters
    [<ParameterMetadataFunction(typeof<AddressSpaceAttribute>)>]
    let ADDRESS_SPACE(m: AddressSpace, a) = 
        a
    [<ParameterMetadataFunction(typeof<TransferModeAttribute>)>]
    let TRANSFER_MODE(m: TransferMode, a) = 
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
    [<ReturnMetadataFunction(typeof<BufferWriteModeAttribute>)>]
    let RETURN_BUFFER_WRITE_MODE(m: BufferWriteMode, a) = 
        a     

    [<AllowNullLiteral>]
    type AlternativeFunctionAttribute(s:string) =
        inherit Attribute()
        member this.AlternativeFunctionName 
            with get() = s  
            
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
        
    // Math functions
    [<AlternativeFunction("Math.Acos")>]
    let acos(x) =
        Math.Acos(x)

    let acosh(x) =
        Math.Log(x + Math.Sqrt(Math.Pow(x, 2.0) - 1.0))

    let acospi(x) =
        Math.Acos(x) / Math.PI
        
    [<AlternativeFunction("Math.Asin")>]
    let asin(x) =
        Math.Asin(x)
        
    let asinh(x) =
        Math.Log(x + Math.Sqrt(Math.Pow(x, 2.0) + 1.0))

    let asinpi(x) =
        Math.Asin(x) / Math.PI
        
    [<AlternativeFunction("Math.Atan")>]
    let atan(x) =
        Math.Atan(x)
        
    [<AlternativeFunction("Math.Atan2")>]
    let atan2(y, x) =
        Math.Atan2(y, x)
        
    let atanh(x) =
        1.0/2.0 * Math.Log((1.0 + x) / (1.0 - x))

    let atanpi(x) =
        atan(x) / Math.PI
        
    let atan2pi(y, x) =
        atan2(y, x) / Math.PI

    let cbrt(x) =
        Math.Pow(x, 1.0/3.0)
        
    [<AlternativeFunction("Math.Ceiling")>]
    let ceil(x: float) =
        Math.Ceiling(x)

    let copysign(x, y) =
        if (x / y < 0) then
            -x
        else
            x
    
    [<AlternativeFunction("Math.Cos")>]
    let cos(x) =
        Math.Cos(x)
        
    [<AlternativeFunction("Math.Cosh")>]
    let cosh(x) =
        Math.Cosh(x)

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
        
    [<AlternativeFunction("Math.Exp")>]
    let exp(x) =
        Math.Exp(x)
        
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
            
    [<AlternativeFunction("Math.Floor")>]
    let floor(x: float) =
        Math.Floor(x)

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
    type WorkItemIdContainer(global_size: int[], local_size: int[], global_id: int[], local_id: int [], global_offset: int[]) = 
        ///
        ///<summary>
        ///OpenCL get_global_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The work-item global id relative to the input dimension</returns>
        ///
        member this.GlobalID(dim) =
            global_id.[dim]
        ///
        ///<summary>
        ///OpenCL get_local_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The work-item local id relative to the input dimension</returns>
        ///
        member this.LocalID(dim) =
            local_id.[dim]
        ///
        ///<summary>
        ///OpenCL get_global_size function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The workspace global size relative to the input dimension</returns>
        ///
        member this.GlobalSize(dim) =
            global_size.[dim]
        ///
        ///<summary>
        ///OpenCL get_local_size function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The workspace local size relative to the input dimension</returns>
        ///
        member this.LocalSize(dim) =
            local_size.[dim]
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
            (int)(Math.Floor((float)global_id.[dim] / (float)global_size.[dim]))
        ///
        ///<summary>
        ///OpenCL get_global_offset function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The global_offset relative to the input dimension</returns>
        ///
        member this.GlobalOffset(dim) =
            global_offset.[dim]
        ///
        ///<summary>
        ///OpenCL get_work_idm function
        ///</summary>
        ///<returns>The number workspace dimensions</returns>
        ///
        member this.WorkDim() =
            global_size.Rank
    
    // Datatype mapping    
    ///
    ///<summary>
    ///Alias of <see cref="System.SByte"/>
    ///</summary>
    ///
    type char = sbyte  
    //let char c =
      //  c |> sbyte
    // Datatype mapping    
    ///
    ///<summary>
    ///Alias of <see cref="System.Byte"/>
    ///</summary>
    ///
    type uchar = byte  
  //  let uchar c =
    //    c |> byte
    ///
    ///<summary>
    ///Alias of <see cref="System.UInt32"/>
    ///</summary>
    ///
    type uint = uint32  
  //  let uint c = 
    //    c |> uint32    
    ///
    ///<summary>
    ///Alias of <see cref="System.UInt32"/>
    ///</summary>
    ///
    type size_t = uint32     
 //   let size_t c = 
   //     c |> uint32   
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
  //  let ulong c = 
    //    c |> uint64

    





