namespace MetricBase

open Microsoft.FSharp.Quotations
open System.IO

type MetricBase() =
    let mutable (children:MetricBase list) = []
    member this.SubMetric with get() = children and set v = children <- v
       
[<AbstractClass>]
type Metric<'T,'U,'Z,'CDATA>() =
    inherit MetricBase()
    member val DumpFolder:string option = None with get, set 
    member this.Dump(name, content) =        
        if this.DumpFolder.IsSome then
            if not (Directory.Exists(this.DumpFolder.Value)) then
                Directory.CreateDirectory(this.DumpFolder.Value) |> ignore

            let fileName = this.DumpFolder.Value + "\\" + name + ".csv"  
            File.WriteAllText(fileName, content)
    abstract member Evaluate: 'T * Expr -> 'U
    abstract member Instantiate: 'T * 'U * Expr * 'CDATA -> 'Z
    
[<AbstractClass>]
type RelativeMetric<'T,'U,'Z,'W,'CDATA>() =
    inherit Metric<'U,'Z,'W,'CDATA>()
    abstract member Profile: 'T list -> 'U
    
[<AbstractClass>]
type AbsoluteMetric<'T,'U,'Z,'W,'CDATA>() =
    inherit Metric<'U,'Z,'W,'CDATA>()
    abstract member Profile: 'T -> 'U       

