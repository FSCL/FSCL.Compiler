namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("FSCL.Compiler")>]
[<assembly: AssemblyProductAttribute("FSCL.Compiler")>]
[<assembly: AssemblyDescriptionAttribute("F# to OpenCL compiler")>]
[<assembly: AssemblyVersionAttribute("1.5.4")>]
[<assembly: AssemblyFileVersionAttribute("1.5.4")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.5.4"
