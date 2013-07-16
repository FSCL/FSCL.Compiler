// Compiler user interface
open FSCL.Compiler
// Kernel language library
open FSCL.Compiler.KernelLanguage
    
[<EntryPoint>]
let main argv =
    let compiler = new Compiler()
    compiler.Compile(<@@ AdvancedFeatures.Fmad @@>) |> ignore
    0


