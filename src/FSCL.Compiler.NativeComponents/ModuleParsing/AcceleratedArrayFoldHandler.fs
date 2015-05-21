namespace FSCL.Compiler.AcceleratedCollections

open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System
open Microsoft.FSharp.Reflection
open AcceleratedCollectionUtil
open FSCL.Compiler.Util
open Microsoft.FSharp.Linq.RuntimeHelpers

// Fold has no parallel implementation, just handle it as a sequential node
type AcceleratedArrayFoldHandler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, cleanArgs, root, meta, step, env, opts) =
                
            // Inspect operator
            let computationFunction, subExpr =                
                AcceleratedCollectionUtil.ParseOperatorLambda(cleanArgs.[0], step, env, opts)
                                
            match subExpr with
            | Some(kfg, newEnv) ->
                // This coll fun is a composition 
                let node = new KFGCollectionCompositionNode(methodInfo, kfg, newEnv)

                // Parse arguments
                let subnode1 = step.Process(cleanArgs.[1], env, opts)
                let subnode2 = step.Process(cleanArgs.[2], env, opts)
                node.InputNodes.Add(subnode1)
                node.InputNodes.Add(subnode2)
                Some(node :> IKFGNode)   
            | _ ->
                None
