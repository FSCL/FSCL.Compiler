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
    using OpenCL.Bindings;

    /// <summary>
    /// Represents an OpenCL 3D image.
    /// </summary>
    /// <seealso cref="OpenCLImage"/>
    public class OpenCLImage3D : OpenCLImage
    {
        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLImage3D"/>.
        /// </summary>
        /// <param name="context"> A valid <see cref="OpenCLContext"/> in which the <see cref="OpenCLImage3D"/> is created. </param>
        /// <param name="flags"> A bit-field that is used to specify allocation and usage information about the <see cref="OpenCLImage3D"/>. </param>
        /// <param name="format"> A structure that describes the format properties of the <see cref="OpenCLImage3D"/>. </param>
        /// <param name="width"> The width of the <see cref="OpenCLImage3D"/> in pixels. </param>
        /// <param name="height"> The height of the <see cref="OpenCLImage3D"/> in pixels. </param>
        /// <param name="depth"> The depth of the <see cref="OpenCLImage3D"/> in pixels. </param>
        /// <param name="rowPitch"> The size in bytes of each row of elements of the <see cref="OpenCLImage3D"/>. If <paramref name="rowPitch"/> is zero, OpenCL will compute the proper value based on <see cref="OpenCLImage.Width"/> and <see cref="OpenCLImage.ElementSize"/>. </param>
        /// <param name="slicePitch"> The size in bytes of each 2D slice in the <see cref="OpenCLImage3D"/>. If <paramref name="slicePitch"/> is zero, OpenCL will compute the proper value based on <see cref="OpenCLImage.RowPitch"/> and <see cref="OpenCLImage.Height"/>. </param>
        /// <param name="data"> The data to initialize the <see cref="OpenCLImage3D"/>. Can be <c>IntPtr.Zero</c>. </param>
        public OpenCLImage3D(OpenCLContext context, OpenCLMemoryFlags flags, OpenCLImageFormat format, int width, int height, int depth, long rowPitch, long slicePitch, IntPtr data)
            : base(context, flags)
        {
            OpenCLErrorCode error = OpenCLErrorCode.Success;
            Handle = CL10.CreateImage3D(context.Handle, flags, ref format, new IntPtr(width), new IntPtr(height), new IntPtr(depth), new IntPtr(rowPitch), new IntPtr(slicePitch), data, out error);
            OpenCLException.ThrowOnError(error);

            Init();
        }

        private OpenCLImage3D(CLMemoryHandle handle, OpenCLContext context, OpenCLMemoryFlags flags)
            : base(context, flags)
        {
            Handle = handle;

            Init();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Creates a new <see cref="OpenCLImage3D"/> from an OpenGL 3D texture object.
        /// </summary>
        /// <param name="context"> A <see cref="OpenCLContext"/> with enabled CL/GL sharing. </param>
        /// <param name="flags"> A bit-field that is used to specify usage information about the <see cref="OpenCLImage3D"/>. Only <c>OpenCLMemoryFlags.ReadOnly</c>, <c>OpenCLMemoryFlags.WriteOnly</c> and <c>OpenCLMemoryFlags.ReadWrite</c> are allowed. </param>
        /// <param name="textureTarget"> The image type of texture. Must be GL_TEXTURE_3D. </param>
        /// <param name="mipLevel"> The mipmap level of the OpenGL 2D texture object to be used. </param>
        /// <param name="textureId"> The OpenGL 2D texture object id to use. </param>
        /// <returns> The created <see cref="OpenCLImage2D"/>. </returns>
        public static OpenCLImage3D CreateFromGLTexture3D(OpenCLContext context, OpenCLMemoryFlags flags, int textureTarget, int mipLevel, int textureId)
        {
            CLMemoryHandle image;
            OpenCLErrorCode error = OpenCLErrorCode.Success;
            image = CL10.CreateFromGLTexture3D(context.Handle, flags, textureTarget, mipLevel, textureId, out error);
            OpenCLException.ThrowOnError(error);

            return new OpenCLImage3D(image, context, flags);
        }

        /// <summary>
        /// Gets a collection of supported <see cref="OpenCLImage3D"/> <see cref="OpenCLImageFormat"/>s in a <see cref="OpenCLContext"/>.
        /// </summary>
        /// <param name="context"> The <see cref="OpenCLContext"/> for which the collection of <see cref="OpenCLImageFormat"/>s is queried. </param>
        /// <param name="flags"> The <c>OpenCLMemoryFlags</c> for which the collection of <see cref="OpenCLImageFormat"/>s is queried. </param>
        /// <returns> The collection of the required <see cref="OpenCLImageFormat"/>s. </returns>
        public static ICollection<OpenCLImageFormat> GetSupportedFormats(OpenCLContext context, OpenCLMemoryFlags flags)
        {
            return GetSupportedFormats(context, flags, OpenCLMemoryType.Image3D);
        }

        #endregion
    }
}