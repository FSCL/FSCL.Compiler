namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_STRUCT_DEFINITON_PRETTY_PRINTING_PROCESSOR", "FSCL_MODULE_PRETTY_PRINTING_STEP",
                "FSCL_MODULE_PRETTY_PRINTING_PROCESSOR" // Replace
                )>] 
type StructDefinitionPrinter() =      
    let PrintStructDefinition(t:Type, engine:ModulePrettyPrintingStep) =
        let mutable print = "struct " + t.Name + " {\n";
        for f in t.GetProperties(BindingFlags.Public ||| BindingFlags.Instance) do
            print <- print + engine.TypeManager.Print(f.PropertyType) + " " + f.Name + ";\n"
        print <- print + "}\n";
        print

    interface ModulePrettyPrintingProcessor with
        member this.Handle(km, currOut, engine:ModulePrettyPrintingStep) =
            let directives = String.concat "\n\n" km.Directives
            let structs = km.CustomInfo.["STRUCT_TYPE_DEFINITIONS"] :?> Type list
            let pstructs = String.concat "\n" (List.map (fun (s: Type) -> PrintStructDefinition(s, engine)) structs)
            let functions = String.concat "\n\n" (List.map (fun (f: FunctionInfo) -> f.PrettyPrinting) km.Functions) 
            let kernels = String.concat "\n\n" (List.map (fun (f: KernelInfo) -> f.PrettyPrinting) km.Kernels)
            String.concat "\n\n" [directives; pstructs; functions; kernels]
             
            