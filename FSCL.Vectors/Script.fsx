// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.
open Microsoft.FSharp.Quotations


module t =
    let arr = [| 1;2;3 |]
    
    [<ReflectedDefinition>]
    let setfirst (a:int[]) =
        a.[0] <- 5

    let test() =
        let quot = <@ setfirst arr @>
        printf "Quotation: %s\n\n" (quot.ToString())
        match quot with
        | Patterns.Call(o, mi, ora) ->
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                printf "Body: %s\n\n" (b.ToString())
                match b with
                | Patterns.Lambda(a, body) ->
                    match body with
                    | Patterns.Call(o, mi, args) ->
                        match ora.[0] with
                        | Patterns.Value(v,t) ->
                            let c = v :?> int[]
                            c.[0] <- 5
                        | _ -> ()
                    | _ -> ()
                | _ -> ()
            | _ -> ()
        | _ -> ()
        printf "First: %d\n" arr.[0]

t.test()

// Define your library scripting code here

