namespace FSCL.Compiler.FunctionTransformation

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Reflection


[<StepProcessor("FSCL_TUPLE_PATTERN_INPUT_TRANSFORMATION_PROCESSOR", 
                "FSCL_FUNCTION_TRANSFORMATION_STEP")>]//,
                //Dependencies = [| "FSCL_ARG_LIFTING_TRANSFORMATION_PROCESSOR" |])>]
(*
 PROCESSING ->
 FROM: let a,b = data in body
 TO: let t = data 
     let a = (fst t)
     let b = (snd t)
     body
*)

type TuplePatternInputProcessor() =
    inherit FunctionTransformationProcessor()
            
    override this.Run((expr, cont, def), en, opts) =
        let engine = en :?> FunctionTransformationStep
        match expr with
        | Patterns.Let (patternInput, 
                        Patterns.Let(v, 
                            Patterns.TupleGet(Patterns.Var(tv), _), _), body) when patternInput.Name = "patternInput" && tv = patternInput ->
            def(expr)
        | _ ->
            def(expr)