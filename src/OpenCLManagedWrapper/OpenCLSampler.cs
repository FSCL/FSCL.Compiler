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
    using System.Threading;
    using OpenCL.Bindings;

    /// <summary>
    /// Represents an OpenCL sampler.
    /// </summary>
    /// <remarks> An object that describes how to sample an image when the image is read in the kernel. The image read functions take a sampler as an argument. The sampler specifies the image addressing-mode i.e. how out-of-range image coordinates are handled, the filtering mode, and whether the input image coordinate is a normalized or unnormalized value. </remarks>
    /// <seealso cref="OpenCLImage"/>
    public class OpenCLSampler : OpenCLResource
    {
        #region Fields

        
        private readonly OpenCLContext context;

        
        private readonly OpenCLImageAddressing addressing;

        
        private readonly OpenCLImageFiltering filtering;

        
        private readonly bool normalizedCoords;

        #endregion

        #region Properties

        /// <summary>
        /// The handle of the <see cref="OpenCLSampler"/>.
        /// </summary>
        public CLSamplerHandle Handle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the <see cref="OpenCLContext"/> of the <see cref="OpenCLSampler"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLContext"/> of the <see cref="OpenCLSampler"/>. </value>
        public OpenCLContext Context { get { return context; } }

        /// <summary>
        /// Gets the <see cref="OpenCLImageAddressing"/> mode of the <see cref="OpenCLSampler"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLImageAddressing"/> mode of the <see cref="OpenCLSampler"/>. </value>
        public OpenCLImageAddressing Addressing { get { return addressing; } }

        /// <summary>
        /// Gets the <see cref="OpenCLImageFiltering"/> mode of the <see cref="OpenCLSampler"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLImageFiltering"/> mode of the <see cref="OpenCLSampler"/>. </value>
        public OpenCLImageFiltering Filtering { get { return filtering; } }

        /// <summary>
        /// Gets the state of usage of normalized x, y and z coordinates when accessing a <see cref="OpenCLImage"/> in a <see cref="OpenCLKernel"/> through the <see cref="OpenCLSampler"/>.
        /// </summary>
        /// <value> The state of usage of normalized x, y and z coordinates when accessing a <see cref="OpenCLImage"/> in a <see cref="OpenCLKernel"/> through the <see cref="OpenCLSampler"/>. </value>
        public bool NormalizedCoords { get { return normalizedCoords; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLSampler"/>.
        /// </summary>
        /// <param name="context"> A <see cref="OpenCLContext"/>. </param>
        /// <param name="normalizedCoords"> The usage state of normalized coordinates when accessing a <see cref="OpenCLImage"/> in a <see cref="OpenCLKernel"/>. </param>
        /// <param name="addressing"> The <see cref="OpenCLImageAddressing"/> mode of the <see cref="OpenCLSampler"/>. Specifies how out-of-range image coordinates are handled while reading. </param>
        /// <param name="filtering"> The <see cref="OpenCLImageFiltering"/> mode of the <see cref="OpenCLSampler"/>. Specifies the type of filter that must be applied when reading data from an image. </param>
        public OpenCLSampler(OpenCLContext context, bool normalizedCoords, OpenCLImageAddressing addressing, OpenCLImageFiltering filtering)
        {
            OpenCLErrorCode error = OpenCLErrorCode.Success;
            Handle = CL10.CreateSampler(context.Handle, normalizedCoords, addressing, filtering, out error);
            OpenCLException.ThrowOnError(error);

            SetID(Handle.Value);
            
            this.addressing = addressing;
            this.context = context;
            this.filtering = filtering;
            this.normalizedCoords = normalizedCoords;

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
            if (Handle.IsValid)
            {
                //Trace.WriteLine("Dispose " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
                CL10.ReleaseSampler(Handle);
                Handle.Invalidate();
            }
        }

        #endregion
    }
}