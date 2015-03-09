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
    abstract InstanceVar: Var option with get
    abstract InstanceExpr: Expr option with get

    abstract OriginalParameters: IReadOnlyList<IOriginalFunctionParameter> with get
    abstract GeneratedParameters: IReadOnlyList<IFunctionParameter> with get
    abstract Parameters: IReadOnlyList<IFunctionParameter> with get
    //abstract EnvVars: Var list with get
    abstract EnvVarsUsed: IReadOnlyList<Var> with get
    abstract OutValsUsed: IReadOnlyList<Expr> with get
    abstract ReturnType: Type with get
    abstract Body: Expr with get
    abstract OriginalBody: Expr with get
    
    abstract CalledFunctions: IReadOnlyList<FunctionInfoID> with get
    
    abstract member Code: string with get
    abstract member SignatureCode: string with get
    
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
type FunctionInfo(objectInstanceVar: Var option,
                  objectInstance: Expr option,
                  parsedSignature: MethodInfo, 
                  paramInfos: ParameterInfo list,
                  paramVars: Quotations.Var list,
                  envVarsUsed: IReadOnlyList<Var>,
                  outValsUsed: IReadOnlyList<Expr>,
                  workSize: Expr option,
                  body: Expr, 
                  calledFunctions: IReadOnlyList<FunctionInfoID>,
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
        member this.InstanceVar 
            with get() =
                this.InstanceVar
        member this.InstanceExpr
            with get() =
                this.InstanceExpr
        member this.OriginalParameters
            with get() =
                let roList = new List<IOriginalFunctionParameter>()
                for item in this.OriginalParameters do
                    roList.Add(item :?> OriginalFunctionParameter)
                roList :> IReadOnlyList<IOriginalFunctionParameter>
        member this.GeneratedParameters 
            with get() =
                let roList = new List<IFunctionParameter>()
                for item in this.GeneratedParameters do
                    roList.Add(item)
                roList :> IReadOnlyList<IFunctionParameter>
        member this.Parameters 
            with get() =
                let roList = new List<IFunctionParameter>()
                for item in this.OriginalParameters do
                    roList.Add(item)
                for item in this.GeneratedParameters do
                    roList.Add(item)
                roList :> IReadOnlyList<IFunctionParameter>
//        member this.EnvVars
//            with get() =
//                this.EnvVars
        member this.EnvVarsUsed
            with get() =
                this.EnvVarsUsed
        member this.OutValsUsed
            with get() =
                this.OutValsUsed
        member this.CalledFunctions 
            with get() =
                this.CalledFunctions 
        member this.ReturnType
            with get() =
                this.ReturnType
        member this.SignatureCode
            with get() =
                this.SignatureCode
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
    member val GeneratedParameters = 
        new List<FunctionParameter>() 
        with get
    
    member val CalledFunctions = calledFunctions
        with get

    ///
    ///<summary>
    /// The set of information about all the function parameters
    ///</summary>
    ///
    member this.Parameters 
        with get() =
            this.OriginalParameters @ List.ofSeq(this.GeneratedParameters)

//    member this.EnvVars 
//        with get() =
//            envVars
            
    member this.EnvVarsUsed 
        with get() =
            envVarsUsed
            
    member this.OutValsUsed 
        with get() =
            outValsUsed
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

    member val InstanceVar = objectInstanceVar with get
    member val InstanceExpr = objectInstance with get
    ///
    ///<summary>
    /// The generated target code for the signature
    ///</summary>
    ///
    member val SignatureCode = "" with get, set   
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
            
//    member this.CloneTo(f:FunctionInfo) =
//        // Copy kernel info fields
//        f.Body <- this.Body
//        f.SignatureCode <- this.SignatureCode
//        f.Code <- this.Code
//        f.ReturnType <- this.ReturnType
//
//        for item in this.CustomInfo do
//            if not (f.CustomInfo.ContainsKey(item.Key)) then
//                f.CustomInfo.Add(item.Key, item.Value)
//        for item in this.CalledFunctions do
//            f.CalledFunctions.Add(item)
//            
//        for i = 0 to this.Parameters.Length - 1 do
//            this.Parameters.[i].CloneTo(f.Parameters.[i])



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
type KernelInfo(objectInstanceVar: Var option,
                objectInstance: Expr option,
                originalSignature: MethodInfo, 
                paramInfos: ParameterInfo list,
                paramVars: Quotations.Var list,
                envVarsUsed: IReadOnlyList<Var>,
                outValsUsed: IReadOnlyList<Expr>,
                workSize: Expr option,
                body: Expr, 
                calledFunctions: IReadOnlyList<FunctionInfoID>,
                meta: ReadOnlyMetaCollection, 
                isLambda: bool) =
    inherit FunctionInfo(objectInstanceVar, objectInstance, originalSignature, paramInfos, paramVars, envVarsUsed, outValsUsed, workSize, body, calledFunctions, isLambda)
    
    let parameters =
        paramInfos |>
        List.mapi(fun i (p:ParameterInfo) ->                        
                        OriginalFunctionParameter(p, paramVars.[i], Some(meta.ParamMeta.[i])) :> FunctionParameter)

    let localVars = new Dictionary<Quotations.Var, Type * (Expr list option)>()

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
                
        member this.CloneTo(k:IKernelInfo) =
            let f = k :?> KernelInfo
            // Copy kernel info fields
            f.Body <- this.Body
            f.SignatureCode <- this.SignatureCode
            f.Code <- this.Code
            f.ReturnType <- this.ReturnType

            for item in this.CustomInfo do
                if not (f.CustomInfo.ContainsKey(item.Key)) then
                    f.CustomInfo.Add(item.Key, item.Value)
//            for item in this.CalledFunctions do
//                f.CalledFunctions.Add(item)
            
            for i = 0 to this.OriginalParameters.Length - 1 do
                this.Parameters.[i].CloneTo(f.Parameters.[i])
            
            for i = 0 to this.GeneratedParameters.Count - 1 do
                let oldParameter = this.GeneratedParameters.[i]
                if oldParameter.IsReturned && oldParameter.IsDynamicArrayParameter then
                    // Must associate new Return Meta
                    let newParameter = new FunctionParameter(oldParameter.Name, oldParameter.OriginalPlaceholder, oldParameter.ParameterType, Some(f.Meta.ReturnMeta :> IParamMetaCollection)) 
                    oldParameter.CloneTo(newParameter)
                    f.GeneratedParameters.Add(newParameter)
                else                    
                    let newParameter = new FunctionParameter(oldParameter.Name, oldParameter.OriginalPlaceholder, oldParameter.ParameterType, None) 
                    oldParameter.CloneTo(newParameter)
                    f.GeneratedParameters.Add(newParameter)
               
