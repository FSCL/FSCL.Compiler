namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL
open FSCL.Compiler.ModulePreprocessing
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System
    
[<StepProcessor("FSCL_STRUCT_DISCOVERY_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP",
                Dependencies = [| "FSCL_FUNCTIONS_DISCOVERY_PROCESSOR";
                                  //"FSCL_ENV_REFS_DISCOVERY_PROCESSOR" 
                               |])>] 
type StructDiscover() = 
    inherit ModulePreprocessingProcessor()
    let rec CollectStructsTypes(types: Type[], structs: Dictionary<Type, unit>) =
        for t in types do
            if FSharpType.IsTuple(t) then
                if not (structs.ContainsKey(t)) then
                    // Add the struct
                    structs.Add(t, ())            
                    // Recursively check fields
                    CollectStructsTypes(FSharpType.GetTupleElements(t), structs)

            // Check if the type is an option type
            else if (t.IsGenericType && t.GetGenericTypeDefinition() = typeof<int option>.GetGenericTypeDefinition()) then
                if not (structs.ContainsKey(t)) then
                    // Add the struct
                    structs.Add(t, ())
                    // Recursively check fields
                    CollectStructsTypes(t.GetGenericArguments(), structs)

            // If the type of the expression is a struct not already added to the collection, add it
            else if (FSharpType.IsRecord(t) || (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum) && (typeof<unit> <> t) && (typeof<System.Void> <> t))) then   
                // Check this is not a ref type
                if (not(t.IsGenericType) || (t.GetGenericTypeDefinition() <> typeof<Microsoft.FSharp.Core.ref<int>>.GetGenericTypeDefinition())) then
                    if not (structs.ContainsKey(t)) then
                        // Vector types are implicitely defined in OpenCL: do not collect them
                        if (t.GetCustomAttribute<VectorTypeAttribute>() = null) then
                            structs.Add(t, ())                            
                            // Recursively check fields
                            if FSharpType.IsRecord(t) then
                                CollectStructsTypes(FSharpType.GetRecordFields(t) |> Array.map(fun i -> i.PropertyType), structs)
                            else
                                // Struct
                                CollectStructsTypes(t.GetFields() |> Array.map(fun i -> i.FieldType), structs)


    let rec CollectStructs(e: Expr, structs: Dictionary<Type, unit>) =
        let isArgumentPreparation =
            match e with
            | Patterns.Var(v) when v.Name = "tupledArg" ->
                true
            | _ ->
                false
        let t = e.Type

        // Check if tuple and not argument preparation
        if not(FSharpType.IsTuple(t)) || not isArgumentPreparation then
            CollectStructsTypes([| t |], structs)

        // Recursive analysis
        match e with
        | ExprShape.ShapeVar(v) ->
            ()
        | ExprShape.ShapeLambda(v, body) ->
            CollectStructs(body, structs)            
        | ExprShape.ShapeCombination(o, l) ->
            // If the type of the object is a struct not already added to the collection, add it
            List.iter(fun (e:Expr) -> CollectStructs(e, structs)) l

    override this.Run(km, en, opts) =
        let engine = en :?> ModulePreprocessingStep
        // Collect structs in kernel
        let structsDict = new Dictionary<Type, unit>()
        CollectStructs(km.Kernel.Body, structsDict)
        for t in structsDict.Keys do
            km.GlobalTypes.Add(t) |> ignore
        // Collect structs in functions
        for f in km.Functions do
            let structsDict = new Dictionary<Type, unit>()
            CollectStructs(f.Value.Body, structsDict)
            for t in structsDict.Keys do
                km.GlobalTypes.Add(t) |> ignore
            
             
            