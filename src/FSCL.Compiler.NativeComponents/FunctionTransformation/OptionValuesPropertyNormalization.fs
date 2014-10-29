namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System
open FSCL.Compiler.Util.ReflectionUtil

[<StepProcessor("FSCL_OPTION_VALUES_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP",
                Dependencies = [| "FSCL_FUNCTION_RETURN_DISCOVERY_PROCESSOR";
                                  "FSCL_DYNAMIC_ALLOCATION_LIFTING_TRANSFORMATION_PROCESSOR";
                                  "FSCL_CONDITIONAL_ASSIGN_TRANSFORMATION_PROCESSOR";
                                  "FSCL_ARRAY_ACCESS_TRANSFORMATION_PROCESSOR";
                                  "FSCL_REF_VAR_TRANSFORMATION_PROCESSOR";
                                  "FSCL_RETURN_LIFTING_TRANSFORMATION_PROCESSOR" |])>]
type OptionValuesPropertyNormalization() =       
    inherit FunctionTransformationProcessor()

    let NotMethod() =
        match <@ not true @> with
        | Patterns.Call(o, mi, a) ->
            Some(mi)
        | _ ->
            None
            
    let IsSomeMethod(t:Type) =
        t.GetMethod("get_IsSome")

    let rec ChangeIsNonePropertyWithIsSome (expr:Expr, engine:FunctionTransformationStep) =
        match expr with
        | Patterns.Call(o, mi, a) when o.IsNone && mi.IsStatic && mi.Name = "get_IsNone" && a.[0].Type.IsOption ->
            Expr.Call(NotMethod().Value, [ Expr.Call(IsSomeMethod(o.Value.Type), a |> List.map(fun i -> engine.Continue(i)))])
        | _ ->
            engine.Default(expr)
              
    override this.Run(expr, en, opts) =
        let engine = en :?> FunctionTransformationStep
        ChangeIsNonePropertyWithIsSome(expr, engine)

