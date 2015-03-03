namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel

type IKernelCacheEntry = 
    abstract member KernelInfo: IKernelInfo with get
    abstract member ModuleCode: string with get
    abstract member ConstantDefines: IReadOnlyDictionary<String, Var option * Expr option * obj>

type IKernelCache =
    abstract member TryGet: FunctionInfoID * ReadOnlyMetaCollection -> IKernelCacheEntry option
    abstract member Store: IKernelCacheEntry -> unit
     
type NullCache() =
    interface IKernelCache with
        member this.TryGet(_, _) =
            None
        member this.Store(_) =
            ()