namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel

///
///<summary>
/// Enumeration describing the access mode to a kernel parameter (R, W, RW or not used)
///</summary>
///
[<Flags>]
type AccessMode =
| NoAccess = 0
| ReadAccess = 1
| WriteAccess = 2

type FunctionParameterType =
| NormalParameter
| SizeParameter
| DynamicParameter of Expr array
| ImplicitParameter

///
///<summary>
/// The set of information about a kernel parameter collected and maintained by the compiler
///</summary>
///
type IFunctionParameter =
    abstract Name: string with get
    abstract DataType: Type with get
    abstract ParameterType: FunctionParameterType with get
    abstract OriginalPlaceholder: Quotations.Var with get
    abstract Placeholder: Quotations.Var with get
    abstract Access: AccessMode with get
    abstract ReturnExpr: Expr option with get
    abstract IsReturned: bool with get
    abstract SizeParameters: ReadOnlyCollection<IFunctionParameter> with get
    abstract Meta: IParamMetaCollection with get
            
    abstract IsSizeParameter: bool with get
    abstract IsNormalParameter: bool with get
    abstract IsDynamicParameter: bool with get
    abstract IsImplicitParameter: bool with get
    abstract DynamicAllocationArguments: Expr array option with get

type FunctionParameter(name:string, 
                       originalPlaceholder: Quotations.Var, 
                       parameterType: FunctionParameterType,
                       meta: IParamMetaCollection option) =
    let sp = new List<IFunctionParameter>()

    interface IFunctionParameter with

        // Override starts
        member this.Name
            with get() = 
                this.Name
        
        member this.DataType 
            with get() =
                this.Placeholder.Type
            
        member this.ParameterType 
            with get() =
                this.ParameterType           
            
        member this.Placeholder 
            with get() =
                this.Placeholder
                
        member this.OriginalPlaceholder 
            with get() =
                this.OriginalPlaceholder

        member this.Access
            with get() =
                this.Access

        member this.ReturnExpr 
            with get() =
                this.ReturnExpr

        member this.IsReturned 
            with get() =
                this.IsReturned

        member this.SizeParameters 
            with get() =
                sp.AsReadOnly()
            
        member this.Meta 
            with get() =
                this.Meta

        member this.IsSizeParameter
            with get() = 
                this.IsSizeParameter 
                       
        member this.IsNormalParameter 
            with get() = 
                this.IsNormalParameter     

        member this.IsDynamicParameter 
            with get() = 
                this.IsDynamicParameter
                 
        member this.IsImplicitParameter 
            with get() = 
                this.IsImplicitParameter

        member this.DynamicAllocationArguments
            with get() =
                this.DynamicAllocationArguments
    // Override ends

    // Get-set properties
    member val Name = name
        with get
       
    member val ParameterType = parameterType
        with get

    member val Placeholder = originalPlaceholder 
        with get, set
            
    member val OriginalPlaceholder = originalPlaceholder
        with get
            
    member this.DataType
        with get() =
            this.Placeholder.Type
            
    member this.SizeParameters
        with get() =
            sp

    member val Access = AccessMode.NoAccess
        with get, set
            
    member val ReturnExpr = None
        with get, set

    member val IsReturned = false 
        with get, set
            
    member val Meta =
        if meta.IsNone then
            new ParamMetaCollection() :> IParamMetaCollection
        else
            meta.Value
        with get
        
    member val IsSizeParameter =
        match parameterType with
        | SizeParameter ->
            true
        | _ ->
            false           
        with get 

    member val IsNormalParameter =
        match parameterType with
        | NormalParameter ->
            true
        | _ ->
            false     
        with get
           
    member val IsDynamicParameter =
        match parameterType with
        | DynamicParameter(_) ->
            true
        | _ ->
            false     
        with get
         
    member val IsImplicitParameter =
        match parameterType with
        | ImplicitParameter ->
            true
        | _ ->
            false  
        with get
              
    member val DynamicAllocationArguments =
        match parameterType with
        | DynamicParameter(allocArgs) ->
            Some(allocArgs)
        | _ ->
            None
        with get
                    
        
type OriginalFunctionParameter(p: ParameterInfo, placeholder: Quotations.Var, meta: IParamMetaCollection option) =
    inherit FunctionParameter(p.Name, placeholder, NormalParameter, meta)


     