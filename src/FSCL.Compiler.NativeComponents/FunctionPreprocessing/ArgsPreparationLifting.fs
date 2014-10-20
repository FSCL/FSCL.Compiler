namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Language
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Linq.RuntimeHelpers
open System.Collections.Generic
open System.Reflection.Emit
open System
open Microsoft.FSharp.Reflection
open System.Runtime.InteropServices
open FSCL.Compiler.Util
open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction

//RETURN_TYPE_TO_OUTPUT_ARG_REPLACING
[<StepProcessor("FSCL_ARGS_PREP_LIFTING_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP")>]
type ArgsPreparationLiftingProcessor() =
    inherit FunctionPreprocessingProcessor()
            
    override this.Run(fInfo, en, opts) =
        let engine = en :?> FunctionPreprocessingStep
        fInfo.Body <- fst (LiftCurriedOrTupledArgs(fInfo.OriginalBody)).Value
       
