namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel
open Cloo

///
///<summary>
/// Base classes for Dynamic Attributes (Attributes with matching functions to be used in kernel calls)
///</summary>
///
[<AllowNullLiteral>]
type DynamicMetadataAttribute() =
    inherit Attribute()
   
///
///<summary>
/// Base classes for Dynamic Attributes associated to kernels
///</summary>
/// 
[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)>]
type DynamicKernelMetadataAttribute() =
    inherit DynamicMetadataAttribute()
    
///
///<summary>
/// Base classes for Dynamic Attributes associated to parameters
///</summary>
///
[<AllowNullLiteral>]
[<AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)>]
type DynamicParameterMetadataAttribute() =
    inherit DynamicMetadataAttribute()
   
///
///<summary>
/// Aliases for collections of dynamic attributes
///</summary>
///
type DynamicParameterMetadataCollection = Dictionary<Type, DynamicParameterMetadataAttribute>
type DynamicKernelMetadataCollection = Dictionary<Type, DynamicKernelMetadataAttribute>
type ReadOnlyDynamicParameterMetadataCollection = ReadOnlyDictionary<Type, DynamicParameterMetadataAttribute>
type ReadOnlyDynamicKernelMetadataCollection = ReadOnlyDictionary<Type, DynamicKernelMetadataAttribute>

    
