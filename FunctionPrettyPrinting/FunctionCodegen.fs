namespace FSCL.Compiler.FunctionCodegen

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_FUNCTION_CODEGEN_STEP",
      Dependencies = [| "FSCL_FUNCTION_TRANSFORMATION_STEP";
                        "FSCL_FUNCTION_PREPROCESSING_STEP";
                        "FSCL_MODULE_PREPROCESSING_STEP";
                        "FSCL_MODULE_PARSING_STEP" |])>]
///
///<summary>
///The function codegen step, whose behavior is generating the target OpenCL code for a given kernel or function, including signature and body
///</summary>
///<remarks>
///The orchestration of the processors is recursive descent
///</remarks> 
///
type FunctionCodegenStep(tm: TypeManager,
                                processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm, processors)
    
    let signatureProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> FunctionSignatureCodegenProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                                 typeof<FunctionSignatureCodegenProcessor>.IsAssignableFrom(p.GetType())) processors)
    let bodyProcessors = List.map(fun (p:ICompilerStepProcessor) ->
                                        p :?> FunctionBodyCodegenProcessor) (List.filter (fun (p:ICompilerStepProcessor) -> 
                                                                            typeof<FunctionBodyCodegenProcessor>.IsAssignableFrom(p.GetType())) processors)
            
    member val private currentFunction = null with get, set
    ///
    ///<summary>
    ///The current kernel or utility function the step is processing
    ///</summary>
    ///
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
            output <- bodyProcessors.[index].Run(expression, this)
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
            output <- signatureProcessors.[index].Run(mi, this)
            index <- index + 1
        // If no suitable generic processor, use specific ones
        if (output.IsNone) then
            raise (CompilerException("Unrecognized kernel signature " + mi.ToString()))
        output.Value
    ///
    ///<summary>
    ///The method that processors should call to recursively apply the step (the entire set of processors) to an AST (sub)node
    ///</summary>
    ///
    member this.Continue(expression: Expr) =
        this.Process(expression)
        
    member private this.Process(f:FunctionInfo) =
        this.FunctionInfo <- f
        this.FunctionInfo.Codegen <- this.Process(this.FunctionInfo.Signature) + "{\n" + this.Process(this.FunctionInfo.Body) + "\n}"
    ///
    ///<summary>
    ///The method called to execute the step
    ///</summary>
    ///<param name="km">The input kernel module</param>
    ///<returns>
    ///The modified kernel module
    ///</returns>
    ///       
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


