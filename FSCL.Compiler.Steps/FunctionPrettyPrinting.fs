namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations

type IFunctionPrettyPrintingProcessor =
    interface
    end

type FunctionSignaturePrettyPrintingProcessor =    
    inherit IFunctionPrettyPrintingProcessor
    abstract member Handle: MethodInfo * FunctionPrettyPrintingStep -> String option

and FunctionBodyPrettyPrintingProcessor =  
    inherit IFunctionPrettyPrintingProcessor  
    abstract member Handle: Expr * FunctionPrettyPrintingStep -> String option

and FunctionPrettyPrintingStep(pipeline: ICompilerPipeline,
                               processors: IFunctionPrettyPrintingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(pipeline)
    
    let signatureProcessors = List.map(fun (p:IFunctionPrettyPrintingProcessor) ->
                                p :?> FunctionSignaturePrettyPrintingProcessor) (List.filter (fun (p:IFunctionPrettyPrintingProcessor) -> 
                                                                                p.GetType() = typeof<FunctionSignaturePrettyPrintingProcessor>) processors)
    let bodyProcessors = List.map(fun (p:IFunctionPrettyPrintingProcessor) ->
                            p :?> FunctionBodyPrettyPrintingProcessor) (List.filter (fun (p:IFunctionPrettyPrintingProcessor) -> 
                                                                            p.GetType() = typeof<FunctionBodyPrettyPrintingProcessor>) processors)
            
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
            output <- bodyProcessors.[index].Handle(expression, this)
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
            output <- signatureProcessors.[index].Handle(mi, this)
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


