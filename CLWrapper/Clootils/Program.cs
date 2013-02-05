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
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Clootils
{
    public class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [STAThread]
        public static void Main()
        {
            bool runningWin32NT = (Environment.OSVersion.Platform == PlatformID.Win32NT);
            bool consoleAllocated = false;
            int allocError = 0;
            bool runClootils = true;

            try
            {
                // This code works around an AMD OpenCL implementation problem,
                // which requires a console to be present during clBuildProgram().

                // Allocate a console.
                if (runningWin32NT)
                {
                    consoleAllocated = AllocConsole();
                    if (!consoleAllocated)
                    {
                        allocError = Marshal.GetLastWin32Error();
                        if (allocError != 0)
                            runClootils = (DialogResult.Yes == MessageBox.Show("Could not allocate console (error code: " + allocError + ").\nRunning examples on the AMD APP OpenCL platform might fail.\nContinue anyway?", "Clootils Error", MessageBoxButtons.YesNo, MessageBoxIcon.Warning));
                    }
                }

                // Run Clootils
                if (runClootils) Application.Run(new MainForm());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Clootils Error");
            }

            // Free the allocated console.
            if (consoleAllocated) FreeConsole();
        }
    }
}