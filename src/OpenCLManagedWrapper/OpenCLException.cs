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
    using System.Diagnostics;

    /// <summary>
    /// Represents an error state that occurred while executing an OpenCL API call.
    /// </summary>
    /// <seealso cref="OpenCLErrorCode"/>
    public class OpenCLException : ApplicationException
    {
        #region Fields

        
        private readonly OpenCLErrorCode code;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="OpenCLErrorCode"/> of the <see cref="OpenCLException"/>.
        /// </summary>
        public OpenCLErrorCode OpenCLErrorCode { get { return code; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLException"/> with a specified <see cref="OpenCLErrorCode"/>.
        /// </summary>
        /// <param name="code"> A <see cref="OpenCLErrorCode"/>. </param>
        public OpenCLException(OpenCLErrorCode code)
            : base("OpenCL error code detected: " + code.ToString() + ".")
        {
            this.code = code;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Checks for an OpenCL error code and throws a <see cref="OpenCLException"/> if such is encountered.
        /// </summary>
        /// <param name="errorCode"> The value to be checked for an OpenCL error. </param>
        public static void ThrowOnError(int errorCode)
        {
            ThrowOnError((OpenCLErrorCode)errorCode);
        }

        /// <summary>
        /// Checks for an OpenCL error code and throws a <see cref="OpenCLException"/> if such is encountered.
        /// </summary>
        /// <param name="errorCode"> The OpenCL error code. </param>
        public static void ThrowOnError(OpenCLErrorCode errorCode)
        {
            switch (errorCode)
            {
                case OpenCLErrorCode.Success:
                    return;

                case OpenCLErrorCode.DeviceNotFound:
                    throw new DeviceNotFoundOpenCLException();

                case OpenCLErrorCode.DeviceNotAvailable:
                    throw new DeviceNotAvailableOpenCLException();

                case OpenCLErrorCode.CompilerNotAvailable:
                    throw new CompilerNotAvailableOpenCLException();

                case OpenCLErrorCode.MemoryObjectAllocationFailure:
                    throw new MemoryObjectAllocationFailureOpenCLException();

                case OpenCLErrorCode.OutOfResources:
                    throw new OutOfResourcesOpenCLException();

                case OpenCLErrorCode.OutOfHostMemory:
                    throw new OutOfHostMemoryOpenCLException();

                case OpenCLErrorCode.ProfilingInfoNotAvailable:
                    throw new ProfilingInfoNotAvailableOpenCLException();

                case OpenCLErrorCode.MemoryCopyOverlap:
                    throw new MemoryCopyOverlapOpenCLException();

                case OpenCLErrorCode.ImageFormatMismatch:
                    throw new ImageFormatMismatchOpenCLException();

                case OpenCLErrorCode.ImageFormatNotSupported:
                    throw new ImageFormatNotSupportedOpenCLException();

                case OpenCLErrorCode.BuildProgramFailure:
                    throw new BuildProgramFailureOpenCLException();

                case OpenCLErrorCode.MapFailure:
                    throw new MapFailureOpenCLException();

                case OpenCLErrorCode.InvalidValue:
                    throw new InvalidValueOpenCLException();

                case OpenCLErrorCode.InvalidDeviceType:
                    throw new InvalidDeviceTypeOpenCLException();

                case OpenCLErrorCode.InvalidPlatform:
                    throw new InvalidPlatformOpenCLException();

                case OpenCLErrorCode.InvalidDevice:
                    throw new InvalidDeviceOpenCLException();

                case OpenCLErrorCode.InvalidContext:
                    throw new InvalidContextOpenCLException();

                case OpenCLErrorCode.InvalidCommandQueueFlags:
                    throw new InvalidCommandQueueFlagsOpenCLException();

                case OpenCLErrorCode.InvalidCommandQueue:
                    throw new InvalidCommandQueueOpenCLException();

                case OpenCLErrorCode.InvalidHostPointer:
                    throw new InvalidHostPointerOpenCLException();

                case OpenCLErrorCode.InvalidMemoryObject:
                    throw new InvalidMemoryObjectOpenCLException();

                case OpenCLErrorCode.InvalidImageFormatDescriptor:
                    throw new InvalidImageFormatDescriptorOpenCLException();

                case OpenCLErrorCode.InvalidImageSize:
                    throw new InvalidImageSizeOpenCLException();

                case OpenCLErrorCode.InvalidSampler:
                    throw new InvalidSamplerOpenCLException();

                case OpenCLErrorCode.InvalidBinary:
                    throw new InvalidBinaryOpenCLException();

                case OpenCLErrorCode.InvalidBuildOptions:
                    throw new InvalidBuildOptionsOpenCLException();

                case OpenCLErrorCode.InvalidProgram:
                    throw new InvalidProgramOpenCLException();

                case OpenCLErrorCode.InvalidProgramExecutable:
                    throw new InvalidProgramExecutableOpenCLException();

                case OpenCLErrorCode.InvalidKernelName:
                    throw new InvalidKernelNameOpenCLException();

                case OpenCLErrorCode.InvalidKernelDefinition:
                    throw new InvalidKernelDefinitionOpenCLException();

                case OpenCLErrorCode.InvalidKernel:
                    throw new InvalidKernelOpenCLException();

                case OpenCLErrorCode.InvalidArgumentIndex:
                    throw new InvalidArgumentIndexOpenCLException();

                case OpenCLErrorCode.InvalidArgumentValue:
                    throw new InvalidArgumentValueOpenCLException();

                case OpenCLErrorCode.InvalidArgumentSize:
                    throw new InvalidArgumentSizeOpenCLException();

                case OpenCLErrorCode.InvalidKernelArguments:
                    throw new InvalidKernelArgumentsOpenCLException();

                case OpenCLErrorCode.InvalidWorkDimension:
                    throw new InvalidWorkDimensionsOpenCLException();

                case OpenCLErrorCode.InvalidWorkGroupSize:
                    throw new InvalidWorkGroupSizeOpenCLException();

                case OpenCLErrorCode.InvalidWorkItemSize:
                    throw new InvalidWorkItemSizeOpenCLException();

                case OpenCLErrorCode.InvalidGlobalOffset:
                    throw new InvalidGlobalOffsetOpenCLException();

                case OpenCLErrorCode.InvalidEventWaitList:
                    throw new InvalidEventWaitListOpenCLException();

                case OpenCLErrorCode.InvalidEvent:
                    throw new InvalidEventOpenCLException();

                case OpenCLErrorCode.InvalidOperation:
                    throw new InvalidOperationOpenCLException();

                case OpenCLErrorCode.InvalidGLObject:
                    throw new InvalidGLObjectOpenCLException();

                case OpenCLErrorCode.InvalidBufferSize:
                    throw new InvalidBufferSizeOpenCLException();

                case OpenCLErrorCode.InvalidMipLevel:
                    throw new InvalidMipLevelOpenCLException();

                default:
                    throw new OpenCLException(errorCode);
            }
        }

        #endregion
    }

    #region Exception classes

    // Disable CS1591 warnings (missing XML comment for publicly visible type or member).
    #pragma warning disable 1591

    public class DeviceNotFoundOpenCLException : OpenCLException
    { public DeviceNotFoundOpenCLException() : base(OpenCLErrorCode.DeviceNotFound) { } }

    public class DeviceNotAvailableOpenCLException : OpenCLException
    { public DeviceNotAvailableOpenCLException() : base(OpenCLErrorCode.DeviceNotAvailable) { } }

    public class CompilerNotAvailableOpenCLException : OpenCLException
    { public CompilerNotAvailableOpenCLException() : base(OpenCLErrorCode.CompilerNotAvailable) { } }

    public class MemoryObjectAllocationFailureOpenCLException : OpenCLException
    { public MemoryObjectAllocationFailureOpenCLException() : base(OpenCLErrorCode.MemoryObjectAllocationFailure) { } }

    public class OutOfResourcesOpenCLException : OpenCLException
    { public OutOfResourcesOpenCLException() : base(OpenCLErrorCode.OutOfResources) { } }

    public class OutOfHostMemoryOpenCLException : OpenCLException
    { public OutOfHostMemoryOpenCLException() : base(OpenCLErrorCode.OutOfHostMemory) { } }

    public class ProfilingInfoNotAvailableOpenCLException : OpenCLException
    { public ProfilingInfoNotAvailableOpenCLException() : base(OpenCLErrorCode.ProfilingInfoNotAvailable) { } }

    public class MemoryCopyOverlapOpenCLException : OpenCLException
    { public MemoryCopyOverlapOpenCLException() : base(OpenCLErrorCode.MemoryCopyOverlap) { } }

    public class ImageFormatMismatchOpenCLException : OpenCLException
    { public ImageFormatMismatchOpenCLException() : base(OpenCLErrorCode.ImageFormatMismatch) { } }

    public class ImageFormatNotSupportedOpenCLException : OpenCLException
    { public ImageFormatNotSupportedOpenCLException() : base(OpenCLErrorCode.ImageFormatNotSupported) { } }

    public class BuildProgramFailureOpenCLException : OpenCLException
    { public BuildProgramFailureOpenCLException() : base(OpenCLErrorCode.BuildProgramFailure) { } }

    public class MapFailureOpenCLException : OpenCLException
    { public MapFailureOpenCLException() : base(OpenCLErrorCode.MapFailure) { } }

    public class InvalidValueOpenCLException : OpenCLException
    { public InvalidValueOpenCLException() : base(OpenCLErrorCode.InvalidValue) { } }

    public class InvalidDeviceTypeOpenCLException : OpenCLException
    { public InvalidDeviceTypeOpenCLException() : base(OpenCLErrorCode.InvalidDeviceType) { } }

    public class InvalidPlatformOpenCLException : OpenCLException
    { public InvalidPlatformOpenCLException() : base(OpenCLErrorCode.InvalidPlatform) { } }

    public class InvalidDeviceOpenCLException : OpenCLException
    { public InvalidDeviceOpenCLException() : base(OpenCLErrorCode.InvalidDevice) { } }

    public class InvalidContextOpenCLException : OpenCLException
    { public InvalidContextOpenCLException() : base(OpenCLErrorCode.InvalidContext) { } }

    public class InvalidCommandQueueFlagsOpenCLException : OpenCLException
    { public InvalidCommandQueueFlagsOpenCLException() : base(OpenCLErrorCode.InvalidCommandQueueFlags) { } }

    public class InvalidCommandQueueOpenCLException : OpenCLException
    { public InvalidCommandQueueOpenCLException() : base(OpenCLErrorCode.InvalidCommandQueue) { } }

    public class InvalidHostPointerOpenCLException : OpenCLException
    { public InvalidHostPointerOpenCLException() : base(OpenCLErrorCode.InvalidHostPointer) { } }

    public class InvalidMemoryObjectOpenCLException : OpenCLException
    { public InvalidMemoryObjectOpenCLException() : base(OpenCLErrorCode.InvalidMemoryObject) { } }

    public class InvalidImageFormatDescriptorOpenCLException : OpenCLException
    { public InvalidImageFormatDescriptorOpenCLException() : base(OpenCLErrorCode.InvalidImageFormatDescriptor) { } }

    public class InvalidImageSizeOpenCLException : OpenCLException
    { public InvalidImageSizeOpenCLException() : base(OpenCLErrorCode.InvalidImageSize) { } }

    public class InvalidSamplerOpenCLException : OpenCLException
    { public InvalidSamplerOpenCLException() : base(OpenCLErrorCode.InvalidSampler) { } }

    public class InvalidBinaryOpenCLException : OpenCLException
    { public InvalidBinaryOpenCLException() : base(OpenCLErrorCode.InvalidBinary) { } }

    public class InvalidBuildOptionsOpenCLException : OpenCLException
    { public InvalidBuildOptionsOpenCLException() : base(OpenCLErrorCode.InvalidBuildOptions) { } }

    public class InvalidProgramOpenCLException : OpenCLException
    { public InvalidProgramOpenCLException() : base(OpenCLErrorCode.InvalidProgram) { } }

    public class InvalidProgramExecutableOpenCLException : OpenCLException
    { public InvalidProgramExecutableOpenCLException() : base(OpenCLErrorCode.InvalidProgramExecutable) { } }

    public class InvalidKernelNameOpenCLException : OpenCLException
    { public InvalidKernelNameOpenCLException() : base(OpenCLErrorCode.InvalidKernelName) { } }

    public class InvalidKernelDefinitionOpenCLException : OpenCLException
    { public InvalidKernelDefinitionOpenCLException() : base(OpenCLErrorCode.InvalidKernelDefinition) { } }

    public class InvalidKernelOpenCLException : OpenCLException
    { public InvalidKernelOpenCLException() : base(OpenCLErrorCode.InvalidKernel) { } }

    public class InvalidArgumentIndexOpenCLException : OpenCLException
    { public InvalidArgumentIndexOpenCLException() : base(OpenCLErrorCode.InvalidArgumentIndex) { } }

    public class InvalidArgumentValueOpenCLException : OpenCLException
    { public InvalidArgumentValueOpenCLException() : base(OpenCLErrorCode.InvalidArgumentValue) { } }

    public class InvalidArgumentSizeOpenCLException : OpenCLException
    { public InvalidArgumentSizeOpenCLException() : base(OpenCLErrorCode.InvalidArgumentSize) { } }

    public class InvalidKernelArgumentsOpenCLException : OpenCLException
    { public InvalidKernelArgumentsOpenCLException() : base(OpenCLErrorCode.InvalidKernelArguments) { } }

    public class InvalidWorkDimensionsOpenCLException : OpenCLException
    { public InvalidWorkDimensionsOpenCLException() : base(OpenCLErrorCode.InvalidWorkDimension) { } }

    public class InvalidWorkGroupSizeOpenCLException : OpenCLException
    { public InvalidWorkGroupSizeOpenCLException() : base(OpenCLErrorCode.InvalidWorkGroupSize) { } }

    public class InvalidWorkItemSizeOpenCLException : OpenCLException
    { public InvalidWorkItemSizeOpenCLException() : base(OpenCLErrorCode.InvalidWorkItemSize) { } }

    public class InvalidGlobalOffsetOpenCLException : OpenCLException
    { public InvalidGlobalOffsetOpenCLException() : base(OpenCLErrorCode.InvalidGlobalOffset) { } }

    public class InvalidEventWaitListOpenCLException : OpenCLException
    { public InvalidEventWaitListOpenCLException() : base(OpenCLErrorCode.InvalidEventWaitList) { } }

    public class InvalidEventOpenCLException : OpenCLException
    { public InvalidEventOpenCLException() : base(OpenCLErrorCode.InvalidEvent) { } }

    public class InvalidOperationOpenCLException : OpenCLException
    { public InvalidOperationOpenCLException() : base(OpenCLErrorCode.InvalidOperation) { } }

    public class InvalidGLObjectOpenCLException : OpenCLException
    { public InvalidGLObjectOpenCLException() : base(OpenCLErrorCode.InvalidGLObject) { } }

    public class InvalidBufferSizeOpenCLException : OpenCLException
    { public InvalidBufferSizeOpenCLException() : base(OpenCLErrorCode.InvalidBufferSize) { } }

    public class InvalidMipLevelOpenCLException : OpenCLException
    { public InvalidMipLevelOpenCLException() : base(OpenCLErrorCode.InvalidMipLevel) { } }

    #endregion
}