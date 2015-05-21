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
            let data = 
                match norm with
                | KernelCall(obv, ob, mi, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta) -> 
                    Some(obv, ob, mi.Name, Some(mi), mi.ReturnType, Some(paramInfo), paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta)
                | KernelLambdaApplication(obv, ob, lambdaName, returnType, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta) -> 
                    Some(obv, ob, lambdaName, None, returnType, None, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta)
                | _ ->
                    None

            match data with
            | Some(obv, ob, miName, mi, returnType, paramInfo, paramVars, body, cleanArgs, workItemInfo, kMeta, rMeta, pMeta) ->
                // Filter and finalize metadata
                let finalMeta = step.ProcessMeta(kMeta, rMeta, pMeta, new Dictionary<string, obj>(), opts)

                // Analyze body
                let envVars, outVals = QuotationAnalysis.KernelParsing.ExtractEnvRefs(body)

                // Create module
                let kernel = new KernelInfo(miName,
                                            mi, 
                                            paramVars,
                                            returnType,
                                            envVars, outVals,
                                            (if workItemInfo.IsSome then
                                                Some(LeafExpressionConverter.EvaluateQuotation(workItemInfo.Value) :?> WorkItemInfo)
                                             else
                                                None),
                                            body,
                                            finalMeta)
                let kernelModule = new KernelModule(obv, ob, kernel)

                // Create node
                let node = new KFGKernelNode(kernelModule)

                // Parse arguments
                for i = 0 to paramVars.Length - 1 do
                    let subnode = step.Process(cleanArgs.[i], env, opts)
                    node.InputNodes.Add(subnode)
                Some(node :> IKFGNode)   
            | _ ->     
                None
        else
            None
            