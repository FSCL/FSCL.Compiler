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
    using OpenCL.Bindings;

    /// <summary>
    /// Represents an OpenCL platform.
    /// </summary>
    /// <remarks> The host plus a collection of devices managed by the OpenCL framework that allow an application to share resources and execute kernels on devices in the platform. </remarks>
    /// <seealso cref="OpenCLDevice"/>
    /// <seealso cref="OpenCLKernel"/>
    /// <seealso cref="OpenCLResource"/>
    public class OpenCLPlatform : OpenCLObject
    {
        #region Fields

        
        private ReadOnlyCollection<OpenCLDevice> devices;

        
        private readonly ReadOnlyCollection<string> extensions;

        
        private readonly string name;

        
        private static ReadOnlyCollection<OpenCLPlatform> platforms;

        
        private readonly string profile;

        
        private readonly string vendor;

        
        private readonly string version;

        #endregion

        #region Properties

        /// <summary>
        /// The handle of the <see cref="OpenCLPlatform"/>.
        /// </summary>
        public CLPlatformHandle Handle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets a read-only collection of <see cref="OpenCLDevice"/>s available on the <see cref="OpenCLPlatform"/>.
        /// </summary>
        /// <value> A read-only collection of <see cref="OpenCLDevice"/>s available on the <see cref="OpenCLPlatform"/>. </value>
        public ReadOnlyCollection<OpenCLDevice> Devices { get { return devices; } }

        /// <summary>
        /// Gets a read-only collection of extension names supported by the <see cref="OpenCLPlatform"/>.
        /// </summary>
        /// <value> A read-only collection of extension names supported by the <see cref="OpenCLPlatform"/>. </value>
        public ReadOnlyCollection<string> Extensions { get { return extensions; } }

        /// <summary>
        /// Gets the <see cref="OpenCLPlatform"/> name.
        /// </summary>
        /// <value> The <see cref="OpenCLPlatform"/> name. </value>
        public string Name { get { return name; } }

        /// <summary>
        /// Gets a read-only collection of available <see cref="OpenCLPlatform"/>s.
        /// </summary>
        /// <value> A read-only collection of available <see cref="OpenCLPlatform"/>s. </value>
        /// <remarks> The collection will contain no items, if no OpenCL platforms are found on the system. </remarks>
        public static ReadOnlyCollection<OpenCLPlatform> Platforms { get { return platforms; } }

        /// <summary>
        /// Gets the name of the profile supported by the <see cref="OpenCLPlatform"/>.
        /// </summary>
        /// <value> The name of the profile supported by the <see cref="OpenCLPlatform"/>. </value>
        public string Profile { get { return profile; } }

        /// <summary>
        /// Gets the <see cref="OpenCLPlatform"/> vendor.
        /// </summary>
        /// <value> The <see cref="OpenCLPlatform"/> vendor. </value>
        public string Vendor { get { return vendor; } }

        /// <summary>
        /// Gets the OpenCL version string supported by the <see cref="OpenCLPlatform"/>.
        /// </summary>
        /// <value> The OpenCL version string supported by the <see cref="OpenCLPlatform"/>. It has the following format: <c>OpenCL[space][major_version].[minor_version][space][vendor-specific information]</c>. </value>
        public string Version { get { return version; } }

        #endregion

        #region Constructors

        static OpenCLPlatform()
        {
            try
            {
                if (platforms != null)
                    return;
                CLPlatformHandle[] handles;
                int handlesLength;
                OpenCLErrorCode error = CL10.GetPlatformIDs(0, null, out handlesLength);
                OpenCLException.ThrowOnError(error);
                handles = new CLPlatformHandle[handlesLength];

                error = CL10.GetPlatformIDs(handlesLength, handles, out handlesLength);
                OpenCLException.ThrowOnError(error);

                List<OpenCLPlatform> platformList = new List<OpenCLPlatform>(handlesLength);
                foreach (CLPlatformHandle handle in handles)
                    platformList.Add(new OpenCLPlatform(handle));

                platforms = platformList.AsReadOnly();
            }
            catch (DllNotFoundException)
            {
                platforms = new List<OpenCLPlatform>().AsReadOnly();
            }
        }

        private OpenCLPlatform(CLPlatformHandle handle)
        {
            Handle = handle;
            SetID(Handle.Value);

            string extensionString = GetStringInfo<CLPlatformHandle, OpenCLPlatformInfo>(Handle, OpenCLPlatformInfo.Extensions, CL10.GetPlatformInfo);
            extensions = new ReadOnlyCollection<string>(extensionString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            name = GetStringInfo<CLPlatformHandle, OpenCLPlatformInfo>(Handle, OpenCLPlatformInfo.Name, CL10.GetPlatformInfo);
            profile = GetStringInfo<CLPlatformHandle, OpenCLPlatformInfo>(Handle, OpenCLPlatformInfo.Profile, CL10.GetPlatformInfo);
            vendor = GetStringInfo<CLPlatformHandle, OpenCLPlatformInfo>(Handle, OpenCLPlatformInfo.Vendor, CL10.GetPlatformInfo);
            version = GetStringInfo<CLPlatformHandle, OpenCLPlatformInfo>(Handle, OpenCLPlatformInfo.Version, CL10.GetPlatformInfo);
            QueryDevices();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets a <see cref="OpenCLPlatform"/> of a specified handle.
        /// </summary>
        /// <param name="handle"> The handle of the queried <see cref="OpenCLPlatform"/>. </param>
        /// <returns> The <see cref="OpenCLPlatform"/> of the matching handle or <c>null</c> if none matches. </returns>
        public static OpenCLPlatform GetByHandle(IntPtr handle)
        {
            foreach (OpenCLPlatform platform in Platforms)
                if (platform.Handle.Value == handle)
                    return platform;

            return null;
        }

        /// <summary>
        /// Gets the first matching <see cref="OpenCLPlatform"/> of a specified name.
        /// </summary>
        /// <param name="platformName"> The name of the queried <see cref="OpenCLPlatform"/>. </param>
        /// <returns> The first <see cref="OpenCLPlatform"/> of the specified name or <c>null</c> if none matches. </returns>
        public static OpenCLPlatform GetByName(string platformName)
        {
            foreach (OpenCLPlatform platform in Platforms)
                if (platform.Name.Equals(platformName))
                    return platform;

            return null;
        }

        /// <summary>
        /// Gets the first matching <see cref="OpenCLPlatform"/> of a specified vendor.
        /// </summary>
        /// <param name="platformVendor"> The vendor of the queried <see cref="OpenCLPlatform"/>. </param>
        /// <returns> The first <see cref="OpenCLPlatform"/> of the specified vendor or <c>null</c> if none matches. </returns>
        public static OpenCLPlatform GetByVendor(string platformVendor)
        {
            foreach (OpenCLPlatform platform in Platforms)
                if (platform.Vendor.Equals(platformVendor))
                    return platform;

            return null;
        }

        /// <summary>
        /// Gets a read-only collection of available <see cref="OpenCLDevice"/>s on the <see cref="OpenCLPlatform"/>.
        /// </summary>
        /// <returns> A read-only collection of the available <see cref="OpenCLDevice"/>s on the <see cref="OpenCLPlatform"/>. </returns>
        /// <remarks> This method resets the <c>OpenCLPlatform.Devices</c>. This is useful if one or more of them become unavailable (<c>OpenCLDevice.Available</c> is <c>false</c>) after a <see cref="OpenCLContext"/> and <see cref="OpenCLCommandQueue"/>s that use the <see cref="OpenCLDevice"/> have been created and commands have been queued to them. Further calls will trigger an <c>OutOfResourcesOpenCLException</c> until this method is executed. You will also need to recreate any <see cref="OpenCLResource"/> that was created on the no longer available <see cref="OpenCLDevice"/>. </remarks>
        public ReadOnlyCollection<OpenCLDevice> QueryDevices()
        {
            int handlesLength = 0;
            OpenCLErrorCode error = CL10.GetDeviceIDs(Handle, OpenCLDeviceType.All, 0, null, out handlesLength);
            OpenCLException.ThrowOnError(error);

            CLDeviceHandle[] handles = new CLDeviceHandle[handlesLength];
            error = CL10.GetDeviceIDs(Handle, OpenCLDeviceType.All, handlesLength, handles, out handlesLength);
            OpenCLException.ThrowOnError(error);

            OpenCLDevice[] devices = new OpenCLDevice[handlesLength];
            for (int i = 0; i < handlesLength; i++)
                devices[i] = new OpenCLDevice(this, handles[i]);

            this.devices = new ReadOnlyCollection<OpenCLDevice>(devices);

            return this.devices;
        }

        #endregion
    }
}