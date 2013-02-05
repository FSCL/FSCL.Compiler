namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_STRUCT_DISCOVERY_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP",
                [||], // Replace
                [| "FSCL_GENERIC_INSTANTIATION_PROCESSOR" |])>] 
type StructDiscover() = 
    let rec CollectStructs(e: Expr, structs: Dictionary<Type, unit>) =
        let t = e.Type
        // If the type of the expression is a struct not already added to the collection, add it
        if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then   
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
            if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then   
                if not (structs.ContainsKey(t)) then
                    structs.Add(t, ())
            List.iter(fun (e:Expr) -> CollectStructs(e, structs)) l

    interface ModulePreprocessingProcessor with
        member this.Handle(km, engine) =
            let structsDict = new Dictionary<Type, unit>()
            CollectStructs(km.Source.Body, structsDict)
            // Store the struct types inside kernel module as a flat list
            let structs = List.ofSeq(seq { for item in structsDict.Keys do yield item })
            km.CustomInfo.Add("STRUCT_TYPE_DEFINITIONS", structs) 
             
            