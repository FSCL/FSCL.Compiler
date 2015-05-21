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
    using System.Runtime.InteropServices;
    using System.Threading;
    using OpenCL.Bindings;

    /// <summary>
    /// Represents the parent type to any OpenCL buffer types.
    /// </summary>
    /// <typeparam name="T"> The type of the elements of the buffer. </typeparam>
    public abstract class OpenCLBufferBase : OpenCLMemory
    {
        #region Fields
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the number of elements in the <see cref="OpenCLBufferBase{T}"/>.
        /// </summary>
        /// <value> The number of elements in the <see cref="OpenCLBufferBase{T}"/>. </value>
        /// 
        public Type ElementType { get; private set; }

        // Declare the same methods/properties of Array types
        public long TotalCount
        {
            get
            {
                long sum = 1;
				foreach (long c in Count)
                    sum *= c;
                return sum;
            }
        }

        public long GetLongLength(int r)
        {
            return this.Count[r];
        }
        public int GetLength(int r)
        {
            return (int)this.Count[r];
        }

        public long[] Count {
			get;
			set;
        }

        public int Length
        {
            get
            {
                return (int)this.TotalCount;
            }
        }
        public long LongLength
        {
            get
            {
                return this.TotalCount;
            }
        }
        public int Rank
        {
            get
            {
				return Count.Length;
            }
        }

        public bool KernelCanRead
        {
            get {
                return ((Flags & OpenCLMemoryFlags.WriteOnly) == 0);
            }
        }

        public bool KernelCanWrite
        {
            get
            {
                return ((Flags & OpenCLMemoryFlags.ReadOnly) == 0);
            }
        }

        public bool HostCanRead
        {
            get
            {
                return ((Flags & OpenCLMemoryFlags.HostNoAccess) == 0) && ((Flags & OpenCLMemoryFlags.HostWriteOnly) == 0);
            }
        }

        public bool HostCanWrite
        {
            get
            {
                return ((Flags & OpenCLMemoryFlags.HostNoAccess) == 0) && ((Flags & OpenCLMemoryFlags.HostReadOnly) == 0);
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="flags"></param>
        protected OpenCLBufferBase(OpenCLContext context, OpenCLMemoryFlags flags, Type elementType, long[] count)
            : base(context, flags)
        {
            ElementType = elementType;
			this.Count = count;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// 
        /// </summary>
        protected void Init()
        {
            SetID(Handle.Value);

            Size = (long)GetInfo<CLMemoryHandle, OpenCLMemoryInfo, IntPtr>(Handle, OpenCLMemoryInfo.Size, CL10.GetMemObjectInfo);

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
        }

        #endregion


    }
}