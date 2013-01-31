namespace FSCL.Transformation.Processors

open FSCL.Transformation
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection

type ArrayAccessProcessor() =          
    let UpdateArrayAccessMode(var:string, mode:KernelParameterAccessMode, engine:KernelBodyTransformationStage) =
        if (engine.TransformationData("KERNEL_PARAMETER_TABLE")).IsNone then
            raise (new KernelTransformationException("KERNEL_PARAMETER_TABLE global data cannot be found, but it is required by ArrayAccessProcessor to execute"))
        
        let data = engine.TransformationData("KERNEL_PARAMETER_TABLE").Value :?> KernelParameterTable
        for pair in data do
            if pair.Key.Name = var then
                let newMode = 
                    match mode, data.[pair.Key].Access with
                    | _, KernelParameterAccessMode.ReadWrite
                    | KernelParameterAccessMode.ReadWrite, _ ->
                        KernelParameterAccessMode.ReadWrite
                    | KernelParameterAccessMode.ReadOnly, KernelParameterAccessMode.WriteOnly
                    | KernelParameterAccessMode.WriteOnly, KernelParameterAccessMode.ReadOnly ->
                        KernelParameterAccessMode.ReadWrite
                    | _, _ ->
                        mode
                data.[pair.Key].Access <- newMode
            
    let GetSizeParameters(var, engine:KernelBodyTransformationStage) = 
        if (engine.TransformationData("KERNEL_PARAMETER_TABLE")).IsNone then
            raise (new KernelTransformationException("KERNEL_PARAMETER_TABLE global data cannot be found, but it is required by ArrayAccessProcessor to execute"))
        
        let data = engine.TransformationData("KERNEL_PARAMETER_TABLE").Value :?> KernelParameterTable
        let mutable sizeParameters = []
                
        for pair in data do
            if pair.Key.Name = var then
                sizeParameters <- pair.Value.SizeParameters
                
        if sizeParameters.IsEmpty then
            raise (KernelTransformationException("Cannot determine the size variables of array " + var + ". This means it is not a kernel parameter or you are eploying aliasing"))
        sizeParameters

    interface CallProcessor with
        member this.Handle(expr, o, methodInfo, args, engine:KernelBodyTransformationStage) =
            if methodInfo.DeclaringType.Name = "IntrinsicFunctions" then
                let arrayName = engine.Process(args.[0])
                let arraySizeParameters = GetSizeParameters(arrayName, engine)
                if methodInfo.Name = "GetArray" then
                    UpdateArrayAccessMode(arrayName, KernelParameterAccessMode.ReadOnly, engine)
                    (true, Some(arrayName + "[" + engine.Process(args.[1]) + "]"))
                elif methodInfo.Name = "GetArray2D" then
                    UpdateArrayAccessMode(arrayName, KernelParameterAccessMode.ReadOnly, engine)
                    let index = "(" + engine.Process(args.[1]) + ")" + " * " + arraySizeParameters.[0] + " + (" + engine.Process(args.[2]) + ")"
                    (true, Some(arrayName + "[" + index + "]"))
                elif methodInfo.Name = "GetArray3D" then
                    UpdateArrayAccessMode(arrayName, KernelParameterAccessMode.ReadOnly, engine)
                    let index = "(" + engine.Process(args.[1]) + ")" + " * " + arraySizeParameters.[0] + " * " + arraySizeParameters.[1] + " + " + arraySizeParameters.[0] + " * (" + engine.Process(args.[2]) + ") + (" + engine.Process(args.[3]) + ")"
                    (true, Some(engine.Process(args.[0]) + "[" + index + "]"))
                elif methodInfo.Name = "SetArray" then
                    UpdateArrayAccessMode(arrayName, KernelParameterAccessMode.WriteOnly, engine)
                    (true, Some(arrayName + "[" + engine.Process(args.[1]) + "] = " + engine.Process(args.[2]) + ";\n"))
                elif methodInfo.Name = "SetArray2D" then
                    UpdateArrayAccessMode(arrayName, KernelParameterAccessMode.WriteOnly, engine)
                    let index = "(" + engine.Process(args.[1]) + ")" + " * " + arraySizeParameters.[0] + " + (" + engine.Process(args.[2]) + ")"
                    (true, Some(arrayName + "[" + index + "] = " + engine.Process(args.[3]) + ";\n"))
                elif methodInfo.Name = "SetArray3D" then
                    UpdateArrayAccessMode(arrayName, KernelParameterAccessMode.WriteOnly, engine)
                    let index = "(" + engine.Process(args.[1]) + ")" + " * " + arraySizeParameters.[0] + " * " + arraySizeParameters.[1] + " + " + arraySizeParameters.[0] + " * (" + engine.Process(args.[2]) + ") + (" + engine.Process(args.[3]) + ")"
                    (true, Some(arrayName + "[" + index + "] = " + engine.Process(args.[4]) + ";\n"))
                else
                    (false, None)

            // Get length replaced with appropriate size parameter
            elif methodInfo.DeclaringType.Name = "Array" && methodInfo.Name = "GetLength" then
                let arrayName = engine.Process(o.Value)
                let arraySizeParameters = GetSizeParameters(arrayName, engine)
                match args.[0] with
                | Patterns.Value(v, ty) -> 
                    (true, Some(arraySizeParameters.[v :?> int]))
                | _ -> 
                    raise (KernelTransformationException("Cannot invoke GetLength using a non-contant parameter [array " + arrayName + "]"))
            else
                (false, None)