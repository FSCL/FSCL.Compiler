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

type AcceleratedKernelInfo(signature: MethodInfo, 
                           paramInfo: ParameterInfo list,
                           paramVars: Quotations.Var list,
                           envVarsUsed: IReadOnlyList<Var>,
                           outValsUsed: IReadOnlyList<Expr>,
                           body: Expr, 
                           meta, 
                           collectionFunction: String, 
                           appliedFunction: IFunctionInfo option,
                           appliedFunctionExpr: Expr option) =
    inherit KernelInfo(signature, paramInfo, paramVars, envVarsUsed, outValsUsed, None, body, meta, false)
            
    member val CollectionFunctionName = collectionFunction with get

    override this.ID
        with get() =     
            // An accelerated collections kernel like Array.map f a is identified by both "Array.map" and the body of f
            if appliedFunctionExpr.IsSome then
                LambdaID(collectionFunction + "_" + appliedFunctionExpr.ToString())
            else
                LambdaID(collectionFunction)

    member val AppliedFunction = appliedFunction with get

                