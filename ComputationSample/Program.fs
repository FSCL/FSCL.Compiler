// Compiler user interface
open FSCL.Compiler
// Kernel language library
open FSCL
    
[<EntryPoint>]
let main argv =
    let compiler = new Compiler()
    compiler.Compile(<@@ AdvancedFeatures.Fmad @@>) |> ignore

    // Test accelerated collections
    let a = Array.create 128 2.0f
    let b = Array.create 128 3.0f
    // Map
    let result = compiler.Compile(<@ Array.map (fun a -> a + 1.0f) a @>) 
    // Mapi
    let result = compiler.Compile(<@ Array.mapi (fun i a -> a + float32(i)) a @>) 
    // Map2
    let result = compiler.Compile(<@ Array.map2 (fun a b -> a + b) a b @>) 
    // Mapi2
    let result = compiler.Compile(<@ Array.mapi2 (fun i a b -> a + b + float32(i)) a b @>) 
    // Reduce
    let result = compiler.Compile(<@ Array.reduce (fun a b -> a + b) a @>) 
    // Sum
    let result = compiler.Compile(<@ Array.sum a @>) 
    // Rev
    let result = compiler.Compile(<@ Array.rev a @>) 

    0


