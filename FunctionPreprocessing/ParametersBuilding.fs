namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Compiler.Language
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_ARGS_BUILDING_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP")>] 
type ArgumentsBuildingProcessor() =        
    inherit FunctionPreprocessingProcessor()

    override this.Run(fInfo, en, opts) =
        // Only if function (kernel parameters processed during parsing)
        if not (fInfo :? KernelInfo) then
            let methodInfo = fInfo.Signature

            // Process each parameter
            for p in methodInfo.GetParameters() do
                // Create parameter info
                let parameterEntry = new KernelParameterInfo(p.Name, p.ParameterType, NormalParameter, null)      
                // Add the parameter to the list of kernel params
                fInfo.Parameters.Add(parameterEntry)

            
