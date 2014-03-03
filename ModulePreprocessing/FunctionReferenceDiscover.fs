namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_FUNCTIONS_DISCOVERY_PROCESSOR", "FSCL_MODULE_PREPROCESSING_STEP")>] 
type FunctionReferenceDiscover() =      
    inherit ModulePreprocessingProcessor()

    let DiscoverFunctionRef(k:KernelInfo) =
        let foundFunctions = Dictionary<MethodInfo, FunctionInfo>()

        let rec DiscoverFunctionRefInner(expr) =
            match expr with
            | Patterns.Call(o, mi, args) ->
                List.iter (fun el -> DiscoverFunctionRefInner(el)) args
                try
                    match mi with
                    | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                        if not (foundFunctions.ContainsKey(mi)) then
                            foundFunctions.Add(mi, new FunctionInfo(mi, b))
                    | _ ->
                        ()
                with
                    :? NullReferenceException -> ()
            | ExprShape.ShapeLambda(v, a) ->
                DiscoverFunctionRefInner(a)
            | ExprShape.ShapeCombination(o, list) ->
                List.iter (fun el -> DiscoverFunctionRefInner(el)) list
            | _ ->
                ()

        DiscoverFunctionRefInner(k.Body)
        foundFunctions

    override this.Run(m, en, opts) =
        let engine = en :?> ModulePreprocessingStep
        for k in m.GetKernels() do
            let found = DiscoverFunctionRef(k.Info)
            for item in found do
                m.AddFunction(item.Value)
                k.RequiredFunctions.Add(item.Value.ID) |> ignore
            