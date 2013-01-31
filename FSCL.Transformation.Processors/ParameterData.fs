namespace FSCL.Transformation.Processors

open FSCL.Transformation
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System

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
    member val Info = parameterInfo with get
    member val SizeParameters = ([]:string list) with get, set
    member val AddressSpace = KernelParameterAddressSpace.AutoSpace with get, set
    member val Access = KernelParameterAccessMode.NoAccess with get, set
    // For kernel return type
    member val Expr = None with get, set

type KernelParameterTable = Dictionary<ParameterInfo, KernelParameterInfo>