namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_ARRAY_PARAMETERS_MANIPULATION_PREPROCESSING_PROCESSOR", "FSCL_FUNCTION_PREPROCESSING_STEP", 
                Dependencies = [|"FSCL_RETURN_TYPE_TO_OUTPUT_ARG_REPLACING_PREPROCESSING_PROCESSOR"|])>] 
type ArrayParametersManipulationProcessor() =        
    inherit FunctionPreprocessingProcessor()

    let GetArrayDimensions (t:Type) =
        // Any better way to do this?
        let dimensionsString = t.FullName.Split([| '['; ']' |]).[1]
        let dimensions = ref 1
        String.iter (fun c -> if (c = ',') then dimensions := !dimensions + 1) dimensionsString
        !dimensions
        
    let GenerateSizeAdditionalArg (name:string, n:obj) =
         String.Format("{0}_length_{1}", name, n.ToString())

    override this.Run(fInfo, en) =
        let step = en :?> FunctionPreprocessingStep

        // Store size parameters separately to enqueue them at the end
        let sizeParameters = new List<KernelParameterInfo>()

        // Process each parameter
        for p in fInfo.Parameters do
            if p.Type.IsArray then
                // Get dimensions (rank)
                let dimensions = GetArrayDimensions(p.Type) 
                // Flatten dimensions (1D array type)
                let pType = p.Type.GetElementType().MakeArrayType()
                p.Type <- pType
                // Create auto-generated size parameters
                for d = 0 to dimensions - 1 do
                    let sizeP = new KernelParameterInfo(GenerateSizeAdditionalArg(p.Name, d), typeof<int>)
                    // A non-array parameter access is always read only
                    sizeP.Access <- ReadOnly
                    // Set var to be used in kernel body
                    sizeP.Placeholder <- Some(Quotations.Var(GenerateSizeAdditionalArg(p.Name, d), typeof<int>, false))
                    // Set this to be a size parameter
                    sizeP.IsSizeParameter <- true
                    p.SizeParameters.Add(sizeP)
                    sizeParameters.Add(sizeP)                 
                // Set var to be used in kernel body
                p.Placeholder <- Some(Quotations.Var(p.Name, pType, false))

        // Add size parameters to the list of kernel params
        fInfo.Parameters.AddRange(sizeParameters)
        // Add the arguments to the call graph
        for p in sizeParameters do
            step.SetArgument(p.Name, RuntimeImplicit)

            
