namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open FSCL.Compiler.Util.ReflectionUtil

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
        if (methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") then
            let mutable code = prefix + engine.TypeManager.Print(v.Type.GetElementType()) + " " + v.Name + "[" + engine.Continue(args.[0]) + "];\n"
            Some(code)
        else if (methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") then
            let mutable code = prefix + engine.TypeManager.Print(v.Type.GetElementType()) + " " + v.Name + "[" + engine.Continue(args.[0]) + "][" + engine.Continue(args.[1]) + "];\n"
            Some(code)
        else if (methodInfo.DeclaringType <> null && methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
            let mutable code = prefix + engine.TypeManager.Print(v.Type.GetElementType()) + " " + v.Name + "[" + engine.Continue(args.[0]) + "][" + engine.Continue(args.[1]) + "][" + engine.Continue(args.[2]) + "];\n"
            Some(code)
        else
            None

    member private this.TryPrintStructOrRecordDecl(v: Var, value: Expr, body: Expr, step: FunctionCodegenStep) =
        match value with
        // Default struct init
        | Patterns.DefaultValue(t) ->
            if t.IsStruct() then
                Some(step.TypeManager.Print(v.Type) + " " + v.Name + ";\n" + step.Continue(body))
            else
                None            
        // Non-Default struct init
        | Patterns.NewObject(constr, args) ->
            if v.Type.IsStruct() then
                let mutable gencode = step.TypeManager.Print(v.Type) + " " + v.Name + " = { ";
                // Gen fields init
                let fields = v.Type.GetFields()
                for i = 0 to fields.Length - 1 do
                    gencode <- gencode + "." + fields.[i].Name + " = " + step.Continue(args.[i])
                    if i < fields.Length - 1 then
                        gencode <- gencode + ", "
                Some(gencode + " };\n" + step.Continue(body))                   
            else
                None
        // Record init
        | Patterns.NewRecord(t, args) ->
            let mutable gencode = step.TypeManager.Print(v.Type) + " " + v.Name + " = { ";
            // Gen fields init
            let fields = FSharpType.GetRecordFields(t)
            for i = 0 to fields.Length - 1 do
                gencode <- gencode + "." + fields.[i].Name + " = " + step.Continue(args.[i])
                if i < fields.Length - 1 then
                    gencode <- gencode + ", "
            Some(gencode + " };\n" + step.Continue(body))  
        | _ ->
            None

    override this.Run(expr, s, opts) =
        let step = s :?> FunctionCodegenStep
        match expr with
        | Patterns.Let(v, value, body) ->
            // Try print struct
            let structCode = this.TryPrintStructOrRecordDecl(v, value, body, step)
            if structCode.IsSome then
                structCode
            else
                match value with            
                // Local declaration
                | DerivedPatterns.SpecificCall <@ local @> (o, tl, a) ->
                    if a.[0].Type.IsArray then
                        // Local array
                        match a.[0] with 
                        | Patterns.Call(_, methodInfo, args) ->
                            Some(this.PrintArray(step, methodInfo, v, args, true).Value + step.Continue(body))                        
                        | _ ->
                            None
                    else
                        let structCode = this.TryPrintStructOrRecordDecl(v, a.[0], body, step)
                        if structCode.IsSome then
                            Some("local " + structCode.Value)
                        else
                            // Local primitive type
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