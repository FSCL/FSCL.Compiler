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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Cloo;

namespace Clootils
{
    class ImageExample : IExample
    {
        public string Name
        {
            get { return "ComputeImage2D and Bitmap"; }
        }

        public string Description
        {
            get { return "Demonstrates cooperation between OpenCL images and Bitmaps in Cloo."; }
        }

        public void Run(ComputeContext context, TextWriter log)
        {
            log.WriteLine("This test has been disabled.");

            try
            {/*
                log.Write("Creating command queue... ");
                ComputeCommandQueue commands = new ComputeCommandQueue(context, context.Devices[0], ComputeCommandQueueFlags.None);
                log.WriteLine("done.");
                
                int width = 16;
                int height = 16;

                log.Write("Creating first bitmap and drawing shapes... ");
                Bitmap firstBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(firstBitmap);                
                graphics.FillEllipse(Brushes.Red, 0, 0, width / 2, height / 2);
                graphics.FillRectangle(Brushes.Green, width / 2 + 1, 0, width / 2, height / 2);
                graphics.FillRectangle(Brushes.Blue, width / 2 + 1, height / 2 + 1, width / 2, height / 2);
                log.WriteLine("done.");

                log.Write("Creating OpenCL image with pixel data from the first bitmap... ");
                ComputeImage2D clImage = new ComputeImage2D(context, ComputeMemoryFlags.ReadWrite | ComputeMemoryFlags.CopyHostPointer, firstBitmap);
                log.WriteLine("done.");

                log.Write("Creating second bitmap... ");
                Bitmap secondBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                BitmapData bmpData = secondBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, secondBitmap.PixelFormat);
                log.WriteLine("done.");

                log.Write("Copying pixel data from OpenCL image to the second bitmap... ");
                commands.ReadFromImage(clImage, bmpData.Scan0, true, null);
                log.WriteLine("done.");
                
                secondBitmap.UnlockBits(bmpData);

                log.Write("Comparing bitmaps... ");
                for (int i = 0; i < width; i++)
                    for (int j = 0; j < height; j++)
                        if (firstBitmap.GetPixel(i, j) != secondBitmap.GetPixel(i, j))
                            throw new Exception("Image data mismatch!");
                log.WriteLine("passed.");
            */}
            catch (Exception e)
            {
                log.WriteLine(e.ToString());
            }
        }
    }
}