namespace FSCL.Compiler.ModuleCodegen

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler
open FSCL.Compiler.Util.VerboseCompilationUtil

[<assembly:DefaultComponentAssembly>]
do()

[<Step("FSCL_MODULE_CODEGEN_STEP",
       Dependencies = [| "FSCL_FUNCTION_CODEGEN_STEP";
                         "FSCL_FUNCTION_TRANSFORMATION_STEP";
                         "FSCL_FUNCTION_POSTPROCESSING_STEP";
                         "FSCL_FUNCTION_PREPROCESSING_STEP";
                         "FSCL_MODULE_PREPROCESSING_STEP";
                         "FSCL_MODULE_PARSING_STEP" |])>]
type ModuleCodegenStep(tm: TypeManager, 
                       processors: ICompilerStepProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule>(tm, processors)
        
    override this.Run(k, opts) =
        let verb = StartVerboseStep(this, opts)
        if not (opts.ContainsKey(CompilerOptions.NoCodegen)) then
            let state = ref ""
            for p in processors do
                state := p.Execute((k, !state), this, opts) :?> string
            k.Code <- Some(!state)

        let r = ContinueCompilation(k)        
        StopVerboseStep(verb)
        r