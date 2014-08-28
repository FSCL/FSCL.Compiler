namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Collections.Generic
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices
open FSCL.Compiler.Util
open System.Reflection

[<StepProcessor("FSCL_CALL_SIGNATURE_FIX_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies=[| "FSCL_DYN_ARRAY_ARGS_PARAM_REF_LIFTING_PREPROCESSING_PROCESSOR"; "FSCL_REF_TYPE_TO_ARRAY_REPLACING_PREPROCESSING_PROCESSOR" |])>]
type FunctionCallSignatureFixProcessor() =
    inherit FunctionPreprocessingProcessor()
            
    let GenerateSizeAdditionalArg (name:string, n:obj) =
         String.Format("{0}_length_{1}", name, n.ToString())

    member private this.AddDynamicArrayParameter(step: FunctionPreprocessingStep, kernel:FunctionInfo, var:Var, allocationArgs:Expr array) =
        if (var.IsMutable) then
            raise (new CompilerException("A kernel dynamic array must be immutable"))
                   
        // Fix signature and kernel parameters
        let kernelInfo = kernel :?> KernelInfo

        // Add parameter
        let pInfo = new FunctionParameter(var.Name, 
                                          var, 
                                          DynamicParameter(allocationArgs),
                                          None)
        kernelInfo.GeneratedParameters.Add(pInfo)
       
    member private this.FixSignatures(expr:Expr, step:FunctionPreprocessingStep, functionInfo:FunctionInfo) =
        match expr with
        | Patterns.Call(o, mi, a) ->
            if mi.ReflectedType <> null then
                match mi with
                | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                    // Must create a new method info with additional length parameters for each array par
                    let newArgs = new List<Var>()
                    let newArgsActualValues = new List<Expr>()
                    let calledParameters = mi.GetParameters()
                    for pIndex = 0 to calledParameters.Length - 1 do
                        let p = calledParameters.[pIndex]
                        if p.ParameterType.IsArray then     
                            // Generate additional length args
                            let dimensions = ArrayUtil.GetArrayDimensions(p.ParameterType) 
                            for d = 0 to dimensions - 1 do
                                let sizeName = GenerateSizeAdditionalArg(p.Name, d)
                                newArgs.Add(Quotations.Var(sizeName, typeof<int>))
                            // Determine which caller array param is references to give an actual value to this par in the call
                            let callerExprForPar = a.[pIndex]
                            match callerExprForPar with
                            | Patterns.Var(callerV) ->
                                let parRef = functionInfo.Parameters |> List.tryFind(fun (f:FunctionParameter) -> f.OriginalPlaceholder = callerV)
                                if parRef.IsNone then
                                    raise (new CompilerException("Cannot find the parameter of the called (" + functionInfo.ParsedSignature.Name + ") referenced in the call to " + mi.Name))
                                else
                                    for lengthPar in parRef.Value.SizeParameters do
                                        newArgsActualValues.Add(Expr.Var(lengthPar.Placeholder))
                            | _ ->
                                raise (new CompilerException("Cannot pass an array to an utility function that is not a reference to a paramter of the caller"))
                                                                     
                    // Create final signature
                    let finalArgsTypes = (mi.GetParameters() |> List.ofSeq |> List.map(fun (p:ParameterInfo) -> p.ParameterType)) @ (newArgs |> List.ofSeq |> List.map (fun v -> v.Type))
                    let finalActualArgs = a @ (newArgsActualValues |> Seq.toList)
                    let assemblyName = mi.Name + "_module";
                    let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run)
                    let moduleBuilder = assemblyBuilder.DefineDynamicModule(mi.Name + "_module");
                    let methodBuilder = moduleBuilder.DefineGlobalMethod(
                                            mi.Name,
                                            MethodAttributes.Public ||| MethodAttributes.Static, mi.ReturnType, 
                                            finalArgsTypes |> List.toArray)
                    for i = 0 to mi.GetParameters().Length - 1 do
                        methodBuilder.DefineParameter(i + 1, ParameterAttributes.In, mi.GetParameters().[i].Name) |> ignore
                    for i = 0 to newArgs.Count - 1 do
                        methodBuilder.DefineParameter(i + mi.GetParameters().Length + 1, ParameterAttributes.In, newArgs.[i].Name) |> ignore  
                    methodBuilder.GetILGenerator().Emit(OpCodes.Ret)
                    moduleBuilder.CreateGlobalFunctions()
                    let signature = moduleBuilder.GetMethod(methodBuilder.Name) 
             
                    // Replace call with call to this function
                    Expr.Call(signature, finalActualArgs)

                | _ ->
                    let fixedArgs = a |> List.map(fun e -> this.FixSignatures(e, step, functionInfo))
                    if o.IsSome then
                        Expr.Call(o.Value, mi, fixedArgs)
                    else
                        Expr.Call(mi, fixedArgs)
                else
                    // Very likely a lambda transformed to method, skip
                    let fixedArgs = a |> List.map(fun e -> this.FixSignatures(e, step, functionInfo))
                    if o.IsSome then
                        Expr.Call(o.Value, mi, fixedArgs)
                    else
                        Expr.Call(mi, fixedArgs)

        | ExprShape.ShapeLambda(v, e) ->   
            Expr.Lambda(v, this.FixSignatures(e, step, functionInfo))
        | ExprShape.ShapeCombination(o, args) ->   
            let fixedArgs = args |> List.map(fun e -> this.FixSignatures(e, step, functionInfo))
            ExprShape.RebuildShapeCombination(o, fixedArgs)
        | ExprShape.ShapeVar(v) ->
            expr
        
    override this.Run(fInfo, en, opts) =
        let engine = en :?> FunctionPreprocessingStep
        fInfo.Body <- this.FixSignatures(fInfo.Body, engine, fInfo)
       
