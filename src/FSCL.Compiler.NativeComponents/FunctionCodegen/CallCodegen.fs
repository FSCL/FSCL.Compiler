namespace FSCL.Compiler.FunctionCodegen

open FSCL
open System
open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open System.Reflection
open System.Runtime.CompilerServices
open Microsoft.FSharp.Linq.RuntimeHelpers
open FSCL.Compiler.Util.QuotationAnalysis
open FSCL.Compiler.Util.ReflectionUtil
open Microsoft.FSharp.Reflection

open FSCL.Compiler.Util.QuotationAnalysis.FunctionsManipulation
open FSCL.Compiler.Util.QuotationAnalysis.KernelParsing
open FSCL.Compiler.Util.QuotationAnalysis.MetadataExtraction

[<StepProcessor("FSCL_CALL_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP",
                Dependencies = [| "FSCL_ARRAY_ACCESS_CODEGEN_PROCESSOR";
                                  "FSCL_DECLARATION_CODEGEN_PROCESSOR";
                                  "FSCL_ARITH_OP_CODEGEN_PROCESSOR" |])>]
                                  
///
///<summary>
///The function codegen step whose behavior is to generate the target representation of method calls 
///</summary>
///  
type CallCodegen() =
    inherit FunctionBodyCodegenProcessor()

    // Set of calls to cast values
    let castMethods = new Dictionary<MethodInfo, string>()
    do
        castMethods.Add(ExtractMethodInfo(<@ int @>).Value.TryGetGenericMethodDefinition(), "int")
        castMethods.Add(ExtractMethodInfo(<@ uint32 @>).Value.TryGetGenericMethodDefinition(), "unsigned int")
        castMethods.Add(ExtractMethodInfo(<@ char @>).Value.TryGetGenericMethodDefinition(), "char")
        castMethods.Add(ExtractMethodInfo(<@ byte @>).Value.TryGetGenericMethodDefinition(), "uchar")
        castMethods.Add(ExtractMethodInfo(<@ sbyte @>).Value.TryGetGenericMethodDefinition(), "char")
        castMethods.Add(ExtractMethodInfo(<@ float32 @>).Value.TryGetGenericMethodDefinition(), "float")
        castMethods.Add(ExtractMethodInfo(<@ float @>).Value.TryGetGenericMethodDefinition(), "double")
        castMethods.Add(ExtractMethodInfo(<@ int64 @>).Value.TryGetGenericMethodDefinition(), "long")
        castMethods.Add(ExtractMethodInfo(<@ uint64 @>).Value.TryGetGenericMethodDefinition(), "ulong")

    // Set of .NET Math functions that can be used in place of the OpenCL matching ones
    let alternativeFunctions = new Dictionary<MethodInfo, string>()
    let populateAlternativeFunctions(quotedMeth: Expr, mathingFunction: string) =
        alternativeFunctions.Add(ExtractMethodInfo(quotedMeth).Value.TryGetGenericMethodDefinition(), mathingFunction)

    do
        populateAlternativeFunctions(<@ Math.Acos @>, "acos")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.acos @>, "acos")
        populateAlternativeFunctions(<@ Math.Asin @>, "asin")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.asin @>, "asin")
        populateAlternativeFunctions(<@ Math.Atan @>, "atan")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.atan @>, "atan")
        populateAlternativeFunctions(<@ Math.Atan2 @>, "atan2")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.atan2 @>, "atan2")
        populateAlternativeFunctions(<@ Math.Ceiling 0.0 @>, "ceil")
        populateAlternativeFunctions(<@ Math.Ceiling 0m @>, "ceil")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.ceil @>, "ceil")
        populateAlternativeFunctions(<@ Math.Cos @>, "cos")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.cos @>, "cos")
        populateAlternativeFunctions(<@ Math.Cosh @>, "cosh")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.cosh @>, "cosh")
        populateAlternativeFunctions(<@ Math.Exp @>, "exp")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.exp @>, "exp")
        populateAlternativeFunctions(<@ Math.Floor 0.0 @>, "floor")
        populateAlternativeFunctions(<@ Math.Floor 0m @>, "floor")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.floor @>, "floor")
        populateAlternativeFunctions(<@ Math.Sqrt 0.0 @>, "sqrt")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.sqrt @>, "sqrt")

        populateAlternativeFunctions(<@ Math.Min(0m, 0m) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0, 0) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0u, 0u) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0uL, 0uL) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0L, 0L) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0y, 0y) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0uy, 0uy) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0s, 0s) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0us, 0us) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0.0, 0.0) @>, "min")
        populateAlternativeFunctions(<@ Math.Min(0.0f, 0.0f) @>, "min")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.min @>, "min")
        
        populateAlternativeFunctions(<@ Math.Max(0m, 0m) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0, 0) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0u, 0u) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0uL, 0uL) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0L, 0L) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0y, 0y) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0uy, 0uy) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0s, 0s) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0us, 0us) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0.0, 0.0) @>, "max")
        populateAlternativeFunctions(<@ Math.Max(0.0f, 0.0f) @>, "max")
        populateAlternativeFunctions(<@ Microsoft.FSharp.Core.Operators.max @>, "max")


    ///
    ///<summary>
    ///The method called to execute the processor
    ///</summary>
    ///<param name="fi">The AST node (expression) to process</param>
    ///<param name="en">The owner step</param>
    ///<returns>
    ///The target code for the method call (a function call in the target)if the AST node can be processed (i.e. if the source node is a method call)
    ///</returns>
    ///  
    override this.Run(expr, en, opts) =
        let step = en :?> FunctionCodegenStep
        match expr with
        | DerivedPatterns.SpecificCall <@ local @> (o, mi, a) ->
            // Ignore call to __local
            None
        | Patterns.Call (o, mi, a) ->
            // Check if the call is the last thing done in the function body
            // If so, prepend "return"
            let returnPrefix = 
                if(step.FunctionInfo.CustomInfo.ContainsKey("FUNCTION_RETURN_EXPRESSIONS")) then
                    let returnTags = 
                        step.FunctionInfo.CustomInfo.["FUNCTION_RETURN_EXPRESSIONS"] :?> Expr list
                    if (List.tryFind(fun (e:Expr) -> e = expr) returnTags).IsSome then
                        "return "
                    else
                        ""
                else
                    ""
            let returnPostfix = if returnPrefix.Length > 0 then ";\n" else ""
            
            // Get args filtering out WorkItemInfo(s)
            let mutable args = a |> 
                               List.filter(fun (e:Expr) -> e.Type <> typeof<WorkItemInfo>) |> 
                               List.map (fun (e:Expr) -> step.Continue(e)) |>  
                               String.concat ", "
                               
            // Check if call to an utility function
            // In such case codegen outsiders (and possibly length args)
           (* let utilityFunction = step.FunctionInfo.CalledFunctions |> Seq.tryFind(fun id -> id = MethodID(mi))
            if utilityFunction.IsSome then
                let funInfo = step.Functions.[utilityFunction.Value]
                let genPars = new List<string>()
                // Pass the length of the array arguments
                for i = 0 to a.Length - 1 do
                    let argument = a.[i]
                    if argument.Type.IsArray then
                        for r = 0 to FSCL.Compiler.Util.ArrayUtil.GetArrayDimensions(argument.Type) - 1 do
                            genPars.Add(step.FunctionInfo.Parameters.[i])
                    ()
                    
                // Check outsiders
                for genP in funInfo.GeneratedParameters do
                    match genP.ParameterType with
                    | OutsiderParameter ->
                        genPars.Add(genP.Name)
                    | _ ->
                        ()
                for genP in funInfo.GeneratedParameters do
                    match genP.ParameterType with
                    | SizeParameter ->
                        genPars.Add(genP.Name)
                    | _ ->
                        ()

                args <- a |> 
                        List.filter(fun (e:Expr) -> e.Type <> typeof<WorkItemInfo>) |> 
                        List.map (fun (e:Expr) -> step.Continue(e)) |>  
                        String.concat ","
*)
            // Check if this is a cast
            if mi.IsGenericMethod && castMethods.ContainsKey(mi.GetGenericMethodDefinition()) then
                Some(returnPrefix + "(" + step.TypeManager.Print(mi.ReturnType) + ")(" + step.Continue(a.[0]) + ")" + returnPostfix)

            // Check if this is a call to IsSome or IsNone for option types            
            else if o.IsNone && mi.IsStatic && mi.Name = "get_IsSome" && a.[0].Type.IsOption then 
                Some(returnPrefix + "(" + step.Continue(a.[0]) + ").IsSome")  
                 
            // Check if this is a call to fst or snd for tuple types         
            else if o.IsNone && mi.IsStatic && mi.Name = "Fst" && a.Length > 0 && FSharpType.IsTuple(a.[0].Type) then 
                Some(returnPrefix + "(" + step.Continue(a.[0]) + ").Item0")     
            else if o.IsNone && mi.IsStatic && mi.Name = "Snd" && a.Length > 0 && FSharpType.IsTuple(a.[0].Type) then 
                Some(returnPrefix + "(" + step.Continue(a.[0]) + ").Item1")     

            // Check Vector operators
            else if (mi.DeclaringType <> null && mi.DeclaringType.GetCustomAttribute<VectorTypeAttribute>() <> null && mi.Name = "vload") then
                // vload function
                let vType = mi.ReturnType
                let vCount = 
                    if vType.GetField("w") <> null then
                        4
                    else if vType.GetField("z") <> null then
                        3
                    else 
                        2
                Some(returnPrefix + "vload" + vCount.ToString() + "(" + args + ")" + returnPostfix)
            // Check Vector convertions
            else if (mi.GetCustomAttribute<VectorTypeConversionAttribute>() <> null) then
                let sat = mi.Name.EndsWith("Sat")
                let rounding = LeafExpressionConverter.EvaluateQuotation(a.[1]) :?> VectorTypeConversionRoundingMode option
                if rounding.IsSome then
                    Some(returnPrefix + "convert_" + step.TypeManager.Print(mi.ReturnType) + (if sat then "_sat" else "") + "_" + rounding.Value.ToString() + "(" + step.Continue(a.[0]) + ")" + returnPostfix)
                else
                    Some(returnPrefix + "convert_" + step.TypeManager.Print(mi.ReturnType) + (if sat then "_sat" else "") + "(" + step.Continue(a.[0]) + ")" + returnPostfix)
            // Pointer arithmetic
            else if (mi.Name = "[]`1.pasum") then
                // Pointer arithmetic sum
                Some(returnPrefix + "(" + step.Continue(a.[0]) + ") + (" + step.Continue(a.[1]) + ")" + returnPostfix)
            else if (mi.Name = "[]`1.pasub") then
                // Pointer arithmetic subtraction
                Some(returnPrefix + "(" + step.Continue(a.[0]) + ") - (" + step.Continue(a.[1]) + ")" + returnPostfix)
            // Check work item info functions
            else if mi.DeclaringType <> null && mi.DeclaringType = typeof<WorkItemInfo> then
                match mi.Name with
                | "GlobalID" ->
                    Some(returnPrefix + "get_global_id(" + args + ")" + returnPostfix)
                | "LocalID" ->
                    Some(returnPrefix + "get_local_id(" + args + ")" + returnPostfix)
                | "GroupID" ->
                    Some(returnPrefix + "get_group_id(" + args + ")" + returnPostfix)
                | "GlobalSize" ->
                    Some(returnPrefix + "get_global_size(" + args + ")" + returnPostfix)
                | "LocalSize" ->
                    Some(returnPrefix + "get_local_size(" + args + ")" + returnPostfix)
                | "GlobalOffset" ->
                    Some(returnPrefix + "get_global_offset(" + args + ")" + returnPostfix)
                | "NumGroups" ->
                    Some(returnPrefix + "get_num_groups(" + args + ")" + returnPostfix)
                | "WorkDim" ->
                    Some(returnPrefix + "get_work_dim()" + returnPostfix)
                | _ ->
                    Some(returnPrefix + "barrier(" + args + ");")
            // Check alternative function
            else 
                if alternativeFunctions.ContainsKey(mi.TryGetGenericMethodDefinition()) then
                    let definition = alternativeFunctions.[mi.TryGetGenericMethodDefinition()]
                    Some(returnPrefix + definition + "(" + args + ");" + returnPostfix)
                else
                    Some(returnPrefix + mi.Name + "(" + args + ")" + returnPostfix)
        | _ ->
            None