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
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

type AcceleratedArrayReverseHandler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, cleanArgs, root, meta, step, env, opts) =
            // We need to get the type of a array whose elements type is the same of the functionInfo parameter
            let inputArrayType = 
                    methodInfo.GetParameters().[0].ParameterType
            let outputArrayType = inputArrayType
            let kernelName, runtimeName = 
                    "ArrayRev", "Array.rev"                    

            // Now we can create the signature and define parameter name
            //let signature = DynamicMethod(kernelPrefix, outputArrayType, [| inputArrayType; outputArrayType; typeof<WorkItemInfo> |])
            //signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
            //signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
            //signature.DefineParameter(3, ParameterAttributes.In, "workItemInfo") |> ignore
                                
            // Create parameters placeholders
            let inputHolder = Quotations.Var("input_array", inputArrayType)
            let outputHolder = Quotations.Var("output_array", outputArrayType)
            let wiHolder = Quotations.Var("workItemInfo", typeof<WorkItemInfo>)
            let tupleHolder = 
                    Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputArrayType; outputArrayType; wiHolder.Type |])) 
                    
            // Finally, create the body of the kernel
            let globalIdVar = Quotations.Var("global_id", typeof<int>)
            let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType(), 1)
            let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType(), 1)
            let subMethod = 
                match ExtractCall(<@ 0 - 0 @>) with
                | Some(_, mi, _, _, _) ->
                    Some(mi)
                | _ ->
                    None
            let addMethod = 
                match ExtractCall(<@ 0 + 0 @>) with
                | Some(_, mi, _, _, _) ->
                    Some(mi)
                | _ ->
                    None
            let kernelBody = 
                Expr.Lambda(tupleHolder,
                        Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                            Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                Expr.Let(wiHolder, Expr.TupleGet(Expr.Var(tupleHolder), 2),
                                    Expr.Let(globalIdVar,
                                                Expr.Call(Expr.Var(wiHolder), typeof<WorkItemInfo>.GetMethod("GlobalID"), [ Expr.Value(0) ]),
                                                Expr.Sequential(
                                                    Expr.Call(setElementMethodInfo,
                                                            [ Expr.Var(outputHolder);
                                                                Expr.Call(
                                                                    subMethod.Value,
                                                                    [ Expr.PropertyGet(Expr.Var(inputHolder), inputArrayType.GetProperty("Length"));
                                                                      Expr.Call(
                                                                        addMethod.Value,
                                                                        [ Expr.Var(globalIdVar);
                                                                          Expr.Value(1) ])
                                                                    ]);
                                                                    Expr.Call(getElementMethodInfo,
                                                                                [ Expr.Var(inputHolder);
                                                                                    Expr.Var(globalIdVar) 
                                                                                ])
                                                            ]),
                                                    Expr.Var(outputHolder)))))))

            //let methodParams = signature.GetParameters()
            let kInfo = new AcceleratedKernelInfo(kernelName, 
                                                    methodInfo,
                                                    [ inputHolder; outputHolder ],
                                                    outputArrayType,
                                                    new List<Var>(),
                                                    new List<Expr>(),
                                                    kernelBody,
                                                    meta, 
                                                    runtimeName, None, None)
            let kernelModule = new KernelModule(None, None, kInfo)
                               
            // Create node
            let node = new KFGKernelNode(kernelModule)

            // Parse arguments
            let subnode = step.Process(cleanArgs.[0], env, opts)
            node.InputNodes.Add(subnode)

            Some(node :> IKFGNode)  