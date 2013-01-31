namespace FSCL

open System 

[<AllowNullLiteral>]
type ConstantAttribute =
    inherit Attribute
    new() =  { }
    
[<AllowNullLiteral>]
type LocalAttribute =
    inherit Attribute
    new() =  { }

namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

type DefaultParameterProcessor() =        
    let GetArrayDimensions (t:Type) =
        // Any better way to do this?
        let dimensionsString = t.FullName.Split([| '['; ']' |]).[1]
        let dimensions = ref 1
        String.iter (fun c -> if (c = ',') then dimensions := !dimensions + 1) dimensionsString
        !dimensions
        
    let GenerateSizeAdditionalArg (name:string, n:obj) =
         String.Format("{0}_length_{1}", name, n.ToString())

    let rec HandleParameterType (t) =
        if (t = typeof<uint32>) then
            "unsigned int"            
        elif (t = typeof<uint64>) then
            "unsigned long"        
        elif (t = typeof<int64>) then
            "long"               
        elif (t = typeof<int>) then
            "int"            
        elif (t = typeof<double>) then
            "double"
        elif (t = typeof<float32>) then
            "float"
        elif (t = typeof<bool>) then
            "int"
        elif (t = typeof<float>) then
            "float"
        else
            raise (KernelTransformationException("Invalid type used for a parameter of a kernel function " + t.ToString()))

    interface ParameterProcessor with
        member this.Handle(p, engine:KernelSignatureTransformationStage) =
            if p.ParameterType.IsArray then
                let dimensions = GetArrayDimensions(p.ParameterType)      
                let mutable data = engine.TransformationData("KERNEL_PARAMETER_TABLE")   
                if data.IsNone then 
                    raise (new KernelTransformationException("KERNEL_PARAMETER_TABLE global data cannot be found, but it is required by ParameterProcessor to execute"))
                               
                let castedData = data.Value :?> KernelParameterTable
                let entry = castedData.[p]
                entry.SizeParameters <- List.ofSeq (seq { for d = 0 to dimensions - 1 do yield GenerateSizeAdditionalArg(p.Name, d) })

                // If the parameters is tagged with Contant attribute, prepend constant keyword, else global
                let constantAttribute = p.GetCustomAttribute<FSCL.ConstantAttribute>()
                let localAttribute = p.GetCustomAttribute<FSCL.LocalAttribute>()
                if constantAttribute <> null then
                    entry.AddressSpace <- KernelParameterAddressSpace.ConstantSpace
                    (true, Some("constant " + HandleParameterType(p.ParameterType.GetElementType()) + "* " + p.Name))
                elif localAttribute <> null then
                    entry.AddressSpace <- KernelParameterAddressSpace.LocalSpace
                    (true, Some("local " + HandleParameterType(p.ParameterType.GetElementType()) + "* " + p.Name))
                else    
                    entry.AddressSpace <- KernelParameterAddressSpace.GlobalSpace
                    (true, Some("global " + HandleParameterType(p.ParameterType.GetElementType()) + "* " + p.Name))
            else
                (true, Some(HandleParameterType(p.ParameterType) + " " + p.Name))

            

