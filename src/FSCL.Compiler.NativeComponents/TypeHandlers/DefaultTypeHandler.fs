namespace FSCL.Compiler.Types

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<TypeHandler("FSCL_DEFAULT_TYPE_HANDLER")>]
type DefaultTypeHandler() =       
    inherit TypeHandler() with

    let (managedScalarTypes:Type list) = [ typeof<unit>; typeof<System.Void>; typeof<uint32>; typeof<uint64>; typeof<int64>; typeof<int>; typeof<float32>; typeof<float> ]   
    
    override this.Print(t:Type) =
        let arrayStar = if t.IsArray then "*" else ""
        let plainType = if t.IsArray then t.GetElementType() else t
        if (plainType = typeof<uint32>) then
            "unsigned int" + arrayStar         
        elif (plainType = typeof<uint64>) then
            "unsigned long" + arrayStar
        elif (plainType = typeof<int64>) then
            "long" + arrayStar
        elif (plainType = typeof<int>) then
            "int" + arrayStar 
        elif (plainType = typeof<double>) then
            "double" + arrayStar 
        elif (plainType = typeof<float32>) then
            "float" + arrayStar 
        elif (plainType = typeof<float>) then
            "float" + arrayStar 
        elif (plainType = typeof<unit>) then
            "void" + arrayStar 
        elif (plainType = typeof<System.Void>) then
            "void" + arrayStar 
        else
            ""

    override this.ManagedGenericInstances
        with get() = 
            managedScalarTypes
            
    override this.CanHandle(t) = 
        if t.IsArray then
            (List.tryFind (fun (mt:Type) -> mt = t.GetElementType()) managedScalarTypes).IsSome
        else
            (List.tryFind (fun (mt:Type) -> mt = t) managedScalarTypes).IsSome               