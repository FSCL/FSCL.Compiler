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
	using System.Collections.Generic;

    /// <summary>
    /// Represents an OpenCL memory object.
    /// </summary>
    /// <remarks> A memory object is a handle to a region of global memory. </remarks>
    /// <seealso cref="OpenCLBuffer{T}"/>
    /// <seealso cref="OpenCLImage"/>
    public abstract class OpenCLMemory : OpenCLResource
    {
        #region Fields

        
        private readonly OpenCLContext context;

        
        private readonly OpenCLMemoryFlags flags;

        #endregion

        #region Properties

        /// <summary>
        /// The handle of the <see cref="OpenCLMemory"/>.
        /// </summary>
        public CLMemoryHandle Handle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the <see cref="OpenCLContext"/> of the <see cref="OpenCLMemory"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLContext"/> of the <see cref="OpenCLMemory"/>. </value>
        public OpenCLContext Context { get { return context; } }

        /// <summary>
        /// Gets the <see cref="OpenCLMemoryFlags"/> of the <see cref="OpenCLMemory"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLMemoryFlags"/> of the <see cref="OpenCLMemory"/>. </value>
        public OpenCLMemoryFlags Flags { get { return flags; } }

        /// <summary>
        /// Gets or sets (protected) the size in bytes of the <see cref="OpenCLMemory"/>.
        /// </summary>
        /// <value> The size in bytes of the <see cref="OpenCLMemory"/>. </value>
        public long Size { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="flags"></param>
        protected OpenCLMemory(OpenCLContext context, OpenCLMemoryFlags flags)
        {
            this.context = context;
            this.flags = flags;

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
                CL10.ReleaseMemObject(Handle);
                Handle.Invalidate();
            }
        }

        #endregion
    }
}