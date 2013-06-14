namespace FSCL.Compiler

open System 
///
///<summary>
///The module exposing functions and constructs that programmers can use inside kernels
///</summary>
///
module KernelLanguage =
    ///
    ///<summary>
    ///The attribute to mark a parameter to be allocated in the constant address-space
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type ConstantAttribute =
        inherit Attribute
        new() =  { }
    
    ///
    ///<summary>
    ///The attribute to mark a parameter to be allocated in the local address-space
    ///</summary>
    ///
    [<AllowNullLiteral>]
    type LocalAttribute =
        inherit Attribute
        new() =  { }
        
    ///
    ///<summary>
    ///Memory fance modes
    ///</summary>
    ///
    type MemFenceMode =
    | CLK_LOCAL_MEM_FENCE
    | CLK_GLOBAL_MEM_FENCE    
        
    ///
    ///<summary>
    ///OpenCL barrier function
    ///</summary>
    ///<param name="fenceMode">The memory fence mode</param>
    ///
    let barrier(fenceMode:MemFenceMode) =
        ()     
    ///
    ///<summary>
    ///OpenCL get_global_id function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The work-item global id relative to the input dimension</returns>
    ///
    let get_global_id(dim:int) = 
        0
    ///
    ///<summary>
    ///OpenCL get_local_id function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The work-item local id relative to the input dimension</returns>
    ///
    let get_local_id(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_global_size function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The workspace global size relative to the input dimension</returns>
    ///
    let get_global_size(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_local_size function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The workspace local size relative to the input dimension</returns>
    ///
    let get_local_size(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_num_groups function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The number of groups relative to the input dimension</returns>
    ///
    let get_num_groups(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_group_id function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The group id of the work-item relative to the input dimension</returns>
    ///
    let get_group_id(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_global_offset function
    ///</summary>
    ///<param name="dim">The dimension index</param>
    ///<returns>The global_offset relative to the input dimension</returns>
    ///
    let get_global_offset(dim:int) =
        0
    ///
    ///<summary>
    ///OpenCL get_work_idm function
    ///</summary>
    ///<returns>The number workspace dimensions</returns>
    ///
    let get_work_dim() =
        0
        
    ///
    ///<summary>
    ///The container of workspace related functions
    ///</summary>
    ///<remarks>
    ///This is not generally used directly since the runtime-support (i.e. the semantic) of workspace functions is given by the 
    ///OpenCL-to-device code compiler (e.g. Intel/AMD OpenCL compiler). Nevertheless this can help the FSCL runtime to produce
    ///debuggable multithrad implementation that programmers can use to test their kernels
    ///</remarks>
    ///
    type WorkItemIdContainer(global_size: int[], local_size: int[], global_id: int[], local_id: int [], global_offset: int[]) = 
        ///
        ///<summary>
        ///OpenCL get_global_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The work-item global id relative to the input dimension</returns>
        ///
        member this.GlobalID(dim) =
            global_id.[dim]
        ///
        ///<summary>
        ///OpenCL get_local_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The work-item local id relative to the input dimension</returns>
        ///
        member this.LocalID(dim) =
            local_id.[dim]
        ///
        ///<summary>
        ///OpenCL get_global_size function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The workspace global size relative to the input dimension</returns>
        ///
        member this.GlobalSize(dim) =
            global_size.[dim]
        ///
        ///<summary>
        ///OpenCL get_local_size function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The workspace local size relative to the input dimension</returns>
        ///
        member this.LocalSize(dim) =
            local_size.[dim]
        ///
        ///<summary>
        ///OpenCL get_num_groups function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The number of groups relative to the input dimension</returns>
        ///
        member this.NumGroups(dim) =
            (int)(Math.Ceiling((float)global_size.[dim] / (float)local_size.[dim]))
        ///
        ///<summary>
        ///OpenCL get_group_id function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The group id of the work-item relative to the input dimension</returns>
        ///
        member this.GroupID(dim) =
            (int)(Math.Floor((float)global_id.[dim] / (float)global_size.[dim]))
        ///
        ///<summary>
        ///OpenCL get_global_offset function
        ///</summary>
        ///<param name="dim">The dimension index</param>
        ///<returns>The global_offset relative to the input dimension</returns>
        ///
        member this.GlobalOffset(dim) =
            global_offset.[dim]
        ///
        ///<summary>
        ///OpenCL get_work_idm function
        ///</summary>
        ///<returns>The number workspace dimensions</returns>
        ///
        member this.WorkDim() =
            global_size.Rank
    
    // Datatype mapping    
    ///
    ///<summary>
    ///Alias of <see cref="System.Char"/>
    ///</summary>
    ///
    type uchar = UChar  
    ///
    ///<summary>
    ///Alias of <see cref="System.UInt32"/>
    ///</summary>
    ///
    type uint = UInt32      
    ///
    ///<summary>
    ///Alias of <see cref="System.UInt32"/>
    ///</summary>
    ///
    type size_t = UInt32      
    ///
    ///<summary>
    ///Alias of <see cref="System.Float16"/>
    ///</summary>
    ///
    type half = Float16  
    ///
    ///<summary>
    ///Alias of <see cref="System.ULong"/>
    ///</summary>
    ///
    type ulong = ULong


    


