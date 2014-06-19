﻿namespace FSCL.Compiler.AcceleratedCollections

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
                AcceleratedCollectionUtil.ExtractComputationFunction(cleanArgs, root)

            // Merge with the eventual subkernels
                
            // Extract the map2 function 
            match computationFunction with
            | Some(functionInfo, functionParamVars, functionBody) ->
                // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                let firstInputArrayType = Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType()
                let secondInputArrayType = Array.CreateInstance(functionInfo.GetParameters().[1].ParameterType, 0).GetType()
                let outputArrayType = Array.CreateInstance(functionInfo.ReturnType, 0).GetType()
                                    
                // Now we can create the signature and define parameter name
                let signature = DynamicMethod("ArrayMap2_" + functionInfo.Name, outputArrayType, [| firstInputArrayType; secondInputArrayType; outputArrayType |])
                signature.DefineParameter(1, ParameterAttributes.In, "input_array_1") |> ignore
                signature.DefineParameter(2, ParameterAttributes.In, "input_array_2") |> ignore
                signature.DefineParameter(3, ParameterAttributes.In, "output_array") |> ignore
                    
                // Create parameters placeholders
                let input1Holder = Quotations.Var("input_array_1", firstInputArrayType)
                let input2Holder = Quotations.Var("input_array_2", secondInputArrayType)
                let outputHolder = Quotations.Var("output_array", outputArrayType)
                let tupleHolder = Quotations.Var("tupledArg", FSharpType.MakeTupleType([| firstInputArrayType; secondInputArrayType; outputArrayType |]))

                // Finally, create the body of the kernel
                let globalIdVar = Quotations.Var("global_id", typeof<int>)
                let firstGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(firstInputArrayType.GetElementType())
                let secondGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(secondInputArrayType.GetElementType())
                let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                let kernelBody = 
                    Expr.Lambda(tupleHolder,
                        Expr.Let(input1Holder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                            Expr.Let(input2Holder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                    Expr.Let(globalIdVar,
                                                Expr.Call(AcceleratedCollectionUtil.FilterCall(<@ get_global_id @>, fun(e, mi, a) -> mi).Value, [ Expr.Value(0) ]),
                                                Expr.Sequential(
                                                    Expr.Call(setElementMethodInfo,
                                                        [ Expr.Var(outputHolder);
                                                            Expr.Var(globalIdVar);
                                                            Expr.Call(functionInfo,
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
                                                     Expr.Var(outputHolder)))))))
                                  
                // Add current kernelbody
                let kInfo = new AcceleratedKernelInfo(signature, 
                                                      [ input1Holder; input2Holder; outputHolder ],
                                                      kernelBody,
                                                      meta, 
                                                      "Array.map2", functionBody)
                let kernelModule = new KernelModule(kInfo, cleanArgs)
                
                // Add the current kernel
                let mapFunctionInfo = new FunctionInfo(functionInfo, functionParamVars, functionBody, lambda.IsSome)
                kernelModule.Functions.Add(mapFunctionInfo.ID, mapFunctionInfo)
                
                // Return module                             
                Some(kernelModule)
            | _ ->
                None