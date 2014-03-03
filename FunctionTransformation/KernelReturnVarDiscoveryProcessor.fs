namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

[<StepProcessor("FSCL_KERNEL_RETURN_DISCOVERY_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_ARG_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_DYNAMIC_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_GLOBAL_VAR_REF_TRANSFORMATION_PROCESSOR";
                                  "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR";
                                  "FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR";
                                  "FSCL_REF_VAR_TRANSFORMATION_PROCESSOR" |])>]
type KernelReturnExpressionDiscoveryProcessor() =                
    inherit FunctionTransformationProcessor()

    let rec SearchReturnExpression(expr:Expr, returnExpression: Var option ref, engine:FunctionTransformationStep) =
        match expr with
        | Patterns.Let(v, value, body) ->
            SearchReturnExpression(body, returnExpression, engine)
        | Patterns.Sequential(e1, e2) ->
            SearchReturnExpression(e2, returnExpression, engine)
        | Patterns.IfThenElse(e, ifb, elseb) ->
            SearchReturnExpression(ifb, returnExpression, engine)
            SearchReturnExpression(elseb, returnExpression, engine)
        | _ ->
            // This could be a return expression if its type is subtype of return type
            let returnType = expr.Type
            if returnType <> typeof<unit> && engine.FunctionInfo.ReturnType.IsAssignableFrom(returnType) then
                // Verify that return expression is a reference to a parameter
                match expr with
                | Patterns.Var(v) -> 
                    if returnExpression.Value.IsSome then
                        if returnExpression.Value.Value = v then
                            // OK
                            ()
                        else
                            raise (new CompilerException("Cannot return different variables/parameters from within a kernel"))
                    else
                        // CHECK THAT THIS IS A REFERENCE TO A PARAMETER
                        let p = Seq.tryFind(fun (e:KernelParameterInfo) -> e.Name = v.Name) (engine.FunctionInfo.Parameters)
                        if p.IsNone then
                            raise (new CompilerException("Kernels can only return a reference to a parameter or to a dynamically allocated array"))
                        
                        p.Value.IsReturnParameter <- true    
                        returnExpression := Some(v)
                | _ ->
                    raise (new CompilerException("Only a reference to a parameter can be returned from within a kernel"))            
            
    override this.Run(expr, en, opts) =    
        let engine = en :?> FunctionTransformationStep
        let returnVariable = ref None

        if engine.FunctionInfo :? KernelInfo then
            SearchReturnExpression(expr, returnVariable , engine)

            // Verify that return expressions are all references
            if returnVariable.Value.IsSome then
                engine.FunctionInfo.CustomInfo.Add("RETURN_VARIABLE", returnVariable.Value.Value)
        expr
        