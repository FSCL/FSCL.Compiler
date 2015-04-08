
module KMeans
    open System
    open FSCL.Compiler
    open FSCL.Language

    type Point =
        struct
            val mutable x: float
            val mutable y: float
            new(a, b) = { x = a; y = b }
        end

    [<ReflectedDefinition>]
    let nearestCenter (centers: Point[]) (p: Point) =
        let mutable minIndex = 0
        let mutable minValue = Double.MaxValue
        for curIndex = 0 to centers.Length - 1 do
            let op = centers.[curIndex]
            let curValue = Math.Sqrt(Math.Pow(p.x - op.x, 2.0) + Math.Pow(p.y - op.y, 2.0))
            if curIndex = 0 || minValue > curValue then
                minValue <- curValue
                minIndex <- curIndex
        minIndex
        

    let Run() =
        let rnd = new Random()
        let centers = Array.init 3 (fun i -> new Point((float)i * 1.0, (float)i * 1.0))
        let points = Array.init 1024 (fun i -> new Point(rnd.NextDouble() * 5.0, rnd.NextDouble() * 3.0))

        let kmeans = 
(*
            <@ 
                (points, centers) ||>
                (fun p c -> Array.groupBy(fun a -> nearestCenter c a) p) |>
 *)
            <@ 
                Array.groupBy(fun a -> nearestCenter centers a) points |>
                Array.map (fun (key, data) -> 
                            data |> 
                            Array.reduce(fun (cp) (p) -> 
                                            let r = new Point(cp.x + p.x, cp.y + p.y)
                                            r) |> 
                            (fun p -> new Point(p.x/(double)(Seq.length data), 
                                                p.y/(double)(Seq.length data)))) 
            @>

        let compiler = new Compiler()
        let w = new System.Diagnostics.Stopwatch()
        let mutable result = compiler.Compile(kmeans)
        w.Start()
        for i = 0 to 99 do
            result <- compiler.Compile(kmeans)
        w.Stop()
        Console.WriteLine((double)w.ElapsedMilliseconds/100.0)
        let t = result.GetType()
        ()
