namespace FSCL.Compiler.ModuleParsing

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler


[<Step("FSCL_MODULE_PARSING_STEP")>] 
type ModuleParsingStep(tm: TypeManager,
                       processors: ModuleParsingProcessor list) = 
    inherit CompilerStep<obj, KernelModule>(tm)

    member this.Process(expr:obj) =
        let mutable index = 0
        let mutable output = None
        while (output.IsNone) && (index < processors.Length) do
            output <- processors.[index].Process(expr, this)
        if output.IsNone then
            raise (CompilerException("The engine is not able to parse a kernel inside the expression [" + expr.ToString() + "]"))
        output.Value

    override this.Run(expr) =
        this.Process(expr)

        

