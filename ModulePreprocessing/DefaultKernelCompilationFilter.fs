namespace FSCL.Compiler.StructHandling

open FSCL.Compiler
open FSCL.Compiler.ModulePreprocessing
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System
    
[<StepProcessor("FSCL_DEFAULT_COMPILATION_FILTER_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP")>] 
type DefaultKernelCompilationFilterProcessor() = 
    inherit ModulePreprocessingProcessor()
    let rec CollectStructs(e: Expr, structs: Dictionary<Type, unit>) =
        let t = e.Type
        // If the type of the expression is a struct not already added to the collection, add it
        if (FSharpType.IsRecord(t) || (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum))) then   
            if not (structs.ContainsKey(t)) then
                // Vector types are implicitely defined in OpenCL: do not collect them
                if not (t.Assembly.GetName().Name = "FSCL.Compiler.Core.Language") then
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
        let structsDict = new Dictionary<Type, unit>()
        CollectStructs(km.Kernel.Body, structsDict)
        for t in structsDict.Keys do
           km.GlobalTypes.Add(t) |> ignore
             
            