namespace FSCL.Compiler.ModuleParsing

open System
open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open FSCL.Language
open FSCL
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

[<assembly:DefaultComponentAssembly>]
do()

[<StepProcessor("FSCL_SEQUENTIAL_PARSING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP", 
                Dependencies = [| "FSCL_CALL_PARSING_PROCESSOR";
                                  "FSCL_LAMBDA_PARSING_PROCESSOR" |])>]
type SequentialFunctionParser() =      
    inherit ModuleParsingProcessor()
    
    override this.Run(expr, s, opts) =
        let step = s :?> ModuleParsingStep
        if (expr :? Expr) then
            let e = expr :?> Expr
            let expr, kernelAttrs, returnAttrs = ParseKernelMetadata(e)
            match LambdaToMethod(expr, false) with
            | Some(m, paramInfo, paramVars, b, _, _, _) ->
                failwith "OK"
            | _ ->
                failwith "NOT OK" 
        else
            None
            