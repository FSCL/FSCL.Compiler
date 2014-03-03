namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Core.Util
open System.Collections.Generic
open System.Reflection
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.RuntimeHelpers

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
                
    override this.Run(mi, en, opts) =
        let engine = en :?> ModuleParsingStep
        let kernelModule = new KernelModule()
        if (mi :? Expr) then
            match mi :?> Expr with
            // Case k2(k1(args), ...) where k1 doesn't return a tuple value
            | Patterns.Call(o, m, args) ->
                // Add the current kernel
                kernelModule.MergeWith(engine.Process(m))
                // Set the root of the flow graph
                kernelModule.FlowGraph <- FlowGraphNode(m)

                // Extract and add eventual subkernels
                let subkernels = List.map(fun (e: Expr) -> 
                                            match engine.TryProcess(e) with
                                            | Some(result) ->
                                                kernelModule.MergeWith(result)
                                                result
                                            | _ ->
                                                null) args
                // Update flow graph
                for i = 0 to m.GetParameters().Length - 1 do
                    if subkernels.[i] = null then
                        FlowGraphManager.SetNodeInput(kernelModule.FlowGraph, 
                                                      m.GetParameters().[i].Name,
                                                      ActualArgument(args.[i]))
                    else
                        FlowGraphManager.SetNodeInput(kernelModule.FlowGraph, m.GetParameters().[i].Name, KernelOutput(subkernels.[i].FlowGraph, 0))

                        
                //kernelModule.GetKernel(currentKernelID).Info.CustomInfo.Add("ARG_EXPRESSIONS", argExpressions)
                // Setup connections ret-type -> parameter
                Some(kernelModule)                
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
                        // Add the current kernel
                        kernelModule.MergeWith(engine.Process(mi))
                        // Extract and add the eventual subkernel
                        let subkernel =                         
                            match engine.TryProcess(value) with
                            | Some(result) ->
                                kernelModule.MergeWith(result)
                                result
                            | _ ->
                                null
                        // Update call graph
                        kernelModule.FlowGraph <- FlowGraphNode(mi)
                        // Setup connections ret-type -> parameter                            
                        if subkernel <> null then   
                            let retTypes =
                                if FSharpType.IsTuple(subkernel.GetKernel(subkernel.FlowGraph.KernelID).Info.ReturnType) then
                                    FSharpType.GetTupleElements(subkernel.GetKernel(subkernel.FlowGraph.KernelID).Info.ReturnType)
                                else
                                    [| subkernel.GetKernel(subkernel.FlowGraph.KernelID).Info.ReturnType |]
                            for i = 0 to retTypes.Length - 1 do        
                                FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                              mi.GetParameters().[i].Name,
                                                              KernelOutput(subkernel.FlowGraph, i))
                        else
                            for i = 0 to mi.GetParameters().Length - 1 do
                                FlowGraphManager.SetNodeInput(kernelModule.FlowGraph,
                                                              mi.GetParameters().[i].Name, 
                                                              ActualArgument(args.[i]))
                        Some(kernelModule)
                    | _ ->
                        None
                else
                    None
            | _ ->
                None
        else
            None
            