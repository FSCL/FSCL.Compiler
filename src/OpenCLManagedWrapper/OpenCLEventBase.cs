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
    /// Represents the parent type to any OpenCL event types.
    /// </summary>
    /// <seealso cref="OpenCLEvent"/>
    /// <seealso cref="OpenCLUserEvent"/>
    public abstract class OpenCLEventBase : OpenCLResource
    {
        #region Fields

        private event OpenCLCommandStatusChanged aborted;        
        private event OpenCLCommandStatusChanged completed;
        
        
        private OpenCLCommandStatusArgs status;
        
        
        private OpenCLEventCallback statusNotify;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the command associated with the event is abnormally terminated.
        /// </summary>
        /// <remarks> Requires OpenCL 1.1. </remarks>
        public event OpenCLCommandStatusChanged Aborted
        {
            add
            {
                aborted += value;
                if (status != null && status.Status != OpenCLCommandExecutionStatus.Complete)
                    value.Invoke(this, status);
            }
            remove
            {
                aborted -= value;
            }
        }

        /// <summary>
        /// Occurs when <c>OpenCLEventBase.Status</c> changes to <c>OpenCLCommandExecutionStatus.Complete</c>.
        /// </summary>
        /// <remarks> Requires OpenCL 1.1. </remarks>
        public event OpenCLCommandStatusChanged Completed
        {
            add
            {
                completed += value;
                if (status != null && status.Status == OpenCLCommandExecutionStatus.Complete)
                    value.Invoke(this, status);
            }
            remove
            {
                completed -= value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The handle of the <see cref="OpenCLEventBase"/>.
        /// </summary>
        public CLEventHandle Handle
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the <see cref="OpenCLContext"/> associated with the <see cref="OpenCLEventBase"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLContext"/> associated with the <see cref="OpenCLEventBase"/>. </value>
        public OpenCLContext Context { get; protected set; }

        /// <summary>
        /// Gets the <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command has finished execution.
        /// </summary>
        /// <value> The <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command has finished execution. </value>
        public long FinishTime
        {
            get { return GetInfo<CLEventHandle, OpenCLCommandProfilingInfo, long>(Handle, OpenCLCommandProfilingInfo.Ended, CL10.GetEventProfilingInfo); }
        }

        /// <summary>
        /// Gets the <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command is enqueued in the <see cref="OpenCLCommandQueue"/> by the host.
        /// </summary>
        /// <value> The <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command is enqueued in the <see cref="OpenCLCommandQueue"/> by the host. </value>
        public long EnqueueTime
        {
            get { return (long)GetInfo<CLEventHandle, OpenCLCommandProfilingInfo, long>(Handle, OpenCLCommandProfilingInfo.Queued, CL10.GetEventProfilingInfo); }
        }

        /// <summary>
        /// Gets the execution status of the associated command.
        /// </summary>
        /// <value> The execution status of the associated command or a negative value if the execution was abnormally terminated. </value>
        public OpenCLCommandExecutionStatus Status
        {
            get { return (OpenCLCommandExecutionStatus)GetInfo<CLEventHandle, OpenCLEventInfo, int>(Handle, OpenCLEventInfo.ExecutionStatus, CL10.GetEventInfo); }
        }

        /// <summary>
        /// Gets the <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command starts execution.
        /// </summary>
        /// <value> The <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command starts execution. </value>
        public long StartTime
        {
            get { return (long)GetInfo<CLEventHandle, OpenCLCommandProfilingInfo, ulong>(Handle, OpenCLCommandProfilingInfo.Started, CL10.GetEventProfilingInfo); }
        }

        /// <summary>
        /// Gets the <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command that has been enqueued is submitted by the host to the device.
        /// </summary>
        /// <value> The <see cref="OpenCLDevice"/> time counter in nanoseconds when the associated command that has been enqueued is submitted by the host to the device. </value>
        public long SubmitTime
        {
            get { return (long)GetInfo<CLEventHandle, OpenCLCommandProfilingInfo, ulong>(Handle, OpenCLCommandProfilingInfo.Submitted, CL10.GetEventProfilingInfo); }
        }

        /// <summary>
        /// Gets the <see cref="OpenCLCommandType"/> associated with the event.
        /// </summary>
        /// <value> The <see cref="OpenCLCommandType"/> associated with the event. </value>
        public OpenCLCommandType Type { get; protected set; }

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
                CL10.ReleaseEvent(Handle);
                Handle.Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected void HookNotifier()
        {
            statusNotify = new OpenCLEventCallback(StatusNotify);
            OpenCLErrorCode error = CL11.SetEventCallback(Handle, (int)OpenCLCommandExecutionStatus.Complete, statusNotify, IntPtr.Zero);
            OpenCLException.ThrowOnError(error);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="evArgs"></param>
        protected virtual void OnCompleted(object sender, OpenCLCommandStatusArgs evArgs)
        {
            //Trace.WriteLine("Complete " + Type + " operation of " + this + ".", "Information");
            if (completed != null)
                completed(sender, evArgs);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="evArgs"></param>
        protected virtual void OnAborted(object sender, OpenCLCommandStatusArgs evArgs)
        {
            //Trace.WriteLine("Abort " + Type + " operation of " + this + ".", "Information");
            if (aborted != null)
                aborted(sender, evArgs);
        }

        #endregion

        #region Private methods

        private void StatusNotify(CLEventHandle eventHandle, int cmdExecStatusOrErr, IntPtr userData)
        {
            status = new OpenCLCommandStatusArgs(this, (OpenCLCommandExecutionStatus)cmdExecStatusOrErr);
            switch (cmdExecStatusOrErr)
            {
                case (int)OpenCLCommandExecutionStatus.Complete: OnCompleted(this, status); break;
                default: OnAborted(this, status); break;
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents the arguments of a command status change.
    /// </summary>
    public class OpenCLCommandStatusArgs : EventArgs
    {
        /// <summary>
        /// Gets the event associated with the command that had its status changed.
        /// </summary>
        public OpenCLEventBase Event { get; private set; }

        /// <summary>
        /// Gets the execution status of the command represented by the event.
        /// </summary>
        /// <remarks> Returns a negative integer if the command was abnormally terminated. </remarks>
        public OpenCLCommandExecutionStatus Status { get; private set; }

        /// <summary>
        /// Creates a new <c>OpenCLCommandStatusArgs</c> instance.
        /// </summary>
        /// <param name="ev"> The event representing the command that had its status changed. </param>
        /// <param name="status"> The status of the command. </param>
        public OpenCLCommandStatusArgs(OpenCLEventBase ev, OpenCLCommandExecutionStatus status)
        {
            Event = ev;
            Status = status;
        }

        /// <summary>
        /// Creates a new <c>OpenCLCommandStatusArgs</c> instance.
        /// </summary>
        /// <param name="ev"> The event of the command that had its status changed. </param>
        /// <param name="status"> The status of the command. </param>
        public OpenCLCommandStatusArgs(OpenCLEventBase ev, int status)
            : this(ev, (OpenCLCommandExecutionStatus)status)
        { }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void OpenCLCommandStatusChanged(object sender, OpenCLCommandStatusArgs args);
}