namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

type DefaultSignatureProcessor() =        
    let rec LiftArgExtraction (expr, parameters: Reflection.ParameterInfo[]) =
        match expr with
        | Patterns.Lambda(v, e) ->
            LiftArgExtraction (e, parameters)
        | Patterns.Let(v, value, body) ->
            let el = Array.tryFind (fun (p : Reflection.ParameterInfo) -> p.Name = v.Name) parameters
            if el.IsSome then
                LiftArgExtraction (body, parameters)
            else
                expr
        | _ ->
            expr
            
    let GetSizeParameters(var:ParameterInfo, engine:KernelSignatureTransformationStage) =   
        if not (var.ParameterType.IsArray) then
            []
        else
            let data = engine.TransformationData("KERNEL_PARAMETER_TABLE").Value :?> KernelParameterTable
            let mutable sizeParameters = []
            for k in data do
                if k.Key = var then
                    sizeParameters <- k.Value.SizeParameters
            if sizeParameters.IsEmpty then
                raise (KernelTransformationException("Cannot determine the size variables of array " + var.Name + ". This means no parameter processor produced the additional size parameters"))
            sizeParameters
            
    interface SignatureProcessor with
        member this.Handle(kernel, engine:KernelSignatureTransformationStage) =
            let kernelBody = 
                match kernel with
                | DerivedPatterns.MethodWithReflectedDefinition(body) ->
                    body
                | _ ->
                    raise (KernelTransformationException("A kernel definition must provide a function marked with ReflectedDefinition attribute [" + kernel.Name + "]"))

            // Convert params and produce additional params
            let kernelParams = kernel.GetParameters()
            // Create KERNEL_PARAMETER_TABLE
            let table = KernelParameterTable()
            for par in kernelParams do
                table.Add(par, new KernelParameterInfo(par))
            engine.AddTransformationData("KERNEL_PARAMETER_TABLE", table)

            let convertedParams = Seq.ofArray (Array.map (engine.Process:ParameterInfo -> String) kernelParams) 
            let additionalParams = seq {
                for param in kernelParams do
                    let ap = GetSizeParameters(param, engine)
                    for p in ap do
                        yield " int " + p }
                   
            // Produce signature
            let prettyArgs = String.concat ", " (Seq.append convertedParams additionalParams)

            // Clean Let a = in Let b = in ... which is the entrypoint of tupled function bodies
            let cleanBody = LiftArgExtraction(kernelBody, kernelParams)

            (true, Some(cleanBody, "kernel void " + kernel.Name + "(" + prettyArgs + ")"))
            

