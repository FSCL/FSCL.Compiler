namespace FSCL

module KernelFunctions =
    type MemFenceMode =
    | CLK_LOCAL_MEM_FENCE
    | CLK_GLOBAL_MEM_FENCE    

    let barrier(fenceMode:MemFenceMode) =
        ()
    let get_global_id(dim:int) = 
        0
    let get_local_id(dim:int) =
        0
    let get_global_size(dim:int) =
        0
    let get_local_size(dim:int) =
        0
    let get_num_groups(dim:int) =
        0
    let get_group_id(dim:int) =
        0
    let get_global_offset(dim:int) =
        0
    let get_work_dim() =
        0

    type uchar = Char
    type uint = Int
    type size_t = Int
    type half = Double
    type ulong = Long
    


