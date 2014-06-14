namespace FSCL.Compiler

type CompilerException(msg: string) =
    inherit System.Exception(msg)
    
type FlowGraphException(msg: string) =
    inherit System.Exception(msg)
