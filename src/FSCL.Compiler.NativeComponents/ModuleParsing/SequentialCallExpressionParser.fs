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

[<StepProcessor("FSCL_SEQ_PARSING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP",
                Dependencies = [| "FSCL_CALL_PARSING_PROCESSOR" |])>]
type SequentialCallExpressionParser() =      
    inherit ModuleParsingProcessor()
                    
    override this.Run((e, env), s, opts) =
        let step = s :?> ModuleParsingStep
        if (e :? Expr) then
            let norm = e :?> Expr
            let data = 
                match norm with
                | SequentialLambdaApplication(mi, args, outsiders) -> 
                    Some(mi, args, outsiders)
                | _ ->
                    None

            match data with
            | Some(mi, args, outsiders) ->
                // Create node
                let node = new KFGSequentialFunctionNode(None, mi, norm)

                // Create data node for outsiders
//                for o in outsiders do 
//                    node.InputNodes.Add(new KFGOutsiderDataNode(VarRef(o)))

                // Parse arguments
                for i = 0 to args.Length - 1 do
                    let subnode = step.Process(args.[i], env)
                    node.InputNodes.Add(subnode)

                Some(node :> IKFGNode)   
            | _ ->     
                None
        else
            None
            