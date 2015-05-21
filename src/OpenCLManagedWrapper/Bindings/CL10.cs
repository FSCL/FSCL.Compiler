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

namespace OpenCL.Bindings
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    /// <summary>
    /// Contains bindings to the OpenCL 1.0 functions.
    /// </summary>
    /// <remarks> See the OpenCL specification for documentation regarding these functions. </remarks>
    [SuppressUnmanagedCodeSecurity]
    public class CL10
    {
        /// <summary>
        /// The name of the library that contains the available OpenCL function points.
        /// </summary>
        protected const string libName = "OpenCL.dll";

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetPlatformIDs")]
        public extern static OpenCLErrorCode GetPlatformIDs(
            Int32 num_entries,
            [Out, MarshalAs(UnmanagedType.LPArray)] CLPlatformHandle[] platforms,
            out Int32 num_platforms);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetPlatformInfo")]
        public extern static OpenCLErrorCode GetPlatformInfo(
            CLPlatformHandle platform,
            OpenCLPlatformInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetDeviceIDs")]
        public extern static OpenCLErrorCode GetDeviceIDs(
            CLPlatformHandle platform,
            OpenCLDeviceType device_type,
            Int32 num_entries,
            [Out, MarshalAs(UnmanagedType.LPArray)] CLDeviceHandle[] devices,
            out Int32 num_devices);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetDeviceInfo")]
        public extern static OpenCLErrorCode GetDeviceInfo(
            CLDeviceHandle device,
            OpenCLDeviceInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateContext")]
        public extern static CLContextHandle CreateContext(
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] properties,
            Int32 num_devices,
            [MarshalAs(UnmanagedType.LPArray)] CLDeviceHandle[] devices,
            OpenCLContextNotifier pfn_notify,
            IntPtr user_data,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateContextFromType")]
        public extern static CLContextHandle CreateContextFromType(
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] properties,
            OpenCLDeviceType device_type,
            OpenCLContextNotifier pfn_notify,
            IntPtr user_data,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clRetainContext")]
        public extern static OpenCLErrorCode RetainContext(
            CLContextHandle context);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clReleaseContext")]
        public extern static OpenCLErrorCode ReleaseContext(
            CLContextHandle context);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetContextInfo")]
        public extern static OpenCLErrorCode GetContextInfo(
            CLContextHandle context,
            OpenCLContextInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateCommandQueue")]
        public extern static CLCommandQueueHandle CreateCommandQueue(
            CLContextHandle context,
            CLDeviceHandle device,
            OpenCLCommandQueueProperties properties,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clRetainCommandQueue")]
        public extern static OpenCLErrorCode RetainCommandQueue(
            CLCommandQueueHandle command_queue);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clReleaseCommandQueue")]
        public extern static OpenCLErrorCode
        ReleaseCommandQueue(
            CLCommandQueueHandle command_queue);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetCommandQueueInfo")]
        public extern static OpenCLErrorCode GetCommandQueueInfo(
            CLCommandQueueHandle command_queue,
            OpenCLCommandQueueInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clSetCommandQueueProperty")]
        public extern static OpenCLErrorCode SetCommandQueueProperty(
            CLCommandQueueHandle command_queue,
            OpenCLCommandQueueProperties properties,
            [MarshalAs(UnmanagedType.Bool)] bool enable,
            out OpenCLCommandQueueProperties old_properties);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateBuffer")]
        public extern static CLMemoryHandle CreateBuffer(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            IntPtr size,
            IntPtr host_ptr,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateImage2D")]
        public extern static CLMemoryHandle CreateImage2D(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            ref OpenCLImageFormat image_format,
            IntPtr image_width,
            IntPtr image_height,
            IntPtr image_row_pitch,
            IntPtr host_ptr,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateImage3D")]
        public extern static CLMemoryHandle CreateImage3D(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            ref OpenCLImageFormat image_format,
            IntPtr image_width,
            IntPtr image_height,
            IntPtr image_depth,
            IntPtr image_row_pitch,
            IntPtr image_slice_pitch,
            IntPtr host_ptr,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clRetainMemObject")]
        public extern static OpenCLErrorCode RetainMemObject(
            CLMemoryHandle memobj);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clReleaseMemObject")]
        public extern static OpenCLErrorCode ReleaseMemObject(
            CLMemoryHandle memobj);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetSupportedImageFormats")]
        public extern static OpenCLErrorCode GetSupportedImageFormats(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            OpenCLMemoryType image_type,
            Int32 num_entries,
            [Out, MarshalAs(UnmanagedType.LPArray)] OpenCLImageFormat[] image_formats,
            out Int32 num_image_formats);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetMemObjectInfo")]
        public extern static OpenCLErrorCode GetMemObjectInfo(
            CLMemoryHandle memobj,
            OpenCLMemoryInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetImageInfo")]
        public extern static OpenCLErrorCode GetImageInfo(
            CLMemoryHandle image,
            OpenCLImageInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateSampler")]
        public extern static CLSamplerHandle CreateSampler(
            CLContextHandle context,
            [MarshalAs(UnmanagedType.Bool)] bool normalized_coords,
            OpenCLImageAddressing addressing_mode,
            OpenCLImageFiltering filter_mode,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clRetainSampler")]
        public extern static OpenCLErrorCode RetainSampler(
            CLSamplerHandle sample);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clReleaseSampler")]
        public extern static OpenCLErrorCode ReleaseSampler(
            CLSamplerHandle sample);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetSamplerInfo")]
        public extern static OpenCLErrorCode GetSamplerInfo(
            CLSamplerHandle sample,
            OpenCLSamplerInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateProgramWithSource")]
        public extern static CLProgramHandle CreateProgramWithSource(
            CLContextHandle context,
            Int32 count,
            String[] strings,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] lengths,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateProgramWithBinary")]
        public extern static CLProgramHandle CreateProgramWithBinary(
            CLContextHandle context,
            Int32 num_devices,
            [MarshalAs(UnmanagedType.LPArray)] CLDeviceHandle[] device_list,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] lengths,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] binaries,
            [MarshalAs(UnmanagedType.LPArray)] Int32[] binary_status,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clRetainProgram")]
        public extern static OpenCLErrorCode RetainProgram(
            CLProgramHandle program);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clReleaseProgram")]
        public extern static OpenCLErrorCode ReleaseProgram(
            CLProgramHandle program);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clBuildProgram")]
        public extern static OpenCLErrorCode BuildProgram(
            CLProgramHandle program,
            Int32 num_devices,
            [MarshalAs(UnmanagedType.LPArray)] CLDeviceHandle[] device_list,
            String options,
            OpenCLProgramBuildNotifier pfn_notify,
            IntPtr user_data);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clUnloadCompiler")]
        public extern static OpenCLErrorCode UnloadCompiler();

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetProgramInfo")]
        public extern static OpenCLErrorCode GetProgramInfo(
            CLProgramHandle program,
            OpenCLProgramInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetProgramBuildInfo")]
        public extern static OpenCLErrorCode GetProgramBuildInfo(
            CLProgramHandle program,
            CLDeviceHandle device,
            OpenCLProgramBuildInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateKernel")]
        public extern static CLKernelHandle CreateKernel(
            CLProgramHandle program,
            String kernel_name,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateKernelsInProgram")]
        public extern static OpenCLErrorCode CreateKernelsInProgram(
            CLProgramHandle program,
            Int32 num_kernels,
            [Out, MarshalAs(UnmanagedType.LPArray)] CLKernelHandle[] kernels,
            out Int32 num_kernels_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clRetainKernel")]
        public extern static OpenCLErrorCode RetainKernel(
            CLKernelHandle kernel);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clReleaseKernel")]
        public extern static OpenCLErrorCode ReleaseKernel(
            CLKernelHandle kernel);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clSetKernelArg")]
        public extern static OpenCLErrorCode SetKernelArg(
            CLKernelHandle kernel,
            Int32 arg_index,
            IntPtr arg_size,
            IntPtr arg_value);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetKernelInfo")]
        public extern static OpenCLErrorCode GetKernelInfo(
            CLKernelHandle kernel,
            OpenCLKernelInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetKernelWorkGroupInfo")]
        public extern static OpenCLErrorCode GetKernelWorkGroupInfo(
            CLKernelHandle kernel,
            CLDeviceHandle device,
            OpenCLKernelWorkGroupInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clWaitForEvents")]
        public extern static OpenCLErrorCode WaitForEvents(
            Int32 num_events,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_list);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetEventInfo")]
        public extern static OpenCLErrorCode GetEventInfo(
            CLEventHandle @event,
            OpenCLEventInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clRetainEvent")]
        public extern static OpenCLErrorCode RetainEvent(
            CLEventHandle @event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clReleaseEvent")]
        public extern static OpenCLErrorCode ReleaseEvent(
            CLEventHandle @event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetEventProfilingInfo")]
        public extern static OpenCLErrorCode GetEventProfilingInfo(
            CLEventHandle @event,
            OpenCLCommandProfilingInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clFlush")]
        public extern static OpenCLErrorCode Flush(
            CLCommandQueueHandle command_queue);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clFinish")]
        public extern static OpenCLErrorCode Finish(
            CLCommandQueueHandle command_queue);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueReadBuffer")]
        public extern static OpenCLErrorCode EnqueueReadBuffer(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_read,
            IntPtr offset,
            IntPtr cb,
            IntPtr ptr,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueWriteBuffer")]
        public extern static OpenCLErrorCode EnqueueWriteBuffer(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_write,
            IntPtr offset,
            IntPtr cb,
            IntPtr ptr,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueCopyBuffer")]
        public extern static OpenCLErrorCode EnqueueCopyBuffer(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle src_buffer,
            CLMemoryHandle dst_buffer,
            IntPtr src_offset,
            IntPtr dst_offset,
            IntPtr cb,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueReadImage")]
        public extern static OpenCLErrorCode EnqueueReadImage(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle image,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_read,
            ref SysIntX3 origin,
            ref SysIntX3 region,
            IntPtr row_pitch,
            IntPtr slice_pitch,
            IntPtr ptr,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueWriteImage")]
        public extern static OpenCLErrorCode EnqueueWriteImage(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle image,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_write,
            ref SysIntX3 origin,
            ref SysIntX3 region,
            IntPtr input_row_pitch,
            IntPtr input_slice_pitch,
            IntPtr ptr,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueCopyImage")]
        public extern static OpenCLErrorCode EnqueueCopyImage(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle src_image,
            CLMemoryHandle dst_image,
            ref SysIntX3 src_origin,
            ref SysIntX3 dst_origin,
            ref SysIntX3 region,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueCopyImageToBuffer")]
        public extern static OpenCLErrorCode EnqueueCopyImageToBuffer(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle src_image,
            CLMemoryHandle dst_buffer,
            ref SysIntX3 src_origin,
            ref SysIntX3 region,
            IntPtr dst_offset,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueCopyBufferToImage")]
        public extern static OpenCLErrorCode EnqueueCopyBufferToImage(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle src_buffer,
            CLMemoryHandle dst_image,
            IntPtr src_offset,
            ref SysIntX3 dst_origin,
            ref SysIntX3 region,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueMapBuffer")]
        public extern static IntPtr EnqueueMapBuffer(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle buffer,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_map,
            OpenCLMemoryMappingFlags map_flags,
            IntPtr offset,
            IntPtr cb,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueMapImage")]
        public extern static IntPtr EnqueueMapImage(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle image,
            [MarshalAs(UnmanagedType.Bool)] bool blocking_map,
            OpenCLMemoryMappingFlags map_flags,
            ref SysIntX3 origin,
            ref SysIntX3 region,
            out IntPtr image_row_pitch,
            out IntPtr image_slice_pitch,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] CLEventHandle[] new_event,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueUnmapMemObject")]
        public extern static OpenCLErrorCode EnqueueUnmapMemObject(
            CLCommandQueueHandle command_queue,
            CLMemoryHandle memobj,
            IntPtr mapped_ptr,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueNDRangeKernel")]
        public extern static OpenCLErrorCode EnqueueNDRangeKernel(
            CLCommandQueueHandle command_queue,
            CLKernelHandle kernel,
            Int32 work_dim,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] global_work_offset,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] global_work_size,
            [MarshalAs(UnmanagedType.LPArray)] IntPtr[] local_work_size,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueTask")]
        public extern static OpenCLErrorCode EnqueueTask(
            CLCommandQueueHandle command_queue,
            CLKernelHandle kernel,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        // <summary>
        // See the OpenCL specification.
        // </summary>
        /*
        [DllImport(libName, EntryPoint = "clEnqueueNativeKernel")]
        public extern static OpenCLErrorCode EnqueueNativeKernel(
            CLCommandQueueHandle command_queue,
            IntPtr user_func,
            IntPtr args,
            IntPtr cb_args,
            Int32 num_mem_objects,
            IntPtr* mem_list,
            IntPtr* args_mem_loc,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);
        */

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueMarker")]
        public extern static OpenCLErrorCode EnqueueMarker(
            CLCommandQueueHandle command_queue,
            out CLEventHandle new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueWaitForEvents")]
        public extern static OpenCLErrorCode EnqueueWaitForEvents(
            CLCommandQueueHandle command_queue,
            Int32 num_events,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_list);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueBarrier")]
        public extern static OpenCLErrorCode EnqueueBarrier(
            CLCommandQueueHandle command_queue);

        
        /// <summary>
        /// Gets the extension function address for the given function name,
        /// or NULL if a valid function can not be found. The client must
        /// check to make sure the address is not NULL, before using or 
        /// calling the returned function address.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetExtensionFunctionAddress")]
        public extern static IntPtr GetExtensionFunctionAddress(
            String func_name);

        /**************************************************************************************/
        // CL/GL Sharing API

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateFromGLBuffer")]
        public extern static CLMemoryHandle CreateFromGLBuffer(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            Int32 bufobj,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateFromGLTexture2D")]
        public extern static CLMemoryHandle CreateFromGLTexture2D(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            Int32 target,
            Int32 miplevel,
            Int32 texture,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateFromGLTexture3D")]
        public extern static CLMemoryHandle CreateFromGLTexture3D(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            Int32 target,
            Int32 miplevel,
            Int32 texture,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clCreateFromGLRenderbuffer")]
        public extern static CLMemoryHandle CreateFromGLRenderbuffer(
            CLContextHandle context,
            OpenCLMemoryFlags flags,
            Int32 renderbuffer,
            out OpenCLErrorCode errcode_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetGLObjectInfo")]
        public extern static OpenCLErrorCode GetGLObjectInfo(
            CLMemoryHandle memobj,
            out OpenCLGLObjectType gl_object_type,
            out Int32 gl_object_name);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clGetGLTextureInfo")]
        public extern static OpenCLErrorCode GetGLTextureInfo(
            CLMemoryHandle memobj,
            OpenCLGLTextureInfo param_name,
            IntPtr param_value_size,
            IntPtr param_value,
            out IntPtr param_value_size_ret);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueAcquireGLObjects")]
        public extern static OpenCLErrorCode EnqueueAcquireGLObjects(
            CLCommandQueueHandle command_queue,
            Int32 num_objects,
            [MarshalAs(UnmanagedType.LPArray)] CLMemoryHandle[] mem_objects,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);

        /// <summary>
        /// See the OpenCL specification.
        /// </summary>
        [DllImport(libName, EntryPoint = "clEnqueueReleaseGLObjects")]
        public extern static OpenCLErrorCode EnqueueReleaseGLObjects(
            CLCommandQueueHandle command_queue,
            Int32 num_objects,
            [MarshalAs(UnmanagedType.LPArray)] CLMemoryHandle[] mem_objects,
            Int32 num_events_in_wait_list,
            [MarshalAs(UnmanagedType.LPArray)] CLEventHandle[] event_wait_list,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeConst=1)] CLEventHandle[] new_event);
    }

    /// <summary>
    /// A callback function that can be registered by the application to report information on errors that occur in the <see cref="OpenCLContext"/>.
    /// </summary>
    /// <param name="errorInfo"> An error string. </param>
    /// <param name="clDataPtr"> A pointer to binary data that is returned by the OpenCL implementation that can be used to log additional information helpful in debugging the error.</param>
    /// <param name="clDataSize"> The size of the binary data that is returned by the OpenCL. </param>
    /// <param name="userDataPtr"> The pointer to the optional user data specified in <paramref name="userDataPtr"/> argument of <see cref="OpenCLContext"/> constructor. </param>
    /// <remarks> This callback function may be called asynchronously by the OpenCL implementation. It is the application's responsibility to ensure that the callback function is thread-safe. </remarks>
    public delegate void OpenCLContextNotifier(String errorInfo, IntPtr clDataPtr, IntPtr clDataSize, IntPtr userDataPtr);

    /// <summary>
    /// A callback function that can be registered by the application to report the <see cref="OpenCLProgram"/> build status.
    /// </summary>
    /// <param name="programHandle"> The handle of the <see cref="OpenCLProgram"/> being built. </param>
    /// <param name="notifyDataPtr"> The pointer to the optional user data specified in <paramref name="notifyDataPtr"/> argument of <see cref="OpenCLProgram.Build"/>. </param>
    /// <remarks> This callback function may be called asynchronously by the OpenCL implementation. It is the application's responsibility to ensure that the callback function is thread-safe. </remarks>
    public delegate void OpenCLProgramBuildNotifier(CLProgramHandle programHandle, IntPtr notifyDataPtr);
}