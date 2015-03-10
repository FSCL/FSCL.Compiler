namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL.Language
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.RuntimeHelpers
open System
    
// This processor looks for FieldGet and PropertyGet of instances declared outside the 
// kernel.
// Accessing a field or a property can turn into
// A) A new parameter if the field or property can be modified (array, struct with modifiable fields, ref cell, mutable primitive or struct)
// B) A #define otherwise
[<StepProcessor("FSCL_GLOBAL_DATA_DISCOVERY_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP",
                Dependencies = [| "FSCL_FUNCTIONS_DISCOVERY_PROCESSOR" |])>] 
type GlobalDataDiscovery() = 
    inherit ModulePreprocessingProcessor()
    let rec FindGlobalDataAccess(e: Expr, dynDef: Dictionary<string, Var option * Expr option * obj>) =
        // We assume access is to global data if the object expression is a value.
        // Access to local data (e.g. struct field) is in fact generally in the form var.field, where
        // var is Expr.Var(v). This is true also if the var is in fact a ref to parameter

        // WE ASSUME INSTANCE IS "THIS", WHICH MEANS THE SAME OBJECT CONTEXT OF THE KERNEL 
        match e with
        | Patterns.FieldGet(o, fi) ->
            let attr = fi.GetCustomAttribute<ConstantDefineAttribute>()
            if attr <> null then               
                if not (dynDef.ContainsKey(fi.Name)) then
                    if o.IsSome then
                        let placeholder = Quotations.Var("ph", o.Value.Type)
                        dynDef.Add(fi.Name, (Some(placeholder), o,
                                            LeafExpressionConverter.EvaluateQuotation(
                                                Expr.Lambda(placeholder, Expr.FieldGet(Expr.Var(placeholder), fi)))))
                    else
                        let placeholder = Quotations.Var("ph", typeof<unit>)
                        dynDef.Add(fi.Name, (None, o,
                                            LeafExpressionConverter.EvaluateQuotation(
                                                Expr.Lambda(placeholder, Expr.FieldGet(fi)))))
        | Patterns.PropertyGet(ob, pi, idx) ->
            let attr = pi.GetCustomAttribute<ConstantDefineAttribute>()
            if attr <> null then               
                if not (dynDef.ContainsKey(pi.Name)) then
                    if ob.IsSome then
                        let placeholder = Quotations.Var("ph", ob.Value.Type)
                        dynDef.Add(pi.Name, (Some(placeholder), ob,
                                            LeafExpressionConverter.EvaluateQuotation(
                                                Expr.Lambda(placeholder, Expr.PropertyGet(Expr.Var(placeholder), pi, idx)))))
                    else
                        let placeholder = Quotations.Var("ph", typeof<unit>)
                        dynDef.Add(pi.Name, (None, ob,
                                            LeafExpressionConverter.EvaluateQuotation(
                                                Expr.Lambda(placeholder, Expr.PropertyGet(pi, idx)))))                                     
        | ExprShape.ShapeVar(v) ->
            ()
        | ExprShape.ShapeLambda(v, body) ->
            FindGlobalDataAccess(body, dynDef)     
        | ExprShape.ShapeCombination(o, l) ->
            l |> List.iter(fun i -> FindGlobalDataAccess(i, dynDef))     

    override this.Run(km, en, opts) =
        let engine = en :?> ModulePreprocessingStep
        // Replace immutable properties with values and generate vars for mutable ones
        let dynDef = new Dictionary<string, Var option * Expr option * obj>()
        FindGlobalDataAccess(km.Kernel.Body, dynDef)

        for t in dynDef do
            km.ConstantDefines.Add(t.Key, t.Value) |> ignore

        // Collect defines in functions
        for f in km.Functions do
            let fdynDef = new Dictionary<string, Var option * Expr option * obj>()
            FindGlobalDataAccess(f.Value.Body, fdynDef)
            for t in fdynDef do
                if not (km.ConstantDefines.ContainsKey(t.Key)) then     
                    km.ConstantDefines.Add(t.Key, t.Value) |> ignore
             
            