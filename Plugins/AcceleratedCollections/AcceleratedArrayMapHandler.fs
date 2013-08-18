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

type AcceleratedArrayMapHandler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, args, root, step) =
                
            let kcg = new ModuleCallGraph()
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
            // Merge with the eventual subkernel
            let subkernel =
                try
                    step.Process(args.[1])
                with
                    :? CompilerException -> null
            if subkernel <> null then
                kcg.MergeWith(subkernel)
                
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

                let endpoints = kcg.EndPoints
                // Add current kernel
                kcg.AddKernel(new KernelInfo(signature, kernelBody)) 
                
                // Detect is device attribute set
                let device = functionInfo.GetCustomAttribute(typeof<DeviceAttribute>)
                if device <> null then
                    kcg.GetKernel(signature).Device <- device :?> DeviceAttribute

                // Add the computation function and connect it to the kernel
                kcg.AddFunction(new FunctionInfo(functionInfo, functionBody))
                kcg.AddCall(signature, functionInfo)
                // Connect with subkernel
                let argExpressions = new Dictionary<string, Expr>()
                if subkernel <> null then   
                    let retTypes =
                        if FSharpType.IsTuple(subkernel.EndPoints.[0].ID.ReturnType) then
                            FSharpType.GetTupleElements(subkernel.EndPoints.[0].ID.ReturnType)
                        else
                            [| subkernel.EndPoints.[0].ID.ReturnType |]
                    for i = 0 to retTypes.Length - 1 do                     
                        kcg.AddConnection(
                            endpoints.[0].ID, 
                            signature, 
                            ReturnValueConnection(i), ParameterConnection(signature.GetParameters().[i].Name)) 
                else
                    argExpressions.Add("input_array", args.[1])
                // Store custom info that allows the rest of the compiler pipeline to correctly build and manipulate kernel parameters
                kcg.GetKernel(signature).CustomInfo.Add("ARG_EXPRESSIONS", argExpressions)
                // Return module                             
                Some(kcg)
            | _ ->
                None
