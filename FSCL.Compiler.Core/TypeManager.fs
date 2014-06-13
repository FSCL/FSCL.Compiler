namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

///
///<summary>
/// The handler for a subset of target-AST types. 
///</summary>
///<remarks>
/// Target-AST types are thos types not lifted/replaced during preprocessing and transformations. In other
/// terms those types that can have a direct representation in the target language (OpenCL)
///</remarks>
///
[<AbstractClass>]
type TypeHandler() =
    ///
    ///<summary>
    /// The set of types that can be used to instantiate generic parameters to obtain a parameterless kernel from a generic one
    ///</summary>
    ///
    abstract member ManagedGenericInstances: Type list
    ///
    ///<summary>
    /// Type printer
    ///</summary>
    ///<returns>
    /// The representation of the type in the target laguage
    ///</returns>
    ///
    abstract member Print: Type -> string
    ///
    ///<summary>
    /// Type handle query
    ///</summary>
    ///<returns>
    /// true is the given types is handled by this type handler, false otherwise
    ///</returns>
    ///
    abstract member CanHandle: Type -> bool
    
///
///<summary>
/// The type manager, which is a set of type handlers together with an application strategy (first-applicable strategy)
///</summary>
///
type TypeManager(typeHandlers: TypeHandler list) =
    let managedInstances = List.concat (seq { for h in typeHandlers do yield h.ManagedGenericInstances })
    
    ///
    ///<summary>
    /// The whole set of types declared by one or more type handlers in the set
    ///</summary>
    ///<see cref="TypeHandler.ManagedGenericInstances"/>
    ///
    member this.ManagedGenericInstances 
        with get() = managedInstances
        
    ///
    ///<summary>
    /// Type printer
    ///</summary>
    ///<returns>
    /// The representation of the type in the target laguage, given by the first-applicable type handler in the set
    ///</returns>
    ///<exception cref="CompilerException">
    ///The exception is thrown if no applicable handler is found for the given type
    ///</exception>
    ///
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
        

        
        

