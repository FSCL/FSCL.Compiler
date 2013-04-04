// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open SimpleAlgorithms
open AdvancedFeatures
open Microsoft.FSharp.Collections
open Microsoft.FSharp.Quotations
    
[<EntryPoint>]
let main argv =
    let compiler = new Compiler()
    let result = compiler.Compile(<@@ VectorAddRecord @@>)
    0
