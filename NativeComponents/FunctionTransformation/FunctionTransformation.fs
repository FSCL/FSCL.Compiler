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
    inherit CompilerStep<KernelModule, KernelModule>(tm, processors)

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
            let r = this.currentProcessor.Execute(e, this) :?> Expr 
            Expr.Lambda(v, r)
        | ExprShape.ShapeCombination(o, args) ->
            let filtered = List.map (fun el -> this.currentProcessor.Execute(el, this) :?> Expr) args
            // Process the expression
            let newExpr = ExprShape.RebuildShapeCombination(o, filtered)
            newExpr

    member this.Continue(expression: Expr) =
        this.currentProcessor.Execute(expression, this) :?> Expr 
         
    member private this.Process(f:FunctionInfo) =
        this.FunctionInfo <- f
        for p in processors do
            this.currentProcessor <- p
            this.FunctionInfo.Body <- p.Execute(this.FunctionInfo.Body, this) :?> Expr 
                                  
    override this.Run(km: KernelModule) =
        for k in km.CallGraph.KernelIDs do
            this.Process(km.CallGraph.GetKernel(k))
        for f in km.CallGraph.FunctionIDs do
            this.Process(km.CallGraph.GetFunction(f))
        km
        


