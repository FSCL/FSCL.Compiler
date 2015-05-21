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

    /// <summary>
    /// Represents an OpenCL context property.
    /// </summary>
    /// <remarks> An OpenCL context property is a (name, value) data pair. </remarks>
    public class OpenCLContextProperty
    {
        #region Fields

        
        private readonly OpenCLContextProperties name;

        
        private readonly IntPtr value;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="OpenCLContextPropertyName"/> of the <see cref="OpenCLContextProperty"/>.
        /// </summary>
        /// <value> The <see cref="OpenCLContextPropertyName"/> of the <see cref="OpenCLContextProperty"/>. </value>
        public OpenCLContextProperties Name { get { return name; } }

        /// <summary>
        /// Gets the value of the <see cref="OpenCLContextProperty"/>.
        /// </summary>
        /// <value> The value of the <see cref="OpenCLContextProperty"/>. </value>
        public IntPtr Value { get { return value; } }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="OpenCLContextProperty"/>.
        /// </summary>
        /// <param name="name"> The name of the <see cref="OpenCLContextProperty"/>. </param>
        /// <param name="value"> The value of the created <see cref="OpenCLContextProperty"/>. </param>
        public OpenCLContextProperty(OpenCLContextProperties name, IntPtr value)
        {
            this.name = name;
            this.value = value;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Gets the string representation of the <see cref="OpenCLContextProperty"/>.
        /// </summary>
        /// <returns> The string representation of the <see cref="OpenCLContextProperty"/>. </returns>
        public override string ToString()
        {
            return GetType().Name + "(" + name + ", " + value + ")";
        }

        #endregion
    }
}