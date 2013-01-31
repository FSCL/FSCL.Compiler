namespace MetricBase.Tools

open Microsoft.FSharp.Quotations
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open System.Diagnostics

module KernelTools =
    // Extracts a method with reflected definition from a quotation containing its name
    let rec ExtractKernelDefinition (expr) =
        match expr with
        | Patterns.Lambda(v, e) -> 
            ExtractKernelDefinition e
        | Patterns.Let (v, e1, e2) ->
            ExtractKernelDefinition (e2)
        | Patterns.Call (e, i, a) ->
            match i with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                i
            | _ ->
                raise (MetricBase.Exceptions.MalformedKernelError("A kernel function must be marked with ReflectedDefinition attribute"))
        | _-> 
            raise (MetricBase.Exceptions.MalformedKernelError("Cannot find a kernel function inside the expression"))

    let rec ExtractKernelInvocation (expr) =
        let getArgs exp =
            match exp with
            | Patterns.Call (e, i, a) ->
                match i with
                | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                    a
                | _ ->
                    raise (MetricBase.Exceptions.MalformedKernelError("A kernel invocation must provide a function marked with ReflectedDefinition attribute"))
            | _-> 
                raise (MetricBase.Exceptions.MalformedKernelError("Cannot find a kernel function invocation inside the expression"))

        let args = getArgs expr
        let kernel = ExtractKernelDefinition expr 
        (kernel, args)

