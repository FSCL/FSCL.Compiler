namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel
   
///
///<summary>
/// The main information built and returned by the compiler, represeting the result of the compilation process together with a set of meta-information generated during the compilation itself
///</summary>
///
[<AllowNullLiteral>]
type IKernelModule =
    abstract InstanceVar: Var option with get
    abstract InstanceExpr: Expr option with get
    abstract Kernel: IKernelInfo with get
    abstract Functions: IReadOnlyDictionary<FunctionInfoID, IFunctionInfo> with get
    abstract GlobalTypes: IReadOnlyList<Type> with get
    abstract Directives: IReadOnlyList<String> with get
    abstract ConstantDefines: IReadOnlyDictionary<String, Var option * Expr option * obj> with get
// TODO: just a nice comment
//    abstract CallArgs: Expr list with get
    abstract Code: string option with get
    abstract CustomInfo: IReadOnlyDictionary<string, obj> with get

[<AllowNullLiteral>]
type KernelModule(objectInstanceVar: Var option,
                  objectInstance: Expr option,
                  k: KernelInfo) =  
    interface IKernelModule with
        member this.InstanceVar 
            with get() =
                this.InstanceVar
        member this.InstanceExpr  
            with get() =
                this.InstanceExpr
        member this.Kernel
            with get() =
                this.Kernel :> IKernelInfo
        member this.Functions
            with get() =
                this.Functions :> IReadOnlyDictionary<FunctionInfoID, IFunctionInfo>
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
        member this.ConstantDefines 
            with get() =
                this.ConstantDefines :> IReadOnlyDictionary<String, Var option * Expr option * obj>

    // Get-Set properties
    member val InstanceVar = objectInstanceVar with get
    member val InstanceExpr = objectInstance with get
    member val Kernel = k with get
    member val Functions = new Dictionary<FunctionInfoID, IFunctionInfo>()
        with get
    member val GlobalTypes = new List<Type>() with get
    member val Directives = new List<String>() with get
    member val ConstantDefines = new Dictionary<String, Var option * Expr option * obj>()
        with get
    
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
        km.ConstantDefines.Clear()
        for item in this.ConstantDefines do
            km.ConstantDefines.Add(item.Key, item.Value)
        km.Functions.Clear()
        for item in this.Functions do
            km.Functions.Add(item.Key, item.Value)
            
    
