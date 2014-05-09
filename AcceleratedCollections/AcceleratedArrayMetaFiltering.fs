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

[<StepProcessor("FSCL_ACCELERATED_ARRAY_META_FILTERING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP")>]
[<UseMetadata(typeof<DeviceTypeAttribute>, typeof<DeviceTypeMetadataComparer>)>] 
type AcceleratedArrayMetaFiltering() = 
    inherit MetadataFinalizerProcessor()
    
    override this.Run((kmeta, rmeta, pmeta, info), s, opts) =
        let step = s :?> ModuleParsingStep
        if info <> null && info.ContainsKey("AcceleratedCollection") then
            // Remove the meta for the first param cause it is a lambda/function reference
            pmeta.RemoveAt(0)
            
            // Prepare params meta
            match info.["AcceleratedCollection"] :?> string with
            | "Map"
            | "Map2" ->
                // Add meta for output parameter
                pmeta.Add(rmeta)
            | "Reduce" ->
                // Check device target
                let targetType = kmeta.Get<DeviceTypeAttribute>()
                // If gpu                
                if targetType.Type = DeviceType.Gpu then                    
                    let localParameterMeta = new ParamMetaCollection()
                    localParameterMeta.Add(new AddressSpaceAttribute(AddressSpace.Local))
                    pmeta.Add(localParameterMeta)
                    pmeta.Add(rmeta)
                // If cpu
                else
                    pmeta.Add(new ParamMetaCollection())
                    pmeta.Add(rmeta)
                // Force output to be read-write (cause it will be switched)     
                pmeta.[pmeta.Count - 1].Add(new MemoryFlagsAttribute(MemoryFlags.ReadWrite))                
            | _ ->
                ()
        (kmeta, rmeta, pmeta)

                

             
            