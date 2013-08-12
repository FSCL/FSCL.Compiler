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
                                            match engine.TryProcess(e) with
                                            | Some(result) ->
                                                kcg.MergeWith(result)
                                                result
                                            | _ ->
                                                null) args
                // Get endpoints
                let endpoints = kcg.EndPoints
                // Add the current kernel
                let currentKernel = engine.Process(m)
                kcg.AddKernel(currentKernel.Kernels.[0])
                // Set argument expressions as custom info
                let argExpressions = new Dictionary<string, Expr>()
                for i = 0 to subkernels.Length - 1 do
                    if subkernels.[i] = null then
                        argExpressions.Add(m.GetParameters().[i].Name, args.[i])
                currentKernel.Kernels.[0].CustomInfo.Add("ARG_EXPRESSIONS", argExpressions)
                // Setup connections ret-type -> parameter
                let mutable endpointIndex = 0
                for i = 0 to subkernels.Length - 1 do
                    if subkernels.[i] <> null then
                        kcg.AddConnection(
                            endpoints.[endpointIndex].ID, 
                            currentKernel.Kernels.[0].ID, 
                            ReturnValueConnection(0), ParameterConnection(currentKernel.Kernels.[0].ID.GetParameters().[i].Name))
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
                            match engine.TryProcess(value) with
                            | Some(result) ->
                                kcg.MergeWith(result)
                                result
                            | _ ->
                                null
                        // Get endpoints
                        let endpoints = kcg.EndPoints
                        // Add the current kernel
                        let currentKernel = engine.Process(mi)
                        let ki = currentKernel.Kernels.[0]
                        kcg.AddKernel(ki)
                        let kp = ki.ID.GetParameters()
                        // Setup connections ret-type -> parameter                            
                        if subkernel <> null then   
                            let retTypes =
                                if FSharpType.IsTuple(subkernel.EndPoints.[0].ID.ReturnType) then
                                    FSharpType.GetTupleElements(subkernel.EndPoints.[0].ID.ReturnType)
                                else
                                    [| subkernel.EndPoints.[0].ID.ReturnType |]
                            for i = 0 to retTypes.Length - 1 do                     
                                kcg.AddConnection(
                                    endpoints.[0].ID, 
                                    ki.ID, 
                                    ReturnValueConnection(i), ParameterConnection(kp.[i].Name))
                        Some(kcg)
                    | _ ->
                        None
                else
                    None
            | _ ->
                None
        else
            None
            