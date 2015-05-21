namespace FSCL.Compiler.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.ModuleParsing
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System.Collections.Generic
open System

type internal IAcceleratedCollectionHandler =
    abstract member Process: MethodInfo * Expr list * Expr * ReadOnlyMetaCollection * ModuleParsingStep * Var list * Map<string,obj> -> IKFGNode option

