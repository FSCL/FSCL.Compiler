namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_DECLARATION_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP",
                Dependencies =[| "FSCL_FOR_RANGE_CODEGEN_PROCESSOR" |])>]
type DeclarationCodegen() =   
    inherit FunctionBodyCodegenProcessor()

    member private this.PrintArray(engine: FunctionCodegenStep,
                                   methodInfo: MethodInfo, 
                                   v: Var, 
                                   args: Expr list, 
                                   isLocal: bool) =
        let prefix = if isLocal then "local " else ""
        if (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") then
            let mutable code = prefix + engine.TypeManager.Print(v.Type.GetElementType()) + " " + v.Name + "[" + engine.Continue(args.[0]) + "];\n"
            Some(code)
        else if (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") then
            let mutable code = prefix + engine.TypeManager.Print(v.Type.GetElementType()) + " " + v.Name + "[" + engine.Continue(args.[0]) + "][" + engine.Continue(args.[1]) + "];\n"
            Some(code)
        else if (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
            let mutable code = prefix + engine.TypeManager.Print(v.Type.GetElementType()) + " " + v.Name + "[" + engine.Continue(args.[0]) + "][" + engine.Continue(args.[1]) + "][" + engine.Continue(args.[2]) + "];\n"
            Some(code)
        else
            None

    override this.Run(expr, s, opts) =
        let step = s :?> FunctionCodegenStep
        match expr with
        | Patterns.Let(v, value, body) ->
            match value with
            // Local declaration
            | DerivedPatterns.SpecificCall <@ local @> (o, tl, a) ->
                if a.Length > 0 then
                    // Local array
                    match a.[0] with 
                    | Patterns.Call(_, methodInfo, args) ->
                        Some(this.PrintArray(step, methodInfo, v, args, true).Value + step.Continue(body))                        
                    | _ ->
                        None
                else
                    // Local scalar
                    Some("local " + step.TypeManager.Print(v.Type) + " " + v.Name + ";\n" + step.Continue(body))
            | _ ->
                // Private array maybe
                match value with
                | Patterns.Call(_, methodInfo, args) ->
                    match this.PrintArray(step, methodInfo, v, args, false) with
                    | Some(c) ->
                        // Private array alloc, return
                        Some(c + step.Continue(body))
                    | None ->
                        // Not array alloc call
                        Some(step.TypeManager.Print(v.Type) + " " + v.Name + " = " + step.Continue(value) + ";\n" + step.Continue(body))
                | _ ->
                    // Normal declaration
                    Some(step.TypeManager.Print(v.Type) + " " + v.Name + " = " + step.Continue(value) + ";\n" + step.Continue(body))
        | _ ->
            None