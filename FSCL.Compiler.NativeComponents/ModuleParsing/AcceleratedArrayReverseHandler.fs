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

type AcceleratedArrayReverseHandler() =
    interface IAcceleratedCollectionHandler with
        member this.Process(methodInfo, cleanArgs, root, meta, step) =
            // We need to get the type of a array whose elements type is the same of the functionInfo parameter
            let inputArrayType = 
                    methodInfo.GetParameters().[0].ParameterType
            let outputArrayType = inputArrayType
            let kernelPrefix, functionName = 
                    "ArrayRev", "Array.rev"                    

            // Now we can create the signature and define parameter name
            let signature = DynamicMethod(kernelPrefix, outputArrayType, [| inputArrayType; outputArrayType |])
            signature.DefineParameter(1, ParameterAttributes.In, "input_array") |> ignore
            signature.DefineParameter(2, ParameterAttributes.In, "output_array") |> ignore
                                
            // Create parameters placeholders
            let inputHolder = Quotations.Var("input_array", inputArrayType)
            let outputHolder = Quotations.Var("output_array", outputArrayType)
            let tupleHolder = 
                    Quotations.Var("tupledArg", FSharpType.MakeTupleType([| inputArrayType; outputArrayType |])) 
                    
            // Finally, create the body of the kernel
            let globalIdVar = Quotations.Var("global_id", typeof<int>)
            let getElementMethodInfo, _ = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(inputArrayType.GetElementType())
            let _, setElementMethodInfo = AcceleratedCollectionUtil.GetArrayAccessMethodInfo(outputArrayType.GetElementType())
            let subMethod = QuotationAnalysis.ExtractMethodFromExpr(<@ 0 - 0 @>)
            let addMethod = QuotationAnalysis.ExtractMethodFromExpr(<@ 0 + 0 @>)
            let kernelBody = 
                Expr.Lambda(tupleHolder,
                        Expr.Let(inputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 0),
                            Expr.Let(outputHolder, Expr.TupleGet(Expr.Var(tupleHolder), 1),
                                Expr.Let(globalIdVar,
                                            Expr.Call(AcceleratedCollectionUtil.FilterCall(<@ get_global_id @>, fun(e, mi, a) -> mi).Value, [ Expr.Value(0) ]),
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
                                                Expr.Var(outputHolder))))))

            let kInfo = new AcceleratedKernelInfo(signature, 
                                                    [ inputHolder; outputHolder ],
                                                    kernelBody, 
                                                    meta, 
                                                    functionName, None)
            let kernelModule = new KernelModule(kInfo, cleanArgs)
                                
            // Return module                             
            Some(kernelModule)
