namespace FSCL

open Cloo
        
type internal KernelRunnerTools() =    
   static member WriteBuffer<'T when 'T: struct and 'T : (new : unit -> 'T) and 'T :> System.ValueType>(c:ComputeContext, q:ComputeCommandQueue, arg:obj, dims, shouldInit) =
        //let dims = FSCL.Util.GetArrayDimensions(arg.Type)
        match dims with
        | 1 ->
            let actualArg = arg :?> 'T[]
            let buffer = new ComputeBuffer<'T>(c, ComputeMemoryFlags.None, actualArg.LongLength)
            if shouldInit then
                q.WriteToBuffer<'T>(actualArg, buffer, false, null)
            buffer :> ComputeMemory
        | 2 ->
            let actualArg = arg :?> 'T[,]
            let buffer = new ComputeBuffer<'T>(c, ComputeMemoryFlags.None, actualArg.LongLength)
            if shouldInit then
                let offset = Cloo.SysIntX2(0, 0)                
                let region = Cloo.SysIntX2(actualArg.GetLength(0), actualArg.GetLength(1))
                q.WriteToBuffer<'T>(actualArg, buffer, false, offset, offset, region, null)
            buffer :> ComputeMemory
        | _ ->
            let actualArg = arg :?> 'T[,,]
            let buffer = new ComputeBuffer<'T>(c, ComputeMemoryFlags.None, actualArg.LongLength)
            if shouldInit then
                let offset = Cloo.SysIntX3(0, 0, 0)
                let region = Cloo.SysIntX3(actualArg.GetLength(0), actualArg.GetLength(1), actualArg.GetLength(2)) 
                q.WriteToBuffer<'T>(actualArg, buffer, false, offset, offset, region, null)
            buffer :> ComputeMemory
            
    static member ReadBuffer<'T when 'T: struct and 'T : (new : unit -> 'T) and 'T :> System.ValueType>(c:ComputeContext, q:ComputeCommandQueue, arg:obj, dims, buffer: ComputeBuffer<'T>) =
        match dims with
        | 1 ->
            let actualArg = arg :?> 'T[]
            q.ReadFromBuffer<'T>(buffer, ref actualArg, true, null)            
        | 2 ->
            let actualArg = arg :?> 'T[,]
            let offset = Cloo.SysIntX2(0,0)
            let region = Cloo.SysIntX2(actualArg.GetLength(0),actualArg.GetLength(1))
            q.ReadFromBuffer<'T>(buffer, ref actualArg, true, offset, offset, region, null)
        | _ ->
            let actualArg = arg :?> 'T[,,]
            let offset = Cloo.SysIntX3(0,0,0)
            let region = Cloo.SysIntX3(actualArg.GetLength(0), actualArg.GetLength(1), actualArg.GetLength(2))
            q.ReadFromBuffer<'T>(buffer, ref actualArg, true, offset, offset, region, null)