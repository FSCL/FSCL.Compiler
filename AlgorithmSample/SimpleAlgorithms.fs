module SimpleAlgorithms 

open FSCL.Compiler
open FSCL.Compiler.Language
    
// Vector addition
[<ReflectedDefinition>]
let VectorAdd(a: float32[], b: float32[], c: float32[]) =
    let gid = get_global_id(0)
    c.[gid] <- a.[gid] + b.[gid]

// Matrix convolution
[<ReflectedDefinition>]
let filterWidth = 3

[<ReflectedDefinition>]
let Convolution(input:float32[,], [<Constant>]filter:float32[,], output:float32[,], [<Local>]block:float32[,]) =
    let output_width = get_global_size(0)
    let input_width = output_width + filterWidth - 1
    let xOut = get_global_id(0)
    let yOut = get_global_id(1)

    let local_x = get_local_id(0)
    let local_y = get_local_id(1)
    let group_width = get_local_size(0)
    let group_height = get_local_size(1)
    let block_width = group_width + filterWidth - 1
    let block_height = group_height + filterWidth - 1

    //Set required rows into the LDS
    let mutable global_start_x = xOut
    let mutable global_start_y = yOut
    for local_start_x in local_x .. group_width .. block_width do
        for local_start_y in local_y .. group_height .. block_height do    
            block.[local_start_y, local_start_x] <- input.[global_start_y, global_start_x]
            global_start_x <- global_start_x + group_width
            global_start_y <- global_start_y + group_height
        
    let mutable sum = 0.0f
    for r = 0 to filterWidth - 1 do
        for c = 0 to filterWidth - 1 do
            sum <- (filter.[r,c]) * (block.[local_y + r, local_x + c])
    output.[yOut,xOut] <- sum
    
// Matrix multiplication
[<ReflectedDefinition>]
let MatrixMult(a: float32[,], b: float32[,], c: float32[,]) =
    let x = get_global_id(0)
    let y = get_global_id(1)

    let mutable accum = 0.0f
    for k = 0 to a.GetLength(1) - 1 do
        accum <- accum + (a.[x,k] * b.[k,y])
    c.[x,y] <- accum
    
// Array reduction
[<ReflectedDefinition>]
let Reduce(g_idata:int[], [<Local>]sdata:int[], n, g_odata:int[]) =
    // perform first level of reduction,
    // reading from global memory, writing to shared memory
    let tid = get_local_id(0)
    let i = get_group_id(0) * (get_local_size(0) * 2) + get_local_id(0)

    sdata.[tid] <- if(i < n) then g_idata.[i] else 0
    if (i + get_local_size(0) < n) then 
        sdata.[tid] <- sdata.[tid] + g_idata.[i + get_local_size(0)]

    barrier(CLK_LOCAL_MEM_FENCE)

    // do reduction in shared mem
    let mutable s = get_local_size(0) >>> 1
    while (s > 0) do 
        if (tid < s) then
            sdata.[tid] <- sdata.[tid] + sdata.[tid + s]
        barrier(CLK_LOCAL_MEM_FENCE)
        s <- s >>> 1

    if (tid = 0) then 
        g_odata.[get_group_id(0)] <- sdata.[0]