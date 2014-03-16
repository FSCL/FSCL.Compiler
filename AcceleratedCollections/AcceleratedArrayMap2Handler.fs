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

type AcceleratedArrayMap2Handler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, cleanArgs, root, kernelAttrs, paramAttrs, step) =
                
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
            | Some(functionInfo, functionBody) ->
                // Now create the kernel
                // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                let firstInputArrayType = Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType()
                let secondInputArrayType = Array.CreateInstance(functionInfo.GetParameters().[1].ParameterType, 0).GetType()
                let outputArrayType = Array.CreateInstance(functionInfo.ReturnType, 0).GetType()
                
                // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output                    
                let firstInputArrayPlaceholder = Expr.Var(Quotations.Var("input_array_1", firstInputArrayType))
                let secondInputArrayPlaceholder = Expr.Var(Quotations.Var("input_array_2", secondInputArrayType))
                let outputArrayPlaceholder = Expr.Var(Quotations.Var("output_array", outputArrayType))
                    
                // Now we can create the signature and define parameter name
                let signature = DynamicMethod("ArrayMap2_" + functionInfo.Name, outputArrayType, [| firstInputArrayType; secondInputArrayType; outputArrayType |])
                signature.DefineParameter(1, ParameterAttributes.In, "input_array_1") |> ignore
                signature.DefineParameter(2, ParameterAttributes.In, "input_array_2") |> ignore
                signature.DefineParameter(3, ParameterAttributes.In, "output_array") |> ignore
                    
                // Finally, create the body of the kernel
                let globalIdVar = Quotations.Var("global_id", typeof<int>)
                let firstGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(firstInputArrayType.GetElementType())
                let secondGetElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(secondInputArrayType.GetElementType())
                let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                let kernelBody = 
                    Expr.Let(globalIdVar,
                                Expr.Call(AcceleratedCollectionUtil.FilterCall(<@ get_global_id @>, fun(e, mi, a) -> mi).Value, [ Expr.Value(0) ]),
                                Expr.Sequential(
                                    Expr.Call(setElementMethodInfo,
                                        [ outputArrayPlaceholder;
                                            Expr.Var(globalIdVar);
                                            Expr.Call(functionInfo,
                                                    [ Expr.Call(firstGetElementMethodInfo,
                                                                [ firstInputArrayPlaceholder;
                                                                    Expr.Var(globalIdVar) 
                                                                ]);
                                                      Expr.Call(secondGetElementMethodInfo,
                                                                [ secondInputArrayPlaceholder;
                                                                    Expr.Var(globalIdVar) 
                                                                ])
                                                    ])
                                        ]),
                                     outputArrayPlaceholder))
                                    

                // Add current kernelbody
                let kInfo = new AcceleratedKernelInfo(signature, kernelBody, kernelAttrs, "Array.map2", functionBody)
                kInfo.CustomInfo.Add("IS_ACCELERATED_COLLECTION_KERNEL", true)
                let kernelModule = new KernelModule(kInfo)
                
                // Create input parameter info
                let input1ParameterEntry = new KernelParameterInfo("input_array_1", firstInputArrayType, null, Some(cleanArgs.[1]), paramAttrs.[1])
                // Set var to be used in kernel body
                input1ParameterEntry.Placeholder <- Some(Quotations.Var("input_array_1", firstInputArrayType, false))         
                // Add the parameter to the list of kernel params
                kernelModule.Kernel.Info.Parameters.Add(input1ParameterEntry)
                // Create input parameter info
                let input2ParameterEntry = new KernelParameterInfo("input_array_2", secondInputArrayType, null, Some(cleanArgs.[2]), paramAttrs.[2])
                // Set var to be used in kernel body
                input2ParameterEntry.Placeholder <- Some(Quotations.Var("input_array_2", secondInputArrayType, false))         
                // Add the parameter to the list of kernel params
                kernelModule.Kernel.Info.Parameters.Add(input2ParameterEntry)
                // Create output parameter info
                let outputParameterEntry = new KernelParameterInfo("output_array", outputArrayType, null, None, null)
                // Set var to be used in kernel body
                outputParameterEntry.Placeholder <- Some(Quotations.Var("output_array", outputArrayType, false))         
                // Add the parameter to the list of kernel params
                kernelModule.Kernel.Info.Parameters.Add(outputParameterEntry)

                // Update call graph
                //kernelModule.FlowGraph <- FlowGraphNode(kInfo.ID, None, kernelAttrs)
                
                // Add the computation function and set that it is required by the kernel
                let mapFunctionInfo = new FunctionInfo(functionInfo, functionBody,  lambda.IsSome)
                kernelModule.AddFunction(mapFunctionInfo)
                kernelModule.Kernel.RequiredFunctions.Add(mapFunctionInfo.ID) |> ignore
                
                // Set that the return value is encoded in the parameter output_array
                kernelModule.Kernel.Info.CustomInfo.Add("RETURN_PARAMETER_NAME", "output_array")

                // Connect with subkernels
                (*
                if firstSubkernel <> null then           
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_1",
                                                  new FlowGraphNodeInputInfo(
                                                    KernelOutput(firstSubkernel.FlowGraph, 0),
                                                    None,
                                                    paramAttrs.[1]))
                else
                    // Store the expression (actual argument) associated to this parameter
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_1",
                                                  new FlowGraphNodeInputInfo(
                                                    ActualArgument(cleanArgs.[1]),
                                                    None,
                                                    paramAttrs.[1]))
                if secondSubkernel <> null then           
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_2",
                                                  new FlowGraphNodeInputInfo(
                                                    KernelOutput(secondSubkernel.FlowGraph, 0),
                                                    None,
                                                    paramAttrs.[2]))
                else
                    // Store the expression (actual argument) associated to this parameter
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_2",
                                                  new FlowGraphNodeInputInfo(
                                                    ActualArgument(cleanArgs.[2]),
                                                    None,
                                                    paramAttrs.[2]))
                FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                              "output_array",
                                              new FlowGraphNodeInputInfo(
                                                BufferAllocationSize(fun(args, localSize, globalSize) ->
                                                                            BufferReferenceAllocationExpression("input_array_1")),
                                                None,
                                                new DynamicParameterMetadataCollection())) *)
                // Return module                             
                Some(kernelModule)
            | _ ->
                None
