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
        member this.Process(methodInfo, cleanArgs, root, meta, step) =
                       
            (*
                Array map looks like: Array.map fun collection
                At first we check if fun is a lambda (first argument)
                and in this case we transform it into a method
                Secondly, we iterate parsing on the second argument (collection)
                since it might be a subkernel
            *)
            let lambda, computationFunction =                
                AcceleratedCollectionUtil.ExtractComputationFunctionForCollection(cleanArgs, root)

            // Merge with the eventual subkernels
                
            // Extract the map2 function 
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
                let signature = DynamicMethod(kernelPrefix + "_" + functionInfo.Name, outputArrayType, [| firstInputArrayType; secondInputArrayType; outputArrayType; typeof<WorkItemInfo> |])
                signature.DefineParameter(1, ParameterAttributes.In, "input_array_1") |> ignore
                signature.DefineParameter(2, ParameterAttributes.In, "input_array_2") |> ignore
                signature.DefineParameter(3, ParameterAttributes.In, "output_array") |> ignore
                signature.DefineParameter(4, ParameterAttributes.In, "workItemInfo") |> ignore
                    
                // Create parameters placeholders
                let input1Holder = Quotations.Var("input_array_1", firstInputArrayType)
                let input2Holder = Quotations.Var("input_array_2", secondInputArrayType)
                let outputHolder = Quotations.Var("output_array", outputArrayType)
                let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| firstInputArrayType; secondInputArrayType; outputArrayType; typeof<WorkItemInfo> |]))
                let wiHolder = Quotations.Var("workItemInfo", typeof<WorkItemInfo>)

                // Finally, create the body of the kernel
                let globalIdVar = Quotations.Var("global_id", typeof<int>)
                let firstGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(firstInputArrayType.GetElementType())
                let secondGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(secondInputArrayType.GetElementType())
                let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                let kernelBody = 
                    if methodInfo.Name = "Map2" then
                        Expr.Lambda(tupleHolder,
                            Expr.Let(input1Holder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(input2Holder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                    Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                        Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 3),
                                            Expr.Let(globalIdVar,
                                                        Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                        Expr.Sequential(
                                                            Expr.Call(setElementMethodInfo,
                                                                [ Expr.Var(outputHolder);
                                                                    Expr.Var(globalIdVar);
                                                                    AcceleratedCollectionUtil.BuildCallExpr(
                                                                            ob, 
                                                                            functionInfo,
                                                                            [ Expr.Call(firstGetElementMethodInfo,
                                                                                        [ Expr.Var(input1Holder);
                                                                                            Expr.Var(globalIdVar) 
                                                                                        ]);
                                                                              Expr.Call(secondGetElementMethodInfo,
                                                                                        [ Expr.Var(input2Holder);
                                                                                            Expr.Var(globalIdVar) 
                                                                                        ])
                                                                            ])
                                                                ]),
                                                             Expr.Var(outputHolder))))))))
                    else
                        Expr.Lambda(tupleHolder,
                            Expr.Let(input1Holder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(input2Holder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                    Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                        Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 3),
                                            Expr.Let(globalIdVar,
                                                        Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                        Expr.Sequential(
                                                            Expr.Call(setElementMethodInfo,
                                                                [ Expr.Var(outputHolder);
                                                                    Expr.Var(globalIdVar);
                                                                    AcceleratedCollectionUtil.BuildCallExpr(
                                                                            ob, 
                                                                            functionInfo,
                                                                            [ Expr.Var(globalIdVar);
                                                                              Expr.Call(firstGetElementMethodInfo,
                                                                                        [ Expr.Var(input1Holder);
                                                                                            Expr.Var(globalIdVar) 
                                                                                        ]);
                                                                              Expr.Call(secondGetElementMethodInfo,
                                                                                        [ Expr.Var(input2Holder);
                                                                                            Expr.Var(globalIdVar) 
                                                                                        ])
                                                                            ])
                                                                ]),
                                                             Expr.Var(outputHolder))))))))
                                  
                // Add current kernelbody
                let methodParams = signature.GetParameters()
                let kInfo = new AcceleratedKernelInfo(signature, 
                                                      [ methodParams.[0]; methodParams.[1]; methodParams.[2] ],
                                                      [ input1Holder; input2Holder; outputHolder ],
                                                      kernelBody,
                                                      meta, 
                                                      "Array.map2", Some(functionBody))
                let kernelModule = new KernelModule(kInfo, cleanArgs)
                
                // Add the current kernel
                let mapFunctionInfo = new FunctionInfo(thisVar, ob,                                    
                                                       functionInfo, 
                                                       functionInfo.GetParameters() |> List.ofArray, 
                                                       functionParamVars,
                                                       None,
                                                       functionBody, lambda.IsSome)
                kernelModule.Functions.Add(mapFunctionInfo.ID, mapFunctionInfo)
                
                // Return module                             
                Some(kernelModule)
            | _ ->
                None
