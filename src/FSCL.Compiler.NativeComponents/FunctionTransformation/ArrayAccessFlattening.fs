namespace FSCL.Compiler.FunctionTransformation

open System
open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Core.LanguagePrimitives
open FSCL.Compiler.Util

[<StepProcessor("FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_DYNAMIC_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR" |])>]
type ArrayAccessTransformation() =     
    inherit FunctionTransformationProcessor()

    let GetGenericMethodInfoFromExpr (q, ty:System.Type) = 
        let gminfo = 
            match q with 
            | Patterns.Call(_,mi,_) -> mi.GetGenericMethodDefinition()
            | _ -> failwith "unexpected failure decoding quotation at ilreflect startup"
        gminfo.MakeGenericMethod [| ty |]

    let GetArrayAccessMethodInfo ty =
        let get = GetGenericMethodInfoFromExpr(<@@ LanguagePrimitives.IntrinsicFunctions.GetArray<int> null 0 @@>, ty)
        let set = GetGenericMethodInfoFromExpr(<@@ LanguagePrimitives.IntrinsicFunctions.SetArray<int> null 0 0 @@>, ty)
        (get, set)
        (*
    let UpdateArrayAccessMode(var:string, mode:AccessAnalysisResult, engine:FunctionTransformationStep) =        
        let data = engine.FunctionInfo :?> KernelInfo
        for pInfo in data.Parameters do
            if pInfo.Name = var then
                let newMode = 
                    mode ||| pInfo.AccessAnalysis
                pInfo.AccessAnalysis <- newMode
            *)
    let GetSizeParameters(var, engine:FunctionTransformationStep) =  
        let data = engine.FunctionInfo
        let mutable sizeParameters = null
                
        for pInfo in data.Parameters do
            if pInfo.OriginalPlaceholder = var then
                sizeParameters <- pInfo.SizeParameters
                
        sizeParameters
        
    let GetPlaceholderVar(var, engine:FunctionTransformationStep) = 
        let data = engine.FunctionInfo
        let mutable placeholder = None
                
        for pInfo in data.Parameters do
            if pInfo.OriginalPlaceholder = var then
                placeholder <- Some(pInfo.Placeholder)
                
        if placeholder.IsNone then
            raise (CompilerException("Cannot determine the parameter referred by the kernel body " + var.Name))
        placeholder.Value

    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionTransformationStep
        match expr with
        | Patterns.Call(o, methodInfo, args) ->
            // If pointer arithmetic then the array must be read-write cause we "cannot" track modification
           (* if (methodInfo.Name = "[]`1.pasum") || (methodInfo.Name = "[]`1.pasub") then
                match args.[0] with
                | Patterns.Var(v) ->
                    UpdateArrayAccessMode(v.Name, AccessAnalysisResult.ReadAccess, engine)
                    UpdateArrayAccessMode(v.Name, AccessAnalysisResult.WriteAccess, engine)
                    engine.Default(expr)        
                | _ ->
                    engine.Default(expr)     *)     
            // If regular access 
            if methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "IntrinsicFunctions" then
                match args.[0] with
                | Patterns.Var(v) ->
                    //If this is a local array then do not flatten
                    if (not (engine.FunctionInfo :? KernelInfo)) || (not ((engine.FunctionInfo :?> KernelInfo).IsLocalVar(v))) then
                        let arraySizeParameters = GetSizeParameters(v, engine)
                        if methodInfo.Name = "GetArray" then
                            // Find the placeholder holding the variable of the flattened array
                            let placeholder = GetPlaceholderVar(v, engine)
                            // Update the access mode of this array
                            //UpdateArrayAccessMode(v.Name, AccessAnalysisResult.ReadAccess, engine)
                            // Recursively process the arguments, except the array reference
                            let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                            Expr.Call(methodInfo, [Expr.Var(placeholder)] @ processedArgs)
                        elif methodInfo.Name = "GetArray2D" then
                            // Find the placeholder holding the variable of the flattened array
                            let placeholder = GetPlaceholderVar(v, engine)
                            // Find the placeholders holding the array sizes
                            let sizePlaceHolders = List.ofSeq(Seq.map (fun (el:IFunctionParameter) -> Expr.Var(el.Placeholder)) (GetSizeParameters(v, engine)))
                            // Update the access mode of this array
                            //UpdateArrayAccessMode(v.Name, AccessAnalysisResult.ReadAccess, engine)
                            // Recursively process the arguments, except the array reference
                            let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                            let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[1]):int)) + %%(processedArgs.[1]):int @@>
                            // Create a new call for the flattened array
                            let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                            Expr.Call(get, [Expr.Var(placeholder); accessIndex])
                        elif methodInfo.Name = "GetArray3D" then 
                            // Find the placeholder holding the variable of the flattened array
                            let placeholder = GetPlaceholderVar(v, engine)
                            // Find the placeholders holding the array sizes
                            let sizePlaceHolders = List.ofSeq(Seq.map (fun (el:IFunctionParameter) -> Expr.Var(el.Placeholder)) (GetSizeParameters(v, engine)))
                            // Update the access mode of this array
                            //UpdateArrayAccessMode(v.Name, AccessAnalysisResult.ReadAccess, engine)
                            // Recursively process the arguments, except the array reference                   
                            let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                            let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[1]):int) * (%%(sizePlaceHolders.[2]):int)) + (%%(sizePlaceHolders.[2]):int) * (%%(processedArgs.[1]):int) + (%%(processedArgs.[2]):int) @@>
                            // Create a new call for the flattened array
                            let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                            Expr.Call(get, [Expr.Var(placeholder); accessIndex])
                        elif methodInfo.Name = "SetArray" then
                            // Find the placeholder holding the variable of the flattened array
                            let placeholder = GetPlaceholderVar(v, engine)
                            // Update the access mode of this array
                            //UpdateArrayAccessMode(v.Name, AccessAnalysisResult.WriteAccess, engine)
                            // Recursively process the arguments, except the array reference
                            let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                            // Create a new call for the flattened array
                            Expr.Call(methodInfo, [Expr.Var(placeholder)] @ processedArgs)
                        elif methodInfo.Name = "SetArray2D" then
                                // Find the placeholder holding the variable of the flattened array
                            let placeholder = GetPlaceholderVar(v, engine)
                            // Find the placeholders holding the array sizes
                            let sizePlaceHolders = List.ofSeq(Seq.map (fun (el:IFunctionParameter) -> Expr.Var(el.Placeholder)) (GetSizeParameters(v, engine)))
                            // Update the access mode of this array
                            //UpdateArrayAccessMode(v.Name, AccessAnalysisResult.WriteAccess, engine)
                            // Recursively process the arguments, except the array reference
                            let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                            let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[1]):int)) + %%(processedArgs.[1]):int @@>
                            // Create a new call for the flattened array
                            let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                            Expr.Call(set, [Expr.Var(placeholder); accessIndex; processedArgs.[2]])
                        elif methodInfo.Name = "SetArray3D" then
                                // Find the placeholder holding the variable of the flattened array
                            let placeholder = GetPlaceholderVar(v, engine)
                            // Find the placeholders holding the array sizes
                            let sizePlaceHolders = List.ofSeq(Seq.map (fun (el:IFunctionParameter) -> Expr.Var(el.Placeholder)) (GetSizeParameters(v, engine)))
                            // Update the access mode of this array
                            //UpdateArrayAccessMode(v.Name, AccessAnalysisResult.WriteAccess, engine)
                            // Recursively process the arguments, except the array reference
                            let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                            let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[1]):int) * (%%(sizePlaceHolders.[2]):int)) + (%%(sizePlaceHolders.[2]):int) * (%%(processedArgs.[1]):int) + (%%(processedArgs.[2]):int) @@>
                            // Create a new call for the flattened array
                            let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                            Expr.Call(set, [Expr.Var(placeholder); accessIndex; processedArgs.[3]])
                        else
                            engine.Default(expr)
                    else
                        engine.Default(expr)
                | _ ->
                    engine.Default(expr)

            // Get length replaced with appropriate size parameter
            elif methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "Array" && (methodInfo.Name = "GetLength" || methodInfo.Name = "GetLongLength") then
                match o.Value with
                | Patterns.Var(v) ->
                    // If local then replace with allocation expression for the proper index
                    if (engine.FunctionInfo :? KernelInfo) && (engine.FunctionInfo :?> KernelInfo).IsLocalVar(v) then
                        match args.[0] with
                        | Patterns.Value(value, _) -> 
                            let localVarData = (engine.FunctionInfo :?> KernelInfo).LocalVars.[v]
                            match localVarData with
                            | _, Some(allocArgs) ->
                                allocArgs.[value :?> int]
                            | _ ->
                                engine.Default(expr)
                        | _ ->
                            engine.Default(expr)
                    else
                        // Else
                        let arraySizeParameters = GetSizeParameters(v, engine)
                        match args.[0] with
                        | Patterns.Value(v, ty) -> 
                            let sizePlaceholder = arraySizeParameters.[v :?> int].Placeholder
                            Expr.Var(sizePlaceholder)
                        | _ -> 
                            engine.Default(expr)
                | _ ->
                    engine.Default(expr)
            else
                engine.Default(expr)
                    
        | Patterns.PropertyGet(eOpt, pInfo, args) ->
            if eOpt.IsSome && eOpt.Value.Type.IsArray then
                match eOpt.Value with
                | Patterns.Var(v) ->
                    if pInfo.DeclaringType.Name = "Array" && (pInfo.Name = "Length" || pInfo.Name = "LongLength") then
                        // If local then replace with allocation expression for the proper index
                        if (engine.FunctionInfo :? KernelInfo) && (engine.FunctionInfo :?> KernelInfo).IsLocalVar(v) then
                            let localVarData = (engine.FunctionInfo :?> KernelInfo).LocalVars.[v]
                            match localVarData with
                            | _, Some(allocArgs) ->
                                if allocArgs.Length = 1 then
                                    // One dim length
                                    allocArgs.[0]
                                else
                                    // Multiply alloc args to ge total length
                                    match QuotationAnalysis.ParseCall(<@ 1 * 1 @>) with
                                    | Some(_, mi, _) ->
                                        if allocArgs.Length = 2 then
                                            Expr.Call(mi, allocArgs)
                                        else
                                            Expr.Call(mi, [ allocArgs.[0]; Expr.Call(mi, allocArgs.Tail) ])
                                    | _ ->
                                        engine.Default(expr)                                            
                            | _ ->
                                engine.Default(expr)
                        else
                            let arraySizeParameters = GetSizeParameters(v, engine)
                            let sizePlaceholder = arraySizeParameters.[0].Placeholder
                            Expr.Var(sizePlaceholder)
                    else
                        engine.Default(expr)
                | _ ->
                    engine.Default(expr)
            else
                engine.Default(expr)
        | _ ->
            engine.Default(expr)