namespace FSCL.Compiler.ModuleParsing

open System
open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

[<assembly:DefaultComponentAssembly>]
do()

[<StepProcessor("FSCL_KERNEL_REF_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
type KernelReferenceParser() =      
    inherit ModuleParsingProcessor()

    let rec GetKernelFromName(expr, k:ModuleParsingStep) =                    
        match expr with
        | Patterns.Lambda(v, e) -> 
            Console.Write("Lambda\n")
            GetKernelFromName (e, k)
        | Patterns.Let (v, e1, e2) ->
            Console.Write("Let\n")
            GetKernelFromName (e2, k)
        | Patterns.Call (e, mi, a) ->
            Console.Write("Call\n")
            match mi with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                Console.Write("Reflected definition\n")
                Some(mi, b)
            | _ ->
                Console.Write("Normal call\n")
                raise (CompilerException("The engine is not able to parse a kernel inside the expression [" + expr.ToString() + "]"))
                None
        | _ ->
            raise (CompilerException("The engine is not able to parse a kernel inside the expression [" + expr.ToString() + "]"))
            None
        
    override this.Run(expr, en) =
        let engine = en :?> ModuleParsingStep
        if (typeof<Expr>.IsAssignableFrom(expr.GetType())) then
            match GetKernelFromName(expr :?> Expr, engine) with
            | Some(mi, b) -> 
                let km = new KernelModule()
                km.Source <- new KernelInfo(mi, b)
                Some(km)
            | _ ->
                None
        else
            None
            