namespace FSCL.Compiler.Processors

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

type RefVariableTypeHandler() =       
    inherit TypeHandler() with

    let (managedScalarTypes:Type list) = [ typeof<uint32>; typeof<uint64>; typeof<int64>; typeof<int>; typeof<float32>; typeof<float> ]   
    
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
        else
            ""
    override this.ManagedGenericInstances
        with get() = 
            []
            
    override this.CanHandle(t) = 
        if t.IsGenericType && (t.GetGenericTypeDefinition() = typeof<Ref<_>>.GetGenericTypeDefinition()) then
            let elType = t.GetGenericArguments().[0]
            (List.tryFind (fun (mt:Type) -> mt = elType) managedScalarTypes).IsSome
        else
            false
                                