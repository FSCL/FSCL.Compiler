namespace FSCL.Compiler.FunctionTransformation

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Core.LanguagePrimitives

[<StepProcessor("FSCL_REF_VAR_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_RETURN_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_GLOBAL_VAR_REF_TRANSFORMATION_PROCESSOR";
                                  "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR";
                                  "FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR" |])>]
type RefVariableTransformationProcessor() =     
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
        for pInfo in data.Parameters do
            if pInfo.Name = var then
                let newMode = 
                    match mode, pInfo.Access with
                    | _, KernelParameterAccessMode.ReadWrite
                    | KernelParameterAccessMode.ReadWrite, _ ->
                        KernelParameterAccessMode.ReadWrite
                    | KernelParameterAccessMode.ReadOnly, KernelParameterAccessMode.WriteOnly
                    | KernelParameterAccessMode.WriteOnly, KernelParameterAccessMode.ReadOnly ->
                        KernelParameterAccessMode.ReadWrite
                    | _, _ ->
                        mode
                pInfo.Access <- newMode
                    
    let GetPlaceholderVar(var, engine:FunctionTransformationStep) = 
        let data = engine.FunctionInfo :?> KernelInfo
        let mutable placeholder = None
                
        for pInfo in data.Parameters do
            if pInfo.Name = var then
                placeholder <- pInfo.Placeholder
                
        if placeholder.IsNone then
            raise (CompilerException("Cannot determine the parameter referred by the kernel body " + var))
        placeholder.Value

    override this.Run(expr, en) =
        let engine = en :?> FunctionTransformationStep
        match expr with
        | DerivedPatterns.SpecificCall (<@ (!) @>) (e, tl, args) ->
            match args.[0] with
            | Patterns.Var(v) ->
                // Find the placeholder holding the variable of the "arrayzed" ref 
                let placeholder = GetPlaceholderVar(v.Name, engine)
                // Update the access mode of this ref
                UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.ReadOnly, engine)
                // Create new array access expression
                let (readArr, _) = GetArrayAccessMethodInfo (placeholder.Type.GetElementType())
                Expr.Call(readArr, [Expr.Var(placeholder); Expr.Value(0)])
            | _ ->
                engine.Default(expr)
        | DerivedPatterns.SpecificCall (<@ (:=) @>) (e, tl, args) -> 
            match args.[0] with
            | Patterns.Var(v) ->
                // Find the placeholder holding the variable of the "arrayzed" ref 
                let placeholder = GetPlaceholderVar(v.Name, engine)
                // Update the access mode of this ref
                UpdateArrayAccessMode(v.Name, KernelParameterAccessMode.WriteOnly, engine)
                // Create new array access expression
                let (_, writeArr) = GetArrayAccessMethodInfo (placeholder.Type.GetElementType())
                Expr.Call(writeArr, [Expr.Var(placeholder); Expr.Value(0); engine.Continue(args.[1])])
            | _ ->
                engine.Default(expr)
        | _ ->
            engine.Default(expr)