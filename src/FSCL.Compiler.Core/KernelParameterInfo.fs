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
type AccessAnalysisResult =
| NoAccess = 0
| ReadAccess = 1
| WriteAccess = 2

type FunctionParameterType =
| NormalParameter
| SizeParameter
| DynamicArrayParameter of Expr array
//| AutoArrayParameter
| OutValParameter of Expr
| EnvVarParameter of Var
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
    abstract AccessAnalysis: AccessAnalysisResult with get
    abstract ReturnExpr: Expr option with get
    abstract IsReturned: bool with get
    abstract IsAutoArray: bool with get
    abstract SizeParameters: ReadOnlyCollection<IFunctionParameter> with get
    abstract Meta: IParamMetaCollection with get
            
    abstract IsSizeParameter: bool with get
    abstract IsNormalParameter: bool with get
    abstract IsDynamicArrayParameter: bool with get
    abstract IsOutValParameter: bool with get
    abstract IsEnvVarParameter: bool with get
    abstract IsImplicitParameter: bool with get

    // These are for utility functions to which kernels/other functions pass global/constant/local parameters
    abstract ForcedGlobalAddressSpace: bool with get
    abstract ForcedLocalAddressSpace: bool with get
    abstract ForcedConstantAddressSpace: bool with get
    abstract ForcedPrivateAddressSpace: bool with get

type FunctionParameter(name:string, 
                       originalPlaceholder: Var, 
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

        member this.AccessAnalysis
            with get() =
                this.AccessAnalysis

        member this.ReturnExpr 
            with get() =
                this.ReturnExpr

        member this.IsReturned 
            with get() =
                this.IsReturned

        member this.IsAutoArray 
            with get() =
                this.IsAutoArray

        member this.SizeParameters 
            with get() =
                sp.AsReadOnly()
            
        member this.Meta 
            with get() =
                this.Meta

        member this.ForcedGlobalAddressSpace
            with get() =
                this.ForcedGlobalAddressSpace
                
        member this.ForcedLocalAddressSpace
            with get() =
                this.ForcedLocalAddressSpace
                
        member this.ForcedConstantAddressSpace
            with get() =
                this.ForcedConstantAddressSpace
                
        member this.ForcedPrivateAddressSpace
            with get() =
                this.ForcedPrivateAddressSpace

        member this.IsSizeParameter
            with get() = 
                this.IsSizeParameter 
                       
        member this.IsNormalParameter 
            with get() = 
                this.IsNormalParameter     

        member this.IsDynamicArrayParameter 
            with get() = 
                this.IsDynamicArrayParameter
                 
        member this.IsImplicitParameter 
            with get() = 
                this.IsImplicitParameter

        member this.IsOutValParameter 
            with get() = 
                this.IsOutValParameter

        member this.IsEnvVarParameter 
            with get() = 
                this.IsEnvVarParameter

//        member this.IsAutoArrayParameter 
//            with get() = 
//                this.IsAutoArrayParameter

//        member this.DynamicAllocationArguments
//            with get() =
//                this.DynamicAllocationArguments
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

    member val AccessAnalysis = AccessAnalysisResult.NoAccess
        with get, set
            
    member val ReturnExpr = None
        with get, set

    member val IsReturned = false 
        with get, set
            
    member val IsAutoArray = false 
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
           
    member val IsDynamicArrayParameter =
        match parameterType with
        | DynamicArrayParameter(_) ->
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
        
    member val IsEnvVarParameter =
        match parameterType with
        | EnvVarParameter(_) ->
            true
        | _ ->
            false  
        with get
        
    member val IsOutValParameter =
        match parameterType with
        | OutValParameter(_) ->
            true
        | _ ->
            false  
        with get
        
    member val ForcedGlobalAddressSpace = false 
        with get, set    
    member val ForcedLocalAddressSpace = false 
        with get, set    
    member val ForcedConstantAddressSpace = false 
        with get, set
    member val ForcedPrivateAddressSpace = false 
        with get, set

    member this.CloneTo(f:FunctionParameter) =
        f.AccessAnalysis <- this.AccessAnalysis
        f.IsReturned <- this.IsReturned
        f.Placeholder <- this.Placeholder
        f.ReturnExpr <- this.ReturnExpr
        f.ForcedGlobalAddressSpace <- this.ForcedGlobalAddressSpace
        f.ForcedConstantAddressSpace <- this.ForcedConstantAddressSpace
        f.ForcedLocalAddressSpace <- this.ForcedLocalAddressSpace
        f.SizeParameters.Clear()
        for item in this.SizeParameters do
            let p = new FunctionParameter(item.Name, item.Placeholder, item.ParameterType, None)
            (item :?> FunctionParameter).CloneTo(p)
            f.SizeParameters.Add(p)
        
//    member val IsAutoArrayParameter =
//        match parameterType with
//        | AutoArrayParameter ->
//            true
//        | _ ->
//            false  
//        with get
//              
//    member val DynamicAllocationArguments =
//        match parameterType with
//        | DynamicArrayParameter(allocArgs) ->
//            Some(allocArgs)
//        | _ ->
//            None
//        with get
    
                    

type IOriginalFunctionParameter =
    inherit IFunctionParameter
    abstract OriginalParamterInfo: ParameterInfo with get

type OriginalFunctionParameter(p: ParameterInfo, placeholder: Quotations.Var, meta: IParamMetaCollection option) =
    inherit FunctionParameter(p.Name, placeholder, NormalParameter, meta)
    interface IOriginalFunctionParameter with
        member this.OriginalParamterInfo 
            with get() =
                p


     