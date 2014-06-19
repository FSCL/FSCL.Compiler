namespace FSCL
open System

// Metadata to mark vector types
[<AllowNullLiteral>]
type VectorTypeAttribute() =
    inherit Attribute()

[<AllowNullLiteral>]
type VectorTypeConversionAttribute() =
    inherit Attribute()
