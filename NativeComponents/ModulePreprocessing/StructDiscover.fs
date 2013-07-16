namespace FSCL.Compiler.StructHandling

open FSCL.Compiler
open FSCL.Compiler.ModulePreprocessing
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System
    
[<StepProcessor("FSCL_STRUCT_DISCOVERY_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP")>] 
type StructDiscover() = 
    inherit ModulePreprocessingProcessor()
    let rec CollectStructs(e: Expr, structs: Dictionary<Type, unit>) =
        let t = e.Type
        // If the type of the expression is a struct not already added to the collection, add it
        if (FSharpType.IsRecord(t) || (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum))) then   
            if not (structs.ContainsKey(t)) then
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

    override this.Run(km, en) =
        let engine = en :?> ModulePreprocessingStep
        let structsDict = new Dictionary<Type, unit>()
        for k in km.CallGraph.Kernels do
            CollectStructs(k.Body, structsDict)
        // Store the struct types inside kernel module as a flat list
        km.GlobalTypes <- km.GlobalTypes @ List.ofSeq(seq { for item in structsDict.Keys do yield item })
             
            