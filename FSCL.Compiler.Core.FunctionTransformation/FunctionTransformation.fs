namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

[<Step("FSCL_FUNCTION_TRANSFORMATION_STEP",
       [| "FSCL_FUNCTION_PREPROCESSING_STEP";
          "FSCL_MODULE_PREPROCESSING_STEP"; 
          "FSCL_MODULE_PARSING_STEP" |])>]
type FunctionTransformationStep(tm: TypeManager, 
                                processors:FunctionTransformationProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm)

    member val private currentFunction = null with get, set
    member val private currentProcessor = processors.[0] with get, set
    
    member this.FunctionInfo 
        with get() =
            this.currentFunction
        and private set(v) =
            this.currentFunction <- v
        
    member this.Default(expression: Expr) =
        match expression with                 
        | ExprShape.ShapeVar(v) ->
            Expr.Var(v)
        | ExprShape.ShapeLambda(v, e) ->
            let r = this.currentProcessor.Process(e, this)
            Expr.Lambda(v, r)
        | ExprShape.ShapeCombination(o, args) ->
            let filtered = List.map (fun el -> this.currentProcessor.Process(el, this)) args
            // Process the expression
            let newExpr = ExprShape.RebuildShapeCombination(o, filtered)
            newExpr

    member this.Continue(expression: Expr) =
        this.currentProcessor.Process(expression, this)
         
    member private this.Process(f:FunctionInfo) =
        this.FunctionInfo <- f
        for p in processors do
            this.currentProcessor <- p
            this.FunctionInfo.Body <- p.Process(this.FunctionInfo.Body, this) 
                                  
    override this.Run(km: KernelModule) =
        for kernel in km.Kernels do
            this.Process(kernel)
        for f in km.Functions do
            this.Process(f)
        km
        


