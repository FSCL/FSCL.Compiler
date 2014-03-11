// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open FSCL.Compiler
open FSCL.Compiler.Configuration
open FSCL.Compiler.Language
// The plugin
open FSCL.Compiler.Plugins.AcceleratedCollections
open System.Reflection

[<ReflectedDefinition>]
let incr x =
    x + 1.0f

[<ReflectedDefinition>]
let reduce (x:float32) (y:float32) =
    x * y

[<EntryPoint>]
let main argv = 
    // Configure compiler to load accelerated collections compiler components
    let conf = new PipelineConfiguration(true, [ SourceConfiguration(FileSource("FSCL.Compiler.AcceleratedCollections.dll")) ])
    let compiler = new Compiler(conf)
    
    //#1: reduce with method reference
    let mutable result = compiler.Compile(<@ Array.reduce(reduce) @>)
        
    //#2: reduce with lambda 
    result <- compiler.Compile(<@ Array.reduce(fun x y -> x * y) @>)

    //#3: composition or reduce and map (thanks to the kernel composition capabilities of the FSCL compiler)
    let a = Array.zeroCreate<float32> 128
    result <- compiler.Compile(<@ Array.reduce reduce (Array.map (fun el -> el + 1.0f) a) @>)

    0 // return an integer exit code
