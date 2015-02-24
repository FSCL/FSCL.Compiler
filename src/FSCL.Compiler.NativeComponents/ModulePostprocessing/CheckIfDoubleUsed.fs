namespace FSCL.Compiler.ModulePostprocessing

open FSCL.Compiler
open FSCL
open FSCL.Compiler.ModulePreprocessing
open FSCL.Compiler.Util.ReflectionUtil
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System
    
[<StepProcessor("FSCL_CHECK_DOUBLE_USED_PROCESSOR", 
                "FSCL_MODULE_POSTPROCESSING_STEP")>] 
type CheckIfDoubleUsed() = 
    inherit ModulePostprocessingProcessor()
    
    let rec CheckDoubleInStructs(t: IEnumerable<Type>) =
        let mutable found = false
        let mutable i = 0
        let ts = new List<Type>(t)
        while (not found && i < ts.Count) do
            let t = ts.[i]
            if t.IsStruct() then
                let fi = t.GetFields(BindingFlags.Public ||| BindingFlags.Instance)
                let r = fi |> Array.tryFind(fun f -> f.FieldType = typeof<double> || CheckDoubleInStructs([ f.FieldType ]))
                found <- r.IsSome
            else if FSharpType.IsRecord(t) then
                let fi = FSharpType.GetRecordFields(t)
                let r = fi |> Array.tryFind(fun f -> f.PropertyType = typeof<double> || CheckDoubleInStructs([ f.PropertyType ]))
                found <- r.IsSome
            i <- i + 1   
        found        

    let rec CheckDoubleInCode(e: Expr) =
        if (e.Type.IsArray && e.Type.GetElementType() = typeof<double>) || e.Type = typeof<double> then
            true
        else 
            match e with
            | ExprShape.ShapeVar(v) ->
                (v.Type.IsArray && v.Type.GetElementType() = typeof<double>) || v.Type = typeof<double>
            | ExprShape.ShapeLambda(v, b) ->    
                CheckDoubleInCode(b)
            | ExprShape.ShapeCombination(o, l) ->
                let r = l |> List.tryFind(CheckDoubleInCode)
                r.IsSome


    override this.Run(km, st, opts) =
        let step = st :?> ModulePostprocessingStep
        let mutable usingDouble = CheckDoubleInStructs(km.GlobalTypes) || CheckDoubleInCode(km.Kernel.Body)
        if not usingDouble then
            for f in km.Functions do
                usingDouble <- usingDouble || CheckDoubleInCode(f.Value.Body)
        if usingDouble then
            // Add pragma
            km.Directives.Add("#pragma OPENCL EXTENSION cl_khr_fp64 : enable") |> ignore



            
             
            