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

[<StepProcessor("FSCL_CALL_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
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
                
    override this.Run((e, env), s, opts) =
        let step = s :?> ModuleParsingStep
        if (e :? Expr) then
            let norm = e :?> Expr
            let data, isLambdaFun = 
                match norm with
                | KernelCall(obv, ob, mi, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta) -> 
                    Some(obv, ob, mi, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta), false
                | KernelLambdaApplication(obv, ob, mi, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta) -> 
                    Some(obv, ob, mi, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta), true
                | _ ->
                    None, false

            match data with
            | Some(obv, ob, mi, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta) ->
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, new Dictionary<string, obj>())

                // Create module
                let kernel = new KernelInfo(obv, ob, mi, paramInfo |> List.ofArray, paramVars, env, workItemInfo, body, finalMeta, isLambdaFun)
                let kernelModule = new KernelModule(kernel, cleanArgs)

                // Create node
                let node = new KFGKernelNode(kernelModule)

                // Parse arguments
                for i = 0 to paramVars.Length do
                    let subnode = step.Process(cleanArgs.[i], env)
                    node.InputNodes.Add(subnode)
                Some(node :> IKFGNode)   
            | _ ->     
                None
        else
            None
            