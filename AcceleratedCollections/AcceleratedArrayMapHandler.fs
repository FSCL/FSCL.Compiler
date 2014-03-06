namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System
open Microsoft.FSharp.Reflection
open AcceleratedCollectionUtil
open FSCL.Compiler.Core.Util
open Microsoft.FSharp.Linq.RuntimeHelpers

type AcceleratedArrayMapHandler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, args, root, step) =
                
            let kernelModule = new KernelModule()
            (*
                Array map looks like: Array.map fun collection
                At first we check if fun is a lambda (first argument)
                and in this case we transform it into a method
                Secondly, we iterate parsing on the second argument (collection)
                since it might be a subkernel
            *)
            let lambda = GetLambdaArgument(args.[0], root)
            let mutable isLambda = false
            let computationFunction =                
                match lambda with
                | Some(l) ->
                    QuotationAnalysis.LambdaToMethod(l)
                | None ->
                    AcceleratedCollectionUtil.FilterCall(args.[0], 
                        fun (e, mi, a) ->                         
                            match mi with
                            | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                                (mi, body)
                            | _ ->
                                failwith ("Cannot parse the body of the computation function " + mi.Name))
            // Merge with the eventual subkernel
            let subkernel =
                try
                    step.Process(args.[1])
                with
                    :? CompilerException -> null
            if subkernel <> null then
                subkernel.MergeWith(subkernel)
                
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
                kernelModule.FlowGraph <- FlowGraphNode(kInfo.ID)

                // Detect if device attribute set
                let device = functionInfo.GetCustomAttribute(typeof<DeviceAttribute>)
                if device <> null then
                    kernelModule.GetKernel(kInfo.ID).Info.Device <- device :?> DeviceAttribute

                // Add the computation function and connect it to the kernel
                let mapFunctionInfo = new FunctionInfo(functionInfo, functionBody, isLambda)
                kernelModule.AddFunction(mapFunctionInfo)
                kernelModule.GetKernel(kInfo.ID).RequiredFunctions.Add(mapFunctionInfo.ID) |> ignore

                // Connect with subkernel
                let argExpressions = new Dictionary<string, Expr>()
                if subkernel <> null then   
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array",
                                                  KernelOutput(subkernel.FlowGraph, 0))
                else
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array",
                                                  ActualArgument(args.[1]))
                FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                              "output_array",
                                              BufferAllocationSize(fun(args, localSize, globalSize) ->
                                                                            BufferReferenceAllocationExpression("input_array")))
                // Return module                             
                Some(kernelModule)
            | _ ->
                None
