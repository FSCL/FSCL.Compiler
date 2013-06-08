namespace FSCL.Compiler

module internal Util =
    let rec ins v i l =
        match i, l with
        | 0, xs -> v::xs
        | i, x::xs -> x::ins v (i - 1) xs
        | i, [] -> failwith "index out of range"

    let rec rem i l =
        match i, l with
        | 0, x::xs -> xs
        | i, x::xs -> x::rem (i - 1) xs
        | i, [] -> failwith "index out of range"