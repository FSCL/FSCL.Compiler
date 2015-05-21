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
    /// Represents an OpenCL image.
    /// </summary>
    /// <remarks> A memory object that stores a two- or three- dimensional structured array. Image data can only be accessed with read and write functions. The read functions use a sampler. </remarks>
    /// <seealso cref="OpenCLMemory"/>
    /// <seealso cref="OpenCLSampler"/>
    public abstract class OpenCLImage : OpenCLMemory
    {
        #region Properties

        /// <summary>
        /// Gets or sets (protected) the depth in pixels of the <see cref="OpenCLImage"/>.
        /// </summary>
        /// <value> The depth in pixels of the <see cref="OpenCLImage"/>. </value>
        public int Depth { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the size of the elements (pixels) of the <see cref="OpenCLImage"/>.
        /// </summary>
        /// <value> The size of the elements (pixels) of the <see cref="OpenCLImage"/>. </value>
        public int ElementSize { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the height in pixels of the <see cref="OpenCLImage"/>.
        /// </summary>
        /// <value> The height in pixels of the <see cref="OpenCLImage"/>. </value>
        public int Height { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the size in bytes of a row of elements of the <see cref="OpenCLImage"/>.
        /// </summary>
        /// <value> The size in bytes of a row of elements of the <see cref="OpenCLImage"/>. </value>
        public long RowPitch { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the size in bytes of a 2D slice of a <see cref="OpenCLImage3D"/>.
        /// </summary>
        /// <value> The size in bytes of a 2D slice of a <see cref="OpenCLImage3D"/>. For a <see cref="OpenCLImage2D"/> this value is 0. </value>
        public long SlicePitch { get; protected set; }

        /// <summary>
        /// Gets or sets (protected) the width in pixels of the <see cref="OpenCLImage"/>.
        /// </summary>
        /// <value> The width in pixels of the <see cref="OpenCLImage"/>. </value>
        public int Width { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="flags"></param>
        protected OpenCLImage(OpenCLContext context, OpenCLMemoryFlags flags)
			: base(context, flags)
        { }

        #endregion

        #region Protected methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="flags"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected static ICollection<OpenCLImageFormat> GetSupportedFormats(OpenCLContext context, OpenCLMemoryFlags flags, OpenCLMemoryType type)
        {
            int formatCountRet = 0;
            OpenCLErrorCode error = CL10.GetSupportedImageFormats(context.Handle, flags, type, 0, null, out formatCountRet);
            OpenCLException.ThrowOnError(error);

            OpenCLImageFormat[] formats = new OpenCLImageFormat[formatCountRet];
            error = CL10.GetSupportedImageFormats(context.Handle, flags, type, formatCountRet, formats, out formatCountRet);
            OpenCLException.ThrowOnError(error);

            return new Collection<OpenCLImageFormat>(formats);
        }

        /// <summary>
        /// 
        /// </summary>
        protected void Init()
        {
            SetID(Handle.Value);

            Depth = (int)GetInfo<CLMemoryHandle, OpenCLImageInfo, IntPtr>(Handle, OpenCLImageInfo.Depth, CL10.GetImageInfo);
            ElementSize = (int)GetInfo<CLMemoryHandle, OpenCLImageInfo, IntPtr>(Handle, OpenCLImageInfo.ElementSize, CL10.GetImageInfo);
            Height = (int)GetInfo<CLMemoryHandle, OpenCLImageInfo, IntPtr>(Handle, OpenCLImageInfo.Height, CL10.GetImageInfo);
            RowPitch = (long)GetInfo<CLMemoryHandle, OpenCLImageInfo, IntPtr>(Handle, OpenCLImageInfo.RowPitch, CL10.GetImageInfo);
            Size = (long)GetInfo<CLMemoryHandle, OpenCLMemoryInfo, IntPtr>(Handle, OpenCLMemoryInfo.Size, CL10.GetMemObjectInfo);
            SlicePitch = (long)GetInfo<CLMemoryHandle, OpenCLImageInfo, IntPtr>(Handle, OpenCLImageInfo.SlicePitch, CL10.GetImageInfo);
            Width = (int)GetInfo<CLMemoryHandle, OpenCLImageInfo, IntPtr>(Handle, OpenCLImageInfo.Width, CL10.GetImageInfo);

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
        }

        #endregion
    }
}