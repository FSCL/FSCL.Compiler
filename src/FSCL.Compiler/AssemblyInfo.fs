namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSCL.Compiler")>]
[<assembly: AssemblyProductAttribute("FSCL.Compiler")>]
[<assembly: AssemblyDescriptionAttribute("F# to OpenCL compiler")>]
[<assembly: AssemblyVersionAttribute("2.0.1")>]
[<assembly: AssemblyFileVersionAttribute("2.0.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "2.0.1"
