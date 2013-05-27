// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open FSCL.Compiler
open FSCL.Compiler.Configuration
open FSCL.Compiler.KernelLanguage
// The plugin
open FSCL.Compiler.Plugins.AcceleratedCollections
open System.Reflection

[<ReflectedDefinition>]
let incr x =
    x + 1

[<ReflectedDefinition>]
let reduce (x:float32) (y:float32) =
    x * y

[<EntryPoint>]
let main argv = 
    // Configure compiler
    let conf = new CompilerConfiguration()
    conf.LoadDefaultSteps <- true
    conf.Sources.Add(FileSource("FSCL.Compiler.AcceleratedCollections.dll"))

    let compiler = new Compiler(conf)
    let r = compiler.Compile(<@ Array.reduce(reduce) @>)

    0 // return an integer exit code
