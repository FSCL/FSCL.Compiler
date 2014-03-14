namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.ModuleParsing
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System

type internal IAcceleratedCollectionHandler =
    abstract member Process: MethodInfo * Expr list * Expr * DynamicKernelMetadataCollection * DynamicParameterMetadataCollection list * ModuleParsingStep -> KernelModule option

