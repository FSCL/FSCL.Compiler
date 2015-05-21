namespace FSCL.Compiler.AcceleratedCollections

open FSCL
open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Collections.Generic
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System
open FSCL.Compiler.Util

type AcceleratedKernelInfo(name: String,
                           signature: MethodInfo, 
                           //paramInfo: ParameterInfo list,
                           paramVars: Quotations.Var list,
                           returnType: Type,
                           envVarsUsed: IReadOnlyList<Var>,
                           outValsUsed: IReadOnlyList<Expr>,
                           body: Expr, 
                           meta, 
                           collectionFunction: String, 
                           appliedFunction: IFunctionInfo option,
                           appliedFunctionExpr: Expr option) =
    inherit KernelInfo(name, Some(signature), paramVars, returnType, envVarsUsed, outValsUsed, None, body, meta)
            
    member val CollectionFunctionName = collectionFunction with get

    override this.ID
        with get() =     
            // An accelerated collections kernel like Array.map f a is identified by both "Array.map" and the body of f
            if appliedFunctionExpr.IsSome then
                CollectionFunctionID(signature, appliedFunctionExpr) |> box
            else
                CollectionFunctionID(signature, None) |> box

    member val AppliedFunction = appliedFunction with get
    member val AppliedFunctionLambda = appliedFunctionExpr with get

                