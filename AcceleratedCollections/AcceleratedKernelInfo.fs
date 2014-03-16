namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.Language
open System.Collections.Generic
open System.Reflection
open System.Collections.Generic
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System
open FSCL.Compiler.Util

type AcceleratedKernelInfo(signature: MethodInfo, body, dynamicAttributes, collectionFunction: string, appliedFunction: Expr) =
    inherit KernelInfo(signature, body, dynamicAttributes, false)

    override this.ID
        with get() =     
            // An accelerated collections kernel like Array.map f a is identified by both "Array.map" and the body of f
            LambdaID(collectionFunction + "_" + appliedFunction.ToString())