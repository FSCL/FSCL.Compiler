namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_FUNCTION_TRANSFORMATION_STEP",
       Dependencies = [| "FSCL_FUNCTION_PREPROCESSING_STEP";
                         "FSCL_MODULE_PREPROCESSING_STEP"; 
                         "FSCL_MODULE_PARSING_STEP" |])>]
type FunctionTransformationStep(tm: TypeManager, 
                                processors:ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelExpression, KernelExpression>(tm, processors)
    
    member this.Continue (proc: ICompilerStepProcessor) (opts:Map<string, obj>) (expression: Expr) =
        proc.Execute((expression, 
                      (this.Continue proc opts),
                      (this.Default proc opts)), this, opts) :?> Expr

    member val private currentFunction = null with get, set

    member this.FunctionInfo 
        with get() =
            this.currentFunction
        and private set(v) =
            this.currentFunction <- v
        
    member this.Default (proc: ICompilerStepProcessor) (opts:Map<string, obj>) (expression: Expr) =
        match expression with                 
        | ExprShape.ShapeVar(v) ->
            Expr.Var(v)
        | ExprShape.ShapeLambda(v, e) ->
            let r = proc.Execute((e, 
                                  (this.Continue proc opts),
                                  (this.Default proc opts)), this, opts) :?> Expr 
            Expr.Lambda(v, r)
        | ExprShape.ShapeCombination(o, args) ->
            let filtered = List.map (fun el -> proc.Execute((el, 
                                                             (this.Continue proc opts),
                                                             (this.Default proc opts)), this, opts) :?> Expr) args
            // Process the expression
            let newExpr = ExprShape.RebuildShapeCombination(o, filtered)
            newExpr
 
         
    member private this.Process(f:FunctionInfo, opts) =
        this.FunctionInfo <- f
        for p in processors do
            this.FunctionInfo.Body <- p.Execute((this.FunctionInfo.Body, 
                                                 (this.Continue p opts),
                                                 (this.Default p opts)), this, opts) :?> Expr 
                                  
    override this.Run(cem, opts) =
        for km in cem.KernelModulesRequiringCompilation do
            for f in km.Functions do
                this.Process(f.Value :?> FunctionInfo, opts)
            this.Process(km.Kernel, opts)
        ContinueCompilation(cem)
        


