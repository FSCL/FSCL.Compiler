namespace FSCL.Compiler.ModulePrettyPrinting

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System

[<StepProcessor("FSCL_MODULE_PRETTY_PRINTING_PROCESSOR", "FSCL_MODULE_PRETTY_PRINTING_STEP")>]
type ModulePrettyPrinter() =      
    let PrintStructDefinition(t:Type, engine:ModulePrettyPrintingStep) =
        let mutable print = "struct " + t.Name + " {\n";
        if FSharpType.IsRecord(t) then
            for f in t.GetProperties (BindingFlags.Public ||| BindingFlags.Instance) do
                print <- print + engine.TypeManager.Print(f.PropertyType) + " " + f.Name + ";\n"
        else
            for f in t.GetFields (BindingFlags.Public ||| BindingFlags.Instance) do
                print <- print + engine.TypeManager.Print(f.FieldType) + " " + f.Name + ";\n"
        print <- print + "}\n";
        print

    interface ModulePrettyPrintingProcessor with
        member this.Process((km, currOut), en) =
            let engine = en :?> ModulePrettyPrintingStep
            let directives = String.concat "\n\n" km.Directives
            let structs = km.GlobalTypes
            let pstructs = String.concat "\n" (List.map (fun (s: Type) -> PrintStructDefinition(s, engine)) structs)
            let functions = String.concat "\n\n" (List.map (fun (f: FunctionInfo) -> f.PrettyPrinting) km.Functions) 
            let kernels = String.concat "\n\n" (List.map (fun (f: KernelInfo) -> f.PrettyPrinting) km.Kernels)
            String.concat "\n\n" [directives; pstructs; functions; kernels]
             
            