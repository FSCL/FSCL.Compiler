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
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents an OpenCL image format.
    /// </summary>
    /// <remarks> This structure defines the type, count and size of the image channels. </remarks>
    /// <seealso cref="OpenCLImage"/>
    [StructLayout(LayoutKind.Sequential)]
    public struct OpenCLImageFormat
    {
        #region Fields

        
        private readonly OpenCLImageChannelOrder channelOrder;

        
        private readonly OpenCLImageChannelType channelType;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="OpenCLImageChannelOrder"/> of the <see cref="OpenCLImage"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLImageChannelOrder"/> of the <see cref="OpenCLImage"/>. </value>
        public OpenCLImageChannelOrder ChannelOrder { get { return channelOrder; } }

        /// <summary>
        /// Gets the <see cref="OpenCLImageChannelType"/> of the <see cref="OpenCLImage"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLImageChannelType"/> of the <see cref="OpenCLImage"/>. </value>
        public OpenCLImageChannelType ChannelType { get { return channelType; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLImageFormat"/>.
        /// </summary>
        /// <param name="channelOrder"> The number of channels and the channel layout i.e. the memory layout in which channels are stored in the <see cref="OpenCLImage"/>. </param>
        /// <param name="channelType"> The type of the channel data. The number of bits per element determined by the <paramref name="channelType"/> and <paramref name="channelOrder"/> must be a power of two. </param>
        public OpenCLImageFormat(OpenCLImageChannelOrder channelOrder, OpenCLImageChannelType channelType)
        {
            this.channelOrder = channelOrder;
            this.channelType = channelType;
        }

        #endregion
    }
}