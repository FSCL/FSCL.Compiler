namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open FSCL.Compiler.KernelLanguage
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open System

type internal TemporaryKernelParameterTable = Dictionary<String, TemporaryKernelParameterInfo>
type internal KernelParameterTable = Dictionary<String, KernelParameterInfo>

[<StepProcessor("FSCL_SIGNATURE_PREPROCESSING_PROCESSOR", "FSCL_FUNCTION_PREPROCESSING_STEP", 
                Dependencies = [|"FSCL_ARG_EXTRACTION_PREPROCESSING_PROCESSOR"|])>] 
type SignaturePreprocessor() =        
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
        // Get kernel info
        let kernelInfo = fInfo
        // Get kernel signature
        let methodInfo = kernelInfo.Signature

        // Create kernel parameter data
        let temporaryParameterTable = new TemporaryKernelParameterTable()

        // Store kernel parameter types
        let mutable parameterType = []
        let mutable additionalParameterType = []

        // Process each parameter
        for p in methodInfo.GetParameters() do
            // Create an entry for this parameter
            let parameterEntry = new TemporaryKernelParameterInfo(p)
            if p.ParameterType.IsArray then
                let dimensions = GetArrayDimensions(p.ParameterType)    
                parameterEntry.SizeParameters <- List.ofSeq (seq { for d = 0 to dimensions - 1 do yield GenerateSizeAdditionalArg(p.Name, d) })

                // If the parameters is tagged with Contant attribute, prepend constant keyword, else global
                let constantAttribute = p.GetCustomAttribute<ConstantAttribute>()
                let localAttribute = p.GetCustomAttribute<LocalAttribute>()
                if constantAttribute <> null then
                    parameterEntry.AddressSpace <- KernelParameterAddressSpace.ConstantSpace
                elif localAttribute <> null then
                    parameterEntry.AddressSpace <- KernelParameterAddressSpace.LocalSpace
                else    
                    parameterEntry.AddressSpace <- KernelParameterAddressSpace.GlobalSpace

                // Store additional parameters type
                additionalParameterType <- additionalParameterType @ (List.init (parameterEntry.SizeParameters.Length) (fun i -> typeof<int>))
                
            // Store parameter type    
            parameterType <- parameterType @ [p.ParameterType]

            // Store entry into table
            temporaryParameterTable.Add(p.Name, parameterEntry)                        

        // Modify kernel signature to include additional parameters and to flat arrays
        // Flat array types
        parameterType <- List.map (fun (t:Type) ->
            if t.IsArray then
                t.GetElementType().MakeArrayType()
            else
                t) parameterType
        // Create new signature
        let newSignature = new DynamicMethod(methodInfo.Name, methodInfo.ReturnType, Array.ofList (parameterType @ additionalParameterType))
        // Define existing parameters
        let mutable pIndex = 1
        for p in temporaryParameterTable do
            let pb = newSignature.DefineParameter(pIndex, p.Value.Info.Attributes, p.Key)  
            pIndex <- pIndex + 1
        // Define additional parameters
        for p in temporaryParameterTable do
            for i in p.Value.SizeParameters do
                newSignature.DefineParameter(pIndex, ParameterAttributes.None, i) |> ignore     
                pIndex <- pIndex + 1 
        // Replace each original parameter info in parameterTable with new ones
        let newParameterTable = new KernelParameterTable()
        for newParam in newSignature.GetParameters() do
            if temporaryParameterTable.ContainsKey(newParam.Name) then
                let info = temporaryParameterTable.[newParam.Name]
                let newInfo = new KernelParameterInfo(newParam)
                newInfo.Access <- info.Access
                newInfo.Placeholder <- Some(Quotations.Var(newParam.Name, newParam.ParameterType, false))
                newInfo.AddressSpace <- info.AddressSpace
                newInfo.SizeParameterNames <- info.SizeParameters
                newParameterTable.Add(newParam.Name, newInfo)
            else
                newParameterTable.Add(newParam.Name, KernelParameterInfo(newParam))

        // Associate to each kernel parameter info the kernel parameter infos representing their size
        for pInfo in newParameterTable do
            let sizeParameterNames = pInfo.Value.SizeParameterNames
            for sizePName in sizeParameterNames do
                let kSizePInfo = newParameterTable.[sizePName]
                // Define a var to ref the size parameter inside the kernel
                kSizePInfo.Placeholder <- Some(Quotations.Var(sizePName, kSizePInfo.Info.ParameterType, false))
                pInfo.Value.SizeParameters <- pInfo.Value.SizeParameters @ [kSizePInfo]
                    
        // Add kernel parameter table to the global data
        for item in newParameterTable do
            kernelInfo.ParameterInfo.Add(item.Key, item.Value)
        kernelInfo.Signature <- newSignature
            
