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
        member this.Process(methodInfo, cleanArgs, root, meta, step, env) =
                
            // Inspect operator
            let computationFunction, subExpr, isLambda =                
                AcceleratedCollectionUtil.ParseOperatorLambda(cleanArgs.[0], step, env)
                                
            match subExpr with
            | Some(kfg, newEnv) ->
                // This coll fun is a composition 
                let node = new KFGCollectionCompositionNode(methodInfo, kfg, newEnv)

                // Parse arguments
                let subnode = step.Process(cleanArgs.[1], env)
                node.InputNodes.Add(subnode)
                Some(node :> IKFGNode)   
            | _ ->
                // This coll fun is a kernel
                match computationFunction with
                | Some(thisVar, ob, functionInfo, functionParamVars, functionBody) ->
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
                    let signature = DynamicMethod(kernelPrefix + "_" + functionInfo.Name, outputArrayType, [| inputArrayType; typeof<WorkItemInfo> |])
                    signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
                    signature.DefineParameter(2, ParameterAttributes.In, "workItemInfo") |> ignore
                    let wiHolder = Quotations.Var("workItemInfo", typeof<WorkItemInfo>)                                

                    // Create parameters placeholders
                    let inputHolder = Quotations.Var("input_array", inputArrayType)
                    let outputHolder = Quotations.Var("output_array", outputArrayType)
                    let tupleHolder = 
                            Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputArrayType; typeof<WorkItemInfo> |])) 
                    
                    // Finally, create the body of the kernel
                    let globalIdVar = Quotations.Var("global_id", typeof<int>)
                    let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType())
                    let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
                    let kernelBody = 
                        Expr.Lambda(tupleHolder,
                            Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                                Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),                                                
                                    Expr.Let(outputHolder, Expr.Call(
                                                        ZeroCreateMethod(outputArrayType.GetElementType()), 
                                                                            [ Expr.PropertyGet(Expr.Var(inputHolder), 
                                                                                            outputArrayType.GetProperty("Length")) ]),
                                        Expr.Let(globalIdVar,
                                                    Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                    Expr.Sequential(
                                                        Expr.Call(setElementMethodInfo,
                                                                [ Expr.Var(outputHolder);
                                                                    Expr.Var(globalIdVar);
                                                                    AcceleratedCollectionUtil.BuildCallExpr(
                                                                            ob, 
                                                                            functionInfo,

                                                                            if methodInfo.Name = "Map" then
                                                                                [ Expr.Call(getElementMethodInfo,
                                                                                            [ Expr.Var(inputHolder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])]
                                                                            else
                                                                                // Mapi
                                                                                [ Expr.Var(globalIdVar);
                                                                                    Expr.Call(getElementMethodInfo,
                                                                                            [ Expr.Var(inputHolder);
                                                                                                Expr.Var(globalIdVar) 
                                                                                            ])]
                                                                )]),
                                                                Expr.Var(outputHolder)))))))

                    let methodParams = signature.GetParameters()
                    let envVars, outVals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(functionBody)
                                
                    // Add the computation function and connect it to the kernel
                    let mapFunctionInfo = new FunctionInfo(functionInfo, 
                                                           functionInfo.GetParameters() |> List.ofArray, 
                                                           functionParamVars,    
                                                           envVars,
                                                           outVals,      
                                                           None,
                                                           functionBody, isLambda)
                                                           
                    let kInfo = new AcceleratedKernelInfo(signature, 
                                                            [ methodParams.[0] ],
                                                            [ inputHolder ],     
                                                            envVars,
                                                            outVals,                                                 
                                                            kernelBody, 
                                                            meta, 
                                                            functionName, Some(mapFunctionInfo :> IFunctionInfo),
                                                            Some(functionBody))

                    let kernelModule = new KernelModule(thisVar, ob, kInfo)

                    kernelModule.Functions.Add(mapFunctionInfo.ID, mapFunctionInfo)
                    kInfo.CalledFunctions.Add(mapFunctionInfo.ID)

                    // Create node
                    let node = new KFGKernelNode(kernelModule)
                        
                    // Create data node for outsiders
//                        for o in outsiders do 
//                            node.InputNodes.Add(new KFGOutsiderDataNode(o))

                    // Parse arguments
                    let subnode = step.Process(cleanArgs.[1], env)
                    node.InputNodes.Add(subnode)
                    Some(node :> IKFGNode)   
                | _ ->
                    None
