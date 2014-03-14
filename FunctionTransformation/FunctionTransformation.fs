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
    
    let mutable opts = null

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
            let r = this.currentProcessor.Execute(e, this, opts) :?> Expr 
            Expr.Lambda(v, r)
        | ExprShape.ShapeCombination(o, args) ->
            let filtered = List.map (fun el -> this.currentProcessor.Execute(el, this, opts) :?> Expr) args
            // Process the expression
            let newExpr = ExprShape.RebuildShapeCombination(o, filtered)
            newExpr

    member this.Continue(expression: Expr) =
        this.currentProcessor.Execute(expression, this, opts) :?> Expr 
         
    member private this.Process(f:FunctionInfo) =
        this.FunctionInfo <- f
        for p in processors do
            this.currentProcessor <- p
            this.FunctionInfo.Body <- p.Execute(this.FunctionInfo.Body, this, opts) :?> Expr 
                                  
    override this.Run(km: KernelModule, opt) =
        opts <- opt
        for f in km.GetFunctions() do
            if not (f.Info.Skip) then
                this.Process(f.Info)
        this.Process(km.Kernel.Info)
        km
        


