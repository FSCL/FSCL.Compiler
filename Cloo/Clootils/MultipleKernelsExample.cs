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
    public class MultipleKernelsExample : IExample
    {
        string kernelSources = @"
    kernel void k1(           float     bla ) {}
  //kernel void k2(           sampler_t bla ) {}       // Causes havoc in Nvidia's drivers. This is, however, a valid kernel signature.
  //kernel void k3( read_only image2d_t bla ) {}       // The same.
    kernel void k4( constant  float *   bla ) {}       
  //kernel void k5( global    float *   bla ) {}       // Causes InvalidBinary if Nvidia drivers == 64bit and application == 32 bit. Also valid.
    kernel void k6( local     float *   bla ) {}
";
        
        public string Name
        {
            get { return "Multiple kernels"; }
        }

        public string Description
        {
            get { return "Demonstrates how to build all the kernels in a program simultaneously."; }
        }

        public void Run(ComputeContext context, TextWriter log)
        {
            try
            {
                ComputeProgram program = new ComputeProgram(context, kernelSources);
                program.Build(null, null, null, IntPtr.Zero);
                log.WriteLine("Program successfully built.");
                program.CreateAllKernels();
                log.WriteLine("Kernels successfully created.");
            }
            catch (Exception e)
            {
                log.WriteLine(e.ToString());
            }
        }
    }
}