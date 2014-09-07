namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSCL.Compiler")>]
[<assembly: AssemblyProductAttribute("FSCL.Compiler")>]
[<assembly: AssemblyDescriptionAttribute("F# to OpenCL compiler")>]
[<assembly: AssemblyVersionAttribute("1.3.7")>]
[<assembly: AssemblyFileVersionAttribute("1.3.7")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.3.7"
