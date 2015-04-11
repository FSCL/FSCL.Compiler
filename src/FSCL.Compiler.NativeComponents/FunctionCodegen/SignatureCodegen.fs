namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open FSCL.Language
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

type AddressSpaceMetadataComparer() =
    inherit MetadataComparer() with
    override this.MetaEquals(meta1, meta2) =
       match meta1, meta2 with
       | :? AddressSpaceAttribute, :? AddressSpaceAttribute ->
            let sp1, sp2 = meta1 :?> AddressSpaceAttribute, meta2 :?> AddressSpaceAttribute
            match sp1.AddressSpace, sp2.AddressSpace with
            | AddressSpace.Auto, AddressSpace.Auto 
            | AddressSpace.Auto, AddressSpace.Global 
            | AddressSpace.Global, AddressSpace.Auto ->
                true
            | _ ->
                sp1.AddressSpace = sp2.AddressSpace
       | _ ->
            true
            
[<StepProcessor("FSCL_SIGNATURE_CODEGEN_PROCESSOR", "FSCL_FUNCTION_CODEGEN_STEP")>]
[<UseMetadata(typeof<AddressSpaceAttribute>, typeof<AddressSpaceMetadataComparer>)>] 
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
    
    let paramsPrint(f:FunctionInfo, step:FunctionCodegenStep) = 
        List.map(fun (p:FunctionParameter) ->
            let forcedAddressSpace = 
                if p.ForcedGlobalAddressSpace then
                    "global "
                else if p.ForcedConstantAddressSpace then
                    "constant "
                else if p.ForcedLocalAddressSpace then
                    "local "                        
                else if p.ForcedPrivateAddressSpace then
                    "private "
                else
                    null
            if forcedAddressSpace <> null then
                forcedAddressSpace + step.TypeManager.Print(p.DataType) + " " + p.Name
            else
                let defaultAddressSpace =
                    if p.DataType.IsArray then
                        "global "
                    else
                        "private " 
                // If the parameters is tagged with Contant attribute, prepend constant keyword, else global
                let addressSpace = p.Meta.Get<AddressSpaceAttribute>().AddressSpace
                if addressSpace = AddressSpace.Local then
                    "local " + step.TypeManager.Print(p.DataType) + " " + p.Name
                elif addressSpace = AddressSpace.Constant then
                    "constant " + step.TypeManager.Print(p.DataType) + " " + p.Name
                elif addressSpace = AddressSpace.Global then
                    "global " + step.TypeManager.Print(p.DataType) + " " + p.Name
                elif addressSpace = AddressSpace.Private then
                    "private " + step.TypeManager.Print(p.DataType) + " " + p.Name
                else
                    defaultAddressSpace + step.TypeManager.Print(p.DataType) + " " + p.Name) f.Parameters
            
    override this.Run((name, parameters), st, opts) =
        let step = st :?> FunctionCodegenStep
        // Convert params and produce additional params
        if step.FunctionInfo :? KernelInfo then
            let kernelInfo = step.FunctionInfo :?> KernelInfo
            let signature = Some("kernel void " + name + "(" + (String.concat ", " (paramsPrint(kernelInfo, step))) + ")")
            signature
        else
            let kernelInfo = step.FunctionInfo
            // Check if inline
            let inlinePrefix =
                if step.FunctionInfo.IsLambda || step.FunctionInfo.ParsedSignature.GetCustomAttribute(typeof<InlineAttribute>) <> null then
                    "inline "
                else
                    ""
            // Print signature    
            let signature = Some(inlinePrefix + step.TypeManager.Print(kernelInfo.ReturnType) + " " + name + "(" + (String.concat ", " (paramsPrint(kernelInfo, step))) + ")")
            signature

           