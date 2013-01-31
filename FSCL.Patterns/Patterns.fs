namespace FSCL
open FSCL.KernelFunctions

module Patterns =
    [<Kernel>][<ReflectedDefinition>]
    let sum (a:float32[]) (b:float32[]) =
        let c = Array.zeroCreate<float32> (a.Length)

        let x = get_global_id(0)
        c.[x] <- a.[x] + b.[x]

        c
        