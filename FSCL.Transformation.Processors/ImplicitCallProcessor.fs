namespace FSCL.Transformation.Processors

open FSCL.Transformation
open Microsoft.FSharp.Quotations

type ImplicitCallProcessor() =
    interface CallProcessor with
        member this.Handle(expr, o, methodInfo, args, engine:KernelBodyTransformationStage) =
            if methodInfo.DeclaringType.Name = "fscl" then
                // the function is defined in FSCL
                let args = String.concat ", " (List.map (fun (e:Expr) -> engine.Process(e)) args)
                if methodInfo.Name = "barrier" then
                    (true, Some(methodInfo.Name + "(" + args + ");"))
                else
                    (true, Some(methodInfo.Name + "(" + args + ")"))
            else           
                (false, None)