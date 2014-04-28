namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Compiler.Language
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

[<StepProcessor("FSCL_ARRAY_LENGHT_ARGS_GENERATOR_PREPROCESSING_PROCESSOR", "FSCL_FUNCTION_PREPROCESSING_STEP", 
                Dependencies = [| "FSCL_REF_TYPE_TO_ARRAY_REPLACING_PREPROCESSING_PROCESSOR" |])>] 
type ArrayLenghtArgumentsGenerator() =        
    inherit FunctionPreprocessingProcessor()

    let GetArrayDimensions (t:Type) =
        // Any better way to do this?
        let dimensionsString = t.FullName.Split([| '['; ']' |]).[1]
        let dimensions = ref 1
        String.iter (fun c -> if (c = ',') then dimensions := !dimensions + 1) dimensionsString
        !dimensions
        
    let GenerateSizeAdditionalArg (name:string, n:obj) =
         String.Format("{0}_length_{1}", name, n.ToString())

    override this.Run(fInfo, s, opts) =
        let step = s :?> FunctionPreprocessingStep

        // Store size parameters separately to enqueue them at the end
        let sizeParameters = new List<FunctionParameter>()

        // Get node input for each flow graph node instance of this kernel
        //let nodes = FlowGraphManager.GetKernelNodes(fInfo.ID,  step.FlowGraph)

        // Process each parameter
        for p in fInfo.Parameters do
            if p.DataType.IsArray then                
                // Get dimensions (rank)
                let dimensions = GetArrayDimensions(p.DataType) 
                // Get "GetLength" method info
                //let getLengthMethod = p.Type.GetMethod("GetLength")
                // Flatten dimensions (1D array type)
                let pType = p.DataType.GetElementType().MakeArrayType()
                p.Placeholder <- Quotations.Var(p.Placeholder.Name, pType, false)
                // Create auto-generated size parameters
                for d = 0 to dimensions - 1 do
                    let sizeName = GenerateSizeAdditionalArg(p.Name, d)
                    let sizeP = new FunctionParameter(sizeName, 
                                                      Quotations.Var(sizeName, typeof<int>, false),
                                                      FunctionParameterType.SizeParameter,
                                                      None)
                    // A non-array parameter access is always read only
                    p.SizeParameters.Add(sizeP)
                    sizeParameters.Add(sizeP)     
                    
        // Add size parameters to the list of kernel params
        fInfo.GeneratedParameters.AddRange(sizeParameters)

            
