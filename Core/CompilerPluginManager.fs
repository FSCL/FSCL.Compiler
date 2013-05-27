namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open System.Runtime
open System.IO

exception CompilerPluginException of string

type CompilerPluginManager(root) =
    let FindPlugins(dll: Assembly) =
        let types = dll.GetTypes()
        let mutable manifestType = None
        let mutable index = 0
        while manifestType.IsNone && index < types.Length do
            let t = types.[index]
            let interf = t.GetInterfaces()
            let isManifest = Array.tryFind(fun (it:Type) -> it = typeof<CompilerPluginManifest>) interf
            if isManifest.IsSome then
                if manifestType.IsSome then
                    raise (CompilerPluginException("A DLL can contain only one Compiler Plugin Manifest [" + dll.FullName + "]"))
                else
                    manifestType <- Some(t)
            else
                index <- index + 1

        if manifestType.IsNone then
            raise (CompilerPluginException("The DLL doesn't contain a manifest [" + dll.FullName + "]"))
        manifestType.Value
          
    member this.Root 
        with get() = root
              
    member this.Load(file: string) =
        let dll = Assembly.LoadFile(file)
        let manifest = FindManifest(dll)
        manifest

    member this.Startup() =
        if Directory.Exists(root) then
            let files = Directory.GetFiles(root, "*.dll")
            for file in files do
                let manifest = this.Load(file)
                ()
        ()
                // Register 

        
        


        
