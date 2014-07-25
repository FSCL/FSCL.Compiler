module FSCL.Compiler.ConfigurationTests

open NUnit
open NUnit.Framework
open System.IO
open FSCL.Compiler

[<Test>]
let ``First instantiation of compiler creates configuration folder`` () =
    if Directory.Exists(Compiler.DefaultConfigurationRoot) then
        printf "Compiler configuration root exists: %s\n" (Compiler.DefaultConfigurationRoot)
    else
        printf "Compiler configuration root doesn't exist: %s\n" (Compiler.DefaultConfigurationRoot)
    let compiler = new Compiler()
    Assert.AreEqual(Directory.Exists(Compiler.DefaultConfigurationRoot), true)
