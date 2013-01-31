namespace FSCL

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open System.Diagnostics

// This defines the type provider. When compiled to a DLL it can be added as a reference to an F#
// command-line compilation, script or project.
[<TypeProvider>]
type OpenCLVectorTypeProvider(config: TypeProviderConfig) as this = 
    // Inheriting from this type provides implementations of ITypeProvider in terms of the
    // provided types below.
    inherit TypeProviderForNamespaces()

    let namespaceName = "FSCL"
    let thisAssembly = Assembly.GetExecutingAssembly()

    // Make one provided type, called TypeN
    let createVectorTypes(compSet: int list) = 
        List.map (fun components -> ProvidedTypeDefinition(thisAssembly, namespaceName, "OpenCLVector" + string components + "D", baseType = Some typeof<float[]>)) compSet |> List.zip compSet
        
    let initVectorTypes(vectorTypes : (int * ProvidedTypeDefinition) list) = 
        for vectorTypeIndex = 0 to vectorTypes.Length - 1 do
            let componentCount = fst(vectorTypes.[vectorTypeIndex]);
            let providedType = snd(vectorTypes.[vectorTypeIndex]);
            
            let mutable componentNames = [ "x"; "y"; "z"; "w" ];
            while componentNames.Length > componentCount do
                componentNames <- List.tail componentNames     
                  
            // This is the provided type. It is an erased provided type, and in compiled code 
            // will appear as type 'obj'.
            let namedComponentCount = Math.Max(componentCount, 4);
       
            // Set properties to access components, including swizzle and subcomponents
            // I.e vector.xyzw, vector.wzxy, vector.xw
            // I.e. vector.s0012, vector.s2130 (index-based)
            // Only properties where an index or a letter is not repeated can be set (written)
            for i in [1 .. namedComponentCount] do
                let componentsCombinations = VectorTypeUtil.getPermsWithRep i [0 .. namedComponentCount - 1] 
                for combination in componentsCombinations do
                    if (combination.Length = 1) then    
                        let index = combination.[0]
                        let namedProperty = 
                            ProvidedProperty(propertyName = componentNames.Item index,
                                             propertyType = typeof<float>, 
                                             IsStatic = false,
                                             GetterCode = (fun args -> <@@ 
                                                                            (((%%args.[0]:float[]) :> obj) :?> float[]).[index] 
                                             @@>),                                         
                                             SetterCode = (fun args -> <@@ 
                                                                            (((%%args.[0]:float[]) :> obj) :?> float[]).[index] <- (((%%args.[1]:float) :> obj) :?> float) 
                                             @@>))
                        let indexProperty = 
                            ProvidedProperty(propertyName = "s" + string index,
                                             propertyType = namedProperty.PropertyType, 
                                             IsStatic = namedProperty.IsStatic,
                                             GetterCode = (fun args -> <@@ 
                                                                            (((%%args.[0]:float[]) :> obj) :?> float[]).[index] 
                                             @@>),                                         
                                             SetterCode = (fun args -> <@@ 
                                                                            (((%%args.[0]:float[]) :> obj) :?> float[]).[index] <- (((%%args.[1]:float) :> obj) :?> float) 
                                             @@>))
                    
                        providedType.AddMember namedProperty
                        providedType.AddMember indexProperty
                    elif VectorTypeUtil.isDistinct combination then  
                        let propertyName = VectorTypeUtil.concatWithIndex componentNames combination 
                        let indexPropertyName = combination |> List.map(fun el -> string el) |> String.concat ""
                        let namedProperty = 
                            ProvidedProperty(propertyName = propertyName,
                                             propertyType = typeof<float>.MakeArrayType(), 
                                             IsStatic = false,
                                             GetterCode = (fun args -> <@@ 
                                                                            let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                            Seq.toArray (seq { for c in combination do yield casted.[c] })
                                                                        
                                             @@>),
                                             SetterCode = (fun args -> <@@ 
                                                                            let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                            let values = (((%%args.[1]:float[]) :> obj) :?> float[])
                                                                            for index = 0 to values.Length - 1 do
                                                                                let comp_index = combination.[index]
                                                                                casted.[comp_index] <- values.[index]
                                                                        
                                             @@>))
                    
                        let indexProperty = 
                            ProvidedProperty(propertyName = "s" + indexPropertyName,
                                             propertyType = namedProperty.PropertyType, 
                                             IsStatic = namedProperty.IsStatic,
                                             GetterCode = (fun args -> <@@ 
                                                                            let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                            Seq.toArray (seq { for c in combination do yield casted.[c] })
                                                                        
                                             @@>),
                                             SetterCode = (fun args -> <@@ 
                                                                            let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                            let values = (((%%args.[1]:float[]) :> obj) :?> float[])
                                                                            for index = 0 to values.Length - 1 do
                                                                                let comp_index = combination.[index]
                                                                                casted.[comp_index] <- values.[index]
                                                                        
                                             @@>))
                    
                        providedType.AddMember namedProperty
                        providedType.AddMember indexProperty
                    else  
                        let propertyName = VectorTypeUtil.concatWithIndex componentNames combination 
                        let indexPropertyName = combination |> List.map(fun el -> string el) |> String.concat ""
                        let namedProperty = 
                            ProvidedProperty(propertyName = propertyName,
                                             propertyType = typeof<float>.MakeArrayType(), 
                                             IsStatic = false,
                                             GetterCode = (fun args -> <@@ 
                                                                            let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                            Seq.toArray (seq { for c in combination do yield casted.[c] }) @@>))
                                        
                        let indexProperty = 
                            ProvidedProperty(propertyName = "s" + indexPropertyName,
                                             propertyType = namedProperty.PropertyType, 
                                             IsStatic = namedProperty.IsStatic,
                                             GetterCode = (fun args -> <@@ 
                                                                            let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                            Seq.toArray (seq { for c in combination do yield casted.[c] })
                                                                        
                                             @@>))
                    
                        providedType.AddMember namedProperty
                        providedType.AddMember indexProperty
       
            // Add even, odd, hi and lo properties
            if(componentCount = 2) then
                let evenProperty = ProvidedProperty(propertyName = "even",
                                                    propertyType = typeof<float>, 
                                                    IsStatic = false,
                                                    GetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[0] 
                                                    @@>),                                         
                                                    SetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[0] <- (((%%args.[1]:float) :> obj) :?> float) 
                                                    @@>))
                let oddProperty = ProvidedProperty(propertyName = "odd",
                                                   propertyType = typeof<float>, 
                                                   IsStatic = false,
                                                   GetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[1] 
                                                   @@>),                                         
                                                   SetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[1] <- (((%%args.[1]:float) :> obj) :?> float) 
                                                   @@>))
                let loProperty = ProvidedProperty(propertyName = "lo",
                                                  propertyType = typeof<float>, 
                                                  IsStatic = false,
                                                  GetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[0] 
                                                  @@>),                                         
                                                  SetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[0] <- (((%%args.[1]:float) :> obj) :?> float) 
                                                  @@>))
                let hiProperty = ProvidedProperty(propertyName = "hi",
                                                  propertyType = typeof<float>, 
                                                  IsStatic = false,
                                                  GetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[1] 
                                                  @@>),                                         
                                                  SetterCode = (fun args -> <@@ 
                                                                                (((%%args.[0]:float[]) :> obj) :?> float[]).[1] <- (((%%args.[1]:float) :> obj) :?> float) 
                                                  @@>))
                providedType.AddMember evenProperty
                providedType.AddMember oddProperty
                providedType.AddMember loProperty
                providedType.AddMember hiProperty
            else
                let evenProperty = ProvidedProperty(propertyName = "even",
                                                    propertyType = typeof<float>.MakeArrayType(), 
                                                    IsStatic = false,
                                                    GetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    Seq.toArray (seq { for index = 0 to casted.Length - 1 do if (index % 2 = 0) then yield casted.[index] }) 
                                                    @@>),                                         
                                                    SetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    let values = (((%%args.[1]:float[]) :> obj) :?> float[])
                                                                                    let values_index = ref 0
                                                                                    for index = 0 to casted.Length - 1 do
                                                                                        if(index % 2 = 0) then
                                                                                            casted.[index] <- values.[!values_index]
                                                                                            values_index := !values_index + 1
                                                    @@>))
                let oddProperty = ProvidedProperty(propertyName = "odd",
                                                   propertyType = typeof<float>.MakeArrayType(), 
                                                   IsStatic = false,
                                                   GetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    Seq.toArray (seq { for index = 0 to casted.Length - 1 do if (index % 2 <> 0) then yield casted.[index] }) 
                                                   @@>),                                           
                                                   SetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    let values = (((%%args.[1]:float[]) :> obj) :?> float[])
                                                                                    let values_index = ref 0
                                                                                    for index = 0 to casted.Length - 1 do
                                                                                        if(index % 2 <> 0) then
                                                                                            casted.[index] <- values.[!values_index]
                                                                                            values_index := !values_index + 1
                                                   @@>))
                let halfCount = (int) (Math.Ceiling(float componentCount / 2.0))
                let loProperty = ProvidedProperty(propertyName = "lo",
                                                  propertyType = typeof<float>.MakeArrayType(), 
                                                  IsStatic = false,
                                                  GetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    Seq.toArray (seq { for index = 0 to halfCount - 1 do yield casted.[index] }) 
                                                  @@>),                                           
                                                  SetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    let values = (((%%args.[1]:float[]) :> obj) :?> float[])
                                                                                    let values_index = ref 0
                                                                                    for index = 0 to halfCount - 1 do
                                                                                        casted.[index] <- values.[!values_index]
                                                                                        values_index := !values_index + 1
                                                  @@>))
                let hiProperty = ProvidedProperty(propertyName = "hi",
                                                  propertyType = typeof<float>.MakeArrayType(), 
                                                  IsStatic = false,                                              
                                                  GetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    Seq.toArray (seq { 
                                                                                                        for index = halfCount to (halfCount * 2) - 1 do 
                                                                                                            if(index < componentCount) then 
                                                                                                                yield casted.[index]
                                                                                                            else 
                                                                                                                yield 0.
                                                                                                 }) 
                                                  @@>),                                           
                                                  SetterCode = (fun args -> <@@ 
                                                                                    let casted = (((%%args.[0]:float[]) :> obj) :?> float[])
                                                                                    let values = (((%%args.[1]:float[]) :> obj) :?> float[])
                                                                                    let values_index = ref 0
                                                                                    for index = halfCount to (halfCount * 2) - 1 do
                                                                                        if(index < componentCount) then
                                                                                            casted.[index] <- values.[!values_index]
                                                                                            values_index := !values_index + 1
                                                                                        else
                                                                                            casted.[index] <- 0.
                                                  @@>))
                providedType.AddMember evenProperty
                providedType.AddMember oddProperty
                providedType.AddMember loProperty
                providedType.AddMember hiProperty

            let defValue = Array.CreateInstance(typeof<float>, ([| componentCount |]: int[]))
            let ctor = ProvidedConstructor(parameters = [ ], 
                                            InvokeCode= (fun args -> <@@ defValue :?> float[] @@>))
                                      
            // Add the provided constructor to the provided type.
            providedType.AddMember ctor

    // Now generate 100 types
    let zip = [ 4 ] |> createVectorTypes
    let types = zip |> List.unzip |> snd
    do
        zip |> initVectorTypes

    // And add them to the namespace
    do 
        this.AddNamespace(namespaceName, types)
                           
[<assembly:TypeProviderAssembly>] 
do()
