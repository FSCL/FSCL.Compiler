namespace FSCL.Compiler

open System 

module KernelLanguage =
    [<AllowNullLiteral>]
    type ConstantAttribute =
        inherit Attribute
        new() =  { }
    
    [<AllowNullLiteral>]
    type LocalAttribute =
        inherit Attribute
        new() =  { }

    type MemFenceMode =
    | CLK_LOCAL_MEM_FENCE
    | CLK_GLOBAL_MEM_FENCE    

    // Workspace functions
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

    // Workspace container
    type WorkItemIdContainer(global_size: int[], local_size: int[], global_id: int[], local_id: int []) =
        member this.GlobalID(i) =
            global_id.[i]
        member this.LocalID(i) =
            local_id.[i]
        member this.GlobalSize(i) =
            global_size.[i]
        member this.LocalSize(i) =
            local_size.[i]
        member this.NumGroups(i) =
            (int)(Math.Ceiling((float)global_size.[i] / (float)local_size.[i]))
        member this.GroupID(i) =
            (int)(Math.Floor((float)global_id.[i] / (float)global_size.[i]))
        member this.WorkDim() =
            global_size.Rank
    
    // Datatype mapping
    type uchar = Char
    type uint = Int
    type size_t = Int
    type half = Double
    type ulong = Long


    


