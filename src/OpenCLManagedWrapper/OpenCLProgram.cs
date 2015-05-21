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
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Threading;
    using OpenCL.Bindings;

    /// <summary>
    /// Represents an OpenCL program.
    /// </summary>
    /// <remarks> An OpenCL program consists of a set of kernels. Programs may also contain auxiliary functions called by the kernel functions and constant data. </remarks>
    /// <seealso cref="OpenCLKernel"/>
    public class OpenCLProgram : OpenCLResource
    {
        #region Fields

        
        private readonly OpenCLContext context;

        
        private readonly ReadOnlyCollection<OpenCLDevice> devices;

        
        private readonly ReadOnlyCollection<string> source;

        
        private ReadOnlyCollection<byte[]> binaries;

        
        private string buildOptions;

        
        private OpenCLProgramBuildNotifier buildNotify;

        #endregion

        #region Properties

        /// <summary>
        /// The handle of the <see cref="OpenCLProgram"/>.
        /// </summary>
        public CLProgramHandle Handle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a read-only collection of program binaries associated with the <see cref="OpenCLProgram.Devices"/>.
        /// </summary>
        /// <value> A read-only collection of program binaries associated with the <see cref="OpenCLProgram.Devices"/>. </value>
        /// <remarks> The bits returned can be an implementation-specific intermediate representation (a.k.a. IR) or device specific executable bits or both. The decision on which information is returned in the binary is up to the OpenCL implementation. </remarks>
        public ReadOnlyCollection<byte[]> Binaries 
        { 
            get 
            {
                if (binaries == null)
                    binaries = GetBinaries();
                return binaries;
            }
        }

        /// <summary>
        /// Gets the <see cref="OpenCLProgram"/> build options as specified in <paramref name="options"/> argument of <see cref="OpenCLProgram.Build"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLProgram"/> build options as specified in <paramref name="options"/> argument of <see cref="OpenCLProgram.Build"/>. </value>
        public string BuildOptions { get { return buildOptions; } }

        /// <summary>
        /// Gets the <see cref="OpenCLContext"/> of the <see cref="OpenCLProgram"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLContext"/> of the <see cref="OpenCLProgram"/>. </value>
        public OpenCLContext Context { get { return context; } }

        /// <summary>
        /// Gets a read-only collection of <see cref="OpenCLDevice"/>s associated with the <see cref="OpenCLProgram"/>.
        /// </summary>
        /// <value> A read-only collection of <see cref="OpenCLDevice"/>s associated with the <see cref="OpenCLProgram"/>. </value>
        /// <remarks> This collection is a subset of <see cref="OpenCLProgram.Context.Devices"/>. </remarks>
        public ReadOnlyCollection<OpenCLDevice> Devices { get { return devices; } }

        /// <summary>
        /// Gets a read-only collection of program source code strings specified when creating the <see cref="OpenCLProgram"/> or <c>null</c> if <see cref="OpenCLProgram"/> was created using program binaries.
        /// </summary>
        /// <value> A read-only collection of program source code strings specified when creating the <see cref="OpenCLProgram"/> or <c>null</c> if <see cref="OpenCLProgram"/> was created using program binaries. </value>
        public ReadOnlyCollection<string> Source { get { return source; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLProgram"/> from a source code string.
        /// </summary>
        /// <param name="context"> A <see cref="OpenCLContext"/>. </param>
        /// <param name="source"> The source code for the <see cref="OpenCLProgram"/>. </param>
        /// <remarks> The created <see cref="OpenCLProgram"/> is associated with the <see cref="OpenCLContext.Devices"/>. </remarks>
        public OpenCLProgram(OpenCLContext context, string source)
        {
            OpenCLErrorCode error = OpenCLErrorCode.Success;
            Handle = CL10.CreateProgramWithSource(context.Handle, 1, new string[] { source }, null, out error);
            OpenCLException.ThrowOnError(error);

            SetID(Handle.Value);

            this.context = context;
            this.devices = context.Devices;
            this.source = new ReadOnlyCollection<string>(new string[] { source });

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
        }

        /// <summary>
        /// Creates a new <see cref="OpenCLProgram"/> from an array of source code strings.
        /// </summary>
        /// <param name="context"> A <see cref="OpenCLContext"/>. </param>
        /// <param name="source"> The source code lines for the <see cref="OpenCLProgram"/>. </param>
        /// <remarks> The created <see cref="OpenCLProgram"/> is associated with the <see cref="OpenCLContext.Devices"/>. </remarks>
        public OpenCLProgram(OpenCLContext context, string[] source)
        {
            OpenCLErrorCode error = OpenCLErrorCode.Success;
            Handle = CL10.CreateProgramWithSource(
                context.Handle,
                source.Length,
                source,
                null,
                out error);
            OpenCLException.ThrowOnError(error);

            this.context = context;
            this.devices = context.Devices;
            this.source = new ReadOnlyCollection<string>(source);

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
        }

        /// <summary>
        /// Creates a new <see cref="OpenCLProgram"/> from a specified list of binaries.
        /// </summary>
        /// <param name="context"> A <see cref="OpenCLContext"/>. </param>
        /// <param name="binaries"> A list of binaries, one for each item in <paramref name="devices"/>. </param>
        /// <param name="devices"> A subset of the <see cref="OpenCLContext.Devices"/>. If <paramref name="devices"/> is <c>null</c>, OpenCL will associate every binary from <see cref="OpenCLProgram.Binaries"/> with a corresponding <see cref="OpenCLDevice"/> from <see cref="OpenCLContext.Devices"/>. </param>
        public OpenCLProgram(OpenCLContext context, IList<byte[]> binaries, IList<OpenCLDevice> devices)
        {
            int count;

            CLDeviceHandle[] deviceHandles = (devices != null) ?
                OpenCLTools.ExtractHandles(devices, out count) :
                OpenCLTools.ExtractHandles(context.Devices, out count);

            IntPtr[] binariesPtrs = new IntPtr[count];
            IntPtr[] binariesLengths = new IntPtr[count];
            int[] binariesStats = new int[count];
            OpenCLErrorCode error = OpenCLErrorCode.Success;
            GCHandle[] binariesGCHandles = new GCHandle[count];

            try
            {
                for (int i = 0; i < count; i++)
                {
                    binariesGCHandles[i] = GCHandle.Alloc(binaries[i], GCHandleType.Pinned);
                    binariesPtrs[i] = binariesGCHandles[i].AddrOfPinnedObject();
                    binariesLengths[i] = new IntPtr(binaries[i].Length);
                }

                Handle = CL10.CreateProgramWithBinary(
                    context.Handle,
                    count,
                    deviceHandles,
                    binariesLengths,
                    binariesPtrs,
                    binariesStats,
                    out error);
                OpenCLException.ThrowOnError(error);
            }
            finally
            {
                for (int i = 0; i < count; i++)
                    binariesGCHandles[i].Free();
            }


            this.binaries = new ReadOnlyCollection<byte[]>(binaries);
            this.context = context;
            this.devices = new ReadOnlyCollection<OpenCLDevice>(
                (devices != null) ? devices : context.Devices);

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Builds (compiles and links) a program executable from the program source or binary for all or some of the <see cref="OpenCLProgram.Devices"/>.
        /// </summary>
        /// <param name="devices"> A subset or all of <see cref="OpenCLProgram.Devices"/>. If <paramref name="devices"/> is <c>null</c>, the executable is built for every item of <see cref="OpenCLProgram.Devices"/> for which a source or a binary has been loaded. </param>
        /// <param name="options"> A set of options for the OpenCL compiler. </param>
        /// <param name="notify"> A delegate instance that represents a reference to a notification routine. This routine is a callback function that an application can register and which will be called when the program executable has been built (successfully or unsuccessfully). If <paramref name="notify"/> is not <c>null</c>, <see cref="OpenCLProgram.Build"/> does not need to wait for the build to complete and can return immediately. If <paramref name="notify"/> is <c>null</c>, <see cref="OpenCLProgram.Build"/> does not return until the build has completed. The callback function may be called asynchronously by the OpenCL implementation. It is the application's responsibility to ensure that the callback function is thread-safe and that the delegate instance doesn't get collected by the Garbage Collector until the build operation triggers the callback. </param>
        /// <param name="notifyDataPtr"> Optional user data that will be passed to <paramref name="notify"/>. </param>
        public void Build(IList<OpenCLDevice> devices, string options, OpenCLProgramBuildNotifier notify, IntPtr notifyDataPtr)
        {
            int handleCount;
            CLDeviceHandle[] deviceHandles = OpenCLTools.ExtractHandles(devices, out handleCount);
            buildOptions = (options != null) ? options : "";
            buildNotify = notify;

            OpenCLErrorCode error = CL10.BuildProgram(Handle, handleCount, deviceHandles, options, buildNotify, notifyDataPtr);
            OpenCLException.ThrowOnError(error);
        }

        /// <summary>
        /// Creates a <see cref="OpenCLKernel"/> for every <c>kernel</c> function in <see cref="OpenCLProgram"/>.
        /// </summary>
        /// <returns> The collection of created <see cref="OpenCLKernel"/>s. </returns>
        /// <remarks> <see cref="OpenCLKernel"/>s are not created for any <c>kernel</c> functions in <see cref="OpenCLProgram"/> that do not have the same function definition across all <see cref="OpenCLProgram.Devices"/> for which a program executable has been successfully built. </remarks>
        public ICollection<OpenCLKernel> CreateAllKernels()
        {
            ICollection<OpenCLKernel> kernels = new Collection<OpenCLKernel>();
            int kernelsCount = 0;
            CLKernelHandle[] kernelHandles;

            OpenCLErrorCode error = CL10.CreateKernelsInProgram(Handle, 0, null, out kernelsCount);
            OpenCLException.ThrowOnError(error);

            kernelHandles = new CLKernelHandle[kernelsCount];
            error = CL10.CreateKernelsInProgram(Handle, kernelsCount, kernelHandles, out kernelsCount);
            OpenCLException.ThrowOnError(error);

            for (int i = 0; i < kernelsCount; i++)
                kernels.Add(new OpenCLKernel(kernelHandles[i], this));

            return kernels;
        }

        /// <summary>
        /// Creates a <see cref="OpenCLKernel"/> for a kernel function of a specified name.
        /// </summary>
        /// <returns> The created <see cref="OpenCLKernel"/>. </returns>
        public OpenCLKernel CreateKernel(string functionName)
        {
            return new OpenCLKernel(functionName, this);
        }

        /// <summary>
        /// Gets the build log of the <see cref="OpenCLProgram"/> for a specified <see cref="OpenCLDevice"/>.
        /// </summary>
        /// <param name="device"> The <see cref="OpenCLDevice"/> building the <see cref="OpenCLProgram"/>. Must be one of <see cref="OpenCLProgram.Devices"/>. </param>
        /// <returns> The build log of the <see cref="OpenCLProgram"/> for <paramref name="device"/>. </returns>
        public string GetBuildLog(OpenCLDevice device)
        {
            return GetStringInfo<CLProgramHandle, CLDeviceHandle, OpenCLProgramBuildInfo>(Handle, device.Handle, OpenCLProgramBuildInfo.BuildLog, CL10.GetProgramBuildInfo);
        }

        /// <summary>
        /// Gets the <see cref="OpenCLProgramBuildStatus"/> of the <see cref="OpenCLProgram"/> for a specified <see cref="OpenCLDevice"/>.
        /// </summary>
        /// <param name="device"> The <see cref="OpenCLDevice"/> building the <see cref="OpenCLProgram"/>. Must be one of <see cref="OpenCLProgram.Devices"/>. </param>
        /// <returns> The <see cref="OpenCLProgramBuildStatus"/> of the <see cref="OpenCLProgram"/> for <paramref name="device"/>. </returns>
        public OpenCLProgramBuildStatus GetBuildStatus(OpenCLDevice device)
        {
            return (OpenCLProgramBuildStatus)GetInfo<CLProgramHandle, CLDeviceHandle, OpenCLProgramBuildInfo, uint>(Handle, device.Handle, OpenCLProgramBuildInfo.Status, CL10.GetProgramBuildInfo);
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Releases the associated OpenCL object.
        /// </summary>
        /// <param name="manual"> Specifies the operation mode of this method. </param>
        /// <remarks> <paramref name="manual"/> must be <c>true</c> if this method is invoked directly by the application. </remarks>
        protected override void Dispose(bool manual)
        {
            if (Handle.IsValid)
            {
                //Trace.WriteLine("Dispose " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
                CL10.ReleaseProgram(Handle);
                Handle.Invalidate();
            }
        }

        #endregion

        #region Private methods

        private ReadOnlyCollection<byte[]> GetBinaries()
        {
            IntPtr[] binaryLengths = GetArrayInfo<CLProgramHandle, OpenCLProgramInfo, IntPtr>(Handle, OpenCLProgramInfo.BinarySizes, CL10.GetProgramInfo);

            GCHandle[] binariesGCHandles = new GCHandle[binaryLengths.Length];
            IntPtr[] binariesPtrs = new IntPtr[binaryLengths.Length];
            IList<byte[]> binaries = new List<byte[]>();
            GCHandle binariesPtrsGCHandle = GCHandle.Alloc(binariesPtrs, GCHandleType.Pinned);

            try
            {
                for (int i = 0; i < binaryLengths.Length; i++)
                {
                    byte[] binary = new byte[binaryLengths[i].ToInt64()];
                    binariesGCHandles[i] = GCHandle.Alloc(binary, GCHandleType.Pinned);
                    binariesPtrs[i] = binariesGCHandles[i].AddrOfPinnedObject();
                    binaries.Add(binary);
                }

                IntPtr sizeRet;
                OpenCLErrorCode error = CL10.GetProgramInfo(Handle, OpenCLProgramInfo.Binaries, new IntPtr(binariesPtrs.Length * IntPtr.Size), binariesPtrsGCHandle.AddrOfPinnedObject(), out sizeRet);
                OpenCLException.ThrowOnError(error);
            }
            finally
            {
                for (int i = 0; i < binaryLengths.Length; i++)
                    binariesGCHandles[i].Free();
                binariesPtrsGCHandle.Free();
            }

            return new ReadOnlyCollection<byte[]>(binaries);
        }

        #endregion
    }
}