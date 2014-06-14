namespace FSCL.Compiler.Types

open System
open FSCL
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<TypeHandler("FSCL_VECTOR_TYPE_HANDLER")>]
type VectorTypeHandler() =       
    inherit TypeHandler() with

    override this.Print(t:Type) =
        let arrayStar = if t.IsArray then "*" else ""
        let plainType = if t.IsArray then t.GetElementType() else t
        if plainType.GetCustomAttribute<VectorTypeAttribute>() <> null then
            plainType.Name + arrayStar
        else
            ""

    override this.ManagedGenericInstances
        with get() = 
            let i = 0
            []
            
    override this.CanHandle(t) = 
        if t.IsArray then
            t.GetElementType().GetCustomAttribute<VectorTypeAttribute>() <> null
        else
            t.GetCustomAttribute<VectorTypeAttribute>() <> null