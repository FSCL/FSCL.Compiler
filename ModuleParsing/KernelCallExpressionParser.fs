namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Util
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
                
    override this.Run(e, s, opts) =
        let step = s :?> ModuleParsingStep
        if (e :? Expr) then
            match QuotationAnalysis.GetKernelFromCall(e :?> Expr) with
            // Case k2(k1(args), ...) where k1 doesn't return a tuple value
            | Some(mi, cleanArgs, body, kMeta, rMeta, pMeta) ->
                
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, new Dictionary<string, obj>())

                // Create module
                let kernel = new KernelInfo(mi, body, finalMeta, false)
                let kernelModule = new KernelModule(kernel, cleanArgs)
                
                (*
                // Extract and add eventual subkernels
                let subkernels = List.map(fun (e: Expr) -> 
                                            match engine.TryProcess(e) with
                                            | Some(result) ->
                                                kernelModule.MergeWith(result)
                                                result
                                            | _ ->
                                                null) cleanArgs
                // Update flow graph
                for i = 0 to mi.GetParameters().Length - 1 do
                    if subkernels.[i] = null then
                        FlowGraphManager.SetNodeInput(kernelModule.FlowGraph, 
                                                      mi.GetParameters().[i].Name,
                                                      new FlowGraphNodeInputInfo(
                                                        ActualArgument(cleanArgs.[i]),
                                                        Some(mi.GetParameters().[i]),
                                                        paramAttrs.[i]))
                    else
                        FlowGraphManager.SetNodeInput(kernelModule.FlowGraph, 
                                                      mi.GetParameters().[i].Name, 
                                                      new FlowGraphNodeInputInfo(
                                                        KernelOutput(subkernels.[i].FlowGraph, 0),
                                                        Some(mi.GetParameters().[i]),
                                                        paramAttrs.[i]))

                        *)
                //kernelModule.GetKernel(currentKernelID).Info.CustomInfo.Add("ARG_EXPRESSIONS", argExpressions)
                // Setup connections ret-type -> parameter
                Some(kernelModule)   
            | _ ->     
                None
            (*        
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
                        kernelModule.FlowGraph <- FlowGraphNode(MethodID(mi))
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
                None*)
        else
            None
            