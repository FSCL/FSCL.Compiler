namespace FSCL.Compiler.Tools

open System
open System.Security.Cryptography
open System.Text

type IDGenerator() =
    static member private GenerateUniqueKey() =
        let maxSize = 8 
        let minSize = 5
        let chars = Array.zeroCreate<char>(62)
        let mutable a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        let chars = a.ToCharArray()
        let mutable size = maxSize
        let mutable data = Array.zeroCreate<byte>(1)
        let crypto = new RNGCryptoServiceProvider()
        crypto.GetNonZeroBytes(data)
        size <- maxSize
        data <- Array.zeroCreate<byte>(size)
        crypto.GetNonZeroBytes(data)
        let result = new StringBuilder(size)
        for b in data do
            result.Append(chars.[(int)b % (chars.Length - 1)]) |> ignore
        result.ToString()

    static member GenerateUniqueID(prefix: string, verification: string list) =
        let finalID = ref (prefix + IDGenerator.GenerateUniqueKey()) 
        while (List.tryFind(fun (s: string) -> s = !finalID) verification).IsSome do
            finalID := prefix + IDGenerator.GenerateUniqueKey()
        !finalID
           
    static member GenerateUniqueID(prefix: string) =
        IDGenerator.GenerateUniqueID(prefix, [])
        
    static member GenerateUniqueID(verification: string list) =
        IDGenerator.GenerateUniqueID("", verification)
        
    static member GenerateUniqueID() =
        IDGenerator.GenerateUniqueID("", [])



