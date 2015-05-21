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

type AcceleratedArray2DMapHandler() =
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
                            functionParamVars.[0].Type.MakeArrayType(2)
                        else
                            functionParamVars.[1].Type.MakeArrayType(2)
                    let outputArrayType = functionReturnType.MakeArrayType(2)
                    let kernelName, runtimeName = 
                        if methodInfo.Name = "Map" then
                            "Array2DMap_" + functionName, "Array2D.map"
                        else
                            "Array2DMapi_" + functionName, "Array2D.mapi"                    

                    // Now we can create the signature and define parameter name
                    let wiHolder = Quotations.Var("workItemInfo", typeof<WorkItemInfo>)                                

                    // Create parameters placeholders
                    let inputHolder = Quotations.Var("input_array", inputArrayType)
                    let outputHolder = Quotations.Var("output_array", outputArrayType)
                    let tupleHolder = 
                            Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputArrayType; typeof<WorkItemInfo> |])) 
                    
                    // Finally, create the body of the kernel
                    let globalIdVar0 = Quotations.Var("global_id_0", typeof<int>)
                    let globalIdVar1 = Quotations.Var("global_id_1", typeof<int>)
                    let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType(), 2)
                    let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType(), 2)

                    let kernelBody = 
                        Expr.Lambda(tupleHolder,
                            Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),                                                
                                    Expr.Let(outputHolder, Expr.Call(
                                                        GetZeroCreateMethod(outputArrayType.GetElementType(), 2), 
                                                                            [ Expr.Call(Expr.Var(inputHolder),
                                                                                        GetArrayLengthMethodInfo(inputHolder.Type),
                                                                                        [ Expr.Value(0) ]);
                                                                              Expr.Call(Expr.Var(inputHolder),
                                                                                        GetArrayLengthMethodInfo(inputHolder.Type),
                                                                                        [ Expr.Value(1) ]) ]),
                                        Expr.Let(globalIdVar0,
                                            Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                Expr.Let(globalIdVar1,
                                                    Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(1) ]),
                                                        Expr.Sequential(
                                                            Expr.Call(setElementMethodInfo,
                                                                    [ Expr.Var(outputHolder);
                                                                        Expr.Var(globalIdVar1);
                                                                        Expr.Var(globalIdVar0);
                                                                        AcceleratedCollectionUtil.BuildApplication(
                                                                                functionBody,

                                                                                if methodInfo.Name = "Map" then
                                                                                    [ Expr.Call(getElementMethodInfo,
                                                                                                [ Expr.Var(inputHolder);
                                                                                                    Expr.Var(globalIdVar1);
                                                                                                    Expr.Var(globalIdVar0) 
                                                                                                ])]
                                                                                else
                                                                                    // Mapi
                                                                                    [ 
                                                                                        Expr.Var(globalIdVar1);
                                                                                        Expr.Var(globalIdVar0);
                                                                                        Expr.Call(getElementMethodInfo,
                                                                                                [ Expr.Var(inputHolder);
                                                                                                    Expr.Var(globalIdVar1);
                                                                                                    Expr.Var(globalIdVar0) 
                                                                                                ])]
                                                                    )]),
                                                                    Expr.Var(outputHolder))))))))

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
