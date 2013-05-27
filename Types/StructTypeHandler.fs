namespace FSCL.Compiler.Processors

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection

[<TypeHandler("FSCL_STRUCT_TYPE_HANDLER")>]
type StructTypeHandler() =       
    inherit TypeHandler() with

    override this.Print(t:Type) =
        let arrayStar = if t.IsArray then "*" else ""
        let plainType = if t.IsArray then t.GetElementType() else t
        "(struct " + plainType.Name + ")" + arrayStar

    override this.ManagedGenericInstances
        with get() = 
            []
            
    override this.CanHandle(t) = 
        let elementType = if t.IsArray then t.GetElementType() else t
        if (FSharpType.IsRecord(elementType) ||
            (elementType.IsValueType && (not elementType.IsPrimitive) && (not elementType.IsEnum))) then   
            true
        else
            false         