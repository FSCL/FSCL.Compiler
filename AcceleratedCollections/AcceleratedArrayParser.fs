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
open Cloo

[<StepProcessor("FSCL_ACCELERATED_ARRAY_MODULE_PARSING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP")>] 
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
            let expr, kernelAttrs = QuotationAnalysis.ParseDynamicKernelMetadataFunctions(o :?> Expr)
            // Filter out potential Lambda/Let (if the function is referenced, not applied)
            match QuotationAnalysis.ParseCall(expr) with
            | Some(o, methodInfo, args) -> 
                if methodInfo.DeclaringType = listModuleType then
                    if (handlers.ContainsKey(methodInfo.GetGenericMethodDefinition())) then
                        // Clean arguments of potential parameter attributes
                        let cleanArgs, paramAttrs = args |> List.map (fun (pe:Expr) -> QuotationAnalysis.ParseDynamicParameterMetadataFunctions(pe)) |> List.unzip
                        // Run the appropriate handler
                        handlers.[methodInfo.GetGenericMethodDefinition()].Process(methodInfo, cleanArgs, expr, kernelAttrs, paramAttrs, step)
                    else
                        None
                else
                    None
            | _ ->
                None
        else
            None
            
    override this.BehaveDifferentlyWithKernelMetadata 
        with get() =
            Some(fun(meta1, meta2) ->
                    let dev1 = 
                        if meta1.ContainsKey(typeof<DeviceAttribute>) then
                            meta1.[typeof<DeviceAttribute>] :?> DeviceAttribute
                        else
                            new DeviceAttribute(0, 0)                    
                    let dev2 = 
                        if meta2.ContainsKey(typeof<DeviceAttribute>) then
                            meta2.[typeof<DeviceAttribute>] :?> DeviceAttribute
                        else
                            new DeviceAttribute(0, 0)
                    let plat1 = ComputePlatform.Platforms.[dev1.Platform]
                    let plat2 = ComputePlatform.Platforms.[dev2.Platform]
                    let dev1Type = plat1.Devices.[dev1.Device].Type
                    let dev2Type = plat2.Devices.[dev2.Device].Type
                    dev1Type <> dev2Type)


             
            