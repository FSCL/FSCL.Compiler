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
        let sizeParameters = new List<KernelParameterInfo>()

        // Get node input for each flow graph node instance of this kernel
        //let nodes = FlowGraphManager.GetKernelNodes(fInfo.ID,  step.FlowGraph)

        // Process each parameter
        for p in fInfo.Parameters do
            if p.Type.IsArray then                
                // Get dimensions (rank)
                let dimensions = GetArrayDimensions(p.Type) 
                // Get "GetLength" method info
                let getLengthMethod = p.Type.GetMethod("GetLength")
                // Flatten dimensions (1D array type)
                let pType = p.Type.GetElementType().MakeArrayType()
                p.Type <- pType
                // Create auto-generated size parameters
                for d = 0 to dimensions - 1 do
                    let sizeP = new KernelParameterInfo(GenerateSizeAdditionalArg(p.Name, d), typeof<int>, null, None, null)
                    // A non-array parameter access is always read only
                    sizeP.Access <- AccessMode.ReadAccess
                    // Set var to be used in kernel body
                    sizeP.Placeholder <- Some(Quotations.Var(GenerateSizeAdditionalArg(p.Name, d), typeof<int>, false))
                    // Set this to be a size parameter
                    sizeP.IsSizeParameter <- true
                    p.SizeParameters.Add(sizeP)
                    sizeParameters.Add(sizeP)              
                    // Update flow graph nodes input
                    (*
                    for n in nodes do
                        let inputBinding = FlowGraphManager.GetNodeInput(n)
                        FlowGraphManager.SetNodeInput(n, 
                                                      sizeP.Name,
                                                      FlowGraphNodeInputInfo(
                                                        ImplicitValue,
                                                        None,
                                                        null)) *)
                // Set var to be used in kernel body
                p.Placeholder <- Some(Quotations.Var(p.Name, pType, false))

        // Add size parameters to the list of kernel params
        fInfo.Parameters.AddRange(sizeParameters)

            
