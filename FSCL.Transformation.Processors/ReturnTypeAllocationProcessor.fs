namespace FSCL.Transformation.Processors

open FSCL.Transformation
open Microsoft.FSharp.Quotations

type ReturnTypeAllocationProcessor() =
    member private this.SetReturnTypeVar(engine:KernelBodyTransformationStage, var:Var, args:Expr list) =
        if (var.IsMutable) then
            raise (new KernelTransformationException("A kernel returned variable must be immutable"))            
        if (engine.TransformationData("KERNEL_RETURN_TYPE")).IsNone then
            engine.AddTransformationData("KERNEL_RETURN_TYPE", (var, args))
        else 
            raise (new KernelTransformationException("A kernel can declare one only variable as a return variable"))
        
    interface LetProcessor with
        member this.Handle(expr, var, value, body, engine:KernelBodyTransformationStage) =
            match value with
            | Patterns.Call(o, methodInfo, args) ->
                if methodInfo.DeclaringType.Name = "IntrinsicFunctions" && methodInfo.Name = "ZeroCreate" then
                    // Only zero create allocation is permitted and it must be assigned to a non mutable variable
                    this.SetReturnTypeVar(engine, var, args)
                    (true, Some(""))
                else
                    (false, None)
            | _ ->           
                (false, None)