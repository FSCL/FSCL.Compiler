namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type CompilerStepGlobalData = Dictionary<string, obj>
    
type [<AbstractClass>] ICompilerStep(tm: TypeManager) =
    member val GlobalData = new Dictionary<string, obj>() with get
    member val TypeManager = tm with get
    member this.Execute(obj) =
        let methods = this.GetType().GetMethods()
        let meth = Array.tryFind(fun (meth: MethodInfo) -> meth.Name = "Run") methods
        meth.Value.Invoke(this, [| obj |])

[<AbstractClass>]
type CompilerStep<'T,'U>(tm) =
    inherit ICompilerStep(tm)

    abstract member Run: 'T -> 'U

    
    
    
