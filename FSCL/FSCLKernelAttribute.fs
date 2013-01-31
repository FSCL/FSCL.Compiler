namespace FSCL

open System

type KernelAttribute =
    inherit Attribute

    val platform: int option
    val dev : int option

    new(p: int, d: int) =  { 
        platform = Some(p)
        dev = Some(d) 
    }
    new() =  { 
        platform = None
        dev = None
    }

    member this.Platform 
        with get() = if(this.platform.IsSome) then this.platform.Value else -1
    member this.Device
        with get() = if(this.dev.IsSome) then this.dev.Value else -1
        
type ConstantAttribute =
    inherit Attribute
    new() =  { }
    
type LocalAttribute =
    inherit Attribute
    new() =  { }



