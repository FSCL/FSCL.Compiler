namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Language
open System.Reflection
open System.Reflection.Emit

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_FUNCTION_PREPROCESSING_STEP", 
       Dependencies = [| "FSCL_MODULE_PREPROCESSING_STEP"; "FSCL_MODULE_PARSING_STEP" |])>]
type FunctionPreprocessingStep(tm: TypeManager, 
                               processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelExpression, KernelExpression>(tm, processors)
    
    member val private currentFunction:FunctionInfo = null with get, set
    member val private functions = null with get, set
   
    member this.Functions 
        with get() =
            this.functions
        and private set(v) =
            this.functions <- v

    member this.FunctionInfo 
        with get() =
            this.currentFunction
        and private set(v) =
            this.currentFunction <- v

    member private this.Process(k, opts) =
        this.FunctionInfo <- k
        for p in processors do
            p.Execute(k, this, opts) |> ignore
            
    // Guarantee calleer and callee have same qualifiers for parameters "connected" 
    member this.UniformQualifiers(p:IFunctionParameter, pp:IFunctionParameter, caller:FunctionInfo) =
        if pp.ForcedGlobalAddressSpace then
            (p :?> FunctionParameter).ForcedGlobalAddressSpace <- true
        else if pp.ForcedConstantAddressSpace then
            (p :?> FunctionParameter).ForcedConstantAddressSpace <- true
        else if pp.ForcedLocalAddressSpace then
            (p :?> FunctionParameter).ForcedLocalAddressSpace <- true
        else if pp.ForcedPrivateAddressSpace then
            (p :?> FunctionParameter).ForcedPrivateAddressSpace <- true
        else
            // Check meta
            let addressSpace = pp.Meta.Get<AddressSpaceAttribute>().AddressSpace
            if addressSpace = AddressSpace.Local then
                (p :?> FunctionParameter).ForcedLocalAddressSpace <- true
            elif addressSpace = AddressSpace.Constant then
                (p :?> FunctionParameter).ForcedConstantAddressSpace <- true
            elif addressSpace = AddressSpace.Private then
                (p :?> FunctionParameter).ForcedPrivateAddressSpace <- true
            else
                // If parent is kernel than no qualifier means global, else private
                if (caller :? KernelInfo) then
                    (p :?> FunctionParameter).ForcedGlobalAddressSpace <- true
                else
                    (p :?> FunctionParameter).ForcedPrivateAddressSpace <- true

    // This is to pass length arguments (for all) and out vals/env refs (only for accelerated collections) to utility functions 
    member private this.FixCalls(caller:FunctionInfo) =
        let rec FixCallsInternal(expr:Expr) =
            match expr with
            | Patterns.Call(o, mi, a) ->
                let utilityFunction = 
                    let mutable found = None
                    for (item:KeyValuePair<FunctionInfoID, IFunctionInfo>) in this.Functions do
                        if item.Value.ParsedSignature = mi then
                            found <- Some(item.Value)
                    found

                if utilityFunction.IsSome then
                    // Must create a new method info with additional length parameters for each array par
                    let newArgsNamesAndTypes = new List<String * Type>(utilityFunction.Value.OriginalParameters |> Seq.map(fun p -> (p.Placeholder.Name, p.Placeholder.Type)))
                    // Remove any WorkItemInfo value from arguments
                    let newArgsActualValues = new List<Expr>(a |> List.filter(fun e -> typeof<WorkItemInfo>.IsAssignableFrom(e.Type) |> not))
                    let newLengths = new List<String * Type>()
                    let newLengthsActualValues = new List<Expr>()

                    // Iterate over parameters and add the required args
                    let mutable argIndex = 0;
                    for p in utilityFunction.Value.Parameters do                    
                        match p.ParameterType with
                        | FunctionParameterType.NormalParameter    
                        | FunctionParameterType.DynamicArrayParameter(_) ->    
                            // If the data type is array, must add the length args
                            if p.DataType.IsArray then
                                // The argument should be a ref to a caller parameter 
                                let callerParam =
                                    match a.[argIndex] with
                                    | Patterns.Var(pv) ->
                                        caller.Parameters |> 
                                        List.tryFind(fun p -> 
                                                        p.OriginalPlaceholder = pv)
                                    | _ ->
                                        raise (new CompilerException("Cannot pass to an utility function an array that doesn't refer a caller parameter or returned array variable (parameter " + p.Name + ")"))
                                match callerParam with
                                | None ->
                                    raise (new CompilerException("Cannot find the caller parameter to which the parameter " + p.Name + " of the utility function " + utilityFunction.Value.ParsedSignature.Name + " refers to"))
                                | Some(pp) ->  
                                    // Caller and callee must have the same parameter qualifier
                                    this.UniformQualifiers(p, pp, caller)
                                    // Add to the args the refs to the size parameters
                                    for i = 0 to pp.SizeParameters.Count - 1 do
                                        newLengths.Add(p.SizeParameters.[i].Name, p.SizeParameters.[i].DataType)
                                        newLengthsActualValues.Add(Expr.Var(pp.SizeParameters.[i].Placeholder))
                             
                        // In case of accelerated collections the kernel can call a utility function
                        // which refers the outside environment
                        | FunctionParameterType.EnvVarParameter(v) ->
                            newArgsNamesAndTypes.Add(p.Name, p.DataType)
                            // The argument is the reference to the placeholder
                            // Since this is a ref to a (immutable) var v, the argument is
                            // Expr.Var(v)
                            let parameter = 
                                            caller.GeneratedParameters |> 
                                            Seq.tryFind(fun p -> 
                                                            match p.ParameterType with
                                                            | FunctionParameterType.EnvVarParameter(v2) when v = v2 ->
                                                                true
                                                            | _ ->
                                                                false)
                            if parameter.IsSome then
                                newArgsActualValues.Add(Expr.Var(parameter.Value.Placeholder))                            
                                // If the data type is array, must add the length args
                                if p.DataType.IsArray then
                                    // Caller and callee must have the same parameter qualifier
                                    this.UniformQualifiers(p, parameter.Value, caller)
                                    // Add to the args the refs to the size parameters
                                    for i = 0 to parameter.Value.SizeParameters.Count - 1 do
                                        newLengths.Add(p.SizeParameters.[i].Name, p.SizeParameters.[i].DataType)
                                        newLengthsActualValues.Add(Expr.Var(parameter.Value.SizeParameters.[i].Placeholder))
                            else
                                raise (new CompilerException("Cannot find the parameter holding the var " + v.Name + " in caller " + caller.ParsedSignature.ToString()))
                        
                        | FunctionParameterType.OutValParameter(e) ->
                            newArgsNamesAndTypes.Add(p.Name, p.DataType)
                            // The argument is the reference to the placeholder
                            // Since this is a ref to a value, the argument is
                            // the variable encoding this value among the caller parameters
                            let parameter = caller.GeneratedParameters |> 
                                            Seq.tryFind(fun p -> 
                                                            match p.ParameterType with
                                                            | FunctionParameterType.OutValParameter(o) when o.Equals(e) ->
                                                                true
                                                            | _ ->
                                                                false)
                            if parameter.IsSome then
                                newArgsActualValues.Add(Expr.Var(parameter.Value.Placeholder))
                                // If the data type is array, must add the length args
                                if p.DataType.IsArray then
                                    // Caller and callee must have the same parameter qualifier
                                    this.UniformQualifiers(p, parameter.Value, caller)
                                    // Add to the args the refs to the size parameters
                                    for i = 0 to parameter.Value.SizeParameters.Count - 1 do
                                        newLengths.Add(p.SizeParameters.[i].Name, p.SizeParameters.[i].DataType)
                                        newLengthsActualValues.Add(Expr.Var(parameter.Value.SizeParameters.[i].Placeholder))
                            else
                                raise (new CompilerException("Cannot find the parameter holding the outsider value " + e.ToString() + " in caller " + caller.ParsedSignature.ToString()))                        
                        | _ ->
                            ()
                
                               
                    // Merge
                    newArgsNamesAndTypes.AddRange(newLengths)
                    newArgsActualValues.AddRange(newLengthsActualValues)    
                                      
                    // Create final signature
                    let finalArgsNames, finalArgsTypes = newArgsNamesAndTypes |> Seq.toArray |> Array.unzip
                    let finalActualArgs = newArgsActualValues |> Seq.toList
                    let assemblyName = mi.Name + "_module";
                    let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run)
                    let moduleBuilder = assemblyBuilder.DefineDynamicModule(mi.Name + "_module");
                    let methodBuilder = moduleBuilder.DefineGlobalMethod(
                                            mi.Name,
                                            MethodAttributes.Public ||| MethodAttributes.Static, mi.ReturnType, 
                                            finalArgsTypes)
                    for i = 0 to finalArgsNames.Length - 1 do
                        methodBuilder.DefineParameter(i + 1, ParameterAttributes.In, finalArgsNames.[i]) |> ignore
                    methodBuilder.GetILGenerator().Emit(OpCodes.Ret)
                    moduleBuilder.CreateGlobalFunctions()
                    let signature = moduleBuilder.GetMethod(methodBuilder.Name) 
             
                    // Replace call with call to this function
                    Expr.Call(signature, finalActualArgs)
                else
                    // Very likely a lambda transformed to method, skip
                    let fixedArgs = a |> List.map(fun e -> FixCallsInternal(e))
                    if o.IsSome then
                        Expr.Call(o.Value, mi, fixedArgs)
                    else
                        Expr.Call(mi, fixedArgs)

            | ExprShape.ShapeLambda(v, e) ->   
                Expr.Lambda(v, FixCallsInternal(e))
            | ExprShape.ShapeCombination(o, args) ->   
                let fixedArgs = args |> List.map(fun e -> FixCallsInternal(e))
                ExprShape.RebuildShapeCombination(o, fixedArgs)
            | ExprShape.ShapeVar(v) ->
                expr
        // Replace body
        caller.Body <- FixCallsInternal(caller.Body)
               
    
    override this.Run(cem, opts) =    
        for km in cem.KernelModulesRequiringCompilation do
            this.Functions <- km.Functions
            for f in km.Functions do            
                this.Process(f.Value :?> FunctionInfo, opts)
            this.Process(km.Kernel, opts)
            // Fix calls
            this.FixCalls(km.Kernel)
            for f in km.Functions do
                this.FixCalls(f.Value :?> FunctionInfo)
        ContinueCompilation(cem)


