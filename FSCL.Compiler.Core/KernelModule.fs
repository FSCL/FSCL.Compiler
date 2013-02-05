namespace FSCL

open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic

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

namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic

// Kernel parameters
type KernelParameterAddressSpace =
| GlobalSpace
| ConstantSpace
| LocalSpace
| PrivateSpace
| AutoSpace

type KernelParameterAccessMode =
| ReadOnly
| WriteOnly
| ReadWrite
| NoAccess

type KernelParameterInfo(parameterInfo:ParameterInfo) =
    member val Info = parameterInfo with get, set
    member val SizeParameters = ([]:KernelParameterInfo list) with get, set
    member val SizeParameterNames = ([]:string list) with get, set
    member val AddressSpace = KernelParameterAddressSpace.AutoSpace with get, set
    member val Access = KernelParameterAccessMode.NoAccess with get, set
    // For kernel return type
    member val Expr = None with get, set
    // Variable holding the parameter inside the kernel body
    member val Placeholder:Var option = None with get, set
    
type TemporaryKernelParameterInfo(parameterInfo:ParameterInfo) =
    member val Info = parameterInfo with get, set
    member val SizeParameters = ([]:string list) with get, set
    member val AddressSpace = KernelParameterAddressSpace.AutoSpace with get, set
    member val Access = KernelParameterAccessMode.NoAccess with get, set
    // For kernel return type
    member val Expr = None with get, set

// Kernels and called functions    
[<AllowNullLiteral>]
type FunctionInfo(source: MethodInfo, expr:Expr) =
    member val Source = source with get
    member val Signature = source with get, set
    member val Body = expr with get, set
    member val PrettyPrinting = "" with get, set
    member val ParameterInfo = new Dictionary<String, KernelParameterInfo>() with get
    member val CustomInfo = new Dictionary<String, Object>() with get
    
[<AllowNullLiteral>]
type KernelInfo(methodInfo: MethodInfo, expr:Expr) =
    inherit FunctionInfo(methodInfo, expr)
    member this.DeviceAttribute 
        with get() =
            (*if this.Metadata.ContainsKey("DEVICE_INDEX") then 
                this.Metadata.["DEVICE_INDEX"] :?> int * int
            else
                (-1, -1)*)
            let kernelAttribute = this.Source.GetCustomAttribute<FSCL.KernelAttribute>()    
            if kernelAttribute.Device >= 0 && kernelAttribute.Platform >= 0 then       
                (kernelAttribute.Platform, kernelAttribute.Device)
            else
                (-1, -1)

// Module
[<AllowNullLiteral>]
type KernelModule() =
    member val Functions = ([]:FunctionInfo list) with get, set
    member val Kernels = ([]:KernelInfo list) with get, set
    member val Directives = ([]:string list) with get, set
    member val Source:KernelInfo = null with get, set
    member val CustomInfo = new Dictionary<String, Object>() with get
    
