namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_FUNCTIONS_DISCOVERY_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP",
                Dependencies = [| "FSCL_DEFAULT_COMPILATION_FILTER_PROCESSOR" |])>] 
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
                            match QuotationAnalysis.LiftArgs(b) with
                            | Some(liftBody, paramVars) ->       
                                foundFunctions.Add(mi, new FunctionInfo(mi, paramVars, liftBody, false))
                            | _ ->
                                ()
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
        let found = DiscoverFunctionRef(m.Kernel)
        for item in found do
            if not (m.Functions.ContainsKey(item.Value.ID)) then
                m.Functions.Add(item.Value.ID, item.Value)
            