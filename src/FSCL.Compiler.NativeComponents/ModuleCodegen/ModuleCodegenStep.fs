namespace FSCL.Compiler.ModuleCodegen

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_MODULE_CODEGEN_STEP",
       Dependencies = [| "FSCL_FUNCTION_CODEGEN_STEP";
                         "FSCL_MODULE_POSTPROCESSING_STEP";
                         "FSCL_FUNCTION_TRANSFORMATION_STEP";
                         "FSCL_FUNCTION_POSTPROCESSING_STEP";
                         "FSCL_FUNCTION_PREPROCESSING_STEP";
                         "FSCL_MODULE_PREPROCESSING_STEP";
                         "FSCL_MODULE_PARSING_STEP" |])>]
type ModuleCodegenStep(tm: TypeManager, 
                       processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelExpression, KernelExpression>(tm, processors)
        
    override this.Run(cem, opts) =
        if not (opts.ContainsKey(CompilerOptions.NoCodegen)) then    
            for km in cem.KernelModulesRequiringCompilation do
                let state = ref ""
                for p in processors do
                    state := p.Execute((km, !state), this, opts) :?> string
                km.Code <- Some(!state)

        ContinueCompilation(cem)