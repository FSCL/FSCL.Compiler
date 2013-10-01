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
open Microsoft.FSharp.Linq.QuotationEvaluation

type AcceleratedArrayMap2Handler() =
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
            let computationFunction =
                match GetLambdaArgument(args.[0], root) with
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
            // Merge with the eventual subkernels
            let firstSubkernel =
                try
                    step.Process(args.[1])
                with
                    :? CompilerException -> null
            if firstSubkernel <> null then
                kernelModule.MergeWith(firstSubkernel)
            let secondSubkernel =
                try
                    step.Process(args.[2])
                with
                    :? CompilerException -> null
            if secondSubkernel <> null then
                kernelModule.MergeWith(secondSubkernel)
                
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
                let signature = DynamicMethod("ArrayMap2_" + functionInfo.Name, typeof<unit>, [| firstInputArrayType; secondInputArrayType; outputArrayType |])
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
                                        ]))

                // Add current kernel
                kernelModule.AddKernel(new KernelInfo(signature, kernelBody))  
                // Update call graph
                kernelModule.FlowGraph <- FlowGraphNode(signature)
                
                // Detect is device attribute set
                let device = functionInfo.GetCustomAttribute(typeof<DeviceAttribute>)
                if device <> null then
                    kernelModule.GetKernel(signature).Info.Device <- device :?> DeviceAttribute

                // Add the computation function and set that it is required by the kernel
                kernelModule.AddFunction(new FunctionInfo(functionInfo, functionBody))
                kernelModule.GetKernel(signature).RequiredFunctions.Add(functionInfo) |> ignore
                
                // Set that the return value is encoded in the parameter output_array
                kernelModule.GetKernel(signature).Info.CustomInfo.Add("RETURN_PARAMETER_NAME", "output_array")

                // Connect with subkernels
                if firstSubkernel <> null then           
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_1",
                                                  KernelOutput(firstSubkernel.FlowGraph, 0))
                else
                    // Store the expression (actual argument) associated to this parameter
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_1",
                                                  ActualArgument(args.[1]))
                if secondSubkernel <> null then           
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_2",
                                                  KernelOutput(secondSubkernel.FlowGraph, 0))
                else
                    // Store the expression (actual argument) associated to this parameter
                    FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                  "input_array_2",
                                                  ActualArgument(args.[2]))
                FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                              "output_array",
                                              ReturnedBufferAllocationSize(fun(args, localSize, globalSize) ->
                                                                            BufferReferenceAllocationExpression("input_array_1")))
                // Return module                             
                Some(kernelModule)
            | _ ->
                None
