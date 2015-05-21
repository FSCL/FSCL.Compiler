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
    /// Represents an OpenCL event.
    /// </summary>
    /// <remarks> An event encapsulates the status of an operation such as a command. It can be used to synchronize operations in a context. </remarks>
    /// <seealso cref="OpenCLUserEvent"/>
    /// <seealso cref="OpenCLCommandQueue"/>
    /// <seealso cref="OpenCLContext"/>
    public class OpenCLEvent : OpenCLEventBase
    {
        #region Fields

        
        private GCHandle gcHandle;
        
        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="OpenCLCommandQueue"/> associated with the <see cref="OpenCLEvent"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLCommandQueue"/> associated with the <see cref="OpenCLEvent"/>. </value>
        public OpenCLCommandQueue CommandQueue { get; private set; }

        #endregion

        #region Constructors

        internal OpenCLEvent(CLEventHandle handle, OpenCLCommandQueue queue)
        {
            Handle = handle;
            SetID(Handle.Value);

            CommandQueue = queue;
            Type = (OpenCLCommandType)GetInfo<CLEventHandle, OpenCLEventInfo, int>(Handle, OpenCLEventInfo.CommandType, CL10.GetEventInfo);
            Context = queue.Context;

            if (OpenCLTools.ParseVersionString(CommandQueue.Device.Platform.Version, 1) > new Version(1, 0))
                HookNotifier();

            //Trace.WriteLine("Create " + this + " in Thread(" + Thread.CurrentThread.ManagedThreadId + ").", "Information");
        }

        #endregion

        #region Internal methods

        internal void TrackGCHandle(GCHandle handle)
        {
            gcHandle = handle;

            Completed += new OpenCLCommandStatusChanged(Cleanup);
            Aborted += new OpenCLCommandStatusChanged(Cleanup);
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
            FreeGCHandle();
            base.Dispose(manual);
        }

        #endregion

        #region Private methods

        private void Cleanup(object sender, OpenCLCommandStatusArgs e)
        {
            lock (CommandQueue.Events)
            {
                if (CommandQueue.Events.Contains(this))
                {
                    CommandQueue.Events.Remove(this);
                    Dispose();
                }
                else
                    FreeGCHandle();
            }
        }

        private void FreeGCHandle()
        {
            if (gcHandle.IsAllocated && gcHandle.Target != null)
                gcHandle.Free();
        }

        #endregion
    }
}