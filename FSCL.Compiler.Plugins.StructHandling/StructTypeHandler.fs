namespace FSCL.Compiler.Processors

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

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
        if t.IsArray then
            if (t.GetElementType().IsValueType && (not (t.GetElementType().IsPrimitive)) && (not (t.GetElementType().IsEnum))) then
                true
            else
                false
        else
            if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then   
                true
            else
                false         