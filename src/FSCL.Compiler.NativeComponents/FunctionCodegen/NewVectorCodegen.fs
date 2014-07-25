namespace FSCL.Compiler.FunctionCodegen

open System
open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL

[<StepProcessor("FSCL_NEW_VECTOR_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
                                  
///
///<summary>
///The function codegen step whose behavior is to generate the target representation for vector type instantiation like float4(0) and int2(0)
///</summary>
///  
type NewVectorCodegen() =
    inherit FunctionBodyCodegenProcessor()

    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionCodegenStep
        match expr with
        | Patterns.NewObject(constr, args) ->
            let attrs = expr.Type.GetCustomAttributes(typeof<VectorTypeAttribute>, true)
            if attrs.Length > 0 then
                // This is a vector type
                let args = String.concat ", " (List.map (fun (e:Expr) -> engine.Continue(e)) args)
                Some("(" + engine.TypeManager.Print(expr.Type) + ")(" + args + ")")
            else
                None
        | _ ->
            None