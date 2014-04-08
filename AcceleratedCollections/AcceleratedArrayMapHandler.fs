namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.Language
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
            | Some(functionInfo, functionBody) ->

                // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                let inputArrayType = Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType()
                let outputArrayType = Array.CreateInstance(functionInfo.ReturnType, 0).GetType()
                (* let inputEmptyArray = FilterCall(<@ Array.empty @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| inputArrayElementType |])).Value
                // We need to get the type of a array whose elements type is the same of the functionInfo return value
                let outputArrayElementType = functionInfo.ReturnType
                let outputEmptyArray = FilterCall(<@ Array.empty @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| outputArrayElementType |])).Value
                *)
                // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output                    
                    
                // Now we can create the signature and define parameter name
                let signature = DynamicMethod("ArrayMap_" + functionInfo.Name, typeof<unit>, [| inputArrayType; outputArrayType |])
                signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                                
                // Create parameters placeholders
                let inputHolder = Quotations.Var("input_array", inputArrayType)
                let outputHolder = Quotations.Var("output_array", outputArrayType) 
                    
                // Finally, create the body of the kernel
                let globalIdVar = Quotations.Var("global_id", typeof<int>)
                let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType())
                let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                let kernelBody = 
                    Expr.Let(globalIdVar,
                                Expr.Call(AcceleratedCollectionUtil.FilterCall(<@ get_global_id @>, fun(e, mi, a) -> mi).Value, [ Expr.Value(0) ]),
                                Expr.Call(setElementMethodInfo,
                                        [ Expr.Var(outputHolder);
                                            Expr.Var(globalIdVar);
                                            Expr.Call(functionInfo,
                                                    [ Expr.Call(getElementMethodInfo,
                                                                [ Expr.Var(inputHolder);
                                                                    Expr.Var(globalIdVar) 
                                                                ])
                                                    ])
                                        ]))

                let kInfo = new AcceleratedKernelInfo(signature, 
                                                      kernelBody, 
                                                      meta, 
                                                      "Array.map", functionBody)
                let kernelModule = new KernelModule(kInfo, cleanArgs)
                                
                // Store placeholders
                kernelModule.Kernel.OriginalParameters.[0].Placeholder <- inputHolder
                kernelModule.Kernel.OriginalParameters.[1].Placeholder <- outputHolder

                // Add the computation function and connect it to the kernel
                let mapFunctionInfo = new FunctionInfo(functionInfo, functionBody, lambda.IsSome)
                kernelModule.Functions.Add(mapFunctionInfo.ID, mapFunctionInfo)
                                
                // Connect with subkernel
                (*if subkernel <> null then   
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array",
                                                  new FlowGraphNodeInputInfo(
                                                    KernelOutput(subkernel.FlowGraph, 0),
                                                    None,
                                                    paramAttrs.[1]))
                else
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array",                                                  
                                                  new FlowGraphNodeInputInfo(
                                                    ActualArgument(cleanArgs.[1]),
                                                    None,
                                                    paramAttrs.[1]))
                FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                              "output_array",                                  
                                              new FlowGraphNodeInputInfo(
                                                BufferAllocationSize(fun(args, localSize, globalSize) ->
                                                                            BufferReferenceAllocationExpression("input_array")),
                                                None,
                                                new ParameterMetadataCollection()))*)
                // Return module                             
                Some(kernelModule)
            | _ ->
                None
