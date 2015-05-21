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
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using OpenCL.Bindings;
    using System.Linq;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an OpenCL device.
    /// </summary>
    /// <value> A device is a collection of compute units. A command queue is used to queue commands to a device. Examples of commands include executing kernels, or reading and writing memory objects. OpenCL devices typically correspond to a GPU, a multi-core CPU, and other processors such as DSPs and the Cell/B.E. processor. </value>
    /// <seealso cref="OpenCLCommandQueue"/>
    /// <seealso cref="OpenCLKernel"/>
    /// <seealso cref="OpenCLMemory"/>
    /// <seealso cref="OpenCLPlatform"/>
    public class OpenCLDevice : OpenCLObject
    {
        #region Properties

        /// <summary>
        /// The handle of the <see cref="OpenCLDevice"/>.
        /// </summary>
        public CLDeviceHandle Handle
        {
            get;
            protected set;
        }

        public long AddressBits
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.AddressBits);
            }
        }
        public bool Available
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.Available);
            }
        }
        public bool AffinityDomainExtension
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.AffinityDomainsEXT);
            }
        }
        public bool BuiltInKernels
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.BuiltInKernels);
            }
        }
        public OpenCLDeviceFPConfig DoubleFPConfig
        {
            get
            {
                return (OpenCLDeviceFPConfig)GetInfo<long>(OpenCLDeviceInfo.DoubleFPConfig);
            }
        }
        public OpenCLDeviceFPConfig SingleFPConfig
        {
            get
            {
                return (OpenCLDeviceFPConfig)GetInfo<long>(OpenCLDeviceInfo.SingleFPConfig);
            }
        }
        public OpenCLDeviceFPConfig HalfFPConfig
        {
            get
            {
                return (OpenCLDeviceFPConfig)GetInfo<long>(OpenCLDeviceInfo.HalfFPConfig);
            }
        }
        public string DriverVersion
        {
            get
            {
                return GetStringInfo(OpenCLDeviceInfo.DriverVersion);
            }
        }
        public bool ErrorCorrectionSupport
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.ErrorCorrectionSupport);
            }
        }
        public OpenCLDeviceExecutionCapabilities ExecutionCapabilities
        {
            get
            {
                return (OpenCLDeviceExecutionCapabilities)GetInfo<long>(OpenCLDeviceInfo.ExecutionCapabilities);
            }
        }
        public ReadOnlyCollection<string> Extensions
        {
            get
            {
                string extensionString = GetStringInfo(OpenCLDeviceInfo.Extensions);
                var extensions = new ReadOnlyCollection<string>(extensionString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

                return extensions;
            }
        }
        public long GlobalMemoryCachelineSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.GlobalMemoryCachelineSize);
            }
        }
        public long GlobalMemoryCacheSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.GlobalMemoryCacheSize);
            }
        }
        public OpenCLDeviceMemoryCacheType GlobalMemoryCacheType
        {
            get
            {
                return (OpenCLDeviceMemoryCacheType)GetInfo<long>(OpenCLDeviceInfo.GlobalMemoryCacheType);
            }
        }
        public long GlobalMemorySize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.GlobalMemorySize);
            }
        }
        public bool HostUnifiedMemory
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.HostUnifiedMemory);
            }
        }
        public long Image2DMaxHeight
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.Image2DMaxHeight);
            }
        }
        public long Image2DMaxWidth
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.Image2DMaxWidth);
            }
        }
        public long Image3DMaxDepth
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.Image3DMaxDepth);
            }
        }
        public long Image3DMaxHeight
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.Image3DMaxHeight);
            }
        }
        public long Image3DMaxWidth
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.Image3DMaxWidth);
            }
        }
        public long ImageBaseAddressAlignment
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.ImageBaseAddressAlignment);
            }
        }
        public long ImageMaxArraySize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.ImageMaxArraySize);
            }
        }
        public long ImageMaxBufferSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.ImageMaxBufferSize);
            }
        }
        public long ImagePitchAlignment
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.ImagePitchAlignment);
            }
        }
        public bool ImageSupport
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.ImageSupport);
            }
        }
        public bool LinkerAvailable
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.LinkerAvailable);
            }
        }
        public long LocalMemorySize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.LocalMemorySize);
            }
        }
        public OpenCLDeviceLocalMemoryType LocalMemoryType
        {
            get
            {
                return (OpenCLDeviceLocalMemoryType)GetInfo<long>(OpenCLDeviceInfo.LocalMemoryType);
            }
        }
        public long MaxClockFrequency
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxClockFrequency);
            }
        }
        public long MaxConstantArguments
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxConstantArguments);
            }
        }
        public long MaxConstantBufferSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxConstantBufferSize);
            }
        }
        public long MaxMemoryAllocationSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxMemoryAllocationSize);
            }
        }
        public long MaxOpenCLUnits
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxOpenCLUnits);
            }
        }
        public long MaxParameterSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxParameterSize);
            }
        }
        public long MaxReadImageArguments
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxReadImageArguments);
            }
        }
        public long MaxSamplers
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxSamplers);
            }
        }
        public long MaxWorkGroupSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxWorkGroupSize);
            }
        }
        public long MaxWorkItemDimensions
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxWorkItemDimensions);
            }
        }
        public ReadOnlyCollection<long> MaxWorkItemSizes
        {
            get
            {
                return new ReadOnlyCollection<long>(
                    OpenCLTools.ConvertArray(GetArrayInfo<CLDeviceHandle, OpenCLDeviceInfo, IntPtr>(Handle, OpenCLDeviceInfo.MaxWorkItemSizes, CL10.GetDeviceInfo)));
            }
        }
        public long MaxWriteImageArguments
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MaxWriteImageArguments);
            }
        }
        public long MemoryBaseAddressAlignment
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MemoryBaseAddressAlignment);
            }
        }
        public long MinDataTypeAlignmentSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.MinDataTypeAlignmentSize);
            }
        }
        public string Name
        {
            get
            {
                return GetStringInfo(OpenCLDeviceInfo.Name);
            }
        }
        public long NativeVectorWidthChar
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.NativeVectorWidthChar);
            }
        }
        public long NativeVectorWidthDouble
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.NativeVectorWidthDouble);
            }
        }
        public long NativeVectorWidthFloat
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.NativeVectorWidthFloat);
            }
        }
        public long NativeVectorWidthHalf
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.NativeVectorWidthHalf);
            }
        }
        public long NativeVectorWidthInt
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.NativeVectorWidthInt);
            }
        }
        public long NativeVectorWidthLong
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.NativeVectorWidthLong);
            }
        }
        public long NativeVectorWidthShort
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.NativeVectorWidthShort);
            }
        }
        public string OpenCLVersionString
        {
            get
            {
                return GetStringInfo(OpenCLDeviceInfo.OpenCLCVersion);
            }
        }

        public Version OpenCLVersion
        {
            get
            {
                return OpenCLTools.ParseVersionString(this.OpenCLVersionString, 1);
            }
        }

        public OpenCLDevice ParentDevice
        {
            get
            {
                return null;
            }
        }
        public OpenCLDevice ParentDeviceExtension
        {
            get
            {
                return null;
            }
        }
        public OpenCLDeviceAffinityDomain PartitionAffinityDomain
        {
            get
            {
                return GetInfo<OpenCLDeviceAffinityDomain>(OpenCLDeviceInfo.PartitionAffinity);
            }
        }
        public ReadOnlyCollection<OpenCLDevicePartitionProperties> PartitionProperties
        {
            get
            {
                var longs = OpenCLTools.ConvertArray(GetArrayInfo<CLDeviceHandle, OpenCLDeviceInfo, IntPtr>(Handle, OpenCLDeviceInfo.PartitionProperties, CL10.GetDeviceInfo));
                var r = longs.Select(s => (OpenCLDevicePartitionProperties)s).ToArray();
                return new ReadOnlyCollection<OpenCLDevicePartitionProperties>(r);
            }
        }
        public long PartitionStyleExtension
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PartitionStyleEXT);
            }
        }
        public ReadOnlyCollection<OpenCLDevicePartitionProperties> PartitionType
        {
            get
            {
                var longs = OpenCLTools.ConvertArray(GetArrayInfo<CLDeviceHandle, OpenCLDeviceInfo, IntPtr>(Handle, OpenCLDeviceInfo.PartitionType, CL10.GetDeviceInfo));
                var r = longs.Select(s => (OpenCLDevicePartitionProperties)s).ToArray();
                return new ReadOnlyCollection<OpenCLDevicePartitionProperties>(r);
            }
        }
        public ReadOnlyCollection<OpenCLDevicePartitionProperties> PartitionTypesExtension
        {
            get
            {
                var longs = OpenCLTools.ConvertArray(GetArrayInfo<CLDeviceHandle, OpenCLDeviceInfo, IntPtr>(Handle, OpenCLDeviceInfo.PartitionTypesEXT, CL10.GetDeviceInfo));
                var r = longs.Select(s => (OpenCLDevicePartitionProperties)s).ToArray();
                return new ReadOnlyCollection<OpenCLDevicePartitionProperties>(r);
            }
        }
        public OpenCLPlatform Platform
        {
            get;
            private set;
        }
        public bool PreferredInteropUserSync
        {
            get
            {
                return GetBoolInfo(OpenCLDeviceInfo.PreferredInteropUserSync);
            }
        }
        public long PreferredVectorWidthDouble
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PreferredVectorWidthDouble);
            }
        }
        public long PreferredVectorWidthFloat
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PreferredVectorWidthFloat);
            }
        }
        public long PreferredVectorWidthHalf
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PreferredVectorWidthHalf);
            }
        }
        public long PreferredVectorWidthChar
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PreferredVectorWidthChar);
            }
        }
        public long PreferredVectorWidthInt
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PreferredVectorWidthInt);
            }
        }
        public long PreferredVectorWidthLong
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PreferredVectorWidthLong);
            }
        }
        public long PreferredVectorWidthShort
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PreferredVectorWidthShort);
            }
        }
        public long PrintfBufferSize
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.PrintfBufferSize);
            }
        }
        public string Profile
        {
            get
            {
                return GetStringInfo(OpenCLDeviceInfo.Profile);
            }
        }
        public long ProfilingTimerResolution
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.ProfilingTimerResolution);
            }
        }
        public long ReferenceCount
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.ReferenceCount);
            }
        }
        public long ReferenceCountExtension
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.ReferenceCountEXT);
            }
        }
        public OpenCLDeviceType Type
        {
            get
            {
                return (OpenCLDeviceType)GetInfo<long>(OpenCLDeviceInfo.Type);
            }
        }
        public string Vendor
        {
            get
            {
                return GetStringInfo(OpenCLDeviceInfo.Vendor);
            }
        }
        public long VendorID
        {
            get
            {
                return GetInfo<long>(OpenCLDeviceInfo.VendorId);
            }
        }
        
        public Version Version 
        { 
            get 
            { 
                return OpenCLTools.ParseVersionString(VersionString, 1); 
            } 
        }

        public string VersionString 
        { 
            get 
            { 
                return GetStringInfo(OpenCLDeviceInfo.Version); 
            } 
        }

        #endregion

        #region Constructors

        internal OpenCLDevice(OpenCLPlatform platform, CLDeviceHandle handle)
        {
            Handle = handle;
            SetID(Handle.Value);

            this.Platform = platform;
        }

        #endregion

        #region Private methods

        private bool GetBoolInfo(OpenCLDeviceInfo paramName)
        {
            return GetBoolInfo<CLDeviceHandle, OpenCLDeviceInfo>(Handle, paramName, CL10.GetDeviceInfo);
        }

        private NativeType GetInfo<NativeType>(OpenCLDeviceInfo paramName) where NativeType : struct
        {
            return GetInfo<CLDeviceHandle, OpenCLDeviceInfo, NativeType>(Handle, paramName, CL10.GetDeviceInfo);
        }

        private string GetStringInfo(OpenCLDeviceInfo paramName)
        {
            return GetStringInfo<CLDeviceHandle, OpenCLDeviceInfo>(Handle, paramName, CL10.GetDeviceInfo);
        }

        #endregion
    }
}