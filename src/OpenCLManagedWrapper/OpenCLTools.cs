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
    using System.Globalization;
    using OpenCL.Bindings;
    using System.Runtime.InteropServices;
    using System.Reflection;
    using System.Reflection.Emit;

    /// <summary>
    /// Contains various helper methods.
    /// </summary>
    public class OpenCLTools
    {
        #region Public methods

        /*
        /// <summary>
        /// Attempts to convert a PixelFormat to a <see cref="OpenCLImageFormat"/>.
        /// </summary>
        /// <param name="format"> The format to convert. </param>
        /// <returns> A <see cref="OpenCLImageFormat"/> that matches the specified argument. </returns>
        /// <remarks> Note that only <c>Alpha</c>, <c>Format16bppRgb555</c>, <c>Format16bppRgb565</c> and <c>Format32bppArgb</c> input values are currently supported. </remarks>        
        public static OpenCLImageFormat ConvertImageFormat(PixelFormat format)
        {
            switch(format)
            {
                case PixelFormat.Alpha:
                    return new OpenCLImageFormat(OpenCLImageChannelOrder.A, OpenCLImageChannelType.UnsignedInt8);
                case PixelFormat.Format16bppRgb555:
                    return new OpenCLImageFormat(OpenCLImageChannelOrder.Rgb, OpenCLImageChannelType.UNormShort555);
                case PixelFormat.Format16bppRgb565:
                    return new OpenCLImageFormat(OpenCLImageChannelOrder.Rgb, OpenCLImageChannelType.UNormShort565);
                case PixelFormat.Format32bppArgb:
                    return new OpenCLImageFormat(OpenCLImageChannelOrder.Argb, OpenCLImageChannelType.UnsignedInt8);
                default: throw new ArgumentException("Pixel format not supported.");
            }
        }
        */

        /// <summary>
        /// Parses an OpenCL version string.
        /// </summary>
        /// <param name="versionString"> The version string to parse. Must be in the format: <c>Additional substrings[space][major_version].[minor_version][space]Additional substrings</c>. </param>
        /// <param name="substringIndex"> The index of the substring that specifies the OpenCL version. </param>
        /// <returns> A <c>Version</c> instance containing the major and minor version from <paramref name="versionString"/>. </returns>
        public static Version ParseVersionString(String versionString, int substringIndex)
        {
            string[] verstring = versionString.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new Version(verstring[substringIndex]);
        }

        #endregion

        #region Internal methods

        internal static IntPtr[] ConvertArray(long[] array)
        {
            if (array == null) return null;

            
            IntPtr[] result = new IntPtr[array.Length];
            for (long i = 0; i < array.Length; i++)
                result[i] = new IntPtr(array[i]);
            return result;
        }

        internal static long[] ConvertArray(IntPtr[] array)
        {
            if (array == null) return null;

            
            long[] result = new long[array.Length];
            for (long i = 0; i < array.Length; i++)
                result[i] = array[i].ToInt64();
            return result;
        }
        
        internal static CLDeviceHandle[] ExtractHandles(IList<OpenCLDevice> computeObjects, out int handleCount)
        {
            if (computeObjects == null || computeObjects.Count == 0)
            {
                handleCount = 0;
                return null;
            }

            CLDeviceHandle[] result = new CLDeviceHandle[computeObjects.Count];
            int i = 0;
            foreach (OpenCLDevice computeObj in computeObjects)
            {
                result[i] = computeObj.Handle;
                i++;
            }
            handleCount = computeObjects.Count;
            return result;
        }

        internal static CLEventHandle[] ExtractHandles(IReadOnlyList<OpenCLEventBase> computeObjects, out int handleCount)
        {
            if (computeObjects == null || computeObjects.Count == 0)
            {
                handleCount = 0;
                return null;
            }

            CLEventHandle[] result = new CLEventHandle[computeObjects.Count];
            for (int i = 0; i < computeObjects.Count; i++)
            {
                result[i] = computeObjects[i].Handle;
            }
            handleCount = computeObjects.Count;
            return result;
        }

        internal static CLMemoryHandle[] ExtractHandles(IList<OpenCLMemory> computeObjects, out int handleCount)
        {
            if (computeObjects == null || computeObjects.Count == 0)
            {
                handleCount = 0;
                return null;
            }

            CLMemoryHandle[] result = new CLMemoryHandle[computeObjects.Count];
            int i = 0;
            foreach (OpenCLMemory computeObj in computeObjects)
            {
                result[i] = computeObj.Handle;
                i++;
            }
            handleCount = computeObjects.Count;
            return result;
        }
        
        #endregion
    }

    /*
    public class SizeHelper
    {
        private static Dictionary<Type, int> sizes = new Dictionary<Type, int>();
        private static Dictionary<Type, Object> defInstances = new Dictionary<Type, Object>();

        public static Object DefaultInstance(Type type)
        {
            if (!type.IsGenericType)
            {
                return Activator.CreateInstance(type);
            }

            Object obj;
            if (defInstances.TryGetValue(type, out obj))
            {
                return obj;
            }

            obj = DefaultInstanceOfType(type);
            defInstances.Add(type, obj);
            return obj;
        }

        public static int SizeOf(Type type)
        {
            if (!type.IsGenericType)
                return Marshal.SizeOf(type);

            int size;
            if (sizes.TryGetValue(type, out size))
            {
                return size;
            }

            size = SizeOfType(type);
            sizes.Add(type, size);
            return size;
        }

        private static int SizeOfType(Type type)
        {            
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, type);
            il.Emit(OpCodes.Ret);
            return (int)dm.Invoke(null, null);
        }
        private static Object DefaultInstanceOfType(Type type)
        {
            var args = type.GetGenericArguments();
            var vals = new Object[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                vals[i] = DefaultInstance(args[i]);
            }
            return Activator.CreateInstance(type, vals);
        }
    }*/
}