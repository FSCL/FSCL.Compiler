namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<StepProcessor("FSCL_RETURN_TAG_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_RETURN_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_RETURN_TYPE_TRANSFORMATION_PROCESSOR";
                                  "FSCL_GLOBAL_VAR_REF_TRANSFORMATION_PROCESSOR";
                                  "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR";
                                  "FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR";
                                  "FSCL_REF_VAR_TRANSFORMATION_PROCESSOR" |])>]
type ReturnTagProcessor() =                
    inherit FunctionTransformationProcessor()

    let rec SearchReturnExpression(expr:Expr, returnExpressions: List<Expr>, engine:FunctionTransformationStep) =
        match expr with
        | Patterns.Let(v, value, body) ->
            SearchReturnExpression(body, returnExpressions, engine)
        | Patterns.Sequential(e1, e2) ->
            SearchReturnExpression(e2, returnExpressions, engine)
        | Patterns.IfThenElse(e, ifb, elseb) ->
            SearchReturnExpression(ifb, returnExpressions, engine)
            SearchReturnExpression(elseb, returnExpressions, engine)
        | _ ->
            // This could be a return expression if its value is not void
            let returnType = expr.Type
            if returnType <> typeof<unit> then
                returnExpressions.Add(expr)
            
    override this.Run(expr, en) =    
        let engine = en :?> FunctionTransformationStep
        let returnTags = new List<Expr>()
        SearchReturnExpression(expr, returnTags, engine)
        engine.FunctionInfo.CustomInfo.Add("RETURN_EXPRESSIONS", List.ofSeq returnTags)
        expr