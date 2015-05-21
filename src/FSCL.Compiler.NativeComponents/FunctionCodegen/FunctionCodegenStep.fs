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
                        "FSCL_FUNCTION_POSTPROCESSING_STEP";
                        "FSCL_MODULE_POSTPROCESSING_STEP";
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
    inherit CompilerStep<KernelExpression, KernelExpression>(tm, processors)
    
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
                        
    member this.GetFunctionInfo (id:FunctionInfoID) =
        0//km.Functions.[id]            

    member private this.Process(expression: Expr, opts) =
        // At first, check generic processors (for complex constructs)
        let mutable index = 0
        let mutable output = None        
        while (output.IsNone) && (index < bodyProcessors.Length) do
            output <- bodyProcessors.[index].Run((expression, fun e -> this.Process(e, opts)), this, opts)
            index <- index + 1
        // If no suitable generic processor, use specific ones
        if (output.IsNone) then
            raise (CompilerException("Unrecognized construct in kernel body " + expression.ToString()))
        output.Value
        
    member private this.Process(name: string, parameters: FunctionParameter list, opts) =
        // At first, check generic processors (for complex constructs)
        let mutable index = 0
        let mutable output = None        
        while (output.IsNone) && (index < signatureProcessors.Length) do
            output <- signatureProcessors.[index].Run((name, parameters), this, opts)
            index <- index + 1
        // If no suitable generic processor, use specific ones
        if (output.IsNone) then
            raise (CompilerException("Unrecognized kernel signature " + name))
        output.Value
    ///
    ///<summary>
    ///The method that processors should call to recursively apply the step (the entire set of processors) to an AST (sub)node
    ///</summary>
    ///
        
    member private this.Process (f:FunctionInfo, opts:Map<string, obj>) =
        this.FunctionInfo <- f
        let processedSignature = this.Process(this.FunctionInfo.Name, this.FunctionInfo.Parameters, opts)
        let processedBody = this.Process(this.FunctionInfo.Body, opts)
        this.FunctionInfo.SignatureCode <- processedSignature
        this.FunctionInfo.Code <- processedSignature + "{\n" + processedBody + "\n}"
    ///
    ///<summary>
    ///The method called to execute the step
    ///</summary>
    ///<param name="km">The input kernel module</param>
    ///<returns>
    ///The modified kernel module
    ///</returns>
    ///       
    override this.Run(cem, opts) =    
        if not (opts.ContainsKey(CompilerOptions.NoCodegen)) then
            for km in cem.KernelModulesRequiringCompilation do 
                // Process functions
                for f in km.Functions do
                    this.Process (f.Value :?> FunctionInfo, opts)
                // Process kernel
                this.Process (km.Kernel, opts)
            // Process defines
            //for d in km.StaticConstantDefines do
              //  km.StaticConstantDefinesCode.Add(d.Key, this.Process(d.Value))
         
        ContinueCompilation(cem)


