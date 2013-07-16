namespace FSCL.Compiler.FunctionTransformation

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Core.LanguagePrimitives

type internal KernelParameterTable = Dictionary<String, KernelParameterInfo>

[<StepProcessor("FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_RETURN_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_GLOBAL_VAR_REF_TRANSFORMATION_PROCESSOR";
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

    let UpdateArrayAccessMode(var:string, mode:KernelParameterAccessMode, engine:FunctionTransformationStep) =        
        let data = engine.FunctionInfo :?> KernelInfo
        for pInfo in data.ParameterInfo do
            if pInfo.Key = var then
                let newMode = 
                    match mode, pInfo.Value.Access with
                    | _, KernelParameterAccessMode.ReadWrite
                    | KernelParameterAccessMode.ReadWrite, _ ->
                        KernelParameterAccessMode.ReadWrite
                    | KernelParameterAccessMode.ReadOnly, KernelParameterAccessMode.WriteOnly
                    | KernelParameterAccessMode.WriteOnly, KernelParameterAccessMode.ReadOnly ->
                        KernelParameterAccessMode.ReadWrite
                    | _, _ ->
                        mode
                pInfo.Value.Access <- newMode
            
    let GetSizeParameters(var, engine:FunctionTransformationStep) =  
        let data = engine.FunctionInfo :?> KernelInfo
        let mutable sizeParameters = []
                
        for pInfo in data.ParameterInfo do
            if pInfo.Key = var then
                sizeParameters <- pInfo.Value.SizeParameters
                
        if sizeParameters.IsEmpty then
            raise (CompilerException("Cannot determine the size variables of array " + var + ". This means it is not a kernel parameter or you are eploying aliasing"))
        sizeParameters
        
    let GetPlaceholderVar(var, engine:FunctionTransformationStep) = 
        let data = engine.FunctionInfo :?> KernelInfo
        let mutable placeholder = None
                
        for pInfo in data.ParameterInfo do
            if pInfo.Key = var then
                placeholder <- pInfo.Value.Placeholder
                
        if placeholder.IsNone then
            raise (CompilerException("Cannot determine the parameter referred by the kernel body " + var))
        placeholder.Value

    override this.Run(expr, en) =
        let engine = en :?> FunctionTransformationStep
        match expr with
        | Patterns.Call(o, methodInfo, args) ->
            if methodInfo.DeclaringType.Name = "IntrinsicFunctions" then
                match args.[0] with
                | Patterns.Var(v) ->
                    let arraySizeParameters = GetSizeParameters(v.Name, engine)
                    if methodInfo.Name = "GetArray" then
                        // Find the placeholder holding the variable of the flattened array
                        let placeholder = GetPlaceholderVar(v.Name, engine)
                        // Update the access mode of this array
                        UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.ReadOnly, engine)
                        // Recursively process the arguments, except the array reference
                        let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                        Expr.Call(methodInfo, [Expr.Var(placeholder)] @ processedArgs)
                    elif methodInfo.Name = "GetArray2D" then
                        // Find the placeholder holding the variable of the flattened array
                        let placeholder = GetPlaceholderVar(v.Name, engine)
                        // Find the placeholders holding the array sizes
                        let sizePlaceHolders = List.map (fun (el:KernelParameterInfo) -> Expr.Var(el.Placeholder.Value)) (GetSizeParameters(v.Name, engine))
                        // Update the access mode of this array
                        UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.ReadOnly, engine)
                        // Recursively process the arguments, except the array reference
                        let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                        let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[1]):int)) + %%(processedArgs.[1]):int @@>
                        // Create a new call for the flattened array
                        let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                        Expr.Call(get, [Expr.Var(placeholder); accessIndex])
                    elif methodInfo.Name = "GetArray3D" then 
                        // Find the placeholder holding the variable of the flattened array
                        let placeholder = GetPlaceholderVar(v.Name, engine)
                        // Find the placeholders holding the array sizes
                        let sizePlaceHolders = List.map (fun (el:KernelParameterInfo) -> Expr.Var(el.Placeholder.Value)) (GetSizeParameters(v.Name, engine))
                        // Update the access mode of this array
                        UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.ReadOnly, engine)
                        // Recursively process the arguments, except the array reference                   
                        let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                        let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[0]):int) * (%%(sizePlaceHolders.[1]):int)) + (%%(sizePlaceHolders.[0]):int) * (%%(processedArgs.[1]):int) + (%%(processedArgs.[2]):int) @@>
                        // Create a new call for the flattened array
                        let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                        Expr.Call(get, [Expr.Var(placeholder); accessIndex])
                    elif methodInfo.Name = "SetArray" then
                        // Find the placeholder holding the variable of the flattened array
                        let placeholder = GetPlaceholderVar(v.Name, engine)
                        // Update the access mode of this array
                        UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.WriteOnly, engine)
                        // Recursively process the arguments, except the array reference
                        let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                        // Create a new call for the flattened array
                        Expr.Call(methodInfo, [Expr.Var(placeholder)] @ processedArgs)
                    elif methodInfo.Name = "SetArray2D" then
                            // Find the placeholder holding the variable of the flattened array
                        let placeholder = GetPlaceholderVar(v.Name, engine)
                        // Find the placeholders holding the array sizes
                        let sizePlaceHolders = List.map (fun (el:KernelParameterInfo) -> Expr.Var(el.Placeholder.Value)) (GetSizeParameters(v.Name, engine))
                        // Update the access mode of this array
                        UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.WriteOnly, engine)
                        // Recursively process the arguments, except the array reference
                        let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                        let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[1]):int)) + %%(processedArgs.[1]):int @@>
                        // Create a new call for the flattened array
                        let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                        Expr.Call(set, [Expr.Var(placeholder); accessIndex; processedArgs.[2]])
                    elif methodInfo.Name = "SetArray3D" then
                            // Find the placeholder holding the variable of the flattened array
                        let placeholder = GetPlaceholderVar(v.Name, engine)
                        // Find the placeholders holding the array sizes
                        let sizePlaceHolders = List.map (fun (el:KernelParameterInfo) -> Expr.Var(el.Placeholder.Value)) (GetSizeParameters(v.Name, engine))
                        // Update the access mode of this array
                        UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.WriteOnly, engine)
                        // Recursively process the arguments, except the array reference
                        let processedArgs = args |> List.tail |> List.map (fun (a:Expr) -> engine.Continue(a))
                        let accessIndex = <@@ ((%%(processedArgs.[0]):int) * (%%(sizePlaceHolders.[0]):int) * (%%(sizePlaceHolders.[1]):int)) + (%%(sizePlaceHolders.[0]):int) * (%%(processedArgs.[1]):int) + (%%(processedArgs.[2]):int) @@>
                        // Create a new call for the flattened array
                        let (get,set) = GetArrayAccessMethodInfo(v.Type.GetElementType())
                        Expr.Call(set, [Expr.Var(placeholder); accessIndex; processedArgs.[3]])
                    else
                        engine.Default(expr)
                | _ ->
                    engine.Default(expr)

            // Get length replaced with appropriate size parameter
            elif methodInfo.DeclaringType.Name = "Array" && methodInfo.Name = "GetLength" then
                match o.Value with
                | Patterns.Var(v) ->
                    let arrayName = v.Name
                    let arraySizeParameters = GetSizeParameters(arrayName, engine)
                    match args.[0] with
                    | Patterns.Value(v, ty) -> 
                        let sizePlaceholder = arraySizeParameters.[v :?> int].Placeholder
                        Expr.Var(sizePlaceholder.Value)
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
                    let arrayName = v.Name
                    let arraySizeParameters = GetSizeParameters(arrayName, engine)
                    if pInfo.DeclaringType.Name = "Array" && pInfo.Name = "Length" then
                        let sizePlaceholder = arraySizeParameters.[0].Placeholder
                        Expr.Var(sizePlaceholder.Value)
                    else
                        engine.Default(expr)
                | _ ->
                    engine.Default(expr)
            else
                engine.Default(expr)
        | _ ->
            engine.Default(expr)