namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSCL.Compiler.Core")>]
[<assembly: AssemblyProductAttribute("FSCL.Compiler")>]
[<assembly: AssemblyDescriptionAttribute("F# to OpenCL compiler")>]
[<assembly: AssemblyVersionAttribute("2.0")>]
[<assembly: AssemblyFileVersionAttribute("2.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "2.0"
