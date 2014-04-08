namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.Language
open FSCL.Compiler.Util
open FSCL.Compiler.ModuleParsing
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System
open AcceleratedCollectionUtil

type DeviceTypeMetadataComparer() =
    inherit MetadataComparer() with
    override this.MetaEquals(meta1, meta2) =
       match meta1, meta2 with
       | :? DeviceTypeAttribute, :? DeviceTypeAttribute ->
            let dev1, dev2 = meta1 :?> DeviceTypeAttribute, meta2 :?> DeviceTypeAttribute
            match dev1.Type, dev2.Type with
            | DeviceType.Cpu, DeviceType.Gpu 
            | DeviceType.Gpu, DeviceType.Cpu ->
                false
            | _ ->
                true
       | _ ->
            true

[<StepProcessor("FSCL_ACCELERATED_ARRAY_MODULE_PARSING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP")>]
[<UseMetadata(typeof<DeviceTypeAttribute>, typeof<DeviceTypeMetadataComparer>)>] 
type AcceleratedArrayParser() = 
    inherit ModuleParsingProcessor()
    
    // The List module type        
    let listModuleType = FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.DeclaringType).Value

    // The set of List functions handled by the parser
    let handlers = new Dictionary<MethodInfo, IAcceleratedCollectionHandler>()
    do 
        handlers.Add(FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMapHandler())
        handlers.Add(FilterCall(<@ Array.map2 @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMap2Handler())
        handlers.Add(FilterCall(<@ Array.reduce @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayReduceHandler())
            
    override this.Run(o, s, opts) =
        let step = s :?> ModuleParsingStep
        if o :? Expr then
            // Lift and get potential kernel attributes
            let expr, kMeta, rMeta = QuotationAnalysis.ParseKernelMetadata(o :?> Expr)

            // Filter out potential Lambda/Let (if the function is referenced, not applied)
            match QuotationAnalysis.ParseCall(expr) with
            | Some(o, methodInfo, args) -> 
                if methodInfo.DeclaringType = listModuleType then
                    if (handlers.ContainsKey(methodInfo.GetGenericMethodDefinition())) then
                        // Clean arguments of potential parameter attributes
                        let pmeta = new List<ParamMetaCollection>()
                        let cleanArgs = new List<Expr>()
                        for i = 0 to args.Length - 1 do
                            match QuotationAnalysis.ParseParameterMetadata(args.[i]) with
                            | e, meta ->
                                pmeta.Add(meta)
                                cleanArgs.Add(e)
                            
                        // Filter and finalize metadata
                        let metaFilterInfo = new Dictionary<string, obj>()
                        metaFilterInfo.Add("AcceleratedCollection", methodInfo.GetGenericMethodDefinition().Name)
                        let finalMeta = step.ProcessMeta(kMeta, rMeta, pmeta, metaFilterInfo)

                        // Run the appropriate handler
                        handlers.[methodInfo.GetGenericMethodDefinition()].Process(methodInfo, List.ofSeq cleanArgs, expr, finalMeta, step)
                    else
                        None
                else
                    None
            | _ ->
                None
        else
            None

             
            