namespace FSCL.Compiler.Util
    open System.Reflection
    open FSCL.Compiler

    module DynamicAttributesUtil =
        type MethodInfo with
            member this.DynamicAttributes() =
                let dictionary = new DynamicKernelMetadataCollection()        
                for item in this.GetCustomAttributes() do
                    if typeof<DynamicKernelMetadataAttribute>.IsAssignableFrom(item.GetType()) then
                        dictionary.Add(item.GetType(), item :?> DynamicKernelMetadataAttribute)
                new ReadOnlyDynamicKernelMetadataCollection(dictionary)
                
            member this.DynamicAttribute<'T when 'T :> System.Attribute>() =
                this.GetCustomAttribute<'T>()
                
        type ParameterInfo with
            member this.DynamicAttributes() =
                let dictionary = new DynamicKernelMetadataCollection()        
                for item in this.GetCustomAttributes() do
                    if typeof<DynamicKernelMetadataAttribute>.IsAssignableFrom(item.GetType()) then
                        dictionary.Add(item.GetType(), item :?> DynamicKernelMetadataAttribute)
                new ReadOnlyDynamicKernelMetadataCollection(dictionary)
                
            member this.DynamicAttribute<'T when 'T :> System.Attribute>() =
                this.GetCustomAttribute<'T>()
        

