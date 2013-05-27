namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

[<AbstractClass>]
type TypeHandler() =
    abstract member ManagedGenericInstances: Type list
    abstract member Print: Type -> string
    abstract member CanHandle: Type -> bool

type TypeManager(typeHandlers: TypeHandler list) =
    let managedInstances = List.concat (seq { for h in typeHandlers do yield h.ManagedGenericInstances })
    
    member this.ManagedGenericInstances 
        with get() = managedInstances

    member this.Print(t:Type) =
        let mutable found = false
        let mutable index = 0
        let mutable print = null
        while index < typeHandlers.Length  && not found do
            if typeHandlers.[index].CanHandle(t) then
                print <- typeHandlers.[index].Print(t)
                found <- true
            else
                index <- index + 1
        if (print = null) then
            raise (CompilerException("The engine is not able to handle the type " + t.ToString()))
        print
        

        
        

