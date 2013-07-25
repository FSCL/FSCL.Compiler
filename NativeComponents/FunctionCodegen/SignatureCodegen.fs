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
    override this.Run((name, parameters), en) =
        let engine = en :?> FunctionCodegenStep
        // Convert params and produce additional params
        if engine.FunctionInfo.GetType() = typeof<KernelInfo> then
            let kernelInfo = engine.FunctionInfo :?> KernelInfo
            let paramsPrint = List.map(fun (p:KernelParameterInfo) ->
                if p.Info.ParameterType.IsArray then
                    // If the parameters is tagged with Contant attribute, prepend constant keyword, else global
                    let addressSpace = p.AddressSpace
                    if addressSpace = KernelParameterAddressSpace.LocalSpace then
                        "local " + engine.TypeManager.Print(p.Info.ParameterType) + p.Info.Name
                    elif addressSpace = KernelParameterAddressSpace.ConstantSpace then
                        "constant " + engine.TypeManager.Print(p.Info.ParameterType) + p.Info.Name
                    else
                        "global " + engine.TypeManager.Print(p.Info.ParameterType) + p.Info.Name
                else
                    engine.TypeManager.Print(p.Info.ParameterType) + " " + p.Info.Name) parameters
            
            let signature = Some("kernel void " + name + "(" + (String.concat ", " paramsPrint) + ")")
            signature
        else
            let kernelInfo = engine.FunctionInfo
            // Create KERNEL_PARAMETER_TABLE
            let paramsPrint = List.map(fun (p:KernelParameterInfo) ->
                engine.TypeManager.Print(p.Info.ParameterType) + " " + p.Info.Name) parameters
            
            let signature = Some(engine.TypeManager.Print(kernelInfo.ReturnType) + " " + name + "(" + (String.concat ", " paramsPrint) + ")")
            signature

           