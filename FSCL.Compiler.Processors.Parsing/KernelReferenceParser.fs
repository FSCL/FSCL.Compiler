namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<StepProcessor("FSCL_KERNEL_REF_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
type KernelReferenceParser() =      
    let rec GetKernelFromName(expr, k:ModuleParsingStep) =                    
        match expr with
        | Patterns.Lambda(v, e) -> 
            GetKernelFromName (e, k)
        | Patterns.Let (v, e1, e2) ->
            GetKernelFromName (e2, k)
        | Patterns.Call (e, mi, a) ->
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                Some(mi, b)
            | _ ->
                None
        | _ ->
            None
        
    interface ModuleParsingProcessor with
        member this.Handle(expr, engine:ModuleParsingStep) =
            if (expr.GetType() = typeof<Expr>) then
                match GetKernelFromName(expr :?> Expr, engine) with
                | Some(mi, b) -> 
                    let km = new KernelModule()
                    km.Source <- new KernelInfo(mi, b)
                    Some(km)
                | _ ->
                    None
            else
                None
            