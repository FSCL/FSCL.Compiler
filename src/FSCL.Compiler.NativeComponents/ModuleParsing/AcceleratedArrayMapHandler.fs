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

type AcceleratedArrayMapHandler() =
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
                let subnode = step.Process(cleanArgs.[1], env, opts)
                node.InputNodes.Add(subnode)
                Some(node :> IKFGNode)   
            | _ ->
                // This coll fun is a kernel
                match computationFunction with
                | Some(thisVar, ob, functionName, functionInfo, functionParamVars, functionReturnType, functionBody) ->
                    // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                    let inputArrayType = 
                        if methodInfo.Name = "Map" then
                            functionParamVars.[0].Type.MakeArrayType()
                        else
                            functionParamVars.[1].Type.MakeArrayType()
                    let outputArrayType = functionReturnType.MakeArrayType()
                    let kernelName, runtimeName = 
                        if methodInfo.Name = "Map" then
                            "ArrayMap_" + functionName, "Array.map"
                        else
                            "ArrayMapi_" + functionName, "Array.mapi"                    

                    // Now we can create the signature and define parameter name
                    let wiHolder = Quotations.Var("workItemInfo", typeof<WorkItemInfo>)                                

                    // Create parameters placeholders
                    let inputHolder = Quotations.Var("input_array", inputArrayType)
                    let outputHolder = Quotations.Var("output_array", outputArrayType)
                    let tupleHolder = 
                            Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputArrayType; typeof<WorkItemInfo> |])) 
                    
                    // Finally, create the body of the kernel
                    let globalIdVar = Quotations.Var("global_id", typeof<int>)
                    let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType(), 1)
                    let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType(), 1)
                    let kernelBody = 
                        Expr.Lambda(tupleHolder,
                            Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),                                                
                                    Expr.Let(outputHolder, Expr.Call(
                                                        GetZeroCreateMethod(outputArrayType.GetElementType(), 1), 
                                                                            [ Expr.PropertyGet(Expr.Var(inputHolder), 
                                                                                            outputArrayType.GetProperty("Length")) ]),
                                        Expr.Let(globalIdVar,
                                                    Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                    Expr.Sequential(
                                                        Expr.Call(setElementMethodInfo,
                                                                [ Expr.Var(outputHolder);
                                                                    Expr.Var(globalIdVar);
                                                                    AcceleratedCollectionUtil.BuildApplication(
                                                                            functionBody,

                                                                            if methodInfo.Name = "Map" then
                                                                                [ Expr.Call(getElementMethodInfo,
                                                                                            [ Expr.Var(inputHolder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])]
                                                                            else
                                                                                // Mapi
                                                                                [ Expr.Var(globalIdVar);
                                                                                    Expr.Call(getElementMethodInfo,
                                                                                            [ Expr.Var(inputHolder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])]
                                                                )]),
                                                                Expr.Var(outputHolder)))))))

                    let envVars, outVals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(functionBody)
                                
                    // Add the computation function and connect it to the kernel
                    let mapFunctionInfo = new FunctionInfo(functionName,    
                                                           functionInfo, 
                                                           functionParamVars,   
                                                           functionReturnType, 
                                                           envVars,
                                                           outVals,     
                                                           functionBody)
                                                           
                    let kInfo = new AcceleratedKernelInfo(kernelName,
                                                          methodInfo,
                                                            [ inputHolder ], 
                                                            outputArrayType,    
                                                            envVars,
                                                            outVals,                                                 
                                                            kernelBody, 
                                                            meta, 
                                                            runtimeName, Some(mapFunctionInfo :> IFunctionInfo),
                                                            Some(functionBody))

                    let kernelModule = new KernelModule(thisVar, ob, kInfo)

                    kernelModule.Functions.Add(mapFunctionInfo.ID, mapFunctionInfo)
                    kInfo.CalledFunctions.Add(mapFunctionInfo.ID)

                    // Create node
                    let node = new KFGKernelNode(kernelModule)
                        
                    // Create data node for outsiders
//                        for o in outsiders do 
//                            node.InputNodes.Add(new KFGOutsiderDataNode(o))

                    // Parse arguments
                    let subnode = step.Process(cleanArgs.[1], env, opts)
                    node.InputNodes.Add(subnode)
                    Some(node :> IKFGNode)   
                | _ ->
                    None
