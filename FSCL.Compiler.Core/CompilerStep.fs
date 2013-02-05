namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type [<AbstractClass>] ICompilerStep(tm: TypeManager) =
    member val TypeManager = tm with get
    member this.Execute(obj) =
        let methods = this.GetType().GetMethods()
        let meth = Array.tryFind(fun (meth: MethodInfo) -> meth.Name = "Run") methods
        meth.Value.Invoke(this, [| obj |])
        
[<AbstractClass>]
type CompilerStep<'T,'U>(tm) =
    inherit ICompilerStep(tm)

    abstract member Run: 'T -> 'U
    
type ICompilerStepProcessor =
    interface
    end

type CompilerStepProcessor<'T,'U> =
    inherit ICompilerStepProcessor

    abstract member Process: 'T * ICompilerStep -> 'U
        
type CompilerStepProcessor<'T> =
    inherit ICompilerStepProcessor

    abstract member Process: 'T * ICompilerStep -> unit

// Alias for processors of deafult steps
type NoResult = unit
type ModuleParsingProcessor = CompilerStepProcessor<obj, KernelModule option>
type ModulePreprocessingProcessor = CompilerStepProcessor<KernelModule>
type FunctionPreprocessingProcessor = CompilerStepProcessor<FunctionInfo>
type FunctionTransformationProcessor = CompilerStepProcessor<Expr, Expr>
type FunctionSignaturePrettyPrintingProcessor = CompilerStepProcessor<MethodInfo, String option>
type FunctionBodyPrettyPrintingProcessor = CompilerStepProcessor<Expr, String option>
type ModulePrettyPrintingProcessor = CompilerStepProcessor<KernelModule * String, String>
    
    
    
