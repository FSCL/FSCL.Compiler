namespace FSCL

// Expose this Attrbute via the FSCL namespace
open System

type KernelAttribute =
    inherit Attribute

    val platform: int option
    val dev : int option

    new(p: int, d: int) =  { 
        platform = Some(p)
        dev = Some(d) 
    }
    new() =  { 
        platform = None
        dev = None
    }

    member this.Platform 
        with get() = if(this.platform.IsSome) then this.platform.Value else -1
    member this.Device
        with get() = if(this.dev.IsSome) then this.dev.Value else -1


namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations

type KernelByNameDiscoveryProcessor() =   
    let rec GetKernelFromName(expr, k:KernelDiscoveryStage) =                    
        match expr with
        | Patterns.Lambda(v, e) -> 
            GetKernelFromName (e, k)
        | Patterns.Let (v, e1, e2) ->
            GetKernelFromName (e2, k)
        | Patterns.Call (e, i, a) ->
            match i with
            | DerivedPatterns.MethodWithReflectedDefinition(b) ->
                let kernelAttribute = i.GetCustomAttribute<FSCL.KernelAttribute>()  
                if kernelAttribute.Device >= 0 && kernelAttribute.Platform >= 0 then                       
                    k.AddTransformationData("KERNEL_PLATFORM", kernelAttribute.Platform.ToString())   
                    k.AddTransformationData("KERNEL_DEVICE", kernelAttribute.Device.ToString())        
                i
            | _ ->
                raise (KernelTransformationException("A kernel definition must provide a function marked with ReflectedDefinition attribute"))
        | _-> 
            raise (KernelTransformationException("Cannot find a kernel function definition inside the expression"))

        
    interface DiscoveryProcessor with
        member this.Handle(expr, engine:KernelDiscoveryStage) =
            (true, Some(GetKernelFromName(expr, engine)))
            