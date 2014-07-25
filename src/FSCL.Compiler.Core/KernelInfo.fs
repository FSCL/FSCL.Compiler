namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel

type FunctionInfoID =
    | LambdaID of string
    | MethodID of MethodInfo

///
///<summary>
/// The set of information about utility functions collected and maintained by the compiler
///</summary>
///

[<AllowNullLiteral>]
type IFunctionInfo =
    abstract ID: FunctionInfoID with get
    abstract ParsedSignature: MethodInfo with get

    abstract OriginalParameters: ReadOnlyCollection<IOriginalFunctionParameter> with get
    abstract GeneratedParameters: ReadOnlyCollection<IFunctionParameter> with get
    abstract Parameters: ReadOnlyCollection<IFunctionParameter> with get
    abstract ReturnType: Type with get
    abstract Body: Expr with get
    abstract OriginalBody: Expr with get

    abstract member Code: string with get
    
    abstract IsLambda: bool with get
    abstract CustomInfo: IReadOnlyDictionary<string, obj> with get

    abstract GetParameter: string -> IFunctionParameter option

[<AllowNullLiteral>]
type IKernelInfo =
    inherit IFunctionInfo
    abstract WorkSize: Expr option
    abstract LocalVars: IReadOnlyDictionary<Quotations.Var, Type * (Expr list option)>
    abstract Meta: ReadOnlyMetaCollection with get
    abstract CloneTo: IKernelInfo -> unit
    
[<AllowNullLiteral>]
type FunctionInfo(parsedSignature: MethodInfo, 
                  paramInfos: ParameterInfo list,
                  paramVars: Quotations.Var list,
                  workSize: Expr option,
                  body: Expr, 
                  isLambda: bool) =   

    let parameters =
        paramInfos |> 
        List.mapi(fun i (p:ParameterInfo) ->
                    OriginalFunctionParameter(p, paramVars.[i], None) :> FunctionParameter)

    interface IFunctionInfo with
        member this.ID 
            with get() = 
                this.ID
        member this.ParsedSignature 
            with get() =
                this.ParsedSignature
        member this.OriginalParameters
            with get() =
                let roList = new List<IOriginalFunctionParameter>()
                for item in this.OriginalParameters do
                    roList.Add(item :?> OriginalFunctionParameter)
                roList.AsReadOnly()
        member this.GeneratedParameters 
            with get() =
                let roList = new List<IFunctionParameter>()
                for item in this.GeneratedParameters do
                    roList.Add(item)
                roList.AsReadOnly()
        member this.Parameters 
            with get() =
                let roList = new List<IFunctionParameter>()
                for item in this.OriginalParameters do
                    roList.Add(item)
                for item in this.GeneratedParameters do
                    roList.Add(item)
                roList.AsReadOnly()
        member this.ReturnType
            with get() =
                this.ReturnType
        member this.Body
            with get() =
                this.Body
        member this.OriginalBody
            with get() =
                this.OriginalBody
        member this.Code
            with get() =
                this.Code
        member this.IsLambda 
            with get() =
                this.IsLambda
        member this.CustomInfo
            with get() =
                this.CustomInfo :> IReadOnlyDictionary<string, obj>
        member this.GetParameter(name) =
            match this.GetParameter(name) with
            | Some(p) ->
                Some(p :> IFunctionParameter)
            | _ ->
                None

    // Get-Set properties
    ///
    ///<summary>
    /// The ID of the function
    ///</summary>
    ///
    abstract member ID: FunctionInfoID
    default this.ID 
        with get() =     
            // A lambda kernels/function is identified by its AST    
            if isLambda then
                LambdaID(body.ToString())
            else
                MethodID(parsedSignature)

    member val ParsedSignature = parsedSignature with get
    ///
    ///<summary>
    /// The set of information about original (signature-extracted) function parameters
    ///</summary>
    ///
    abstract member OriginalParameters: FunctionParameter list
    default this.OriginalParameters 
        with get() =
            parameters
    ///
    ///<summary>
    /// The set of information about generated function parameters
    ///</summary>
    ///
    member val GeneratedParameters = new List<FunctionParameter>() with get
    ///
    ///<summary>
    /// The set of information about all the function parameters
    ///</summary>
    ///
    member this.Parameters 
        with get() =
            this.OriginalParameters @ List.ofSeq(this.GeneratedParameters)
    ///
    ///<summary>
    /// The function return type
    ///</summary>
    ///
    member val ReturnType = parsedSignature.ReturnType with get, set
    ///
    ///<summary>
    /// The body of the function
    ///</summary>
    ///
    member val OriginalBody = body with get
    ///
    ///<summary>
    /// The body of the function
    ///</summary>
    ///
    member val Body = body with get, set
    ///
    ///<summary>
    /// The generated target code
    ///</summary>
    ///
    member val Code = "" with get, set      
    ///
    ///<summary>
    /// Whether this function has been generated from a lambda
    ///</summary>
    ///
    member val IsLambda = isLambda with get
    ///
    ///<summary>
    /// A set of custom additional information to be stored in the function
    ///</summary>
    ///<remarks>
    /// This set can be useful to collect and share additional information between custom steps/processors (compiler extensions)
    ///</remarks>
    ///
    member val CustomInfo = new Dictionary<String, Object>() with get
    
    member this.GetParameter(name) =
        let r =  Seq.tryFind(fun (p: FunctionParameter) -> p.Name = name) (this.OriginalParameters)
        match r with
        | Some(p) ->
            Some(p)
        | _ ->
            Seq.tryFind(fun (p: FunctionParameter) -> p.Name = name) (this.GeneratedParameters)
///
///<summary>
/// The set of information about kernels collected and maintained by the compiler
///</summary>
///<remarks>
/// This type inherits from FunctionInfo without exposing any additional property/member. The set
/// of information contained in FunctionInfo is in fact enough expressive to represent a kernel. 
/// From another point of view, a function can be considered a special case of a kernel, where the address-space is fixed, some
/// OpenCL functions cannot be called (e.g. get_global_id) and with some other restrictions.
/// KernelInfo is kept an independent, different class from FunctionInfo with the purpose to trigger different compiler processing on the basis of the
/// actual type.
///</remarks>
///     
[<AllowNullLiteral>]
type KernelInfo(originalSignature: MethodInfo, 
                paramInfos: ParameterInfo list,
                paramVars: Quotations.Var list,
                workSize: Expr option,
                body: Expr, 
                meta: ReadOnlyMetaCollection, 
                isLambda: bool) =
    inherit FunctionInfo(originalSignature, paramInfos, paramVars, workSize, body, isLambda)
    
    let parameters =
        paramInfos |>
        List.mapi(fun i (p:ParameterInfo) ->                        
                        OriginalFunctionParameter(p, paramVars.[i], Some(meta.ParamMeta.[i])) :> FunctionParameter)

    let localVars = new Dictionary<Quotations.Var, Type * (Expr list option)>()

    // Get static kernel and return meta
    (*
    let metaCollection = 
        let staticKernelMeta = new KernelMetaCollection()
        let staticReturnMeta = new ParamMetaCollection()
        let staticParamMeta = new List<ReadOnlyParamMetaCollection>()
        
        for attr in signature.GetCustomAttributes() do
            if typeof<ParameterMetadataAttribute>.IsAssignableFrom(attr.GetType()) then
                staticReturnMeta.Add(attr.GetType(), attr :?> ParameterMetadataAttribute)
            else if typeof<KernelMetadataAttribute>.IsAssignableFrom(attr.GetType()) then
                staticKernelMeta.Add(attr.GetType(), attr :?> KernelMetadataAttribute)
        for p in signature.GetParameters() do
            let coll = new ParamMetaCollection()
            for attr in p.GetCustomAttributes() do
                if typeof<ParameterMetadataAttribute>.IsAssignableFrom(attr.GetType()) then
                    coll.Add(attr.GetType(), attr :?> ParameterMetadataAttribute)
            staticParamMeta.Add(coll)
                
        // Merge with dynamic meta
        for item in meta.KernelMeta do
            if staticKernelMeta.ContainsKey(item.Key) then
                staticKernelMeta.[item.Key] <- item.Value
            else
                staticKernelMeta.Add(item.Key, item.Value)
        for item in meta.ReturnMeta do
            if staticReturnMeta.ContainsKey(item.Key) then
                staticReturnMeta.[item.Key] <- item.Value
            else
                staticReturnMeta.Add(item.Key, item.Value)
        for i = 0 to  signature.GetParameters().Length - 1 do
            for item in meta.ParamMeta.[i] do
                if staticParamMeta.[i].ContainsKey(item.Key) then
                    (staticParamMeta.[i] :?> ParamMetaCollection).[item.Key] <- item.Value
                else
                    (staticParamMeta.[i] :?> ParamMetaCollection).Add(item.Key, item.Value)

        new ReadOnlyMetaCollection(staticKernelMeta, staticReturnMeta, List.ofSeq staticParamMeta)
     *)
    override this.OriginalParameters
        with get() =
            parameters
    ///
    ///<summary>
    /// The dynamic metadata of the kernel
    ///</summary>
    ///
    member val Meta = meta 
        with get

    member this.LocalVars
        with get() =
            localVars
    
    member this.WorkSize 
        with get() =
            workSize

    member this.IsLocalVar(v: Quotations.Var) =
        localVars.ContainsKey(v)

    interface IKernelInfo with
        member this.Meta 
            with get() =
                this.Meta
                
        member this.LocalVars 
            with get() =
                this.LocalVars :> IReadOnlyDictionary<Quotations.Var, Type * (Expr list option)> 
            
        member this.WorkSize
            with get() =
                this.WorkSize

        member this.CloneTo(ikInfo: IKernelInfo) =
            let kInfo = ikInfo :?> KernelInfo
            // Copy kernel info fields
            kInfo.Body <- this.Body
            kInfo.Code <- this.Code
            for item in this.CustomInfo do
                if not (kInfo.CustomInfo.ContainsKey(item.Key)) then
                    kInfo.CustomInfo.Add(item.Key, item.Value)
            for item in this.GeneratedParameters do
                if item.IsReturned && item.IsDynamicParameter then
                    // Must associate new Return Meta
                    let oldParameter = item
                    let newParameter = new FunctionParameter(item.Name, item.OriginalPlaceholder, item.ParameterType, Some(ikInfo.Meta.ReturnMeta :> IParamMetaCollection)) 
                    newParameter.AccessAnalysis <- oldParameter.AccessAnalysis
                    newParameter.IsReturned <- oldParameter.IsReturned
                    newParameter.ReturnExpr <- oldParameter.ReturnExpr
                    newParameter.Placeholder <- oldParameter.Placeholder
                    for i = 0 to oldParameter.SizeParameters.Count - 1 do
                        newParameter.SizeParameters.Add(oldParameter.SizeParameters.[i])
                    kInfo.GeneratedParameters.Add(newParameter)
                else                    
                    kInfo.GeneratedParameters.Add(item)
            for i = 0 to this.OriginalParameters.Length - 1 do
                let oldParameter = this.OriginalParameters.[i]
                let newParameter = kInfo.OriginalParameters.[i]
                newParameter.AccessAnalysis <- oldParameter.AccessAnalysis
                newParameter.IsReturned <- oldParameter.IsReturned
                newParameter.ReturnExpr <- oldParameter.ReturnExpr
                newParameter.Placeholder <- oldParameter.Placeholder
                for i = 0 to oldParameter.SizeParameters.Count - 1 do
                    newParameter.SizeParameters.Add(oldParameter.SizeParameters.[i])
            kInfo.ReturnType <- this.ReturnType            

