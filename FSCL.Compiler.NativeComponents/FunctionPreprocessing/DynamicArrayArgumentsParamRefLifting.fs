namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_DYN_ARRAY_ARGS_PARAM_REF_LIFTING_PREPROCESSING_PROCESSOR", "FSCL_FUNCTION_PREPROCESSING_STEP", 
                Dependencies = [| "FSCL_ARRAY_LENGHT_ARGS_GENERATOR_PREPROCESSING_PROCESSOR" |])>] 
type DynamicArrayArgumentsParamRefLifting() =        
    inherit FunctionPreprocessingProcessor()
    
    let fixArrayRefInAllocationArg(a: Expr, kInfo: KernelInfo) =
        let rec fixInternal(e: Expr) =
            match e with
            | Patterns.Call(o, methodInfo, args) ->
                if methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "Array" && methodInfo.Name = "GetLength" then
                    match o.Value with
                    | Patterns.Var(v) ->
                        let arrayName = v.Name
                        let arraySizeParameters = kInfo.GetParameter(arrayName).Value.SizeParameters
                        match args.[0] with
                        | Patterns.Value(v, ty) -> 
                            let sizePlaceholder = arraySizeParameters.[v :?> int].Placeholder
                            Expr.Var(sizePlaceholder)
                        | _ -> 
                            raise (CompilerException("Cannot use non-constant values to address array dimensions in dynamic allocation arguments"))
                    | _ ->
                        raise (CompilerException("Cannot refer to sothing different from function parameters in dynamic allocation arguments"))
                else
                    if o.IsSome then
                        Expr.Call(o.Value, methodInfo, List.map fixInternal args)
                    else
                        Expr.Call(methodInfo, List.map fixInternal args)
            | ExprShape.ShapeVar(v) ->
                Expr.Var(v)
            | ExprShape.ShapeLambda(l, b) ->
                Expr.Lambda(l, fixInternal(b))
            | ExprShape.ShapeCombination(o, l) ->
                ExprShape.RebuildShapeCombination(o, List.map fixInternal l)            
                
        fixInternal(a)

    override this.Run(fInfo, s, opts) =
        let step = s :?> FunctionPreprocessingStep

        // In "dynamic" allocation such as Array.zeroCreate (size), the size expression might contain references 
        // to vector parameters (e.g. v.GetLength(0)) that must be lifted
        for i = 0 to fInfo.OriginalParameters.Length - 1 do
            let p = fInfo.Parameters.[i]
            match p.ParameterType with
            | DynamicParameter(args) ->
                for i = 0 to args.Length - 1 do
                    let fixedArg = fixArrayRefInAllocationArg(args.[i], fInfo :?> KernelInfo)
                    args.[i] <- fixedArg
            | _ ->
                ()

                    



            
