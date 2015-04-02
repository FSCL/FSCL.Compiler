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

type AcceleratedArrayMap2Handler() =    
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, cleanArgs, root, meta, step, env) =
            // Inspect operator
            let computationFunction, subExpr, isLambda =                
                AcceleratedCollectionUtil.ParseOperatorLambda(cleanArgs.[0], step, env)
                                
            match subExpr with
            | Some(kfg, newEnv) ->
                // This coll fun is a composition 
                let node = new KFGCollectionCompositionNode(methodInfo, kfg, newEnv)
                
                // Parse arguments
                let subnode1 = step.Process(cleanArgs.[1], env)
                let subnode2 = step.Process(cleanArgs.[2], env)
                node.InputNodes.Add(subnode1)
                node.InputNodes.Add(subnode2)
                Some(node :> IKFGNode)   
            | _ ->
                // This coll fun is a kernel
                match computationFunction with
                | Some(thisVar, ob, functionInfo, functionParamVars, functionBody) ->
                    // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                    let firstInputArrayType, secondInputArrayType = 
                        if methodInfo.Name = "Map2" then
                            (functionInfo.GetParameters().[0].ParameterType.MakeArrayType(), functionInfo.GetParameters().[1].ParameterType.MakeArrayType())
                        else
                            (functionInfo.GetParameters().[1].ParameterType.MakeArrayType(), functionInfo.GetParameters().[2].ParameterType.MakeArrayType())
                    let outputArrayType = functionInfo.ReturnType.MakeArrayType()
                    let kernelPrefix, functionName = 
                        if methodInfo.Name = "Map2" then
                            "ArrayMap2_", "Array.map2"
                        else
                            "ArrayMapi2_", "Array.mapi2"     
                                    
                    // Now we can create the signature and define parameter name
                    let signature = DynamicMethod(kernelPrefix + "_" + functionInfo.Name, outputArrayType, [| firstInputArrayType; secondInputArrayType; typeof<WorkItemInfo> |])
                    signature.DefineParameter(1, ParameterAttributes.In, "input_array_1") |> ignore
                    signature.DefineParameter(2, ParameterAttributes.In, "input_array_2") |> ignore
                    //signature.DefineParameter(3, ParameterAttributes.In, "output_array") |> ignore
                    signature.DefineParameter(3, ParameterAttributes.In, "workItemInfo") |> ignore
                    
                    // Create parameters placeholders
                    let input1Holder = Quotations.Var("input_array_1", firstInputArrayType)
                    let input2Holder = Quotations.Var("input_array_2", secondInputArrayType)
                    let outputHolder = Quotations.Var("output_array", outputArrayType)
                    let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| firstInputArrayType; secondInputArrayType; typeof<WorkItemInfo> |]))
                    let wiHolder = Quotations.Var("workItemInfo", typeof<WorkItemInfo>)

                    // Finally, create the body of the kernel
                    let globalIdVar = Quotations.Var("global_id", typeof<int>)
                    let firstGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(firstInputArrayType.GetElementType())
                    let secondGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(secondInputArrayType.GetElementType())
                    let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                    let kernelBody = 
                        Expr.Lambda(tupleHolder,
                            Expr.Let(input1Holder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(input2Holder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                    Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                        Expr.Let(outputHolder, Expr.Call(
                                                            ZeroCreateMethod(outputArrayType.GetElementType()), 
                                                                                [ Expr.PropertyGet(Expr.Var(input1Holder), 
                                                                                                firstInputArrayType.GetProperty("Length")) ]),
                                            Expr.Let(globalIdVar,
                                                        Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                        Expr.Sequential(
                                                            Expr.Call(setElementMethodInfo,
                                                                [ Expr.Var(outputHolder);
                                                                    Expr.Var(globalIdVar);
                                                                    AcceleratedCollectionUtil.BuildCallExpr(
                                                                            ob, 
                                                                            functionInfo,
                                                                            
                                                                            if methodInfo.Name = "Map2" then
                                                                                [ Expr.Call(firstGetElementMethodInfo,
                                                                                            [ Expr.Var(input1Holder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ]);
                                                                                    Expr.Call(secondGetElementMethodInfo,
                                                                                            [ Expr.Var(input2Holder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])
                                                                                ]
                                                                            else
                                                                                // Mapi2
                                                                                [ Expr.Var(globalIdVar);
                                                                                  Expr.Call(firstGetElementMethodInfo,
                                                                                            [ Expr.Var(input1Holder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ]);
                                                                                  Expr.Call(secondGetElementMethodInfo,
                                                                                            [ Expr.Var(input2Holder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])
                                                                                ]
                                                                )]),
                                                                Expr.Var(outputHolder))))))))

                    // Add current kernelbody
                    let methodParams = signature.GetParameters()
                    let envVars, outVals = 
                        QuotationAnalysis.KernelParsing.ExtractEnvRefs(functionBody)
                    let kInfo = new AcceleratedKernelInfo(signature, 
                                                          [ methodParams.[0]; methodParams.[1] ],
                                                          [ input1Holder; input2Holder ],
                                                          envVars, outVals,
                                                          kernelBody,
                                                          meta, 
                                                          (if methodInfo.Name = "Map2" then
                                                            "Array.map2" 
                                                           else 
                                                            "Array.mapi2"), Some(functionBody))
                    let kernelModule = new KernelModule(thisVar, ob, kInfo)
                
                    // Add the current kernel
                    let mapFunctionInfo = new FunctionInfo(functionInfo, 
                                                           functionInfo.GetParameters() |> List.ofArray, 
                                                           functionParamVars,
                                                           envVars, outVals,
                                                           None,
                                                           functionBody, isLambda)
                    kernelModule.Functions.Add(mapFunctionInfo.ID, mapFunctionInfo)
                    kInfo.CalledFunctions.Add(mapFunctionInfo.ID)
                
                    // Create node
                    let node = new KFGKernelNode(kernelModule)
                    
//                    // Create data node for outsiders
//                    for o in outsiders do 
//                        node.InputNodes.Add(new KFGOutsiderDataNode(o))

                    // Parse arguments
                    let subnode1 = step.Process(cleanArgs.[1], env)
                    node.InputNodes.Add(subnode1)
                    let subnode2 = step.Process(cleanArgs.[2], env)
                    node.InputNodes.Add(subnode2)

                    Some(node :> IKFGNode)  
                | _ ->
                    None
