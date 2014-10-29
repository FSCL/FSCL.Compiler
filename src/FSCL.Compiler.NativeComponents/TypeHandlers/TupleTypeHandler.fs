namespace FSCL.Compiler.Types

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

[<TypeHandler("FSCL_TUPLE_TYPE_HANDLER")>]
type TupleTypeHandler() =       
    inherit TypeHandler() with

    override this.Print(t:Type) =
        let arrayStar = if t.IsArray then "*" else ""
        let plainType = if t.IsArray then t.GetElementType() else t
        "struct Tuple_" + 
        (FSharpType.GetTupleElements(plainType) |> Array.map(fun t -> t.Name) |> String.concat "") + 
        "" + arrayStar

    override this.ManagedGenericInstances
        with get() = 
            []
            
    override this.CanHandle(t) = 
        let elementType = if t.IsArray then t.GetElementType() else t
        FSharpType.IsTuple(elementType)