namespace FSCL.Compiler.Configuration

open System.Collections.Generic

type ConfigurationUtil() =
    static member FlattenList<'T>(t: obj) =
        List.ofSeq(t :?> IEnumerable<'T>)

