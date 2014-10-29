namespace FSCL.Compiler.ModuleCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System
open FSCL.Compiler.Util.ReflectionUtil

[<StepProcessor("FSCL_MODULE_CODEGEN_PROCESSOR", "FSCL_MODULE_CODEGEN_STEP")>]
type ModuleCodegen() =   
    inherit ModuleCodegenProcessor()
    let PrintStructDefinition(t:Type, engine:ModuleCodegenStep) =
        if FSharpType.IsTuple(t) then
            let mutable print = engine.TypeManager.Print(t) + " { \n";
            let mutable idx = 0
            for f in FSharpType.GetTupleElements(t) do
                print <- print + engine.TypeManager.Print(f) + " Item" + idx.ToString() + ";\n"
                idx <- idx + 1
            print <- print + "};\n";
            print
        else if t.IsOption then        
            let mutable print = 
                engine.TypeManager.Print(t) + " {\n" + 
                engine.TypeManager.Print(t.OptionInnerType) + " Value;\n" +
                "int IsSome;\n };\n"
            print
        else
            let mutable print = "struct " + t.Name + " {\n";
            if FSharpType.IsRecord(t) then
                for f in t.GetProperties (BindingFlags.Public ||| BindingFlags.Instance) do
                    print <- print + engine.TypeManager.Print(f.PropertyType) + " " + f.Name + ";\n"
            else
                for f in t.GetFields (BindingFlags.Public ||| BindingFlags.Instance) do
                    print <- print + engine.TypeManager.Print(f.FieldType) + " " + f.Name + ";\n"
            print <- print + "};\n";
            print
        
    //let PrintDefine(name: string, value: string, engine:ModuleCodegenStep) =
      //  "#define " + name + " (" + value + ")"

    override this.Run((km, currOut), en, opts) =
        let engine = en :?> ModuleCodegenStep
        let directives = String.concat "\n" (km.Directives)        
        //let mutable defines = ""
        //for d in km. do
            //defines <- defines + "\n" + PrintDefine(d.Key, d.Value, engine)

        let structs = km.GlobalTypes
        let pstructs = String.concat "\n" (Seq.map (fun (s: Type) -> PrintStructDefinition(s, engine)) structs)
        let functionDecls = String.concat "\n" (Seq.map (fun (f: KeyValuePair<FunctionInfoID, IFunctionInfo>) -> f.Value.SignatureCode + ";") (km.Functions))
        let functionDefs = String.concat "\n" (Seq.map (fun (f: KeyValuePair<FunctionInfoID, IFunctionInfo>) -> f.Value.Code) (km.Functions))
        let kernels = km.Kernel.Code
        String.concat "\n" [directives; pstructs; functionDecls; functionDefs; kernels]