namespace FSCL.Compiler.Processors

open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Reflection.Emit
open System
open System.Reflection

[<StepProcessor("FSCL_RETURN_TYPE_TRANSFORMATION_PROCESSOR", "FSCL_FUNCTION_TRANSFORMATION_STEP")>]
type ReturnTypeTransformation() =
    let GetArrayDimensions (t:Type) =
        // Any better way to do this?
        let dimensionsString = t.FullName.Split([| '['; ']' |]).[1]
        let dimensions = ref 1
        String.iter (fun c -> if (c = ',') then dimensions := !dimensions + 1) dimensionsString
        !dimensions

    let GenerateSizeAdditionalArg (name:string, n:obj) =
         String.Format("{0}_length_{1}", name, n.ToString())
              
    static member private hasRun = false

    member private this.SetReturnTypeVar(engine:FunctionTransformationStep, var:Var, args:Expr list) =
        if (var.IsMutable) then
            raise (new CompilerException("A kernel returned variable must be immutable"))            
        if not (engine.FunctionInfo.CustomInfo.ContainsKey("KERNEL_RETURN_TYPE")) then
            engine.FunctionInfo.CustomInfo.Add("KERNEL_RETURN_TYPE", (var, args))
            
            let varType = var.Type.GetElementType().MakeArrayType()

            // Fix signature and kernel parameters
            let kernelInfo = engine.FunctionInfo :?> KernelInfo
            let kernelParameters = kernelInfo.ParameterInfo
            let mutable kernelSignature = kernelInfo.Signature
            let originalParamsCount = kernelSignature.GetParameters().Length

            // Get parameter types
            let parameterType = List.ofArray (Array.map (fun (e:ParameterInfo) -> e.ParameterType) (kernelSignature.GetParameters()))
            // Create additional parameters for the returned array
            let dimensions = GetArrayDimensions(var.Type)    
            let additionalParameterType = List.init (dimensions) (fun i -> typeof<int>)
                                 
            // Create new signature
            let newSignature = new DynamicMethod(kernelSignature.Name, typeof<unit>, Array.ofList(parameterType @ [varType] @ additionalParameterType))
            // Add old params
            Array.iteri(fun i (p:ParameterInfo) -> 
                newSignature.DefineParameter(i + 1, p.Attributes, p.Name) |> ignore) (kernelSignature.GetParameters())
            // Add return array
            newSignature.DefineParameter(originalParamsCount + 1, ParameterAttributes.None, var.Name) |> ignore
            // Add additional params
            for i = 1 to dimensions do
                newSignature.DefineParameter(originalParamsCount + i + 1, ParameterAttributes.None, GenerateSizeAdditionalArg(var.Name, i - 1)) |> ignore
         
            // Define new parameter info
            let parameterEntry = new KernelParameterInfo(newSignature.GetParameters().[originalParamsCount + 1])
            parameterEntry.SizeParameterNames <- List.ofSeq (seq { for d = 0 to dimensions - 1 do yield GenerateSizeAdditionalArg(var.Name, d) })
            parameterEntry.SizeParameters <- List.ofSeq (seq {
                for i = 0 to dimensions - 1 do
                    let sizeP = newSignature.GetParameters().[originalParamsCount + i + 1]
                    let sizeInfo = new KernelParameterInfo(sizeP)
                    sizeInfo.Placeholder <- Some(Quotations.Var(sizeP.Name, sizeP.ParameterType, false))
                    yield sizeInfo })
            parameterEntry.AddressSpace <- KernelParameterAddressSpace.GlobalSpace
            parameterEntry.Access <- KernelParameterAccessMode.WriteOnly
            parameterEntry.Expr <- Some(args :> obj)
            parameterEntry.Placeholder <- Some(Quotations.Var(var.Name, varType, false))

            // Add kernel parameter table to the global data
            kernelInfo.ParameterInfo.Add(parameterEntry.Info.Name, parameterEntry)
            kernelInfo.Signature <- newSignature     
        else 
            raise (new CompilerException("A kernel can declare one only variable as a return variable"))
        
    interface FunctionTransformationProcessor with
        member this.Process(expr, en) =
            let engine = en :?> FunctionTransformationStep
            match expr with
            | Patterns.Let(var, value, body) ->
                match value with
                | Patterns.Call(o, methodInfo, args) ->
                    if (methodInfo.DeclaringType.Name = "ArrayModule" && methodInfo.Name = "ZeroCreate") ||
                        (methodInfo.DeclaringType.Name = "Array2DModule" && methodInfo.Name = "ZeroCreate") ||
                        (methodInfo.DeclaringType.Name = "Array3DModule" && methodInfo.Name = "ZeroCreate") then
                        // Only zero create allocation is permitted and it must be assigned to a non mutable variable
                        this.SetReturnTypeVar(engine, var, args)
                        let processedBody = engine.Continue(body)
                        processedBody
                    else
                        engine.Default(expr)
                | _ ->           
                    engine.Default(expr)
            | _ ->
                engine.Default(expr)
