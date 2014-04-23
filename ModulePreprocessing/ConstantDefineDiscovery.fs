namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL.Compiler.Language
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.RuntimeHelpers
open System
    
[<StepProcessor("FSCL_DEFINE_DISCOVERY_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP",
                Dependencies = [| "FSCL_FUNCTIONS_DISCOVERY_PROCESSOR" |])>] 
type DefineDiscover() = 
    inherit ModulePreprocessingProcessor()
    let rec CollectDefine(e: Expr, defineDict: Dictionary<string, Expr * bool>) =
        // Recursive analysis
        match e with
        | Patterns.PropertyGet(o, pi, value) ->
            // A property get can be handled only if the property has a reflected definition attribute
            // If the property is mutables no #define is generated when producing OpenCL code, but a marker
            // is added to KernelModule to remember the define has to be added when compiling OpenCL code to target code
            // If the property is immutable, the define is generated when producing kernel code.
            let isStatic =
                let attr = List.ofSeq (pi.GetCustomAttributes<DynamicConstantDefineAttribute>())
                if attr.Length > 0 then
                    false
                else
                    true
            match pi with
            | DerivedPatterns.PropertyGetterWithReflectedDefinition(e) ->
                if not (defineDict.ContainsKey(pi.Name)) then
                    defineDict.Add(pi.Name, (e, isStatic))     
            | _ ->
                ()           
        | ExprShape.ShapeVar(v) ->
            ()
        | ExprShape.ShapeLambda(v, body) ->
            CollectDefine(body, defineDict)            
        | ExprShape.ShapeCombination(o, l) ->
            let t = o.GetType()           
            List.iter(fun (e:Expr) -> CollectDefine(e, defineDict)) l

    override this.Run(km, en, opts) =
        let engine = en :?> ModulePreprocessingStep
        // Collect defines in kernel
        let defineDict = new Dictionary<string, Expr * bool>()
        CollectDefine(km.Kernel.Body, defineDict)
        for t in defineDict do
            km.ConstantDefines.Add(t.Key, t.Value) |> ignore
        // Collect defines in functions
        for f in km.Functions do
            let defineDict = new Dictionary<string, Expr * bool>()
            CollectDefine(f.Value.Body, defineDict)
            for t in defineDict do
                if not (km.ConstantDefines.ContainsKey(t.Key)) then
                    km.ConstantDefines.Add(t.Key, t.Value) |> ignore
             
            