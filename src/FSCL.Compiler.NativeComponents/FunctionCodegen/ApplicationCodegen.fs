namespace FSCL.Compiler.FunctionCodegen

open FSCL
open System
open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.FSharp.Linq.RuntimeHelpers
open FSCL.Compiler.Util.QuotationAnalysis
open FSCL.Compiler.Util.ReflectionUtil
open Microsoft.FSharp.Reflection
open FSCL.Compiler.AcceleratedCollections

open FSCL.Compiler.Util.QuotationAnalysis.FunctionsManipulation
open FSCL.Compiler.Util.QuotationAnalysis.KernelParsing
open FSCL.Compiler.Util.QuotationAnalysis.MetadataExtraction

[<StepProcessor("FSCL_APPLICATION_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
                                  
///
///<summary>
///The function codegen step whose behavior is to generate the target representation of method calls 
///</summary>
///  
type ApplicationCodegen() =
    inherit FunctionBodyCodegenProcessor()

    override this.Run((expr, cont), st, opts) =
        let step = st :?> FunctionCodegenStep
        match expr with
        | Patterns.Application (_, a) ->
            // Check if this is the applicaton of the utility function of an accelerated collection
            if step.FunctionInfo :? AcceleratedKernelInfo then
                let aki = step.FunctionInfo :?> AcceleratedKernelInfo
                let args = 
                    match Util.QuotationAnalysis.FunctionsManipulation.LiftLambdaApplication(expr).Value with
                    | l, args ->
                        args |>
                        List.filter(fun (e:Expr) -> e.Type <> typeof<WorkItemInfo>) |> 
                        List.map (fun (e:Expr) -> cont(e)) |>  
                        String.concat ", "
                        
                // Check if the call is the last thing done in the function body
                // If so, prepend "return"
                let returnPrefix = 
                    if(step.FunctionInfo.CustomInfo.ContainsKey("FUNCTION_RETURN_EXPRESSIONS")) then
                        let returnTags = 
                            step.FunctionInfo.CustomInfo.["FUNCTION_RETURN_EXPRESSIONS"] :?> Expr list
                        if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                            "return "
                        else
                            ""
                    else
                        ""
                let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""
                Some(returnPrefix + aki.AppliedFunction.Value.Name + "(" + args + ")" + returnPostfix)
            else
                None
        | _ ->
            None