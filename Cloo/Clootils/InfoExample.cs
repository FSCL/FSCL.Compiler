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
using System.Text;
using Cloo;

namespace Clootils
{
    public class InfoExample : IExample
    {
        public string Name
        {
            get { return "System info"; }
        }

        public string Description
        {
            get { return "Prints some information about the current platform and its devices."; }
        }

        public void Run(ComputeContext context, TextWriter log)
        {
            log.WriteLine("[HOST]");
            log.WriteLine(Environment.OSVersion);

            log.WriteLine();
            log.WriteLine("[OPENCL PLATFORM]");

            ComputePlatform platform = context.Platform;

            log.WriteLine("Name: " + platform.Name);
            log.WriteLine("Vendor: " + platform.Vendor);
            log.WriteLine("Version: " + platform.Version);
            log.WriteLine("Profile: " + platform.Profile);
            log.WriteLine("Extensions:");

            foreach (string extension in platform.Extensions)
                log.WriteLine(" + " + extension);

            log.WriteLine();

            log.WriteLine("Devices:");

            foreach (ComputeDevice device in context.Devices)
            {
                log.WriteLine("\tName: " + device.Name);
                log.WriteLine("\tVendor: " + device.Vendor);
                log.WriteLine("\tDriver version: " + device.DriverVersion);
                log.WriteLine("\tOpenCL version: " + device.Version);
                log.WriteLine("\tCompute units: " + device.MaxComputeUnits);
                log.WriteLine("\tGlobal memory: " + device.GlobalMemorySize + " bytes");
                log.WriteLine("\tLocal memory: " + device.LocalMemorySize + " bytes");
                log.WriteLine("\tImage support: " + device.ImageSupport);
                log.WriteLine("\tExtensions:");

                foreach (string extension in device.Extensions)
                    log.WriteLine("\t + " + extension);
            }
        }
    }
}