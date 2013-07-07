namespace FSCL.Compiler.Plugins.AcceleratedCollections

open FSCL.Compiler
open FSCL.Compiler.ModuleParsing
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System
open AcceleratedCollectionUtil

[<StepProcessor("FSCL_ACCELERATED_ARRAY_MODULE_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>] 
type AcceleratedArrayParser() = 
    inherit ModuleParsingProcessor()

    // The List module type        
    let listModuleType = FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.DeclaringType).Value

    // The set of List functions handled by the parser
    let handlers = new Dictionary<MethodInfo, IAcceleratedCollectionHandler>()
    do 
        handlers.Add(FilterCall(<@ Array.map @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayMapHandler())
        handlers.Add(FilterCall(<@ Array.reduce @>, fun(e, mi, a) -> mi.GetGenericMethodDefinition()).Value, new AcceleratedArrayReduceHandler())

    // The dynamic module that hosts the generated kernels
    let assemblyName = "FSCL.Compiler.Plugins.AcceleratedCollections.AcceleratedArray";
    let assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
    let moduleBuilder = assemblyBuilder.DefineDynamicModule("AcceleratedArrayModule");
    
    override this.Run(o, en) =
        let engine = en :?> ModuleParsingStep
        match FilterCall(o :?> Expr, fun a -> a) with
        | Some(item, methodInfo, args) -> 
            if methodInfo.DeclaringType = listModuleType then
                if (handlers.ContainsKey(methodInfo.GetGenericMethodDefinition())) then
                    handlers.[methodInfo.GetGenericMethodDefinition()].Process(methodInfo, args, moduleBuilder)
                else
                    None
            else
                None
        | _ ->
            None
             
            