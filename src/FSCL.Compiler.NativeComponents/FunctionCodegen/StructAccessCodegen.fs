namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open System.Reflection

[<StepProcessor("FSCL_STRUCT_ACCESS_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>] 
type StructAccessCodegen() =                 
    inherit FunctionBodyCodegenProcessor()
    override this.Run((expr, cont), st, opts) =
        let step = st :?> FunctionCodegenStep
        match expr with          
        // Records  
        | Patterns.PropertyGet(e, propertyInfo, args) ->
            if e.IsSome then
                let t = e.Value.Type
                if (FSharpType.IsRecord(t)) then
                    Some(cont(e.Value) + "." + propertyInfo.Name)
                else
                    None
            else 
                None
        | Patterns.PropertySet(e, propertyInfo, args, body) ->                
            if e.IsSome then
                let t = e.Value.Type
                if (FSharpType.IsRecord(t)) then
                    Some(cont(e.Value) + "." + propertyInfo.Name + " = " + cont(body) + ";\n")
                else
                    None
            else 
                None
        // Structs
        | Patterns.FieldGet(e, fieldInfo) ->
            if e.IsSome then
                let t = e.Value.Type
                if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then
                    Some(cont(e.Value) + "." + fieldInfo.Name)
                else
                    None
            else 
                None
        | Patterns.FieldSet(e, fieldInfo, args) ->                
            if e.IsSome then
                let t = e.Value.Type
                if (t.IsValueType && (not t.IsPrimitive) && (not t.IsEnum)) then
                    Some(cont(e.Value) + "." + fieldInfo.Name + " = " + cont(args) + ";\n")
                else
                    None
            else 
                None
        | _ ->
            None