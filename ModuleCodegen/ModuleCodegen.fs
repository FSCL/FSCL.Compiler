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
        
    let PrintDefine(name: string, value: string, engine:ModuleCodegenStep) =
        "#define " + name + " (" + value + ")"

    override this.Run((km, currOut), en, opts) =
        let engine = en :?> ModuleCodegenStep
        let directives = String.concat "\n\n" (km.Directives)        
        let mutable defines = ""
        for d in km.StaticConstantDefinesCode do
            defines <- defines + "\n" + PrintDefine(d.Key, d.Value, engine)

        let structs = km.GlobalTypes
        let pstructs = String.concat "\n" (Seq.map (fun (s: Type) -> PrintStructDefinition(s, engine)) structs)
        let functions = String.concat "\n\n" (Seq.map (fun (f: KeyValuePair<FunctionInfoID, IFunctionInfo>) -> f.Value.Code) (km.Functions))
        let kernels = km.Kernel.Code
        String.concat "\n\n" [directives; defines; pstructs; functions; kernels]