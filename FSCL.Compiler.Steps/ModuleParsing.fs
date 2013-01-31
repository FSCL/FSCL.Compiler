namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type ModuleParsingProcessor =
    abstract member Handle : obj * ModuleParsingStep -> KernelModule option

and ModuleParsingStep(pipeline: ICompilerPipeline,
                      processors: ModuleParsingProcessor list) = 
    inherit CompilerStep<obj, KernelModule>(pipeline)
               
    member this.NewKernelModule() = 
        pipeline.KernelModuleType.GetConstructor([||]).Invoke([||]) :?> KernelModule

    member this.Process(expr:obj) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < processors.Length) do
            output <- processors.[index].Handle(expr, this)
        if output.IsNone then
            raise (CompilerException("The engine is not able to parse a kernel inside the expression [" + expr.ToString() + "]"))
        output.Value

    override this.Run(expr) =
        this.Process(expr)

        

