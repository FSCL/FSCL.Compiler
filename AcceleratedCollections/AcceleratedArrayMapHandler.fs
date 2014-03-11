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
        member this.Process(methodInfo, cleanArgs, root, kernelAttrs, paramAttrs, step) =
                
            let kernelModule = new KernelModule()
            (*
                Array map looks like: Array.map fun collection
                At first we check if fun is a lambda (first argument)
                and in this case we transform it into a method
                Secondly, we iterate parsing on the second argument (collection)
                since it might be a subkernel
            *)
            let lambda, computationFunction =                
                AcceleratedCollectionUtil.ExtractComputationFunction(cleanArgs, root)

            // Merge with the eventual subkernel
            let subkernel =
                try
                    step.Process(cleanArgs.[1])
                with
                    :? CompilerException -> null
            if subkernel <> null then
                kernelModule.MergeWith(subkernel)
                
            // Extract the map function 
            match computationFunction with
            | Some(functionInfo, functionBody) ->
                // Now create the kernel
                // We need to get the type of a array whose elements type is the same of the functionInfo parameter
                let inputArrayType = Array.CreateInstance(functionInfo.GetParameters().[0].ParameterType, 0).GetType()
                let outputArrayType = Array.CreateInstance(functionInfo.ReturnType, 0).GetType()
                (* let inputEmptyArray = FilterCall(<@ Array.empty @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| inputArrayElementType |])).Value
                // We need to get the type of a array whose elements type is the same of the functionInfo return value
                let outputArrayElementType = functionInfo.ReturnType
                let outputEmptyArray = FilterCall(<@ Array.empty @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition().MakeGenericMethod([| outputArrayElementType |])).Value
                *)
                // Now that we have the types of the input and output arrays, create placeholders (var) for the kernel input and output                    
                let inputArrayPlaceholder = Expr.Var(Quotations.Var("input_array", inputArrayType))
                let outputArrayPlaceholder = Expr.Var(Quotations.Var("output_array", outputArrayType))
                    
                // Now we can create the signature and define parameter name
                let signature = DynamicMethod("ArrayMap_" + functionInfo.Name, typeof<unit>, [| inputArrayType; outputArrayType |])
                signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                    
                // Finally, create the body of the kernel
                let globalIdVar = Quotations.Var("global_id", typeof<int>)
                let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType())
                let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                let kernelBody = 
                    Expr.Let(globalIdVar,
                                Expr.Call(AcceleratedCollectionUtil.FilterCall(<@ get_global_id @>, fun(e, mi, a) -> mi).Value, [ Expr.Value(0) ]),
                                Expr.Call(setElementMethodInfo,
                                        [ outputArrayPlaceholder;
                                            Expr.Var(globalIdVar);
                                            Expr.Call(functionInfo,
                                                    [ Expr.Call(getElementMethodInfo,
                                                                [ inputArrayPlaceholder;
                                                                    Expr.Var(globalIdVar) 
                                                                ])
                                                    ])
                                        ]))

                // Add current kernel
                let kInfo = new AcceleratedKernelInfo(signature, kernelBody, "Array.map", functionBody)
                kInfo.CustomInfo.Add("IS_ACCELERATED_COLLECTION_KERNEL", true)
                kernelModule.AddKernel(kInfo) 
                // Update call graph
                kernelModule.FlowGraph <- FlowGraphNode(kInfo.ID, None, kernelAttrs)

                // Add the computation function and connect it to the kernel
                let mapFunctionInfo = new FunctionInfo(functionInfo, functionBody, lambda.IsSome)
                kernelModule.AddFunction(mapFunctionInfo)
                kernelModule.GetKernel(kInfo.ID).RequiredFunctions.Add(mapFunctionInfo.ID) |> ignore
                                
                // Set that the return value is encoded in the parameter output_array
                kernelModule.GetKernel(kInfo.ID).Info.CustomInfo.Add("RETURN_PARAMETER_NAME", "output_array")

                // Connect with subkernel
                let argExpressions = new Dictionary<string, Expr>()
                if subkernel <> null then   
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
                                                new DynamicParameterAttributeCollection()))
                // Return module                             
                Some(kernelModule)
            | _ ->
                None
