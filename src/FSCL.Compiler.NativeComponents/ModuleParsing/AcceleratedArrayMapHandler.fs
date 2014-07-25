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
                                
            // Extract the map function 
            match computationFunction with
            | Some(functionInfo, functionParamVars, functionBody) ->

                // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                let inputArrayType = 
                    if methodInfo.Name = "Map" then
                        functionInfo.GetParameters().[0].ParameterType.MakeArrayType()
                    else
                        functionInfo.GetParameters().[1].ParameterType.MakeArrayType()
                let outputArrayType = functionInfo.ReturnType.MakeArrayType()
                let kernelPrefix, functionName = 
                    if methodInfo.Name = "Map" then
                        "ArrayMap_", "Array.map"
                    else
                        "ArrayMapi_", "Array.mapi"                    

                // Now we can create the signature and define parameter name
                let signature = DynamicMethod(kernelPrefix + "_" + functionInfo.Name, outputArrayType, [| inputArrayType; outputArrayType; typeof<WorkItemInfo> |])
                signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                signature.DefineParameter(3, ParameterAttributes.In, "workItemInfo") |> ignore
                let wiHolder = Quotations.Var("workItemInfo", typeof<WorkItemInfo>)                                

                // Create parameters placeholders
                let inputHolder = Quotations.Var("input_array", inputArrayType)
                let outputHolder = Quotations.Var("output_array", outputArrayType)
                let tupleHolder = 
                        Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputArrayType; outputArrayType; typeof<WorkItemInfo> |])) 
                    
                // Finally, create the body of the kernel
                let globalIdVar = Quotations.Var("global_id", typeof<int>)
                let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType())
                let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                let kernelBody = 
                    if methodInfo.Name = "Map" then
                        Expr.Lambda(tupleHolder,
                                Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                    Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                        Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                            Expr.Let(globalIdVar,
                                                        Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                        Expr.Sequential(
                                                            Expr.Call(setElementMethodInfo,
                                                                    [ Expr.Var(outputHolder);
                                                                        Expr.Var(globalIdVar);
                                                                        Expr.Call(functionInfo,
                                                                                [ Expr.Call(getElementMethodInfo,
                                                                                            [ Expr.Var(inputHolder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])
                                                                                ])
                                                                    ]),
                                                            Expr.Var(outputHolder)))))))
                    else
                        Expr.Lambda(tupleHolder,
                                Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                    Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                        Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                            Expr.Let(globalIdVar,
                                                        Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                        Expr.Sequential(
                                                            Expr.Call(setElementMethodInfo,
                                                                    [ Expr.Var(outputHolder);
                                                                        Expr.Var(globalIdVar);
                                                                        Expr.Call(functionInfo,
                                                                                [ Expr.Var(globalIdVar);
                                                                                  Expr.Call(getElementMethodInfo,
                                                                                            [ Expr.Var(inputHolder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])
                                                                                ])
                                                                    ]),
                                                            Expr.Var(outputHolder)))))))

                let methodParams = signature.GetParameters()
                let kInfo = new AcceleratedKernelInfo(signature, 
                                                      [ methodParams.[0]; methodParams.[1] ],
                                                      [ inputHolder; outputHolder ],                                                      
                                                      kernelBody, 
                                                      meta, 
                                                      functionName, Some(functionBody))
                let kernelModule = new KernelModule(kInfo, cleanArgs)
                                
                // Add the computation function and connect it to the kernel
                let mapFunctionInfo = new FunctionInfo(functionInfo, 
                                                       functionInfo.GetParameters() |> List.ofArray, 
                                                       functionParamVars, 
                                                       None,
                                                       functionBody, lambda.IsSome)
                kernelModule.Functions.Add(mapFunctionInfo.ID, mapFunctionInfo)
                                
                // Return module                             
                Some(kernelModule)
            | _ ->
                None
