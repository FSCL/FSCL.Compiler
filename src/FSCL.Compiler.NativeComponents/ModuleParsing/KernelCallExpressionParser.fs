namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.RuntimeHelpers

open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

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
            let norm = e :?> Expr
            let data, isLambda = 
                match GetKernelFromApplication(norm) with
                | Some(data) ->
                    Some(data), true
                | _ ->
                    match GetKernelFromCall(norm) with 
                    | Some(data) ->
                        Some(data), false
                    | _ ->
                        None, false

            match data with
            | Some(mi, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta) ->
                
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, new Dictionary<string, obj>())

                // Create module
                let kernel = new KernelInfo(mi, paramInfo, paramVars, workItemInfo, body, finalMeta, isLambda)
                let kernelModule = new KernelModule(kernel, cleanArgs)
                Some(kernelModule)   
            | _ ->     
                None
        else
            None
            