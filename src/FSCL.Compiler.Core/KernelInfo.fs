namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.ExprShape

module QuotationComparison =        
    let rec AreStructuralEquivalent(e1: Expr, e2: Expr, map:Map<Var,Var>) =
        match e1, e2 with
        | Let(v1, val1, body1), Patterns.Let(v2, val2, body2) ->
            v1.Type = v2.Type && 
            AreStructuralEquivalent(val1, val2, map) &&
            AreStructuralEquivalent(body1, body2, map.Add(v1, v2)) 
        | ForIntegerRangeLoop(v1, st1, en1, body1), Patterns.ForIntegerRangeLoop(v2, st2, en2, body2) ->
            v1.Type = v2.Type && 
            AreStructuralEquivalent(st1, st2, map) &&
            AreStructuralEquivalent(en1, en2, map) &&
            AreStructuralEquivalent(body1, body2, map.Add(v1, v2)) 
        | ShapeLambda(v1, body1), ExprShape.ShapeLambda(v2, body2) ->
            v1.Type = v2.Type && 
            AreStructuralEquivalent(body1, body2, map.Add(v1, v2)) 
        | ShapeVar(v1), ExprShape.ShapeVar(v2) ->
            map.[v1] = v2
        | ShapeCombination(o1, args1), ExprShape.ShapeCombination(o2, args2) ->
            o1 = o2 && 
            args1.Length = args2.Length &&
            ((args1, args2) ||> List.zip |> List.tryFind(fun (a,b) -> not (AreStructuralEquivalent(a,b,map)))).IsSome
        | _, _ ->
            false
    
    let hashList f seed = List.fold (fun h v -> h * 37 + f v) seed
 
    let (<+) x y = x * 37 + y
    let (!<) f x y = x <+ f y
 
    let rec hashC funcs =
        let hashc e = hashC funcs e
        function
        | Lambda(v, body) -> hashV v <+ hashc body <+ 11
        | Call(target, m, args) ->
            match Expr.TryGetReflectedDefinition m, Set.contains m.Name funcs with
            | Some f, false -> 
                let hashc e = hashC (Set.add m.Name funcs) e
                args |> hashList hashc (hashOpt hashc target <+ hashc f) <+ 13
            | _ -> args |> hashList hashc (hashOpt hashc target <+ hash m.Name) <+ 17
        | Var v -> hashV v <+ 19
        | IfThenElse(cond,t,f) -> hashc cond <+ hashc t <+ hashc f <+ 23
        | UnionCaseTest(e, caseInfo) -> hashc e <+ hash caseInfo.Name <+ 29
        | Let(v, e, body) -> hashV v <+ hashc e <+ hashc body <+ 31
        | PropertyGet(target, prop, args) -> args |> hashList hashc (hashOpt hashc target <+ hashP prop) <+ 37
        | TupleGet(e, i) -> hashc e <+ hash i <+ 41
        | AddressOf e ->  hashc e <+ 43
        | AddressSet(e1, e2) -> hashc e1 <+ hashc e2 <+ 47
        | Application(e1, e2) -> hashc e1 <+ hashc e2 <+ 53
        | Coerce(e,t) -> hashc e <+ hashT t <+ 59
        | DefaultValue(t) -> hashT t <+ 61
        | FieldGet(target, field) -> hashOpt hashc target <+ hashF field <+ 67
        | FieldSet(target, field, v) -> hashOpt hashc target <+ hashF field <+ hashc v <+ 71
        | ForIntegerRangeLoop(v, s, e, st) -> hashV v <+ hashc s <+ hashc e <+ hashc st <+ 73
        | LetRecursive(bindings, body) -> bindings |> hashList (hashB funcs) (hashc body) <+ 79
        | NewArray(t, args) -> args |> hashList hashc (hashT t)  <+ 83
        | NewDelegate(t, args, e) -> args |> hashList hashV (hashT t <+ hashc e) <+ 83
        | NewObject(c, args) -> args |> hashList hashc (hashCst c) <+ 89
        | NewRecord(t, args) -> args |> hashList hashc (hashT t) <+ 97
        | NewTuple(args) -> args |> hashList hashc 101
        | NewUnionCase(case, args) -> args |> hashList hashc (hashCse case) <+ 103
        | PropertySet(target, prop, args, v) -> args |> hashList hashc (hashOpt hashc target <+ hashP prop <+ hashc v) <+ 107
        | Quote(e) -> hashc e <+ 109
        | Sequential(f,s) -> hashc f <+ hashc s <+ 113
        | TryFinally(body, f) -> hashc body <+ hashc f <+ 127
        | TryWith(body, v, e, v2, e2) -> hashc body <+ hashV v <+ hashc e <+ hashV v2 <+ hashc e2 <+ 131
        | TypeTest(e, t) -> hashc e <+ hashT t <+ 137
        | UnionCaseTest(e, case) -> hashc e <+ hashCse case <+ 139
        | Value(v, t) -> hash v <+ hashT t  <+ 149
        | VarSet(v, e) -> hashV v <+ hashc e <+ 151
        | WhileLoop(cond, body) -> hashc cond <+ hashc body <+ 157
        | e -> failwithf "Unsupported expression %A" e
 
    and hashV v =
        hash v.IsMutable <+ hashT v.Type
    and hashT t =
        let rec recHashT types (t: System.Type) =
            if t.IsPrimitive || t = typeof<string> then
                hash t.FullName
            else
                t.GetFields(BindingFlags.Instance ||| BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.GetField) |> Seq.fold (!< (fun f -> 
                    if Set.contains f.FieldType.FullName types 
                    then  hash f.Name 
                    else recHashT (Set.add f.FieldType.FullName types) f.FieldType <+ hash f.Name)  ) (hash t.FullName)
        recHashT Set.empty t         
    and hashP p =
        hash p.Name <+ hashT p.PropertyType
    and hashF f =
        hash f.Name <+ hashT f.FieldType
    and hashOpt f o = 
        match o with
        | Some e -> f e
        | None -> 0
    and hashB funcs (v,e) = hashV v <+ hashC funcs e
    and hashCst c = hash c.Name
    and hashCse c = hash c.Name   
    
    let ComputeHashCode(e:Expr) =
        hashC Set.empty e
         
type WorkItemInfo = 
    interface
        abstract member GlobalID: int -> int
        abstract member LocalID: int -> int
        abstract member GlobalSize: int -> int
        abstract member LocalSize: int -> int
        abstract member NumGroups: int -> int
        abstract member GroupID: int -> int
        abstract member GlobalOffset: int -> int
        abstract member WorkDim: unit -> int
        abstract member LocalBarrier: unit -> unit
        abstract member GlobalBarrier: unit -> unit
    end

type FunctionInfoID = obj
         
type LambdaFunctionID(e:Expr) =
    member val Expr = e with get
    member val private hashcode = None with get, set

    override this.Equals(o1) =
        o1.GetHashCode() = this.GetHashCode()
            
    override this.GetHashCode() =
        if this.hashcode.IsNone then
            this.hashcode <- Some(QuotationComparison.ComputeHashCode(e))
        this.hashcode.Value
        
type CollectionFunctionID(m:MethodInfo, e:Expr option) =
    member val MethodInfo = m with get
    member val Expr = e with get
    member val private hashcode = None with get, set

    override this.Equals(o1) =
        o1.GetHashCode() = this.GetHashCode()
            
    override this.GetHashCode() =
        if this.hashcode.IsNone then               
            let hash = (((17 * 31) + m.GetHashCode()) * 31) + 
                        (if e.IsSome then
                             QuotationComparison.ComputeHashCode(e.Value)   
                         else 
                            0)
            this.hashcode <- Some(hash)
        this.hashcode.Value
        
type MethodID(m:MethodInfo) =
    member val MethodInfo = m with get
    member val private hashcode = None with get, set

    override this.Equals(o1) =
        o1.GetHashCode() = this.GetHashCode()
            
    override this.GetHashCode() =
        if this.hashcode.IsNone then               
            let hash = m.GetHashCode()
            this.hashcode <- Some(hash)
        this.hashcode.Value
               
///
///<summary>
/// The set of information about utility functions collected and maintained by the compiler
///</summary>
///

[<AllowNullLiteral>]
type IFunctionInfo =
    abstract ID: obj with get
    abstract Name: String with get
    abstract ParsedSignature: MethodInfo option with get

    abstract OriginalParameters: IReadOnlyList<IFunctionParameter> with get
    abstract GeneratedParameters: IReadOnlyList<IFunctionParameter> with get
    abstract Parameters: IReadOnlyList<IFunctionParameter> with get
    //abstract EnvVars: Var list with get
    abstract EnvVarsUsed: IReadOnlyList<Var> with get
    abstract OutValsUsed: IReadOnlyList<Expr> with get
    abstract ReturnType: Type with get
    abstract Body: Expr with get
    abstract OriginalBody: Expr with get
    
    abstract CalledFunctions: IReadOnlyList<obj> with get
    
    abstract member Code: string with get
    abstract member SignatureCode: string with get
    
    abstract IsLambda: bool with get
    abstract CustomInfo: IReadOnlyDictionary<string, obj> with get

    abstract GetParameter: string -> IFunctionParameter option

[<AllowNullLiteral>]
type IKernelInfo =
    inherit IFunctionInfo
    abstract WorkSize: WorkItemInfo option
    abstract LocalVars: IReadOnlyDictionary<Quotations.Var, Type * (Expr list option)>
    abstract Meta: ReadOnlyMetaCollection with get
    abstract CloneTo: IKernelInfo -> unit

[<AllowNullLiteral>]
type FunctionInfo(functionName: String,
                  parsedSignature: MethodInfo option, 
                  //paramInfos: ParameterInfo list option,
                  paramVars: Quotations.Var list,
                  returnType: Type,
                  envVarsUsed: IReadOnlyList<Var>,
                  outValsUsed: IReadOnlyList<Expr>,
                  body: Expr) =   
                  
    let parameters =
//        if paramInfos.IsSome then
//            paramInfos.Value |> 
//            List.mapi(fun i (p:ParameterInfo) ->
//                        FunctionParameter(p.Name, paramVars.[i], FunctionParameterType.NormalParameter, None))
//        else
            paramVars |> 
            List.mapi(fun i (p:Var) ->
                        FunctionParameter(p.Name, paramVars.[i], FunctionParameterType.NormalParameter, None)) 
            
    interface IFunctionInfo with
        member this.ID 
            with get() = 
                this.ID
        member this.Name
            with get() =
                this.Name
        member this.ParsedSignature 
            with get() =
                this.ParsedSignature
        member this.OriginalParameters
            with get() =
                let roList = new List<IFunctionParameter>()
                for item in this.OriginalParameters do
                    roList.Add(item)
                roList :> IReadOnlyList<IFunctionParameter>
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
                this.CalledFunctions :> IReadOnlyList<obj>
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
    abstract member ID: obj
    default this.ID 
        with get() =     
            // A lambda kernels/function is identified by its AST    
            if this.IsLambda then
                LambdaFunctionID(body) |> box
            else
                MethodID(parsedSignature.Value) |> box

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
    
    member val CalledFunctions = new List<obj>()
        with get

    member val Name = functionName with get
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
    member val ReturnType = returnType with get, set
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
    /// The generated target code for the signature
    ///</summary>
    ///
    member val SignatureCode = "" with get, set   
    ///
    ///<summary>
    /// Whether this function has been generated from a lambda
    ///</summary>
    ///
    member val IsLambda = parsedSignature.IsNone with get
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
        
     member val CallMapping = new Dictionary<Expr, Expr list>() with get    
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
type KernelInfo(kernelName: String,
                originalSignature: MethodInfo option, 
                //paramInfos: ParameterInfo list option,
                paramVars: Quotations.Var list,
                returnType: Type,
                envVarsUsed: IReadOnlyList<Var>,
                outValsUsed: IReadOnlyList<Expr>,
                workSize: WorkItemInfo option,
                body: Expr, 
                meta: ReadOnlyMetaCollection) =
    inherit FunctionInfo(kernelName, originalSignature, paramVars, returnType, envVarsUsed, outValsUsed, body)
                      
    let parameters =
//        if paramInfos.IsSome then
//            paramInfos.Value |> 
//            List.mapi(fun i (p:ParameterInfo) ->
//                        FunctionParameter(p.Name, paramVars.[i], FunctionParameterType.NormalParameter, Some(meta.ParamMeta.[i])))
//        else
            paramVars |> 
            List.mapi(fun i (p:Var) ->
                        FunctionParameter(p.Name, paramVars.[i], FunctionParameterType.NormalParameter, Some(meta.ParamMeta.[i]))) 

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
                if oldParameter.IsReturned && oldParameter.IsDynamicReturnArrayParameter then
                    // Must associate new Return Meta
                    let newParameter = new FunctionParameter(oldParameter.Name, oldParameter.OriginalPlaceholder, oldParameter.ParameterType, Some(f.Meta.ReturnMeta :> IParamMetaCollection)) 
                    oldParameter.CloneTo(newParameter)
                    f.GeneratedParameters.Add(newParameter)
                else                    
                    let newParameter = new FunctionParameter(oldParameter.Name, oldParameter.OriginalPlaceholder, oldParameter.ParameterType, None) 
                    oldParameter.CloneTo(newParameter)
                    f.GeneratedParameters.Add(newParameter)
               
