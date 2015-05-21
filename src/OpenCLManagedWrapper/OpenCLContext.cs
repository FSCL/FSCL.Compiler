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
    using System.Threading;
    using OpenCL.Bindings;

    /// <summary>
    /// Represents an OpenCL context.
    /// </summary>
    /// <remarks> The environment within which the kernels execute and the domain in which synchronization and memory management is defined. </remarks>
    /// <br/>
    /// <example> 
    /// This example shows how to create a <see cref="OpenCLContext"/> that is able to share data with an OpenGL context in a Microsoft Windows OS:
    /// <code>
    /// <![CDATA[
    /// 
    /// // NOTE: If you see some non C# bits surrounding this code section, ignore them. They're not part of the code.
    /// 
    /// // We will need the device context, which is obtained through an OS specific function.
    /// [DllImport("opengl32.dll")]
    /// extern static IntPtr wglGetCurrentDC();
    /// 
    /// // Query the device context.
    /// IntPtr deviceContextHandle = wglGetCurrentDC();
    /// 
    /// // Select a platform which is capable of OpenCL/OpenGL interop.
    /// OpenCLPlatform platform = OpenCLPlatform.GetByName(name);
    /// 
    /// // Create the context property list and populate it.
    /// OpenCLContextProperty p1 = new OpenCLContextProperty(OpenCLContextPropertyName.Platform, platform.Handle.Value);
    /// OpenCLContextProperty p2 = new OpenCLContextProperty(OpenCLContextPropertyName.CL_GL_CONTEXT_KHR, openGLContextHandle);
    /// OpenCLContextProperty p3 = new OpenCLContextProperty(OpenCLContextPropertyName.CL_WGL_HDC_KHR, deviceContextHandle);
    /// OpenCLContextPropertyList cpl = new OpenCLContextPropertyList(new OpenCLContextProperty[] { p1, p2, p3 });
    /// 
    /// // Create the context. Usually, you'll want this on a GPU but other options might be available as well.
    /// OpenCLContext context = new OpenCLContext(OpenCLDeviceTypes.Gpu, cpl, null, IntPtr.Zero);
    /// 
    /// // Create a shared OpenCL/OpenGL buffer.
    /// // The generic type should match the type of data that the buffer contains.
    /// // glBufferId is an existing OpenGL buffer identifier.
    /// OpenCLBuffer<float> clglBuffer = OpenCLBuffer.CreateFromGLBuffer<float>(context, OpenCLMemoryFlags.ReadWrite, glBufferId);
    /// 
    /// ]]>
    /// </code>
    /// Before working with the <c>clglBuffer</c> you should make sure of two things:<br/>
    /// 1) OpenGL isn't using <c>glBufferId</c>. You can achieve this by calling <c>glFinish</c>.<br/>
    /// 2) Make it available to OpenCL through the <see cref="OpenCLCommandQueue.AcquireGLObjects"/> method.<br/>
    /// When finished, you should wait until <c>clglBuffer</c> isn't used any longer by OpenCL. After that, call <see cref="OpenCLCommandQueue.ReleaseGLObjects"/> to make the buffer available to OpenGL again.
    /// </example>
    /// <seealso cref="OpenCLDevice"/>
    /// <seealso cref="OpenCLPlatform"/>
    public class OpenCLContext : OpenCLResource
    {
        #region Fields

        
        private readonly ReadOnlyCollection<OpenCLDevice> devices;

        
        private readonly OpenCLPlatform platform;

        
        private readonly OpenCLContextPropertyList properties;

        
        private OpenCLContextNotifier callback;

        #endregion

        #region Properties

        /// <summary>
        /// The handle of the <see cref="OpenCLContext"/>.
        /// </summary>
        public CLContextHandle Handle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a read-only collection of the <see cref="OpenCLDevice"/>s of the <see cref="OpenCLContext"/>.
        /// </summary>
        /// <value> A read-only collection of the <see cref="OpenCLDevice"/>s of the <see cref="OpenCLContext"/>. </value>
        public ReadOnlyCollection<OpenCLDevice> Devices { get { return devices; } }

        /// <summary>
        /// Gets the <see cref="OpenCLPlatform"/> of the <see cref="OpenCLContext"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLPlatform"/> of the <see cref="OpenCLContext"/>. </value>
        public OpenCLPlatform Platform { get { return platform; } }

        /// <summary>
        /// Gets a collection of <see cref="OpenCLContextProperty"/>s of the <see cref="OpenCLContext"/>.
        /// </summary>
        /// <value> A collection of <see cref="OpenCLContextProperty"/>s of the <see cref="OpenCLContext"/>. </value>
        public OpenCLContextPropertyList Properties { get { return properties; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLContext"/> on a collection of <see cref="OpenCLDevice"/>s.
        /// </summary>
        /// <param name="devices"> A collection of <see cref="OpenCLDevice"/>s to associate with the <see cref="OpenCLContext"/>. </param>
        /// <param name="properties"> A <see cref="OpenCLContextPropertyList"/> of the <see cref="OpenCLContext"/>. </param>
        /// <param name="notify"> A delegate instance that refers to a notification routine. This routine is a callback function that will be used by the OpenCL implementation to report information on errors that occur in the <see cref="OpenCLContext"/>. The callback function may be called asynchronously by the OpenCL implementation. It is the application's responsibility to ensure that the callback function is thread-safe and that the delegate instance doesn't get collected by the Garbage Collector until <see cref="OpenCLContext"/> is disposed. If <paramref name="notify"/> is <c>null</c>, no callback function is registered. </param>
        /// <param name="notifyDataPtr"> Optional user data that will be passed to <paramref name="notify"/>. </param>
        public OpenCLContext(List<OpenCLDevice> devices, OpenCLContextPropertyList properties, OpenCLContextNotifier notify, IntPtr notifyDataPtr)
        {
            int handleCount;
            CLDeviceHandle[] deviceHandles = OpenCLTools.ExtractHandles(devices, out handleCount);
            IntPtr[] propertyArray = (properties != null) ? properties.ToIntPtrArray() : null;
            callback = notify;

            OpenCLErrorCode error = OpenCLErrorCode.Success;
            Handle = CL10.CreateContext(propertyArray, handleCount, deviceHandles, notify, notifyDataPtr, out error);
            OpenCLException.ThrowOnError(error);
            
            SetID(Handle.Value);
            
            this.properties = properties;
            OpenCLContextProperty platformProperty = properties.GetByName(OpenCLContextProperties.Platform);
            this.platform = OpenCLPlatform.GetByHandle(platformProperty.Value);
            this.devices = GetDevices();

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
        }

        /// <summary>
        /// Creates a new <see cref="OpenCLContext"/> on all the <see cref="OpenCLDevice"/>s that match the specified <see cref="OpenCLDeviceTypes"/>.
        /// </summary>
        /// <param name="deviceType"> A bit-field that identifies the type of <see cref="OpenCLDevice"/> to associate with the <see cref="OpenCLContext"/>. </param>
        /// <param name="properties"> A <see cref="OpenCLContextPropertyList"/> of the <see cref="OpenCLContext"/>. </param>
        /// <param name="notify"> A delegate instance that refers to a notification routine. This routine is a callback function that will be used by the OpenCL implementation to report information on errors that occur in the <see cref="OpenCLContext"/>. The callback function may be called asynchronously by the OpenCL implementation. It is the application's responsibility to ensure that the callback function is thread-safe and that the delegate instance doesn't get collected by the Garbage Collector until <see cref="OpenCLContext"/> is disposed. If <paramref name="notify"/> is <c>null</c>, no callback function is registered. </param>
        /// <param name="userDataPtr"> Optional user data that will be passed to <paramref name="notify"/>. </param>
        public OpenCLContext(OpenCLDeviceType deviceType, OpenCLContextPropertyList properties, OpenCLContextNotifier notify, IntPtr userDataPtr)
        {
            IntPtr[] propertyArray = (properties != null) ? properties.ToIntPtrArray() : null;
            callback = notify;

            OpenCLErrorCode error = OpenCLErrorCode.Success;
            Handle = CL10.CreateContextFromType(propertyArray, deviceType, notify, userDataPtr, out error);
            OpenCLException.ThrowOnError(error);

            SetID(Handle.Value);

            this.properties = properties;
            OpenCLContextProperty platformProperty = properties.GetByName(OpenCLContextProperties.Platform);
            this.platform = OpenCLPlatform.GetByHandle(platformProperty.Value);
            this.devices = GetDevices();

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
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
            if (manual)
            {
                //free managed resources
            }

            // free native resources
            if (Handle.IsValid)
            {
                //Trace.WriteLine("Dispose " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
                CL10.ReleaseContext(Handle);
                Handle.Invalidate();
            }
        }

        #endregion

        #region Private methods

        private ReadOnlyCollection<OpenCLDevice> GetDevices()
        {
            List<CLDeviceHandle> deviceHandles = new List<CLDeviceHandle>(GetArrayInfo<CLContextHandle, OpenCLContextInfo, CLDeviceHandle>(Handle, OpenCLContextInfo.Devices, CL10.GetContextInfo));
            List<OpenCLDevice> devices = new List<OpenCLDevice>();
            foreach (OpenCLPlatform platform in OpenCLPlatform.Platforms)
            {
                foreach (OpenCLDevice device in platform.Devices)
                    if (deviceHandles.Contains(device.Handle))
                        devices.Add(device);
            }
            return new ReadOnlyCollection<OpenCLDevice>(devices);
        }

        #endregion
    }
}