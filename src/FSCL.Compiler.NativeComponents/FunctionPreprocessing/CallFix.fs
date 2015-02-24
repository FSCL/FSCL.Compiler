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
                Dependencies=[| "FSCL_DYN_ARRAY_ARGS_PARAM_REF_LIFTING_PREPROCESSING_PROCESSOR"; 
                                "FSCL_REF_TYPE_TO_ARRAY_REPLACING_PREPROCESSING_PROCESSOR";
                                "FSCL_ADD_LENGTH_ARGS_PREPROCESSING_PROCESSOR" |])>]
type CallFixProcessor() =
    inherit FunctionPreprocessingProcessor()
            
    let GenerateSizeAdditionalArg (name:string, n:obj) =
         String.Format("{0}_length_{1}", name, n.ToString())
                
    member private this.FixCalls(expr:Expr, step:FunctionPreprocessingStep, parent:FunctionInfo) =
        match expr with
        | Patterns.Call(o, mi, a) ->
            let utilityFunction = 
                let mutable found = None
                for item in step.Functions do
                    if item.Value.ParsedSignature = mi then
                        found <- Some(item.Value)
                found

            if utilityFunction.IsSome then
                // Must create a new method info with additional length parameters for each array par
                let newArgs = new List<Var>(utilityFunction.Value.OriginalParameters |> Seq.map(fun p -> p.Placeholder))
                let newArgsActualValues = new List<Expr>(a)
                let newArgsLengths = new List<Var>()
                let newArgsLengthsActualValues = new List<Expr>()

                let n = utilityFunction.Value.ParsedSignature

                // Add array lengths
                // For each parameter of type array, we get the corresponding argument
                // Then we look which parameter of the caller it refers to and we pass the
                // proper lengths
                let originalParameters = utilityFunction.Value.OriginalParameters
                for i = 0 to a.Length - 1 do
                    let arg = a.[i]
                    if arg.Type.IsArray then
                        match arg with
                        | Patterns.Var(v) ->
                            let parentP = parent.Parameters |> List.tryFind(fun p -> p.OriginalPlaceholder = v)
                            if parentP.IsSome then
                                for d = 0 to parentP.Value.SizeParameters.Count - 1 do
                                    newArgsLengths.Add(Quotations.Var(GenerateSizeAdditionalArg(originalParameters.[i].Name, d), parentP.Value.SizeParameters.[d].DataType))
                                    newArgsLengthsActualValues.Add(Expr.Var(parentP.Value.SizeParameters.[d].Placeholder))
                            else
                                raise (new CompilerException("Cannot pass to an utility function an array that is not a parameter of the caller kernel"))
                        | _ ->
                            raise (new CompilerException("Cannot pass to an utility function an array that is not a parameter of the caller kernel"))
                
                // Do the same for each generated parameter
                for p in utilityFunction.Value.GeneratedParameters do                    
                    match p.ParameterType with
                    | FunctionParameterType.EnvVarParameter(v) ->
                        newArgs.Add(v)
                        // The argument is the reference to the placeholder
                        // Since this is a ref to a (immutable) var v, the argument is
                        // Expr.Var(v)
                        let parameter = 
                                        parent.GeneratedParameters |> 
                                        Seq.tryFind(fun p -> 
                                                        match p.ParameterType with
                                                        | FunctionParameterType.EnvVarParameter(v2) when v = v2 ->
                                                            true
                                                        | _ ->
                                                            false)
                        if parameter.IsSome then
                            newArgsActualValues.Add(Expr.Var(parameter.Value.Placeholder))
                        else
                            raise (new CompilerException("Cannot find the parameter holding the var " + v.Name + " in caller " + parent.ParsedSignature.ToString()))
                        // Add sizes
                        for d = 0 to parameter.Value.SizeParameters.Count - 1 do
                            newArgsLengths.Add(Quotations.Var(GenerateSizeAdditionalArg(p.Name, d), parameter.Value.SizeParameters.[d].DataType))
                            newArgsLengthsActualValues.Add(Expr.Var(parameter.Value.SizeParameters.[d].Placeholder))
                    | FunctionParameterType.OutValParameter(e) ->
                        newArgs.Add(p.Placeholder)
                        // The argument is the reference to the placeholder
                        // Since this is a ref to a value, the argument is
                        // the variable encoding this value among the parent parameters
                        let parameter = parent.GeneratedParameters |> 
                                        Seq.tryFind(fun p -> 
                                                        match p.ParameterType with
                                                        | FunctionParameterType.OutValParameter(o) when o.Equals(e) ->
                                                            true
                                                        | _ ->
                                                            false)
                        if parameter.IsSome then
                            newArgsActualValues.Add(Expr.Var(parameter.Value.Placeholder))
                        else
                            raise (new CompilerException("Cannot find the parameter holding the outsider value " + e.ToString() + " in caller " + parent.ParsedSignature.ToString()))
                        // Add sizes
                        for d = 0 to parameter.Value.SizeParameters.Count - 1 do
                            newArgsLengths.Add(Quotations.Var(GenerateSizeAdditionalArg(p.Name, d), parameter.Value.SizeParameters.[d].DataType))
                            newArgsLengthsActualValues.Add(Expr.Var(parameter.Value.SizeParameters.[d].Placeholder))
                    | _ ->
                        ()                                
                               
                // Merge
                newArgs.AddRange(newArgsLengths)
                newArgsActualValues.AddRange(newArgsLengthsActualValues)    
                                      
                // Create final signature
                let finalArgsTypes = (newArgs |> List.ofSeq |> List.map (fun v -> v.Type))
                let finalActualArgs = (newArgsActualValues |> Seq.toList)
                let assemblyName = mi.Name + "_module";
                let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run)
                let moduleBuilder = assemblyBuilder.DefineDynamicModule(mi.Name + "_module");
                let methodBuilder = moduleBuilder.DefineGlobalMethod(
                                        mi.Name,
                                        MethodAttributes.Public ||| MethodAttributes.Static, mi.ReturnType, 
                                        finalArgsTypes |> List.toArray)
                for i = 0 to newArgs.Count - 1 do
                    methodBuilder.DefineParameter(i + 1, ParameterAttributes.In, newArgs.[i].Name) |> ignore
                methodBuilder.GetILGenerator().Emit(OpCodes.Ret)
                moduleBuilder.CreateGlobalFunctions()
                let signature = moduleBuilder.GetMethod(methodBuilder.Name) 
             
                // Replace call with call to this function
                Expr.Call(signature, finalActualArgs)
            else
                // Very likely a lambda transformed to method, skip
                let fixedArgs = a |> List.map(fun e -> this.FixCalls(e, step, parent))
                if o.IsSome then
                    Expr.Call(o.Value, mi, fixedArgs)
                else
                    Expr.Call(mi, fixedArgs)

        | ExprShape.ShapeLambda(v, e) ->   
            Expr.Lambda(v, this.FixCalls(e, step, parent))
        | ExprShape.ShapeCombination(o, args) ->   
            let fixedArgs = args |> List.map(fun e -> this.FixCalls(e, step, parent))
            ExprShape.RebuildShapeCombination(o, fixedArgs)
        | ExprShape.ShapeVar(v) ->
            expr
        
    override this.Run(fInfo, en, opts) =
        let engine = en :?> FunctionPreprocessingStep
        fInfo.Body <- this.FixCalls(fInfo.Body, engine, fInfo)
       
