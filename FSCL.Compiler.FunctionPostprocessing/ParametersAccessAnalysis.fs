namespace FSCL.Compiler.FunctionPostprocessing

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open FSCL.Compiler.Language
open System

[<StepProcessor("FSCL_PARAM_ACCESS_ANALYSIS_PREPROCESSING_PROCESSOR", "FSCL_FUNCTION_POSTPROCESSING_STEP")>] 
type ParametersAccessAnalysis() =        
    inherit FunctionPostprocessingProcessor()
    
    let DiscoverArrayParametersReferences(a: Expr, kInfo: FunctionInfo) =
        let rec discoverReferencesInternal(e: Expr, parent: Expr option) =
            match e with
            | ExprShape.ShapeVar(v) ->
                // Check if this v is referencing a param
                let par = List.tryFind (fun (p:FunctionParameter) -> p.Placeholder = v) (kInfo.Parameters)
                if par.IsSome then
                    if par.Value.Placeholder.Type.IsArray then
                        // Original array
                        // Check if this is an array access
                        if parent.IsSome then
                            match parent.Value with
                            | Patterns.Call(o, methodInfo, args) ->
                                // If regular access 
                                if methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "IntrinsicFunctions" then
                                    if methodInfo.Name = "GetArray" ||
                                       methodInfo.Name = "GetArray2D" ||
                                       methodInfo.Name = "GetArray3D" then
                                        par.Value.AccessAnalysis <- par.Value.AccessAnalysis ||| AccessAnalysisResult.ReadAccess
                                    if methodInfo.Name = "SetArray" ||
                                       methodInfo.Name = "SetArray2D" ||
                                       methodInfo.Name = "SetArray3D" then
                                        par.Value.AccessAnalysis <- par.Value.AccessAnalysis ||| AccessAnalysisResult.WriteAccess
                                // If access to length or long length        
                                elif methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "Array" && (methodInfo.Name = "GetLength" || methodInfo.Name = "GetLongLength") then
                                    ()
                            // If access to length or long length propery
                            | Patterns.PropertyGet(eOpt, pInfo, args) ->
                                if pInfo.DeclaringType.Name = "Array" && (pInfo.Name = "Length" || pInfo.Name = "LongLength") then
                                    ()
                            | _ ->
                                // Cannot track modifications, set par to ReadWrite
                                par.Value.AccessAnalysis <- AccessAnalysisResult.ReadAccess ||| AccessAnalysisResult.WriteAccess
                         
            | ExprShape.ShapeLambda(l, b) ->
                discoverReferencesInternal(b, Some(e))
            | ExprShape.ShapeCombination(o, l) ->
                List.iter (fun (exp:Expr) -> discoverReferencesInternal(exp, Some(e))) l    
                
        discoverReferencesInternal(a, None)

    override this.Run(fInfo, s, opts) =
        let step = s :?> FunctionPostprocessingStep
        DiscoverArrayParametersReferences(fInfo.Body, fInfo)

                    



            
