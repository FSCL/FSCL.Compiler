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
    using System.Runtime.InteropServices;
    using OpenCL.Bindings;
    using System.Collections.Generic;

    /**/

    /// <summary>
    /// Represents an OpenCL buffer.
    /// </summary>
    /// <seealso cref="OpenCLDevice"/>
    /// <seealso cref="OpenCLKernel"/>
    /// <seealso cref="OpenCLMemory"/>
    public class OpenCLBuffer : OpenCLBufferBase
    {
        #region Properties
        /*
        public GCHandle ArrayHandle
        {
            get;
            private set;
        }
        public IntPtr ArrayPtr
        {
            get;
            private set;
        }
        public bool IsValidArrayHandle
        {
            get;
            private set;
        }*/
        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLBuffer{T}"/>.
        /// </summary>
        /// <param name="context"> A <see cref="OpenCLContext"/> used to create the <see cref="OpenCLBuffer{T}"/>. </param>
        /// <param name="flags"> A bit-field that is used to specify allocation and usage information about the <see cref="OpenCLBuffer{T}"/>. </param>
        /// <param name="count"> The number of elements of the <see cref="OpenCLBuffer{T}"/>. </param>
     
		public OpenCLBuffer(OpenCLContext context, OpenCLMemoryFlags flags, Type elementType, long[] count)
		: this(context, flags, elementType, count, IntPtr.Zero)
		{ }

  		/// <summary>
		/// Creates a new <see cref="OpenCLBuffer{T}"/>.
		/// </summary>
		/// <param name="context"> A <see cref="OpenCLContext"/> used to create the <see cref="OpenCLBuffer{T}"/>. </param>
		/// <param name="flags"> A bit-field that is used to specify allocation and usage information about the <see cref="OpenCLBuffer{T}"/>. </param>
		/// <param name="length"> The number of elements in each dimension <see cref="OpenCLBuffer{T}"/>. </param>
		/// <param name="dataPtr"> A pointer to the data for the <see cref="OpenCLBuffer{T}"/>. </param>
		public OpenCLBuffer(OpenCLContext context, OpenCLMemoryFlags flags, Type elementType, long[] count, IntPtr dataPtr)
		: base(context, flags, elementType, count)
		{
			OpenCLErrorCode error = OpenCLErrorCode.Success;
			long totalCount = 1;
			foreach (var c in count)
				totalCount *= c;
            Handle = CL10.CreateBuffer(context.Handle, flags, new IntPtr(Marshal.SizeOf(elementType) * totalCount), dataPtr, out error);
			OpenCLException.ThrowOnError(error);
			Init();
		}

        /// <summary>
        /// Creates a new <see cref="OpenCLBuffer{T}"/>.
        /// </summary>
        /// <param name="context"> A <see cref="OpenCLContext"/> used to create the <see cref="OpenCLBuffer{T}"/>. </param>
        /// <param name="flags"> A bit-field that is used to specify allocation and usage information about the <see cref="OpenCLBuffer{T}"/>. </param>
        /// <param name="data"> The data for the <see cref="OpenCLBuffer{T}"/>. </param>
        /// <remarks> Note, that <paramref name="data"/> cannot be an "immediate" parameter, i.e.: <c>new T[100]</c>, because it could be quickly collected by the GC causing OpenCL to send and invalid reference to OpenCL. </remarks>
       /* public OpenCLBuffer(OpenCLContext context, OpenCLMemoryFlags flags, Array data)
            : base(context, flags, data.GetType().GetElementType(), OpenCLBuffer.GetArrayLengths(data))
        {
            GCHandle dataPtr = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {

                if ((flags & OpenCLMemoryFlags.UseHostPointer) != 0)
                {
                    this.ArrayHandle = dataPtr;
                    this.IsValidArrayHandle = true;
                }

                OpenCLErrorCode error = OpenCLErrorCode.Success;
                Handle = CL10.CreateBuffer(context.Handle, flags, new IntPtr(Marshal.SizeOf(data.GetType().GetElementType()) * data.Length), dataPtr.AddrOfPinnedObject(), out error);
                OpenCLException.ThrowOnError(error);
            }
            finally 
            {
                if(!this.IsValidArrayHandle)
                    dataPtr.Free(); 
            }
            Init();
        }*/

        private OpenCLBuffer(CLMemoryHandle handle, OpenCLContext context, OpenCLMemoryFlags flags, Type elementType)
            : base(context, flags, elementType, null)
        {
            Handle = handle;
            Init();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Creates a new <see cref="OpenCLBuffer{T}"/> from an existing OpenGL buffer object.
        /// </summary>
        /// <typeparam name="DataType"> The type of the elements of the <see cref="OpenCLBuffer{T}"/>. <typeparamref name="T"/> should match the type of the elements in the OpenGL buffer. </typeparam>
        /// <param name="context"> A <see cref="OpenCLContext"/> with enabled CL/GL sharing. </param>
        /// <param name="flags"> A bit-field that is used to specify usage information about the <see cref="OpenCLBuffer{T}"/>. Only <see cref="OpenCLMemoryFlags.ReadOnly"/>, <see cref="OpenCLMemoryFlags.WriteOnly"/> and <see cref="OpenCLMemoryFlags.ReadWrite"/> are allowed. </param>
        /// <param name="bufferId"> The OpenGL buffer object id to use for the creation of the <see cref="OpenCLBuffer{T}"/>. </param>
        /// <returns> The created <see cref="OpenCLBuffer{T}"/>. </returns>
        public static OpenCLBuffer CreateFromGLBuffer(OpenCLContext context, OpenCLMemoryFlags flags, Type elementType, int bufferId)
        {
            OpenCLErrorCode error = OpenCLErrorCode.Success;
            CLMemoryHandle handle = CL10.CreateFromGLBuffer(context.Handle, flags, bufferId, out error);
            OpenCLException.ThrowOnError(error);
            return new OpenCLBuffer(handle, context, flags, elementType);
        }

        #endregion

        #region Private methods
        
        private static long[] GetArrayLengths(Array a) {            
            int rank = a.Rank;
            List<long> lengths = new List<long>();
            for (int i = 0; i < rank; i++)
                lengths.Add(a.GetLongLength(i));
            return lengths.ToArray();            
        }
#endregion
        protected override void Dispose(bool manual)
        {
            //if (this.IsValidArrayHandle)
              //  this.ArrayHandle.Free();
            base.Dispose(manual);
        }
    }
}