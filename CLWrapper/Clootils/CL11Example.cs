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
using Cloo;

namespace Clootils
{
    class CL11Example : IExample
    {
        public string Name
        {
            get { return "OpenCL 1.1"; }
        }

        public string Description
        {
            get { return "Demonstrates some of the new features of OpenCL 1.1."; }
        }

        public void Run(ComputeContext context, TextWriter log)
        {
            try
            {
                log.Write("Creating command queue... ");
                ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                log.WriteLine("done.");

                log.Write("Generating data... ");

                int linearSize = 24;
                SysIntX2 rectSize = new SysIntX2(4, 6);
                SysIntX3 cubicSize = new SysIntX3(2, 3, 4);
                float[] linearIn = new float[linearSize];
                float[] linearOut = new float[linearSize];
                float[,] rectIn = new float[(int)rectSize.Y, (int)rectSize.X];
                float[,] rectOut = new float[(int)rectSize.Y, (int)rectSize.X];
                float[, ,] cubicIn = new float[(int)cubicSize.Z, (int)cubicSize.Y, (int)cubicSize.X];
                float[, ,] cubicOut = new float[(int)cubicSize.Z, (int)cubicSize.Y, (int)cubicSize.X];

                for (int i = 0; i < linearSize; i++)
                    linearIn[i] = i;

                for (int i = 0; i < (int)rectSize.X; i++)
                    for (int j = 0; j < (int)rectSize.Y; j++)
                        rectIn[j, i] = (float)(rectSize.X.ToInt32() * j + i);

                for (int i = 0; i < (int)cubicSize.X; i++)
                    for (int j = 0; j < (int)cubicSize.Y; j++)
                        for( int k = 0; k < (int)cubicSize.Z; k++ )
                            cubicIn[k, j, i] = (float)(k * cubicSize.Y.ToInt32() * cubicSize.X.ToInt32() + cubicSize.X.ToInt32() * j + i);

                log.WriteLine("done.");

                log.Write("Creating buffer... ");
                ComputeBuffer<float> buffer = new ComputeBuffer<float>(context, ComputeMemoryFlags.ReadWrite, linearSize);
                log.WriteLine("done.");

                GC.Collect();

                log.Write("Writing to buffer (linear)... ");
                commands.WriteToBuffer(linearIn, buffer, false, null);
                log.WriteLine("done.");

                log.Write("Reading from buffer (linear)... ");
                commands.ReadFromBuffer(buffer, ref linearOut, false, null);
                log.WriteLine("done.");

                GC.Collect();

                commands.Finish();

                log.Write("Comparing data... ");
                Compare(linearIn, linearOut);
                log.WriteLine("passed.");

                GC.Collect();

                log.Write("Writing to buffer (rectangular)... ");
                commands.WriteToBuffer(rectIn, buffer, false, new SysIntX2(), new SysIntX2(), rectSize, null);
                log.WriteLine("done.");

                GC.Collect();

                log.Write("Reading from buffer (rectangular)... ");
                commands.ReadFromBuffer(buffer, ref rectOut, false, new SysIntX2(), new SysIntX2(), rectSize, null);
                log.WriteLine("done.");

                GC.Collect();

                commands.Finish();

                log.Write("Comparing data... ");
                Compare(rectIn, rectOut);
                log.WriteLine("passed.");

                GC.Collect();
                
                log.Write("Writing to buffer (cubic)... ");
                commands.WriteToBuffer(cubicIn, buffer, false, new SysIntX3(), new SysIntX3(), cubicSize, null);
                log.WriteLine("done.");

                GC.Collect();
                
                log.Write("Reading from buffer (cubic)... ");
                commands.ReadFromBuffer(buffer, ref cubicOut, false, new SysIntX3(), new SysIntX3(), cubicSize, null);
                log.WriteLine("done.");

                GC.Collect();
                
                commands.Finish();

                log.Write("Comparing data... ");
                Compare(cubicIn, cubicOut);
                log.WriteLine("passed.");
            }
            catch (Exception e)
            {
                log.WriteLine(e.ToString());
            }
        }

        private void Compare<T>(T[] a1, T[] a2)
        {
            for (int i = 0; i < a1.Length; i++)
                if (!a1[i].Equals(a2[i]))
                    throw new Exception("FAILED!");
        }

        private void Compare<T>(T[,] a1, T[,] a2)
        {
            for (int i = 0; i < a1.GetLength(0); i++)
                for (int j = 0; j < a1.GetLength(1); j++)
                    if (!a1[i, j].Equals(a2[i, j]))
                        throw new Exception("FAILED!");
        }

        private void Compare<T>(T[, ,] a1, T[, ,] a2)
        {
            for (int i = 0; i < a1.GetLength(0); i++)
                for (int j = 0; j < a1.GetLength(1); j++)
                    for (int k = 0; k < a1.GetLength(2); k++)
                        if (!a1[i, j, k].Equals(a2[i, j, k]))
                            throw new Exception("FAILED!");
        }
    }
}