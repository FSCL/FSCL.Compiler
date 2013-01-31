namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type CompilerStepGlobalData = Dictionary<string, obj>

type [<AbstractClass>] ICompilerPipeline(typeManager: TypeManager, kernelModuleType: Type) =
    member val TypeManager = typeManager with get
    member val KernelModuleType = kernelModuleType with get
    
type [<AbstractClass>] ICompilerStep(pipeline: ICompilerPipeline) =
    member val GlobalData = new Dictionary<string, obj>() with get
    member val TypeManager = pipeline.TypeManager with get
    member this.Run(obj) =
        let methods = this.GetType().GetMethods()
        let meth = Array.tryFind(fun (meth: MethodInfo) -> meth.Name = "Run" && meth.IsGenericMethod) methods
        meth.Value.Invoke(this, [| obj |])

[<AbstractClass>]
type CompilerStep<'T,'U>(pipeline) =
    inherit ICompilerStep(pipeline)

    abstract member Run: 'T -> 'U

    
    
    
