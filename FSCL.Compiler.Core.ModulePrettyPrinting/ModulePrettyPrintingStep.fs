namespace FSCL.Compiler.ModulePrettyPrinting

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open FSCL.Compiler

[<Step("FSCL_MODULE_PRETTY_PRINTING_STEP",
       Dependencies = [| "FSCL_FUNCTION_PRETTY_PRINTING_STEP";
                         "FSCL_FUNCTION_TRANSFORMATION_STEP";
                         "FSCL_FUNCTION_PREPROCESSING_STEP";
                         "FSCL_MODULE_PREPROCESSING_STEP";
                         "FSCL_MODULE_PARSING_STEP" |])>]
type ModulePrettyPrintingStep(tm: TypeManager, 
                              processors: ModulePrettyPrintingProcessor list) = 
    inherit CompilerStep<KernelModule, KernelModule * String>(tm)
        
    override this.Run(k) =
        let state = ref ""
        for p in processors do
            state := p.Process((k, !state), this)
        (k, !state)