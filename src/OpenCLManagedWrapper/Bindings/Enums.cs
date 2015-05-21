#region License

/*

Copyright (c) 2009 - 2011 Fatjon Sakiqi

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

*/

#endregion

namespace OpenCL
{
    using System;

    /// <summary>
    /// The OpenCL error codes.
    /// </summary>
    public enum OpenCLErrorCode : int
    {
        /// <summary> </summary>
        Success = 0,
        /// <summary> </summary>
        DeviceNotFound = -1,
        /// <summary> </summary>
        DeviceNotAvailable = -2,
        /// <summary> </summary>
        CompilerNotAvailable = -3,
        /// <summary> </summary>
        MemoryObjectAllocationFailure = -4,
        /// <summary> </summary>
        OutOfResources = -5,
        /// <summary> </summary>
        OutOfHostMemory = -6,
        /// <summary> </summary>
        ProfilingInfoNotAvailable = -7,
        /// <summary> </summary>
        MemoryCopyOverlap = -8,
        /// <summary> </summary>
        ImageFormatMismatch = -9,
        /// <summary> </summary>
        ImageFormatNotSupported = -10,
        /// <summary> </summary>
        BuildProgramFailure = -11,
        /// <summary> </summary>
        MapFailure = -12,
        /// <summary> </summary>
        MisalignedSubBufferOffset = -13,
        /// <summary> </summary>
        ExecutionStatusErrorForEventsInWaitList = -14,
        CompileProgramFailure = -15,
        LinkerNotAvailable = -16,
        LinkProgramFailure = -17,
        DevicePartitionFailed = -18,
        KernelArgInfoNotAvailable = -19,
        /// <summary> </summary>
        InvalidValue = -30,
        /// <summary> </summary>
        InvalidDeviceType = -31,
        /// <summary> </summary>
        InvalidPlatform = -32,
        /// <summary> </summary>
        InvalidDevice = -33,
        /// <summary> </summary>
        InvalidContext = -34,
        /// <summary> </summary>
        InvalidCommandQueueFlags = -35,
        /// <summary> </summary>
        InvalidCommandQueue = -36,
        /// <summary> </summary>
        InvalidHostPointer = -37,
        /// <summary> </summary>
        InvalidMemoryObject = -38,
        /// <summary> </summary>
        InvalidImageFormatDescriptor = -39,
        /// <summary> </summary>
        InvalidImageSize = -40,
        /// <summary> </summary>
        InvalidSampler = -41,
        /// <summary> </summary>
        InvalidBinary = -42,
        /// <summary> </summary>
        InvalidBuildOptions = -43,
        /// <summary> </summary>
        InvalidProgram = -44,
        /// <summary> </summary>
        InvalidProgramExecutable = -45,
        /// <summary> </summary>
        InvalidKernelName = -46,
        /// <summary> </summary>
        InvalidKernelDefinition = -47,
        /// <summary> </summary>
        InvalidKernel = -48,
        /// <summary> </summary>
        InvalidArgumentIndex = -49,
        /// <summary> </summary>
        InvalidArgumentValue = -50,
        /// <summary> </summary>
        InvalidArgumentSize = -51,
        /// <summary> </summary>
        InvalidKernelArguments = -52,
        /// <summary> </summary>
        InvalidWorkDimension = -53,
        /// <summary> </summary>
        InvalidWorkGroupSize = -54,
        /// <summary> </summary>
        InvalidWorkItemSize = -55,
        /// <summary> </summary>
        InvalidGlobalOffset = -56,
        /// <summary> </summary>
        InvalidEventWaitList = -57,
        /// <summary> </summary>
        InvalidEvent = -58,
        /// <summary> </summary>
        InvalidOperation = -59,
        /// <summary> </summary>
        InvalidGLObject = -60,
        /// <summary> </summary>
        InvalidBufferSize = -61,
        /// <summary> </summary>
        InvalidMipLevel = -62,
        /// <summary> </summary>
        InvalidGlobalWorkSize = -63,
        InvalidProperty = -64,
        InvalidImageDescriptor = -65,
        InvalidCompilerOptions = -66,
        InvalidLinkerOptions = -67,
        InvalidDevicePartitionCount = -68,
        /// Extensions
        InvalidGLShareGroupReferenceKHR = -1000,
        PlatformNotFoundKHR = -1001,
        PartitionFailedEXT = -1057,
        InvalidPartitionCountEXT = -1058,
        InvalidPartitionNameEXT = -1059
    }

    /// <summary>
    /// The OpenCL version.
    /// </summary>
    public enum OpenCLVersion : int
    {
        /// <summary> </summary>
        Version_1_0 = 1,
        /// <summary> </summary>
        Version_1_1 = 1,
        /// <summary> </summary>
        Version_1_2 = 1
    }

    /// <summary>
    /// The OpenCL boolean values expressed as integers.
    /// </summary>
    public enum OpenCLBoolean : int
    {
        /// <summary> </summary>
        False = 0,
        /// <summary> </summary>
        True = 1,
        Blocking = 1,
        NonBlocking = 0
    }

    /// <summary>
    /// The platform info query symbols.
    /// </summary>
    public enum OpenCLPlatformInfo : int
    {
        /// <summary> </summary>
        Profile = 0x0900,
        /// <summary> </summary>
        Version = 0x0901,
        /// <summary> </summary>
        Name = 0x0902,
        /// <summary> </summary>
        Vendor = 0x0903,
        /// <summary> </summary>
        Extensions = 0x0904,
        /// <summary> </summary>
        ICDSuffixKHR = 0x0920
    }

    /// <summary>
    /// The types of devices.
    /// </summary>
    [Flags]
    public enum OpenCLDeviceType : long
    {
        /// <summary> </summary>
        Default = 1 << 0,
        /// <summary> </summary>
        Cpu = 1 << 1,
        /// <summary> </summary>
        Gpu = 1 << 2,
        /// <summary> </summary>
        Accelerator = 1 << 3,
        /// <summary> </summary>
        Custom = 1 << 4,
        /// <summary> </summary>
        All = 0xFFFFFFFF
    }

    /// <summary>
    /// The device info query symbols.
    /// </summary>
    public enum OpenCLDeviceInfo : int
    {
        /// <summary> </summary>
        Type = 0x1000,
        /// <summary> </summary>
        VendorId = 0x1001,
        /// <summary> </summary>
        MaxOpenCLUnits = 0x1002,
        /// <summary> </summary>
        MaxWorkItemDimensions = 0x1003,
        /// <summary> </summary>
        MaxWorkGroupSize = 0x1004,
        /// <summary> </summary>
        MaxWorkItemSizes = 0x1005,
        /// <summary> </summary>
        PreferredVectorWidthChar = 0x1006,
        /// <summary> </summary>
        PreferredVectorWidthShort = 0x1007,
        /// <summary> </summary>
        PreferredVectorWidthInt = 0x1008,
        /// <summary> </summary>
        PreferredVectorWidthLong = 0x1009,
        /// <summary> </summary>
        PreferredVectorWidthFloat = 0x100A,
        /// <summary> </summary>
        PreferredVectorWidthDouble = 0x100B,
        /// <summary> </summary>
        MaxClockFrequency = 0x100C,
        /// <summary> </summary>
        AddressBits = 0x100D,
        /// <summary> </summary>
        MaxReadImageArguments = 0x100E,
        /// <summary> </summary>
        MaxWriteImageArguments = 0x100F,
        /// <summary> </summary>
        MaxMemoryAllocationSize = 0x1010,
        /// <summary> </summary>
        Image2DMaxWidth = 0x1011,
        /// <summary> </summary>
        Image2DMaxHeight = 0x1012,
        /// <summary> </summary>
        Image3DMaxWidth = 0x1013,
        /// <summary> </summary>
        Image3DMaxHeight = 0x1014,
        /// <summary> </summary>
        Image3DMaxDepth = 0x1015,
        /// <summary> </summary>
        ImageSupport = 0x1016,
        /// <summary> </summary>
        MaxParameterSize = 0x1017,
        /// <summary> </summary>
        MaxSamplers = 0x1018,
        /// <summary> </summary>
        MemoryBaseAddressAlignment = 0x1019,
        /// <summary> </summary>
        MinDataTypeAlignmentSize = 0x101A,
        /// <summary> </summary>
        SingleFPConfig = 0x101B,
        /// <summary> </summary>
        GlobalMemoryCacheType = 0x101C,
        /// <summary> </summary>
        GlobalMemoryCachelineSize = 0x101D,
        /// <summary> </summary>
        GlobalMemoryCacheSize = 0x101E,
        /// <summary> </summary>
        GlobalMemorySize = 0x101F,
        /// <summary> </summary>
        MaxConstantBufferSize = 0x1020,
        /// <summary> </summary>
        MaxConstantArguments = 0x1021,
        /// <summary> </summary>
        LocalMemoryType = 0x1022,
        /// <summary> </summary>
        LocalMemorySize = 0x1023,
        /// <summary> </summary>
        ErrorCorrectionSupport = 0x1024,
        /// <summary> </summary>
        ProfilingTimerResolution = 0x1025,
        /// <summary> </summary>
        EndianLittle = 0x1026,
        /// <summary> </summary>
        Available = 0x1027,
        /// <summary> </summary>
        CompilerAvailable = 0x1028,
        /// <summary> </summary>
        ExecutionCapabilities = 0x1029,
        /// <summary> </summary>
        CommandQueueProperties = 0x102A,
        /// <summary> </summary>
        Name = 0x102B,
        /// <summary> </summary>
        Vendor = 0x102C,
        /// <summary> </summary>
        DriverVersion = 0x102D,
        /// <summary> </summary>
        Profile = 0x102E,
        /// <summary> </summary>
        Version = 0x102F,
        /// <summary> </summary>
        Extensions = 0x1030,
        /// <summary> </summary>
        Platform = 0x1031,
        /// <summary> </summary>
        DoubleFPConfig = 0x1032,
        /// <summary> </summary>
        HalfFPConfig = 0x1033,
        /// <summary> </summary>
        PreferredVectorWidthHalf = 0x1034,
        /// <summary> </summary>
        HostUnifiedMemory = 0x1035,
        /// <summary> </summary>
        NativeVectorWidthChar = 0x1036,
        /// <summary> </summary>
        NativeVectorWidthShort = 0x1037,
        /// <summary> </summary>
        NativeVectorWidthInt = 0x1038,
        /// <summary> </summary>
        NativeVectorWidthLong = 0x1039,
        /// <summary> </summary>
        NativeVectorWidthFloat = 0x103A,
        /// <summary> </summary>
        NativeVectorWidthDouble = 0x103B,
        /// <summary> </summary>
        NativeVectorWidthHalf = 0x103C,
        /// <summary> </summary>
        OpenCLCVersion = 0x103D,
        LinkerAvailable = 0x103E,
        BuiltInKernels = 0x103F,
        ImageMaxBufferSize = 0x1040,
        ImageMaxArraySize = 0x1041,
        ParentDevice = 0x1042,
        PartitionMaxSubDevices = 0x1043,
        PartitionProperties = 0x1044,
        PartitionAffinity = 0x1045,
        PartitionType = 0x1046,
        ReferenceCount = 0x1047,
        PreferredInteropUserSync = 0x1048,
        PrintfBufferSize = 0x1049,
        ImagePitchAlignment = 0x104A,
        ImageBaseAddressAlignment = 0x104B,
        // Extension
        ParentDeviceEXT = 0x4054,
        PartitionTypesEXT = 0x4055,
        AffinityDomainsEXT = 0x4056,
        ReferenceCountEXT = 0x4057,
        PartitionStyleEXT = 0x4058
    }

    /// <summary>
    /// </summary>
    [Flags]
    public enum OpenCLDeviceFPConfig : long
    {
        /// <summary> </summary>
        Denorm = 1 << 0,
        /// <summary> </summary>
        InfNan = 1 << 1,
        /// <summary> </summary>
        RoundToNearest = 1 << 2,
        /// <summary> </summary>
        RoundToZero = 1 << 3,
        /// <summary> </summary>
        RoundToInf = 1 << 4,
        /// <summary> </summary>
        Fma = 1 << 5,
        /// <summary> </summary>
        SoftFloat = 1 << 6,
        CorrectlyRoundedDivideSqrt = 1 << 7
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLDeviceMemoryCacheType : int
    {
        /// <summary> </summary>
        None = 0x0,
        /// <summary> </summary>
        ReadOnlyCache = 0x1,
        /// <summary> </summary>
        ReadWriteCache = 0x2,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLDeviceLocalMemoryType : int
    {
        /// <summary> </summary>
        Local = 0x1,
        /// <summary> </summary>
        Global = 0x2
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLDeviceExecutionCapabilities : int
    {
        /// <summary> </summary>
        OpenCLKernel = 1 << 0,
        /// <summary> </summary>
        NativeKernel = 1 << 1
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum OpenCLCommandQueueProperties : long
    {
        /// <summary> </summary>
        None = 0,
        /// <summary> </summary>
        OutOfOrderExecution = 1 << 0,
        /// <summary> </summary>
        Profiling = 1 << 1
    }

    /// <summary>
    /// The context info query symbols.
    /// </summary>
    public enum OpenCLContextInfo : int
    {
        /// <summary> </summary>
        ReferenceCount = 0x1080,
        /// <summary> </summary>
        Devices = 0x1081,
        /// <summary> </summary>
        Properties = 0x1082,
        /// <summary> </summary>
        NumDevices = 0x1083
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLContextProperties : int
    {
        /// <summary> </summary>
        Platform = 0x1084,
        /// <summary> </summary>
        InteropUserSync = 0x1085,
        /// <summary> </summary>
        CL_GL_CONTEXT_KHR = 0x2008,
        /// <summary> </summary>
        CL_EGL_DISPLAY_KHR = 0x2009,
        /// <summary> </summary>
        CL_GLX_DISPLAY_KHR = 0x200A,
        /// <summary> </summary>
        CL_WGL_HDC_KHR = 0x200B,
        /// <summary> </summary>
        CL_CGL_SHAREGROUP_KHR = 0x200C,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLDevicePartitionProperties : int
    {
        /// <summary> </summary>
        PartitionEqually = 0x1086,
        PartitionByCounts = 0x1087,
        PartitionByCountsListEnd = 0x0,
        PartitionByAffinityDomain = 0x1088,
        // Extension
        PartitionEquallyEXT = 0x4050,
        PartitionByCountsEXT = 0x4051,
        PartitionByNamesEXT = 0x4052,
        PartitionByAffinityDomainEXT = 0x4053
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLDeviceAffinityDomain : int
    {
        /// <summary> </summary>
        Numa = (1 << 0),
        L4Cache = (1 << 1),
        L3Cache = (1 << 2),
        L2Cache = (1 << 3),
        L1Cache = (1 << 4),
        NextPartitionable = (1 << 5),
        // Extensions
        NumaEXT = 0x10,
        L4CacheEXT = 0x4,
        L3CacheEXT = 0x3,
        L2CacheEXT = 0x2,
        L1CacheEXT = 0x1,
        NextPartitionableEXT = 0x100
    }

    /// <summary>
    /// The command queue info query symbols.
    /// </summary>
    public enum OpenCLCommandQueueInfo : int
    {
        /// <summary> </summary>
        Context = 0x1090,
        /// <summary> </summary>
        Device = 0x1091,
        /// <summary> </summary>
        ReferenceCount = 0x1092,
        /// <summary> </summary>
        Properties = 0x1093
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum OpenCLMemoryFlags : long
    {
        /// <summary> Let the OpenCL choose the default flags. </summary>
        None = 0,
        /// <summary> The <see cref="OpenCLMemory"/> will be accessible from the <see cref="OpenCLKernel"/> for read and write operations. </summary>
        ReadWrite = 1 << 0,
        /// <summary> The <see cref="OpenCLMemory"/> will be accessible from the <see cref="OpenCLKernel"/> for write operations only. </summary>
        WriteOnly = 1 << 1,
        /// <summary> The <see cref="OpenCLMemory"/> will be accessible from the <see cref="OpenCLKernel"/> for read operations only. </summary>
        ReadOnly = 1 << 2,
        /// <summary> </summary>
        UseHostPointer = 1 << 3,
        /// <summary> </summary>
        AllocateHostPointer = 1 << 4,
        /// <summary> </summary>
        CopyHostPointer = 1 << 5,
        /// <summary> </summary>
        UsePersistentMemAMD = 1 << 6,
        /// <summary> </summary>
        HostWriteOnly = 1 << 7,
        /// <summary> </summary>
        HostReadOnly = 1 << 8,
        /// <summary> </summary>
        HostNoAccess = 1 << 9
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLImageChannelOrder : int
    {
        /// <summary> </summary>
        R = 0x10B0,
        /// <summary> </summary>
        A = 0x10B1,
        /// <summary> </summary>
        RG = 0x10B2,
        /// <summary> </summary>
        RA = 0x10B3,
        /// <summary> </summary>
        Rgb = 0x10B4,
        /// <summary> </summary>
        Rgba = 0x10B5,
        /// <summary> </summary>
        Bgra = 0x10B6,
        /// <summary> </summary>
        Argb = 0x10B7,
        /// <summary> </summary>
        Intensity = 0x10B8,
        /// <summary> </summary>
        Luminance = 0x10B9,
        /// <summary> </summary>
        Rx = 0x10BA,
        /// <summary> </summary>
        Rgx = 0x10BB,
        /// <summary> </summary>
        Rgbx = 0x10BC,
        /// <summary> </summary>
        Depth = 0x10BD,
        /// <summary> </summary>
        DepthStencil = 0x10BE
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLImageChannelType : int
    {
        /// <summary> </summary>
        SNormInt8 = 0x10D0,
        /// <summary> </summary>
        SNormInt16 = 0x10D1,
        /// <summary> </summary>
        UNormInt8 = 0x10D2,
        /// <summary> </summary>
        UNormInt16 = 0x10D3,
        /// <summary> </summary>
        UNormShort565 = 0x10D4,
        /// <summary> </summary>
        UNormShort555 = 0x10D5,
        /// <summary> </summary>
        UNormInt101010 = 0x10D6,
        /// <summary> </summary>
        SignedInt8 = 0x10D7,
        /// <summary> </summary>
        SignedInt16 = 0x10D8,
        /// <summary> </summary>
        SignedInt32 = 0x10D9,
        /// <summary> </summary>
        UnsignedInt8 = 0x10DA,
        /// <summary> </summary>
        UnsignedInt16 = 0x10DB,
        /// <summary> </summary>
        UnsignedInt32 = 0x10DC,
        /// <summary> </summary>
        HalfFloat = 0x10DD,
        /// <summary> </summary>
        Float = 0x10DE,
        UNormInt24 = 0x10DF
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLMemoryType : int
    {
        /// <summary> </summary>
        Buffer = 0x10F0,
        /// <summary> </summary>
        Image2D = 0x10F1,
        /// <summary> </summary>
        Image3D = 0x10F2,
        Image2DArray = 0x10F3,
        /// <summary> </summary>
        Image1D = 0x10F4,
        /// <summary> </summary>
        Image1DArray = 0x10F5,
        /// <summary> </summary>
        Image1DBuffer = 0x10F6
    }

    /// <summary>
    /// The memory info query symbols.
    /// </summary>
    public enum OpenCLMemoryInfo : int
    {
        /// <summary> </summary>
        Type = 0x1100,
        /// <summary> </summary>
        Flags = 0x1101,
        /// <summary> </summary>
        Size = 0x1102,
        /// <summary> </summary>
        HostPointer = 0x1103,
        /// <summary> </summary>
        MapppingCount = 0x1104,
        /// <summary> </summary>
        ReferenceCount = 0x1105,
        /// <summary> </summary>
        Context = 0x1106,
        /// <summary> </summary>
        AssociatedMemoryObject = 0x1107,
        /// <summary> </summary>
        Offset = 0x1108
    }

    /// <summary>
    /// The image info query symbols.
    /// </summary>
    public enum OpenCLImageInfo : int
    {
        /// <summary> </summary>
        Format = 0x1110,
        /// <summary> </summary>
        ElementSize = 0x1111,
        /// <summary> </summary>
        RowPitch = 0x1112,
        /// <summary> </summary>
        SlicePitch = 0x1113,
        /// <summary> </summary>
        Width = 0x1114,
        /// <summary> </summary>
        Height = 0x1115,
        /// <summary> </summary>
        Depth = 0x1116,
        ImageArraySize = 0x1117,
        ImageBuffer = 0x1118,
        ImageNumMipLevels = 0x1119,
        ImageNumSamples = 0x111A
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLImageAddressing : int
    {
        /// <summary> </summary>
        None = 0x1130,
        /// <summary> </summary>
        ClampToEdge = 0x1131,
        /// <summary> </summary>
        Clamp = 0x1132,
        /// <summary> </summary>
        Repeat = 0x1133,
        /// <summary> </summary>
        MirroredRepeat = 0x1134
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLImageFiltering : int
    {
        /// <summary> </summary>
        Nearest = 0x1140,
        /// <summary> </summary>
        Linear = 0x1141
    }

    /// <summary>
    /// The sampler info query symbols.
    /// </summary>
    public enum OpenCLSamplerInfo : int
    {
        /// <summary> </summary>
        ReferenceCount = 0x1150,
        /// <summary> </summary>
        Context = 0x1151,
        /// <summary> </summary>
        NormalizedCoords = 0x1152,
        /// <summary> </summary>
        Addressing = 0x1153,
        /// <summary> </summary>
        Filtering = 0x1154
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum OpenCLMemoryMappingFlags : long
    {
        /// <summary> </summary>
        Read = 1 << 0,
        /// <summary> </summary>
        Write = 1 << 1,
        WriteInvalidateRegion = 1 << 2
    }

    /// <summary>
    /// The program info query symbols.
    /// </summary>
    public enum OpenCLProgramInfo : int
    {
        /// <summary> </summary>
        ReferenceCount = 0x1160,
        /// <summary> </summary>
        Context = 0x1161,
        /// <summary> </summary>
        DeviceCount = 0x1162,
        /// <summary> </summary>
        Devices = 0x1163,
        /// <summary> </summary>
        Source = 0x1164,
        /// <summary> </summary>
        BinarySizes = 0x1165,
        /// <summary> </summary>
        Binaries = 0x1166,
        /// <summary> </summary>
        NumKernels = 0x1167,
        /// <summary> </summary>
        KernelNames = 0x1168
    }

    /// <summary>
    /// The program build info query symbols.
    /// </summary>
    public enum OpenCLProgramBuildInfo : int
    {
        /// <summary> </summary>
        Status = 0x1181,
        /// <summary> </summary>
        Options = 0x1182,
        /// <summary> </summary>
        BuildLog = 0x1183,
        /// <summary> </summary>
        BuildBinaryType = 0x1184
    }
    
    /// <summary>
    /// The program binary type.
    /// </summary>
    public enum OpenCLProgramBinaryType : int
    {
        /// <summary> </summary>
        None = 0x0,
        /// <summary> </summary>
        CompiledObject = 0x1,
        /// <summary> </summary>
        Library = 0x2,
        /// <summary> </summary>
        Executable = 0x4
    }
    
    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLProgramBuildStatus : int
    {
        /// <summary> </summary>
        Success = 0,
        /// <summary> </summary>
        None = -1,
        /// <summary> </summary>
        Error = -2,
        /// <summary> </summary>
        InProgress = -3
    }

    /// <summary>
    /// The kernel info query symbols.
    /// </summary>
    public enum OpenCLKernelInfo : int
    {
        /// <summary> </summary>
        FunctionName = 0x1190,
        /// <summary> </summary>
        ArgumentCount = 0x1191,
        /// <summary> </summary>
        ReferenceCount = 0x1192,
        /// <summary> </summary>
        Context = 0x1193,
        /// <summary> </summary>
        Program = 0x1194,
        /// <summary> </summary>
        Attributes = 0x1195
    }

    public enum OpenCLKernelArgInfo : int 
    {
        AddressQualifier = 0x1196,
        AccessQualifier = 0x1197,
        TypeName = 0x1198,
        TypeQualifier = 0x1199,
        ArgName = 0x119A
    }
    
    public enum OpenCLKernelArgAddressQualifier: int 
    {
        Global = 0x119B,
        Local = 0x119C,
        Constant = 0x119D,
        Private = 0x119E
    }
    
    public enum OpenCLKernelArgAccessQualifier: int 
    {
        ReadOnly = 0x11A0,
        WriteOnly = 0x11A1,
        ReadWrite = 0x11A2,
        None = 0x11A3
    }
    
    public enum OpenCLKernelArgTypeQualifier: int 
    {
        None = 0x0,
        Const = (1 << 0),
        Restrict = (1 << 1),
        Volatile = (1 << 2)
    }

    /// <summary>
    /// The kernel work-group info query symbols.
    /// </summary>
    public enum OpenCLKernelWorkGroupInfo : int
    {
        /// <summary> </summary>
        WorkGroupSize = 0x11B0,
        /// <summary> </summary>
        CompileWorkGroupSize = 0x11B1,
        /// <summary> </summary>
        LocalMemorySize = 0x11B2,
        /// <summary> </summary>
        PreferredWorkGroupSizeMultiple = 0x11B3,
        /// <summary> </summary>
        PrivateMemorySize = 0x11B4,
        /// <summary> </summary>
        GlobalWorkSize = 0x11B5
    }

    /// <summary>
    /// The event info query symbols.
    /// </summary>
    public enum OpenCLEventInfo : int
    {
        /// <summary> </summary>
        CommandQueue = 0x11D0,
        /// <summary> </summary>
        CommandType = 0x11D1,
        /// <summary> </summary>
        ReferenceCount = 0x11D2,
        /// <summary> </summary>
        ExecutionStatus = 0x11D3,
        /// <summary> </summary>
        Context = 0x11D4
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLCommandType : int
    {
        /// <summary> </summary>
        NDRangeKernel = 0x11F0,
        /// <summary> </summary>
        Task = 0x11F1,
        /// <summary> </summary>
        NativeKernel = 0x11F2,
        /// <summary> </summary>
        ReadBuffer = 0x11F3,
        /// <summary> </summary>
        WriteBuffer = 0x11F4,
        /// <summary> </summary>
        CopyBuffer = 0x11F5,
        /// <summary> </summary>
        ReadImage = 0x11F6,
        /// <summary> </summary>
        WriteImage = 0x11F7,
        /// <summary> </summary>
        CopyImage = 0x11F8,
        /// <summary> </summary>
        CopyImageToBuffer = 0x11F9,
        /// <summary> </summary>
        CopyBufferToImage = 0x11FA,
        /// <summary> </summary>
        MapBuffer = 0x11FB,
        /// <summary> </summary>
        MapImage = 0x11FC,
        /// <summary> </summary>
        UnmapMemory = 0x11FD,
        /// <summary> </summary>
        Marker = 0x11FE,
        /// <summary> </summary>
        AcquireGLObjects = 0x11FF,
        /// <summary> </summary>
        ReleaseGLObjects = 0x1200,
        /// <summary> </summary>
        ReadBufferRectangle = 0x1201,
        /// <summary> </summary>
        WriteBufferRectangle = 0x1202,
        /// <summary> </summary>
        CopyBufferRectangle = 0x1203,
        /// <summary> </summary>
        User = 0x1204,
        /// <summary> </summary>
        Barrier = 0x1205,
        /// <summary> </summary>
        MigrateMemObjects = 0x1206,
        /// <summary> </summary>
        FillBuffer = 0x1207,
        /// <summary> </summary>
        FillImage = 0x1208
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLCommandExecutionStatus : int
    {
        /// <summary> </summary>
        Complete = 0x0,
        /// <summary> </summary>
        Running = 0x1,
        /// <summary> </summary>
        Submitted = 0x2,
        /// <summary> </summary>
        Queued = 0x3
    }

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLBufferCreateType : int
    {
        /// <summary> </summary>
        Region = 0x1220
    }

    /// <summary>
    /// The command profiling info query symbols.
    /// </summary>
    public enum OpenCLCommandProfilingInfo : int
    {
        /// <summary> </summary>
        Queued = 0x1280,
        /// <summary> </summary>
        Submitted = 0x1281,
        /// <summary> </summary>
        Started = 0x1282,
        /// <summary> </summary>
        Ended = 0x1283
    }

    /**************************************************************************************/
    // CL/GL Sharing API

    /// <summary>
    /// 
    /// </summary>
    public enum OpenCLGLObjectType : int
    {
        /// <summary> </summary>
        Buffer = 0x2000,
        /// <summary> </summary>
        Texture2D = 0x2001,
        /// <summary> </summary>
        Texture3D = 0x2002,
        /// <summary> </summary>
        Renderbuffer = 0x2003,
        /// <summary> </summary>
        Texture2DArray = 0x200E,
        /// <summary> </summary>
        Texture1D = 0x200F,
        /// <summary> </summary>
        Texture1DArray = 0x2010,
        /// <summary> </summary>
        TextureBuffer = 0x2011
    }

    /// <summary>
    /// The shared CL/GL image/texture info query symbols.
    /// </summary>
    public enum OpenCLGLTextureInfo : int
    {
        /// <summary> </summary>
        TextureTarget = 0x2004,
        /// <summary> </summary>
        MipMapLevel = 0x2005,
        /// <summary> </summary>
        NumSamples = 0x2012
    }

    /// <summary>
    /// The shared CL/GL context info query symbols.
    /// </summary>
    public enum OpenCLGLContextInfo : int
    {
        /// <summary> </summary>
        CL_CURRENT_DEVICE_FOR_GL_CONTEXT_KHR = 0x2006,
        /// <summary> </summary>
        CL_DEVICES_FOR_GL_CONTEXT_KHR = 0x2007
    }

    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum cl_mem_migration_flags_ext
    {
        /// <summary> </summary>
        None = 0,
        /// <summary> </summary>
        CL_MIGRATE_MEM_OBJECT_HOST_EXT = 0x1,
    }

    internal enum CLFunctionNames
    {
        Unknown,
        GetPlatformIDs,
        GetPlatformInfo,
        GetDeviceIDs,
        GetDeviceInfo,
        CreateContext,
        CreateContextFromType,
        RetainContext,
        ReleaseContext,
        GetContextInfo,
        CreateCommandQueue,
        RetainCommandQueue,
        ReleaseCommandQueue,
        GetCommandQueueInfo,
        SetCommandQueueProperty,
        CreateBuffer,
        CreateImage2D,
        CreateImage3D,
        RetainMemObject,
        ReleaseMemObject,
        GetSupportedImageFormats,
        GetMemObjectInfo,
        GetImageInfo,
        CreateSampler,
        RetainSampler,
        ReleaseSampler,
        GetSamplerInfo,
        CreateProgramWithSource,
        CreateProgramWithBinary,
        RetainProgram,
        ReleaseProgram,
        BuildProgram,
        UnloadCompiler,
        GetProgramInfo,
        GetProgramBuildInfo,
        CreateKernel,
        CreateKernelsInProgram,
        RetainKernel,
        ReleaseKernel,
        SetKernelArg,
        GetKernelInfo,
        GetKernelWorkGroupInfo,
        WaitForEvents,
        GetEventInfo,
        RetainEvent,
        ReleaseEvent,
        GetEventProfilingInfo,
        Flush,
        Finish,
        EnqueueReadBuffer,
        EnqueueWriteBuffer,
        EnqueueCopyBuffer,
        EnqueueReadImage,
        EnqueueWriteImage,
        EnqueueCopyImage,
        EnqueueCopyImageToBuffer,
        EnqueueCopyBufferToImage,
        EnqueueMapBuffer,
        EnqueueMapImage,
        EnqueueUnmapMemObject,
        EnqueueNDRangeKernel,
        EnqueueTask,
        EnqueueNativeKernel,
        EnqueueMarker,
        EnqueueWaitForEvents,
        EnqueueBarrier,
        GetExtensionFunctionAddress,
        CreateFromGLBuffer,
        CreateFromGLTexture2D,
        CreateFromGLTexture3D,
        CreateFromGLRenderbuffer,
        GetGLObjectInfo,
        GetGLTextureInfo,
        EnqueueAcquireGLObjects,
        EnqueueReleaseGLObjects,
    }
}