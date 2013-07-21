namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Core.Util
open System.Collections.Generic
open System.Reflection
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection

[<StepProcessor("FSCL_CALL_EXPRESSION_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
type KernelCallExpressionParser() =      
    inherit ModuleParsingProcessor()

    let rec LiftArgExtraction(expr) =
        match expr with
        | Patterns.Let(v, value, body) ->
            match value with
            | Patterns.TupleGet(te, i) ->
                LiftArgExtraction(body)
            | _ ->
                (expr)
        | _ ->
            (expr)
                
    override this.Run(mi, en) =
        let engine = en :?> ModuleParsingStep
        let kcg = new ModuleCallGraph() 
        if (mi :? Expr) then
            match mi :?> Expr with
            // Case k2(k1(args), ...) where k1 doesn't return a tuple value
            | Patterns.Call(o, m, args) ->
                // Extract sub kernels
                let subkernels = List.map(fun (e: Expr) -> 
                    try 
                        engine.Process(e)
                    with
                        | :? CompilerException -> null) args
                // Add them to kernel call graph
                for subkernel in subkernels do
                    if not (subkernel = null) then
                        kcg.MergeWith(subkernel)
                // Get endpoints
                let endpoints = kcg.EndPoints
                // Add the current kernel
                let currentKernel = 
                    try 
                        engine.Process(m)
                    with
                        | :? CompilerException -> null
                if currentKernel = null then
                    None
                else
                    kcg.AddKernel(currentKernel.GetKernel(currentKernel.KernelIDs.[0]))
                    // Setup connections ret-type -> parameter
                    let mutable endpointIndex = 0
                    for i = 0 to subkernels.Length - 1 do
                        if subkernels.[i] <> null then
                            kcg.AddConnection(
                                endpoints.[endpointIndex], 
                                currentKernel.KernelIDs.[0], 
                                ReturnValue(0), ParameterIndex(i))
                            endpointIndex <- endpointIndex + 1
                    // Return module
                    Some(kcg)                
            | Patterns.Let(v, value, body) ->
                (* 
                 * Check if we have something like:
                 * Let(tupleArg, CALLTOSUBKERNEL, Let(a..., Let(b..., CALLTOKERNEL)))
                 * This means we are returning a tuple value from the subkernel and using it
                 * to assign multiple arguments of the outer kernel
                 * This seems to happen only if KERNEL is f(a,b..z) and SUBKERNEL returns (a,b...z)
                 * (i.e. the subkernel "fills" all the parameters of kernel)
                 * but not otherwise (e.g. kernel is f(a,b,...z) and subkernel returns (a,b...x < z)
                 *)
                if v.Name = "tupledArg" then
                    let lifted = LiftArgExtraction(body)
                    match lifted with
                    | Patterns.Call(o, mi, args) ->                                       
                        let subkernel = 
                            try 
                                engine.Process(value)
                            with
                                | :? CompilerException -> null
                        // Add the subkernel to the call graph
                        if (subkernel <> null) then
                            kcg.MergeWith(subkernel)
                        // Get endpoints
                        let endpoints = kcg.EndPoints
                        // Add the current kernel
                        let currentKernel = 
                            try 
                                engine.Process(mi)
                            with
                                | :? CompilerException -> null
                        if currentKernel = null then
                            None
                        else
                            kcg.AddKernel(kcg.GetKernel(currentKernel.KernelIDs.[0]))
                            // Setup connections ret-type -> parameter                            
                            if subkernel <> null then   
                                let retTypes =
                                    if FSharpType.IsTuple(subkernel.EndPoints.[0].ReturnType) then
                                        FSharpType.GetTupleElements(subkernel.EndPoints.[0].ReturnType)
                                    else
                                        [| subkernel.EndPoints.[0].ReturnType |]
                                for i = 0 to retTypes.Length - 1 do                     
                                    kcg.AddConnection(
                                        endpoints.[0], 
                                        currentKernel.KernelIDs.[0], 
                                        ReturnValue(i), ParameterIndex(i))
                            Some(kcg)
                    | _ ->
                        None
                else
                    None
            | _ ->
                None
        else
            None
            