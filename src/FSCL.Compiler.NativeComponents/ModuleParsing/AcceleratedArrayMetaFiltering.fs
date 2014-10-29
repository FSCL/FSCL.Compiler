namespace FSCL.Compiler.AcceleratedCollections

open FSCL
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

[<StepProcessor("FSCL_ACCELERATED_ARRAY_META_FILTERING_PROCESSOR", 
                "FSCL_MODULE_PARSING_STEP")>]
[<UseMetadata(typeof<DeviceTypeAttribute>, typeof<DeviceTypeMetadataComparer>)>] 
type AcceleratedArrayMetaFiltering() = 
    inherit MetadataFinalizerProcessor()
    
    override this.Run((kmeta, rmeta, pmeta, info), s, opts) =
        let step = s :?> ModuleParsingStep
        if info <> null && info.ContainsKey("AcceleratedCollection") then            
            // Prepare params meta
            match info.["AcceleratedCollection"] :?> string with
            | "Map"
            | "Map2"
            | "MapIndexed"
            | "MapIndexed2" ->
                // Remove the meta for the first param cause it is a lambda/function reference
                pmeta.RemoveAt(0)
                // Add meta for output parameter
                pmeta.Add(rmeta)
            | "Reverse" ->
                pmeta.Add(rmeta)
            | "Sum" ->
                // Check device target
                let targetType = kmeta.Get<DeviceTypeAttribute>()
                // If gpu                
                if targetType.Type = DeviceType.Gpu then                    
                    let localParameterMeta = new ParamMetaCollection()
                    localParameterMeta.Add(new AddressSpaceAttribute(AddressSpace.Local))
                    pmeta.Add(localParameterMeta)
                    rmeta.Add(new TransferModeAttribute(TransferMode.NoTransfer, TransferMode.NoTransfer))
                    pmeta.Add(rmeta)
                // If cpu
                else
                    pmeta.Add(new ParamMetaCollection())
                    rmeta.Add(new TransferModeAttribute(TransferMode.NoTransfer, TransferMode.NoTransfer))
                    pmeta.Add(rmeta)
                // Force output to be read-write (cause it will be switched) 
                pmeta.[pmeta.Count - 1].AddOrSet(new MemoryFlagsAttribute(MemoryFlags.ReadWrite))
            | "Reduce" ->
                // Remove the meta for the first param cause it is a lambda/function reference
                pmeta.RemoveAt(0)
                // Check device target
                let targetType = kmeta.Get<DeviceTypeAttribute>()
                // If gpu                
                if targetType.Type = DeviceType.Gpu then                    
                    let localParameterMeta = new ParamMetaCollection()
                    localParameterMeta.Add(new AddressSpaceAttribute(AddressSpace.Local))
                    pmeta.Add(localParameterMeta)
                    rmeta.Add(new TransferModeAttribute(TransferMode.NoTransfer, TransferMode.NoTransfer))
                    pmeta.Add(rmeta)
                // If cpu
                else
                    rmeta.Add(new TransferModeAttribute(TransferMode.NoTransfer, TransferMode.NoTransfer))
                    pmeta.Add(rmeta)
                    pmeta.Add(new ParamMetaCollection())
                // Force output to be read-write (cause it will be switched) 
                pmeta.[pmeta.Count - 1].AddOrSet(new MemoryFlagsAttribute(MemoryFlags.ReadWrite))
            | _ ->
                ()
        (kmeta, rmeta, pmeta)

                

             
            