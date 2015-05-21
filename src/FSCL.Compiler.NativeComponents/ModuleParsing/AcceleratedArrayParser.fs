namespace FSCL.Compiler.AcceleratedCollections

open FSCL.Compiler
open FSCL.Language
open FSCL.Compiler.Util
open FSCL.Compiler.ModuleParsing
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System
open AcceleratedCollectionUtil
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

[<StepProcessor("FSCL_ACCELERATED_ARRAY_MODULE_PARSING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP")>]
[<UseMetadata(typeof<DeviceTypeAttribute>, typeof<DeviceTypeMetadataComparer>)>] 
type AcceleratedArrayParser() = 
    inherit ModuleParsingProcessor()
    
    // The List module type        
    static let validCollectionTypes = 
        [ FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.DeclaringType).Value;
          FilterCall(<@ Array.groupBy @>, fun(e, mi, a) -> mi.DeclaringType).Value;
          FilterCall(<@ Array2D.map @>, fun(e, mi, a) -> mi.DeclaringType).Value ]

    // The set of List functions handled by the parser
    let handlers = new Dictionary<MethodInfo, IAcceleratedCollectionHandler>()
    do 
        handlers.Add(FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMapHandler())
        handlers.Add(FilterCall(<@ Array.mapi @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMapHandler())
        handlers.Add(FilterCall(<@ Array.map2 @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMap2Handler())
        handlers.Add(FilterCall(<@ Array.mapi2 @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMap2Handler())
        handlers.Add(FilterCall(<@ Array.reduce @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayReduceHandler())
        handlers.Add(FilterCall(<@ Array.sum [| 0 |] @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayReduceHandler())
        handlers.Add(FilterCall(<@ Array.rev @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayReverseHandler())
        handlers.Add(FilterCall(<@ Array.scan @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayReverseHandler())
        handlers.Add(FilterCall(<@ Array.sort @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArraySortHandler())
        handlers.Add(FilterCall(<@ Array.sortBy (fun i -> i) @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArraySortHandler()) 
        handlers.Add(FilterCall(<@ Array.groupBy (fun i -> i) @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayGroupByHandler())
        handlers.Add(FilterCall(<@ Array.fold (fun i j -> i) @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayFoldHandler())  

        handlers.Add(FilterCall(<@ Array2D.map (fun i j -> i) @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArray2DMapHandler())  

          
    override this.Run((o, envBuilder), s, opts) =
        let step = s :?> ModuleParsingStep
        if o :? Expr then            
            // Lift and get potential kernel attributes
            let expr, kMeta, rMeta = ParseKernelMetadata(o :?> Expr)

            // Filter out potential Lambda/Let 
            // This generally happens in Array.reduce(fun a b -> a + b) where the AST is Let(reduction, lambda, Array.reduce(reduction))
            match ExtractCall(AcceleratedCollectionUtil.LiftLambdaDeclarationAhead(expr)) with
            | Some(o, methodInfo, args, _, _) -> 
                if (validCollectionTypes |> List.tryFind(fun i -> i = methodInfo.DeclaringType)).IsSome then
                    if (handlers.ContainsKey(methodInfo.GetGenericMethodDefinition())) then
                        // Clean arguments of potential parameter attributes
                        let pmeta = new List<ParamMetaCollection>()
                        let cleanArgs = new List<Expr>()
                        for i = 0 to args.Length - 1 do
                            match ParseParameterMetadata(args.[i]) with
                            | e, meta ->
                                pmeta.Add(meta)
                                cleanArgs.Add(e)
                            
                        // Filter and finalize metadata
                        let metaFilterInfo = new Dictionary<string, obj>()
                        metaFilterInfo.Add("AcceleratedCollection", methodInfo.GetGenericMethodDefinition().Name)
                        let finalMeta = step.ProcessMeta(kMeta, rMeta, pmeta, metaFilterInfo, opts)

                        // Run the appropriate handler
                        handlers.[methodInfo.GetGenericMethodDefinition()].Process(methodInfo, List.ofSeq cleanArgs, expr, finalMeta, step, envBuilder, opts)
                    else
                        None
                else
                    None
            | _ ->
                None
        else
            None

             
            