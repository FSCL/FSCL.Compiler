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
    using System.Runtime.InteropServices;
    using System.Linq;

    public partial class OpenCLCommandQueue
    {
        #region CopyBuffer

        /// <summary>
        /// Enqueues a command to copy data from a source buffer to a destination buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffers. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBuffer(OpenCLBufferBase source, OpenCLBufferBase destination, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Copy(source, destination, 0, 0, source.TotalCount, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source buffer to a destination buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffers. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBuffer(OpenCLBufferBase source, OpenCLBufferBase destination, long sourceOffset, long destinationOffset, long region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, sourceOffset, destinationOffset, region, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source buffer to a destination buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffers. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBuffer(OpenCLBufferBase source, OpenCLBufferBase destination, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), 0, 0, 0, 0, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source buffer to a destination buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffers. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBuffer(OpenCLBufferBase source, OpenCLBufferBase destination, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, sourceOffset, destinationOffset, region, 0, 0, 0, 0, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source buffer to a destination buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffers. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="sourceRowPitch"> The size of a row of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationRowPitch"> The size of a row of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBuffer(OpenCLBufferBase source, OpenCLBufferBase destination, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, long sourceRowPitch, long destinationRowPitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source buffer to a destination buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffers. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="sourceRowPitch"> The size of a row of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationRowPitch"> The size of a row of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="sourceSlicePitch"> The size of a 2D slice of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationSlicePitch"> The size of a 2D slice of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBuffer(OpenCLBufferBase source, OpenCLBufferBase destination, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, long sourceRowPitch, long destinationRowPitch, long sourceSlicePitch, long destinationSlicePitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, sourceOffset, destinationOffset, region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, events, newEvents);
        }

        #endregion

        #region CopyBufferToImage

        /// <summary>
        /// Enqueues a command to copy data from a buffer to an image.
        /// </summary>
        /// <typeparam name="T"> The type of data in <paramref name="source"/>. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBufferToImage(OpenCLBufferBase source, OpenCLImage destination, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, 0, new SysIntX3(), new SysIntX3(destination.Width, destination.Height, (destination.Depth == 0) ? 1 : destination.Depth), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a buffer to an image.
        /// </summary>
        /// <typeparam name="T"> The type of data in <paramref name="source"/>. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBufferToImage(OpenCLBufferBase source, OpenCLImage2D destination, long sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, sourceOffset, new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a buffer to an image.
        /// </summary>
        /// <typeparam name="T"> The type of data in <paramref name="source"/>. </typeparam>
        /// <param name="source"> The buffer to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyBufferToImage(OpenCLBufferBase source, OpenCLImage3D destination, long sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, sourceOffset, destinationOffset, region, events, newEvents);
        }

        #endregion

        #region CopyImage

        /// <summary>
        /// Enqueues a command to copy data from a source image to a destination image.
        /// </summary>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImage(OpenCLImage source, OpenCLImage destination, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Copy(source, destination, new SysIntX3(), new SysIntX3(), new SysIntX3(source.Width, source.Height, (source.Depth == 0) ? 1 : source.Depth), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source image to a destination image.
        /// </summary>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImage(OpenCLImage2D source, OpenCLImage2D destination, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Copy(source, destination, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source image to a destination image.
        /// </summary>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImage(OpenCLImage2D source, OpenCLImage3D destination, SysIntX2 sourceOffset, SysIntX3 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Copy(source, destination, new SysIntX3(sourceOffset, 0), destinationOffset, new SysIntX3(region, 1), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source image to a destination image.
        /// </summary>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImage(OpenCLImage3D source, OpenCLImage2D destination, SysIntX3 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Copy(source, destination, sourceOffset, new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a source image to a destination image.
        /// </summary>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The image to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImage(OpenCLImage3D source, OpenCLImage3D destination, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Copy(source, destination, sourceOffset, destinationOffset, region, events, newEvents);
        }

        #endregion

        #region CopyImageToBuffer

        /// <summary>
        /// Enqueues a command to copy data from an image to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in <paramref name="destination"/>. </typeparam>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImageToBuffer(OpenCLImage source, OpenCLBufferBase destination, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, new SysIntX3(), 0, new SysIntX3(source.Width, source.Height, (source.Depth == 0) ? 1 : source.Depth), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from an image to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in <paramref name="destination"/>. </typeparam>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImageToBuffer(OpenCLImage2D source, OpenCLBufferBase destination, SysIntX2 sourceOffset, long destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, new SysIntX3(sourceOffset, 0), destinationOffset, new SysIntX3(region, 1), events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to copy data from a 3D image to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in <paramref name="destination"/>. </typeparam>
        /// <param name="source"> The image to copy from. </param>
        /// <param name="destination"> The buffer to copy to. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to copy. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void CopyImageToBuffer(OpenCLImage3D source, OpenCLBufferBase destination, SysIntX3 sourceOffset, long destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            Copy(source, destination, sourceOffset, destinationOffset, region, events, newEvents);
        }

        #endregion

        #region ReadFromBuffer

        /// <summary>
        /// Enqueues a command to read data from a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The buffer to read from. </param>
        /// <param name="destination"> The array to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromBuffer(OpenCLBufferBase source, ref Array destination, bool blocking, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            ReadFromBuffer(source, ref destination, blocking, 0, 0, source.TotalCount, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to read data from a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The buffer to read from. </param>
        /// <param name="destination"> The array to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromBuffer(OpenCLBufferBase source, ref Array destination, bool blocking, long sourceOffset, long destinationOffset, long region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            GCHandle destinationGCHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);
            IntPtr destinationOffsetPtr = Marshal.UnsafeAddrOfPinnedArrayElement(destination, (int)destinationOffset);
            
            if (blocking)
            {
                Read(source, blocking, sourceOffset, region, destinationOffsetPtr, events, newEvents);
                destinationGCHandle.Free();
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Read(source, blocking, sourceOffset, region, destinationOffsetPtr, events, eventList);
                OpenCLEvent newEvent = (OpenCLEvent)eventList.Last();
                newEvent.TrackGCHandle(destinationGCHandle);
            }
        }
        public void ReadFromBuffer(OpenCLBufferBase source, IntPtr destination, bool blocking, long offset, long region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            if (blocking)
            {
                Read(source, blocking, offset, region, destination, events, newEvents);
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Read(source, blocking, offset, region, destination, events, newEvents);
            }
        }

        /// <summary>
        /// Enqueues a command to read data from a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The buffer to read from. </param>
        /// <param name="destination"> The array to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromBuffer(OpenCLBufferBase source, ref Array destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            ReadFromBuffer(source, ref destination, blocking, sourceOffset, destinationOffset, region, 0, 0, events, newEvents);
        }
        public void ReadFromBuffer(OpenCLBufferBase source, IntPtr destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            ReadFromBuffer(source, destination, blocking, sourceOffset, region, 0, 0, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to read data from a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The buffer to read from. </param>
        /// <param name="destination"> The array to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromBuffer(OpenCLBufferBase source, ref Array destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            ReadFromBuffer(source, ref destination, blocking, sourceOffset, destinationOffset, region, 0, 0, 0, 0, events, newEvents);
        }
        public void ReadFromBuffer(OpenCLBufferBase source, IntPtr destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            ReadFromBuffer(source, destination, blocking, sourceOffset, region, 0, 0, 0, 0, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to read data from a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The buffer to read from. </param>
        /// <param name="destination"> The array to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="sourceRowPitch"> The size of a row of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationRowPitch"> The size of a row of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromBuffer(OpenCLBufferBase source, ref Array destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, long sourceRowPitch, long destinationRowPitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            GCHandle destinationGCHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);

            if (blocking)
            {
                Read(source, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, destinationGCHandle.AddrOfPinnedObject(), events, newEvents);
                destinationGCHandle.Free();
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Read(source, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, destinationGCHandle.AddrOfPinnedObject(), events, eventList);
                OpenCLEvent newEvent = (OpenCLEvent)eventList.Last();
                newEvent.TrackGCHandle(destinationGCHandle);
            }
        }
        public void ReadFromBuffer(OpenCLBufferBase source, IntPtr destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 region, long sourceRowPitch, long destinationRowPitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {            
            if (blocking)
            {
                Read(source, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(0, 0, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, destination, events, newEvents);
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Read(source, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(0, 0, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, destination, events, eventList);
            }
        }


        /// <summary>
        /// Enqueues a command to read data from a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The buffer to read from. </param>
        /// <param name="destination"> The array to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="sourceRowPitch"> The size of a row of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationRowPitch"> The size of a row of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="sourceSlicePitch"> The size of a 2D slice of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationSlicePitch"> The size of a 2D slice of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromBuffer(OpenCLBufferBase source, ref Array destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, long sourceRowPitch, long destinationRowPitch, long sourceSlicePitch, long destinationSlicePitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            GCHandle destinationGCHandle = GCHandle.Alloc(destination, GCHandleType.Pinned);

            if (blocking)
            {
                Read(source, blocking, sourceOffset, destinationOffset, region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, destinationGCHandle.AddrOfPinnedObject(), events, newEvents);
                destinationGCHandle.Free();
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Read(source, blocking, sourceOffset, destinationOffset, region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, destinationGCHandle.AddrOfPinnedObject(), events, eventList);
                OpenCLEvent newEvent = (OpenCLEvent)eventList.Last();
                newEvent.TrackGCHandle(destinationGCHandle);
            }
        }
        public void ReadFromBuffer(OpenCLBufferBase source, IntPtr destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 region, long sourceRowPitch, long destinationRowPitch, long sourceSlicePitch, long destinationSlicePitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            if (blocking)
            {
                Read(source, blocking, sourceOffset, new SysIntX3(0, 0, 0), region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, destination, events, newEvents);
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Read(source, blocking, sourceOffset, new SysIntX3(0, 0, 0), region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, destination, events, eventList);
            }
        }

        #endregion

        #region ReadFromImage

        /// <summary>
        /// Enqueues a command to read data from an image.
        /// </summary>
        /// <param name="source"> The image to read from. </param>
        /// <param name="destination"> A valid pointer to a preallocated memory area to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromImage(OpenCLImage source, IntPtr destination, bool blocking, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Read(source, blocking, new SysIntX3(), new SysIntX3(source.Width, source.Height, (source.Depth == 0) ? 1 : source.Depth), 0, 0, destination, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to read data from an image.
        /// </summary>
        /// <param name="source"> The image to read from. </param>
        /// <param name="destination"> A valid pointer to a preallocated memory area to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromImage(OpenCLImage2D source, IntPtr destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Read(source, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(region, 1), 0, 0, destination, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to read data from an image.
        /// </summary>
        /// <param name="source"> The image to read from. </param>
        /// <param name="destination"> A valid pointer to a preallocated memory area to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromImage(OpenCLImage3D source, IntPtr destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Read(source, blocking, sourceOffset, region, 0, 0, destination, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to read data from an image.
        /// </summary>
        /// <param name="source"> The image to read from. </param>
        /// <param name="destination"> A valid pointer to a preallocated memory area to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="sourceRowPitch"> The size of a row of pixels of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromImage(OpenCLImage2D source, IntPtr destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 region, long sourceRowPitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Read(source, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destination, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to read data from an image.
        /// </summary>
        /// <param name="source"> The image to read from. </param>
        /// <param name="destination"> A valid pointer to a preallocated memory area to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="region"> The region of elements to read. </param>
        /// <param name="sourceRowPitch"> The size of a row of pixels of <paramref name="destination"/> in bytes. </param>
        /// <param name="sourceSlicePitch"> The size of a 2D slice of pixels of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void ReadFromImage(OpenCLImage3D source, IntPtr destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 region, long sourceRowPitch, long sourceSlicePitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Read(source, blocking, sourceOffset, region, sourceRowPitch, sourceSlicePitch, destination, events, newEvents);
        }

        #endregion

        #region WriteToBuffer

        /// <summary>
        /// Enqueues a command to write data to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The array to read from. </param>
        /// <param name="destination"> The buffer to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        /*public void WriteToBuffer(Array source, OpenCLBufferBase destination, bool blocking, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            WriteToBuffer(source, destination, blocking, 0, 0, destination.TotalCount, events, newEvents);
        }*/
        
        /// <summary>
        /// Enqueues a command to write data to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The array to read from. </param>
        /// <param name="destination"> The buffer to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        /*public void WriteToBuffer(Array source, OpenCLBufferBase destination, bool blocking, long sourceOffset, long destinationOffset, long region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            GCHandle sourceGCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);
            IntPtr sourceOffsetPtr = Marshal.UnsafeAddrOfPinnedArrayElement(source, (int)sourceOffset);

            if (blocking)
            {
                Write(destination, blocking, destinationOffset, region, sourceOffsetPtr, events, newEvents);
                sourceGCHandle.Free();
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Write(destination, blocking, destinationOffset, region, sourceOffsetPtr, events, eventList);
                OpenCLEvent newEvent = (OpenCLEvent)eventList.Last();
                newEvent.TrackGCHandle(sourceGCHandle);
            }
        }*/
        public void WriteToBuffer(IntPtr source, OpenCLBufferBase destination, bool blocking,  long offset, long region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            if (blocking)
            {
                Write(destination, blocking, offset, region, source, events, newEvents);
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Write(destination, blocking, offset, region, source, events, eventList);
            }
        }

        /// <summary>
        /// Enqueues a command to write data to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The array to read from. </param>
        /// <param name="destination"> The buffer to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        /*public void WriteToBuffer(Array source, OpenCLBufferBase destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            WriteToBuffer(source, destination, blocking, sourceOffset, destinationOffset, region, 0, 0, events, newEvents);
        }*/
        public void WriteToBuffer(IntPtr source, OpenCLBufferBase destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            WriteToBuffer(source, destination, blocking, sourceOffset, destinationOffset, region, 0, 0, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to write data to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The array to read from. </param>
        /// <param name="destination"> The buffer to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        /*public void WriteToBuffer(Array source, OpenCLBufferBase destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            WriteToBuffer(source, destination, blocking, sourceOffset, destinationOffset, region, 0, 0, 0, 0, events, newEvents);
        }*/
        public void WriteToBuffer(IntPtr source, OpenCLBufferBase destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            WriteToBuffer(source, destination, blocking, sourceOffset, destinationOffset, region, 0, 0, 0, 0, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to write data to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The array to read from. </param>
        /// <param name="destination"> The buffer to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="sourceRowPitch"> The size of a row of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationRowPitch"> The size of a row of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        /*public void WriteToBuffer(Array source, OpenCLBufferBase destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, long sourceRowPitch, long destinationRowPitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            GCHandle sourceGCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);

            if (blocking)
            {
                Write(destination, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, sourceGCHandle.AddrOfPinnedObject(), events, newEvents);
                sourceGCHandle.Free();
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Write(destination, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, sourceGCHandle.AddrOfPinnedObject(), events, eventList);
                OpenCLEvent newEvent = (OpenCLEvent)eventList.Last();
                newEvent.TrackGCHandle(sourceGCHandle);
            }
        }*/
        public void WriteToBuffer(IntPtr source, OpenCLBufferBase destination, bool blocking, SysIntX2 sourceOffset, SysIntX2 destinationOffset, SysIntX2 region, long sourceRowPitch, long destinationRowPitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            if (blocking)
            {
                Write(destination, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, source, events, newEvents);                
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Write(destination, blocking, new SysIntX3(sourceOffset, 0), new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), sourceRowPitch, 0, destinationRowPitch, 0, source, events, eventList);
                
            }
        }

        /// <summary>
        /// Enqueues a command to write data to a buffer.
        /// </summary>
        /// <typeparam name="T"> The type of data in the buffer. </typeparam>
        /// <param name="source"> The array to read from. </param>
        /// <param name="destination"> The buffer to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="sourceOffset"> The <paramref name="source"/> element position where reading starts. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="sourceRowPitch"> The size of a row of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationRowPitch"> The size of a row of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="sourceSlicePitch"> The size of a 2D slice of elements of <paramref name="source"/> in bytes. </param>
        /// <param name="destinationSlicePitch"> The size of a 2D slice of elements of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        /*public void WriteToBuffer(Array source, OpenCLBufferBase destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, long sourceRowPitch, long destinationRowPitch, long sourceSlicePitch, long destinationSlicePitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null) 
        {
            GCHandle sourceGCHandle = GCHandle.Alloc(source, GCHandleType.Pinned);

            if (blocking)
            {
                Write(destination, blocking, sourceOffset, destinationOffset, region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, sourceGCHandle.AddrOfPinnedObject(), events, newEvents);
                sourceGCHandle.Free();
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Write(destination, blocking, sourceOffset, destinationOffset, region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, sourceGCHandle.AddrOfPinnedObject(), events, eventList);
                OpenCLEvent newEvent = (OpenCLEvent)eventList.Last();
                newEvent.TrackGCHandle(sourceGCHandle);
            }
        }*/
        public void WriteToBuffer(IntPtr source, OpenCLBufferBase destination, bool blocking, SysIntX3 sourceOffset, SysIntX3 destinationOffset, SysIntX3 region, long sourceRowPitch, long destinationRowPitch, long sourceSlicePitch, long destinationSlicePitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            if (blocking)
            {
                Write(destination, blocking, sourceOffset, destinationOffset, region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, source, events, newEvents);
            }
            else
            {
                IList<OpenCLEventBase> eventList = (newEvents != null) ? newEvents : Events;
                Write(destination, blocking, sourceOffset, destinationOffset, region, sourceRowPitch, sourceSlicePitch, destinationRowPitch, destinationSlicePitch, source, events, eventList);

            }
        }

        #endregion

        #region WriteToImage

        /// <summary>
        /// Enqueues a command to write data to an image.
        /// </summary>
        /// <param name="source"> A pointer to a memory area to read from. </param>
        /// <param name="destination"> The image to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void WriteToImage(IntPtr source, OpenCLImage destination, bool blocking, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Write(destination, blocking, new SysIntX3(), new SysIntX3(destination.Width, destination.Height, (destination.Depth == 0) ? 1 : destination.Depth), 0, 0, source, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to write data to an image.
        /// </summary>
        /// <param name="source"> A pointer to a memory area to read from. </param>
        /// <param name="destination"> The image to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void WriteToImage(IntPtr source, OpenCLImage2D destination, bool blocking, SysIntX2 destinationOffset, SysIntX2 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Write(destination, blocking, new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), 0, 0, source, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to write data to an image.
        /// </summary>
        /// <param name="source"> A pointer to a memory area to read from. </param>
        /// <param name="destination"> The image to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void WriteToImage(IntPtr source, OpenCLImage3D destination, bool blocking, SysIntX3 destinationOffset, SysIntX3 region, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Write(destination, blocking, destinationOffset, region, 0, 0, source, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to write data to an image.
        /// </summary>
        /// <param name="source"> A pointer to a memory area to read from. </param>
        /// <param name="destination"> The image to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="destinationRowPitch"> The size of a row of pixels of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void WriteToImage(IntPtr source, OpenCLImage2D destination, bool blocking, SysIntX2 destinationOffset, SysIntX2 region, long destinationRowPitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Write(destination, blocking, new SysIntX3(destinationOffset, 0), new SysIntX3(region, 1), destinationRowPitch, 0, source, events, newEvents);
        }

        /// <summary>
        /// Enqueues a command to write data to an image.
        /// </summary>
        /// <param name="source"> A pointer to a memory area to read from. </param>
        /// <param name="destination"> The image to write to. </param>
        /// <param name="blocking"> The mode of operation of this command. If <c>true</c> this call will not return until the command has finished execution. </param>
        /// <param name="destinationOffset"> The <paramref name="destination"/> element position where writing starts. </param>
        /// <param name="region"> The region of elements to write. </param>
        /// <param name="destinationRowPitch"> The size of a row of pixels of <paramref name="destination"/> in bytes. </param>
        /// <param name="destinationSlicePitch"> The size of a 2D slice of pixels of <paramref name="destination"/> in bytes. </param>
        /// <param name="events"> A collection of events that need to complete before this particular command can be executed. If <paramref name="events"/> is not <c>null</c> a new event identifying this command is attached to the end of the collection. </param>
        public void WriteToImage(IntPtr source, OpenCLImage3D destination, bool blocking, SysIntX3 destinationOffset, SysIntX3 region, long destinationRowPitch, long destinationSlicePitch, IReadOnlyList<OpenCLEventBase> events = null, IList<OpenCLEventBase> newEvents = null)
        {
            Write(destination, blocking, destinationOffset, region, destinationRowPitch, destinationSlicePitch, source, events, newEvents);
        }

        #endregion
    }
}