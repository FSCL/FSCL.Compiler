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

using System;
using System.IO;
using System.Runtime.InteropServices;
using Cloo;

namespace Clootils
{
    public class MappingExample : IExample
    {
        public string Name
        {
            get { return "Buffer mapping"; }
        }

        public string Description
        {
            get { return "Shows how to map data from an OpenCL buffer to host memory"; }
        }

        public void Run(ComputeContext context, TextWriter log)
        {
            try
            {
                ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);

                log.WriteLine("Original content:");

                Random rand = new Random();
                int count = 6;
                long[] bufferContent = new long[count];
                for (int i = 0; i < count; i++)
                {
                    bufferContent[i] = (long)(rand.NextDouble() * long.MaxValue);
                    log.WriteLine("\t" + bufferContent[i]);
                }

                ComputeBuffer<long> buffer = new ComputeBuffer<long>(context, ComputeMemoryFlags.CopyHostPointer, bufferContent);
                
                IntPtr mappedPtr = commands.Map(buffer, true, ComputeMemoryMappingFlags.Read, 0, bufferContent.Length, null);

                log.WriteLine("Mapped content:");

                for (int i = 0; i < bufferContent.Length; i++)
                {
                    IntPtr ptr = new IntPtr(mappedPtr.ToInt64() + i * sizeof(long));
                    log.WriteLine("\t" + Marshal.ReadInt64(ptr));
                }

                commands.Unmap(buffer, ref mappedPtr, null);
            }
            catch (Exception e)
            {
                log.WriteLine(e.ToString());
            }
        }
    }
}