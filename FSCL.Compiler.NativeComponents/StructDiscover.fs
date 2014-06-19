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
                Dependencies = [| "FSCL_FUNCTIONS_DISCOVERY_PROCESSOR" |])>] 
type StructDiscover() = 
    inherit ModulePreprocessingProcessor()
    let rec CollectStructs(e: Expr, structs: Dictionary<Type, unit>) =
        let t = e.Type
        // If the type of the expression is a struct not already added to the collection, add it
        if (FSharpType.IsRecord(t) || (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum))) then   
            if not (structs.ContainsKey(t)) then
                // Vector types are implicitely defined in OpenCL: do not collect them
                if (t.GetCustomAttribute<VectorTypeAttribute>() = null) then
                    structs.Add(t, ())

        // Recursive analysis
        match e with
        | ExprShape.ShapeVar(v) ->
            ()
        | ExprShape.ShapeLambda(v, body) ->
            CollectStructs(body, structs)            
        | ExprShape.ShapeCombination(o, l) ->
            let t = o.GetType()
           
            // If the type of the object is a struct not already added to the collection, add it
            if (FSharpType.IsRecord(t) || (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum))) then      
                if not (structs.ContainsKey(t)) then
                    structs.Add(t, ())
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
            
             
            