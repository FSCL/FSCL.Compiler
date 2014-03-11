namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Compiler.Language
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System
open Cloo

[<StepProcessor("FSCL_ARGS_BUILDING_PREPROCESSING_PROCESSOR", 
                "FSCL_FUNCTION_PREPROCESSING_STEP")>] 
type ArgumentsBuildingProcessor() =        
    inherit FunctionPreprocessingProcessor()

    override this.Run(fInfo, en, opts) =
        // Get kernel info
        let kernelInfo = fInfo
        // Get kernel signature
        let methodInfo = kernelInfo.Signature

        // Process each parameter
        for p in methodInfo.GetParameters() do
            // Create parameter info
            let parameterEntry = new KernelParameterInfo(p.Name, p.ParameterType)
            // Set var to be used in kernel body
            parameterEntry.Placeholder <- Some(Quotations.Var(p.Name, p.ParameterType, false))                    
            // If the parameter is not an array set the access mode to read
            if not (p.ParameterType.IsArray) then
                parameterEntry.Access <- AccessMode.ReadAccess
            // Add the parameter to the list of kernel params
            fInfo.Parameters.Add(parameterEntry)

            
