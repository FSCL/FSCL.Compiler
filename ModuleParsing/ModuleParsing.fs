namespace FSCL.Compiler.ModuleParsing

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler

[<Step("FSCL_MODULE_PARSING_STEP")>] 
type ModuleParsingStep(tm: TypeManager,
                       processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<obj, KernelModule>(tm, processors)

    let mutable opts = null
         
    member this.TryProcess(expr:obj) =
        let mutable index = 0 
        let mutable output = None
        while (output.IsNone) && (index < processors.Length) do
            output <- processors.[index].Execute(expr, this, opts) :?> KernelModule option
            index <- index + 1
        output
        
    member this.Process(expr:obj) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < processors.Length) do
            output <- processors.[index].Execute(expr, this, opts) :?> KernelModule option
            index <- index + 1
        if output.IsNone then
            raise (CompilerException("The engine is not able to parse a kernel inside the expression [" + expr.ToString() + "]"))
        output.Value

    override this.Run(expr, opt) =
        opts <- opt
        this.Process(expr)
        //kmodule.MergeWith(cg)
        //kmodule.FlowGraph <- cg.FlowGraph

        

