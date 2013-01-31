namespace FSCL.Compiler.Processors

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System

type RefVariablePreprocessor() =        
    let IsRef(t:Type) =
        if (t.IsGenericType && (t.GetGenericTypeDefinition() = typeof<Ref<_>>.GetGenericTypeDefinition())) then
            true
        else 
            false

    interface FunctionPreprocessingProcessor with
        member this.Handle(fi, engine:FunctionPreprocessingStep) =
            // Get kernel info
            let kernelInfo = fi :?> KernelInfo
            // Get kernel signature
            let methodInfo = kernelInfo.Signature
            let oldParams = methodInfo.GetParameters()

            // Transform each ref variable in an array of 1 element
            let newParamsTypes = Array.map(fun (p:ParameterInfo) ->
                                            let t = p.ParameterType
                                            if IsRef(t) then
                                               (FSharpType.GetRecordFields(t)).[0].PropertyType.MakeArrayType()
                                            else 
                                                t) oldParams
            // Create new signature
            let newSignature = new DynamicMethod(methodInfo.Name, methodInfo.ReturnType, newParamsTypes)
            
            // Define parameters (names and attributes)
            let mutable pIndex = 1
            for p in oldParams do
                let pb = newSignature.DefineParameter(pIndex, p.Attributes, p.Name)  
                pIndex <- pIndex + 1
            
            // Replace each param of type ref in parameter info of kernel with array 
            pIndex <- 0
            for oldParam in oldParams do
                if IsRef(oldParam.ParameterType) then
                    let newParam = newSignature.GetParameters().[pIndex];
                    let newInfo = new KernelParameterInfo(newParam)
                    newInfo.Access <- NoAccess
                    newInfo.Placeholder <- Some(Quotations.Var(newParam.Name, newParam.ParameterType, false))
                    newInfo.AddressSpace <- GlobalSpace
                    kernelInfo.ParameterInfo.[newParam.Name] <- newInfo
                pIndex <- pIndex + 1

            // Store new signature
            kernelInfo.Signature <- newSignature
            
