namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.ModuleParsing
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_ACCELERATED_ARRAY_MODULE_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>] 
type AcceleratedArrayParser() = 
    // Find a call inside an expr and do something on the matched pattern
    let rec FilterCall(expr, f) =                 
        match expr with
        | Patterns.Lambda(v, e) -> 
            FilterCall (e, f)
        | Patterns.Let (v, e1, e2) ->
            FilterCall (e2, f)
        | Patterns.Call (e, mi, a) ->
            Some(f(e, mi, a))
        | _ ->
            None 

    // The List module type        
    let listModuleType = FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.DeclaringType).Value

    // The set of List functions handled by the parser
    let handlers = new Dictionary<MethodInfo, IAcceleratedCollectionHandler>()
    do 
        handlers.Add(FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMapHandler())
        //handlers.Add(FilterCall(<@ Array.reduce @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayReduceHandler())
    
    interface ModuleParsingProcessor with
        member this.Process(o, en) =
            let engine = en :?> ModuleParsingStep
            match FilterCall(o :?> Expr, fun a -> a) with
            | Some(item, methodInfo, args) -> 
                if methodInfo.DeclaringType = listModuleType then
                    if (handlers.ContainsKey(methodInfo.GetGenericMethodDefinition())) then
                        handlers.[methodInfo.GetGenericMethodDefinition()].Process(methodInfo, args)
                    else
                        None
                else
                    None
            | _ ->
                None
             
            