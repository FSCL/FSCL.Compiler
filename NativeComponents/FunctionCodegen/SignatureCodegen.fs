namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

type internal KernelParameterTable = Dictionary<String, KernelParameterInfo>

[<StepProcessor("FSCL_SIGNATURE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
///
///<summary>
///The function codegen step processor whose behavior is to produce the target code for a given kernel or function signature
///</summary>
///
type SignatureCodegen() =      
    inherit FunctionSignatureCodegenProcessor()  
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
    ///
    ///<summary>
    ///The method called to execute the processor
    ///</summary>
    ///<param name="fi">The signature to process</param>
    ///<param name="en">The owner step</param>
    ///<returns>
    ///The target code for the signature
    ///</returns>
    ///       
    override this.Run(fi, en) =
        let engine = en :?> FunctionCodegenStep
        // Convert params and produce additional params
        let kernelParams = fi.GetParameters()
        if engine.FunctionInfo.GetType() = typeof<KernelInfo> then
            let kernelInfo = engine.FunctionInfo :?> KernelInfo
            let kernelParamsInfo = kernelInfo.ParameterInfo
            let paramsPrint = Array.map(fun (p:ParameterInfo) ->
                if p.ParameterType.IsArray then
                    // If the parameters is tagged with Contant attribute, prepend constant keyword, else global
                    let addressSpace = kernelParamsInfo.[p.Name].AddressSpace
                    if addressSpace = KernelParameterAddressSpace.LocalSpace then
                        "local " + engine.TypeManager.Print(p.ParameterType) + p.Name
                    elif addressSpace = KernelParameterAddressSpace.ConstantSpace then
                        "constant " + engine.TypeManager.Print(p.ParameterType) + p.Name
                    else
                        "global " + engine.TypeManager.Print(p.ParameterType) + p.Name
                else
                    engine.TypeManager.Print(p.ParameterType) + " " + p.Name) kernelParams
            
            let signature = Some("kernel void " + fi.Name + "(" + (String.concat ", " paramsPrint) + ")")
            signature
        else
            let kernelInfo = engine.FunctionInfo
            let kernelParamsInfo = kernelInfo.ParameterInfo
            // Create KERNEL_PARAMETER_TABLE
            let paramsPrint = Array.map(fun (p:ParameterInfo) ->
                engine.TypeManager.Print(p.ParameterType) + " " + p.Name) kernelParams
            
            let signature = Some(engine.TypeManager.Print(kernelInfo.Signature.ReturnType) + " " + fi.Name + "(" + (String.concat ", " paramsPrint) + ")")
            signature

           