namespace FSCL.Compiler.FunctionPrettyPrinting

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler

[<Step("FSCL_FUNCTION_PRETTY_PRINTING_STEP",
       [| "FSCL_FUNCTION_TRANSFORMATION_STEP";
          "FSCL_FUNCTION_PREPROCESSING_STEP";
          "FSCL_MODULE_PREPROCESSING_STEP";
          "FSCL_MODULE_PARSING_STEP" |])>]
type FunctionPrettyPrintingStep(tm: TypeManager,
                                processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm)
    
    let signatureProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> FunctionSignaturePrettyPrintingProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                                 typeof<FunctionSignaturePrettyPrintingProcessor>.IsAssignableFrom(p.GetType())) processors)
    let bodyProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> FunctionBodyPrettyPrintingProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                            typeof<FunctionBodyPrettyPrintingProcessor>.IsAssignableFrom(p.GetType())) processors)
            
    member val private currentFunction = null with get, set
    
    member this.FunctionInfo 
        with get() =
            this.currentFunction
        and private set(v) =
            this.currentFunction <- v

    member private this.Process(expression: Expr) =
        // At first, check generic processors (for complex constructs)
        let mutable index = 0
        let mutable output = None        
        while (output.IsNone) && (index < bodyProcessors.Length) do
            output <- bodyProcessors.[index].Process(expression, this)
            index <- index + 1
        // If no suitable generic processor, use specific ones
        if (output.IsNone) then
            raise (CompilerException("Unrecognized construct in kernel body " + expression.ToString()))
        output.Value
        
    member private this.Process(mi: MethodInfo) =
        // At first, check generic processors (for complex constructs)
        let mutable index = 0
        let mutable output = None        
        while (output.IsNone) && (index < signatureProcessors.Length) do
            output <- signatureProcessors.[index].Process(mi, this)
            index <- index + 1
        // If no suitable generic processor, use specific ones
        if (output.IsNone) then
            raise (CompilerException("Unrecognized kernel signature " + mi.ToString()))
        output.Value
        
    member this.Continue(expression: Expr) =
        this.Process(expression)
        
    member private this.Process(f:FunctionInfo) =
        this.FunctionInfo <- f
        this.FunctionInfo.PrettyPrinting <- this.Process(this.FunctionInfo.Signature) + "{\n" + this.Process(this.FunctionInfo.Body) + "\n}"
                             
    override this.Run(km: KernelModule) =    
        for kernel in km.Kernels do
            this.Process(kernel)
        for f in km.Functions do
            this.Process(f)
        km
    (*
        let mutable output = ""
        let directives = String.concat "\n" (seq { for i in km.Directives do yield i })
        let kernels = String.concat "\n\n" (seq { for i in km.Kernels do 
                                                    if (this.GlobalData.ContainsKey("KERNEL_INFO")) then
                                                        this.GlobalData.["KERNEL_INFO"] <- i
                                                    else
                                                        this.GlobalData.Add("KERNEL_INFO", i)
                                                    yield (this.Process(i.Signature) + "{\n" + this.Process(i.Body) + "\n}") })
        (km, directives + "\n" + kernels) *)


