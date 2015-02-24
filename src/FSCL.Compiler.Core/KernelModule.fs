namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel
   
type ConstantDefineValue =
| StaticValue of Expr
| DynamicValue of (Expr -> string)

///
///<summary>
/// The main information built and returned by the compiler, represeting the result of the compilation process together with a set of meta-information generated during the compilation itself
///</summary>
///
[<AllowNullLiteral>]
type IKernelModule =
    abstract Kernel: IKernelInfo with get
    abstract Functions: IReadOnlyDictionary<FunctionInfoID, IFunctionInfo> with get
    abstract GlobalTypes: Type list with get
    abstract Directives: String list with get
    abstract DynamicConstantDefines: IReadOnlyDictionary<String, Var option * Expr option * obj> with get

    abstract CallArgs: Expr list with get
    abstract Code: string option with get
    abstract CustomInfo: IReadOnlyDictionary<string, obj> with get

[<AllowNullLiteral>]
type KernelModule(k: KernelInfo, callArgs: Expr list) =  
    interface IKernelModule with
        member this.Kernel
            with get() =
                this.Kernel :> IKernelInfo
        member this.Functions
            with get() =
                this.Functions :> IReadOnlyDictionary<FunctionInfoID, IFunctionInfo>
        member this.GlobalTypes
            with get() =
                this.GlobalTypes |> List.ofSeq
        member this.Directives
            with get() =
                this.Directives |> List.ofSeq
        member this.CallArgs
            with get() =
                this.CallArgs
        member this.Code
            with get() =
                this.Code
        member this.CustomInfo
            with get() =
                this.CustomInfo :> IReadOnlyDictionary<string, obj>
        member this.DynamicConstantDefines 
            with get() =
                this.DynamicConstantDefines :> IReadOnlyDictionary<String, Var option * Expr option * obj>      
     //   member this.DynamicConstantDefinesEvaluator
       //     with get() = 
         //       this.DynamicConstantDefinesEvaluator

    // Get-Set properties
    member val Kernel = k with get
    member val Functions = new Dictionary<FunctionInfoID, IFunctionInfo>() with get
    member val GlobalTypes = new HashSet<Type>() with get
    member val Directives = new HashSet<String>() with get
    member val DynamicConstantDefines = new Dictionary<string, Var option * Expr option * obj>() with get
    
    member val CallArgs = callArgs with get
    member val Code:string option = None with get, set
    member val CustomInfo = new Dictionary<String, Object>() with get
    