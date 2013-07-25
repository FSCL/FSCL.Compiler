namespace FSCL.Compiler.ModuleCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System

[<StepProcessor("FSCL_MODULE_CODEGEN_PROCESSOR", "FSCL_MODULE_CODEGEN_STEP")>]
type ModuleCodegen() =   
    inherit ModuleCodegenProcessor()
    let PrintStructDefinition(t:Type, engine:ModuleCodegenStep) =
        let mutable print = "struct " + t.Name + " {\n";
        if FSharpType.IsRecord(t) then
            for f in t.GetProperties (BindingFlags.Public ||| BindingFlags.Instance) do
                print <- print + engine.TypeManager.Print(f.PropertyType) + " " + f.Name + ";\n"
        else
            for f in t.GetFields (BindingFlags.Public ||| BindingFlags.Instance) do
                print <- print + engine.TypeManager.Print(f.FieldType) + " " + f.Name + ";\n"
        print <- print + "}\n";
        print

    override this.Run((km, currOut), en) =
        let engine = en :?> ModuleCodegenStep
        let directives = String.concat "\n\n" km.CallGraph.Directives
        let structs = km.CallGraph.GlobalTypes
        let pstructs = String.concat "\n" (List.map (fun (s: Type) -> PrintStructDefinition(s, engine)) structs)
        let functions = String.concat "\n\n" (List.map (fun (f: FunctionInfo) -> f.Code) km.CallGraph.Functions)
        let kernels = String.concat "\n\n" (List.map (fun (f: KernelInfo) -> f.Code) km.CallGraph.Kernels)
        String.concat "\n\n" [directives; pstructs; functions; kernels]
             
            