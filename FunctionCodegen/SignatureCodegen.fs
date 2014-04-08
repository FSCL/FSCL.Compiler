namespace FSCL.Compiler.FunctionCodegen

open FSCL.Compiler
open FSCL.Compiler.Language
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
    override this.Run((name, parameters), en, opts) =
        let engine = en :?> FunctionCodegenStep
        // Convert params and produce additional params
        if engine.FunctionInfo :? KernelInfo then
            let kernelInfo = engine.FunctionInfo :?> KernelInfo
            let paramsPrint = List.map(fun (p:FunctionParameter) ->
                if p.DataType.IsArray then
                    // If the parameters is tagged with Contant attribute, prepend constant keyword, else global
                    let addressSpace = p.Meta.Get<AddressSpaceAttribute>().AddressSpace
                    if addressSpace = AddressSpace.Local then
                        "local " + engine.TypeManager.Print(p.DataType) + p.Name
                    elif addressSpace = AddressSpace.Constant then
                        "constant " + engine.TypeManager.Print(p.DataType) + p.Name
                    elif addressSpace = AddressSpace.Global then
                        "global " + engine.TypeManager.Print(p.DataType) + p.Name
                    else
                        "global " + engine.TypeManager.Print(p.DataType) + p.Name
                        
                else
                    engine.TypeManager.Print(p.DataType) + " " + p.Name) parameters
            
            let signature = Some("kernel void " + name + "(" + (String.concat ", " paramsPrint) + ")")
            signature
        else
            let kernelInfo = engine.FunctionInfo

            let paramsPrint = List.map(fun (p:FunctionParameter) ->
                engine.TypeManager.Print(p.DataType) + " " + p.Name) parameters
            
            let signature = Some(engine.TypeManager.Print(kernelInfo.ReturnType) + " " + name + "(" + (String.concat ", " paramsPrint) + ")")
            signature

           