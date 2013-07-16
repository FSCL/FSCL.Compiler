namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.LanguagePrimitives
open System

type AcceleratedArrayMapHandler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, args, dynModule) =
            let kcg = new KernelCallGraph()
            // Extract the map function 
            match AcceleratedCollectionUtil.FilterCall(args.[0], id) with
            | Some(expr, functionInfo, args) ->
                // Check if the referred function has a ReflectedDefinition attribute
                match functionInfo with
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->
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
                    let body = 
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

                    kcg.AddKernel(new KernelInfo(signature, body))                                   
                    Some(new KernelModule(kcg))
                | _ ->
                    None
            | _ ->
                None
