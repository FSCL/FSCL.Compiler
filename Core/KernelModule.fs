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

// Module
[<AllowNullLiteral>]
type KernelModule() =
    member val Functions = ([]:FunctionInfo list) with get, set
    member val Kernels = ([]:KernelInfo list) with get, set
    member val Directives = ([]:string list) with get, set
    member val GlobalTypes = ([]:Type list) with get, set
    member val Source:KernelInfo = null with get, set
    member val CustomInfo = new Dictionary<String, Object>() with get
    
