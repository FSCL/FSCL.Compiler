namespace FSCL

type KernelAttributeException(msg: string) =
    inherit System.Exception(msg)

type KernelCallException(msg: string) =
    inherit System.Exception(msg)
    
type KernelDefinitionException(msg: string) =
    inherit System.Exception(msg)

type KernelBindingException(msg: string) =
    inherit System.Exception(msg)
