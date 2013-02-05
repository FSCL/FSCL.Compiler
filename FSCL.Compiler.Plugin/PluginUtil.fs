namespace FSCL.Compiler.Plugin

open System.Collections.Generic

type PluginUtil() =
    static member FlattenList<'T>(t: obj) =
        List.ofSeq(t :?> IEnumerable<'T>)

