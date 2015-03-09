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
    abstract GlobalTypes: IReadOnlyList<Type> with get
    abstract Directives: IReadOnlyList<String> with get
    abstract DynamicConstantDefines: IReadOnlyDictionary<String, Var option * Expr option * obj> with get

//    abstract CallArgs: Expr list with get
    abstract Code: string option with get
    abstract CustomInfo: IReadOnlyDictionary<string, obj> with get

[<AllowNullLiteral>]
type KernelModule(k: KernelInfo, 
                  f: IReadOnlyDictionary<FunctionInfoID, IFunctionInfo>,
                  constantDefines: IReadOnlyDictionary<String, Var option * Expr option * obj>) =  
    interface IKernelModule with
        member this.Kernel
            with get() =
                this.Kernel :> IKernelInfo
        member this.Functions
            with get() =
                this.Functions 
        member this.GlobalTypes
            with get() =
                this.GlobalTypes :> IReadOnlyList<Type>
        member this.Directives
            with get() =
                this.Directives :> IReadOnlyList<String>
        member this.Code
            with get() =
                this.Code
        member this.CustomInfo
            with get() =
                this.CustomInfo :> IReadOnlyDictionary<string, obj>
        member this.DynamicConstantDefines 
            with get() =
                this.ConstantDefines //:> IReadOnlyDictionary<String, Var option * Expr option * obj>      
     //   member this.DynamicConstantDefinesEvaluator
       //     with get() = 
         //       this.DynamicConstantDefinesEvaluator

    // Get-Set properties
    member val Kernel = k with get
    member val Functions = f with get
    member val GlobalTypes = new List<Type>() with get
    member val Directives = new List<String>() with get
    member val ConstantDefines = constantDefines
    
    member val Code:string option = None with get, set
    member val CustomInfo = new Dictionary<String, Object>() with get

    member this.CloneTo(m:IKernelModule) = 
        let km = m :?> KernelModule
        (this.Kernel :> IKernelInfo).CloneTo(km.Kernel)
        km.GlobalTypes.Clear()
        for item in this.GlobalTypes do
            km.GlobalTypes.Add(item) |> ignore
        km.Directives.Clear()
        for item in this.Directives do
            km.Directives.Add(item) |> ignore
        km.Code <- this.Code
        km.CustomInfo.Clear()
        for item in this.CustomInfo do
            km.CustomInfo.Add(item.Key, item.Value)
            
    
