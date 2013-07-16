namespace FSCL.Compiler.Core.Util

open System
open System.Text
open System.Security.Cryptography
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns

module QuotationAnalysis =
    let LiftTupledCallArgs(expr) =
        let rec GetCallArgs(expr, parameters: Var list) =
            match expr with
            | Patterns.Let(v, value, body) ->
                match value with
                | Patterns.TupleGet(te, i) ->
                    GetCallArgs(body, parameters @ [ v ])
                | _ ->
                    (expr, parameters)
            | _ ->
                (expr, parameters)
                
        match expr with
        | Patterns.Lambda(v, e) ->
            if v.Name = "tupledArg" then
                Some(GetCallArgs(e, []))
            else
                None
        | _ ->
            None
            
    let LiftCurriedCallArgs(expr) =
        let rec GetCallArgs(expr, parameters: Var list) =
            match expr with
            | Patterns.Lambda(v, body) ->
                GetCallArgs(body, parameters @ [ v ])
            | _ ->
                (expr, parameters)
                
        match expr with
        | Patterns.Lambda(v, e) ->
            Some(GetCallArgs(e, [v]))
        | _ ->
            None
            
    let IsKernel(mi: MethodInfo) =     
        match mi with
        | DerivedPatterns.MethodWithReflectedDefinition(b) ->
            true
        | _ ->
            false

    let rec GetKernelFromName(expr) =                    
        match expr with
        | Patterns.Lambda(v, e) -> 
            GetKernelFromName (e)
        | Patterns.Let (v, e1, e2) ->
            GetKernelFromName (e2)
        | Patterns.Call (e, mi, a) ->
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                Some(mi, b)
            | _ ->
                None
        | _ ->
            None
            
    let rec GetKernelFromMethodInfo(mi: MethodInfo) =                    
        match mi with
        | DerivedPatterns.MethodWithReflectedDefinition(b) ->
            Some(mi, b)
        | _ ->
            None

    let GetKernelFromLambda(expr) = 
        let mutable body = expr
        let mutable parameters = []
        
        // Extract args from lambda
        match LiftTupledCallArgs(expr) with
        | Some(b, p) ->
            body <- b
            parameters <- p
        | None ->
            match LiftCurriedCallArgs(expr) with
            | Some(b, p) ->
                body <- b
                parameters <- p
            | _ ->
                ()

        if body = expr then
            None
        else
            // Get MD5 o identify the lambda 
            let md5 = MD5.Create()
            let hash = md5.ComputeHash(Encoding.UTF8.GetBytes(expr.ToString()))
            let sb = new StringBuilder("lambda_")
            for i = 0 to hash.Length - 1 do
                sb.Append(hash.[i].ToString("x2")) |> ignore

            // Create appropriate MethodInfo
            let newSignature = new DynamicMethod(sb.ToString(), body.Type, Array.ofList(List.map(fun (v: Var) -> v.Type) parameters))
            for p = 1 to parameters.Length do
                newSignature.DefineParameter(p, ParameterAttributes.None, parameters.[p-1].Name) |> ignore
            Some(newSignature, body)

        

