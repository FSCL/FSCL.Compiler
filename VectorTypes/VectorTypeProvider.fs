namespace FSCL.Compiler.Types

open System
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Microsoft.FSharp.Reflection
open System.Diagnostics
open Microsoft.FSharp.Data.TypeProviders
open Microsoft.FSharp.TypeProvider.Emit
open CombinationGenerator
open System.IO


[<TypeProvider>]
type VectorTypeProvider(config: TypeProviderConfig) as this = 
    // Inheriting from this type provides implementations of ITypeProvider in terms of the
    // provided types below.
    inherit TypeProviderForNamespaces()
    let namespaceName = "FSCL.Compiler.Vectors"
    let thisAssembly = Assembly.GetExecutingAssembly()
    let componentIndexNames = [ "0"; "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "a"; "b"; "c"; "d"; "e" ]
    
    let createVectorTypes(thisAssembly, namespaceName, compSet: int list) = 
        let pt = List<int * ProvidedTypeDefinition>()
        for c in compSet do 
            pt.Add(c, ProvidedTypeDefinition(thisAssembly, namespaceName, "Float" + c.ToString() + "D", baseType = Some (typeof<FloatVector>)))
        List.ofSeq pt
        
    // Make one provided type, called TypeN
    let initVectorTypes(vectorTypes : (int * ProvidedTypeDefinition) list) = 
        for (componentCount, providedType) in vectorTypes do            
            let cNames = List<string>();
            cNames.Add("x")
            cNames.Add("y")
            if(componentCount >= 3) then
                cNames.Add("z")
            if(componentCount >= 4) then
                cNames.Add("w")
            let componentNames = List.ofSeq cNames    
                  
            let namedComponentCount = Math.Min(componentCount, 4)
            // Set properties to access components, including swizzle and subcomponents
            // I.e vector.xyzw, vector.wzxy, vector.xw
            // I.e. vector.s0012, vector.s2130 (index-based)
            // Only properties where an index or a letter is not repeated can be set (written)
            for i in [1 .. namedComponentCount] do
                let componentsCombinations = Generator.getPermsWithRep i [0 .. namedComponentCount - 1] 
                for c in componentsCombinations do
                    let combination = Array.ofList c
                    if (combination.Length = 1) then    
                        let index = combination.[0]
                        if(componentCount <= 4) then
                            let namedProperty = 
                                ProvidedProperty(propertyName = componentNames.[index],
                                                 propertyType = typeof<float32>, 
                                                 IsStatic = false,
                                                 GetterCode = (fun args -> <@@ 
                                                                            (%%args.[0]:FloatVector).Get(index)  
                                                 @@>),                                         
                                                 SetterCode = (fun args -> <@@ 
                                                                            (%%args.[0]:FloatVector).Set(index, (%%args.[1]:float32)) 
                                                 @@>))
                            providedType.AddMember namedProperty

                        let indexProperty = 
                            ProvidedProperty(propertyName = "s" + componentIndexNames.[index],
                                             propertyType = typeof<float32>, 
                                             IsStatic = false,
                                             GetterCode = (fun args -> <@@ 
                                                                        (%%args.[0]:FloatVector).Get(index)  
                                             @@>),                                         
                                             SetterCode = (fun args -> <@@ 
                                                                        (%%args.[0]:FloatVector).Set(index, (%%args.[1]:float32)) 
                                             @@>))
                        providedType.AddMember indexProperty
                    elif Generator.isDistinct combination then  
                        let indexPropertyName = combination |> Array.map(fun el -> string el) |> String.concat ""
                        if(componentCount <= 4) then
                            let propertyName = Generator.concatWithIndex componentNames c 
                            let namedProperty = 
                                ProvidedProperty(propertyName = propertyName,
                                                 propertyType = typeof<FloatVector>, 
                                                 IsStatic = false,
                                                 GetterCode = (fun args -> <@@ 
                                                                                let casted = (%%args.[0]:FloatVector)
                                                                                FloatVector(Seq.toArray (seq { for c in combination do yield casted.Get(c) }))
                                                                        
                                                 @@>),
                                                 SetterCode = (fun args -> <@@ 
                                                                                let casted = (%%args.[0]:FloatVector)
                                                                                let values = (%%args.[1]:FloatVector)
                                                                                for index = 0 to values.Count - 1 do
                                                                                    let comp_index = combination.[index]
                                                                                    casted.Set(comp_index, values.Get(index))                                                                        
                                                 @@>))                    
                            providedType.AddMember namedProperty
                    
                        let indexProperty = 
                            ProvidedProperty(propertyName = "s" + indexPropertyName,
                                             propertyType = typeof<FloatVector>, 
                                             IsStatic = false,
                                             GetterCode = (fun args -> <@@ 
                                                                            let casted = (%%args.[0]:FloatVector)
                                                                            FloatVector(Seq.toArray (seq { for c in combination do yield casted.Get(c) }))
                                                                        
                                             @@>),
                                             SetterCode = (fun args -> <@@ 
                                                                            let casted = (%%args.[0]:FloatVector)
                                                                            let values = (%%args.[1]:FloatVector)
                                                                            for index = 0 to values.Count - 1 do
                                                                                let comp_index = combination.[index]
                                                                                casted.Set(comp_index, values.Get(index))                                                                        
                                             @@>))
                        providedType.AddMember indexProperty
                    else  
                        let indexPropertyName = combination |> Array.map(fun el -> string el) |> String.concat ""
                        
                        if(componentCount <= 4) then
                            let propertyName = Generator.concatWithIndex componentNames c                     
                            let namedProperty = 
                                ProvidedProperty(propertyName = propertyName,
                                                 propertyType = typeof<FloatVector>, 
                                                 IsStatic = false,
                                                 GetterCode = (fun args -> <@@ 
                                                                                let casted = (%%args.[0]:FloatVector)
                                                                                FloatVector(Seq.toArray (seq { for c in combination do yield casted.Get(c) }))
                                                                            @@>))
                            providedType.AddMember namedProperty

                        let indexProperty = 
                            ProvidedProperty(propertyName = "s" + indexPropertyName,
                                             propertyType = typeof<FloatVector>, 
                                             IsStatic = false,
                                             GetterCode = (fun args -> <@@ 
                                                                            let casted = (%%args.[0]:FloatVector)
                                                                            FloatVector(Seq.toArray (seq { for c in combination do yield casted.Get(c) }))
                                                                        
                                             @@>))
                        providedType.AddMember indexProperty
       
            // Add even, odd, hi and lo properties
            if(componentCount = 2) then
                let evenProperty = ProvidedProperty(propertyName = "even",
                                                    propertyType = typeof<float32>, 
                                                    IsStatic = false,
                                                    GetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Get(0) 
                                                    @@>),                                         
                                                    SetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Set(0, (%%args.[1]:float32)) 
                                                    @@>))
                let oddProperty = ProvidedProperty(propertyName = "odd",
                                                   propertyType = typeof<float32>, 
                                                   IsStatic = false,
                                                   GetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Get(1)
                                                   @@>),                                         
                                                   SetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Set(1, (%%args.[1]:float32)) 
                                                   @@>))
                let loProperty = ProvidedProperty(propertyName = "lo",
                                                  propertyType = typeof<float32>, 
                                                  IsStatic = false,
                                                    GetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Get(0) 
                                                    @@>),                                         
                                                    SetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Set(0, (%%args.[1]:float32)) 
                                                    @@>))
                let hiProperty = ProvidedProperty(propertyName = "hi",
                                                   propertyType = typeof<float32>, 
                                                   IsStatic = false,
                                                   GetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Get(1)
                                                   @@>),                                         
                                                   SetterCode = (fun args -> <@@ 
                                                                                (%%args.[0]:FloatVector).Set(1, (%%args.[1]:float32)) 
                                                   @@>))
                providedType.AddMember evenProperty
                providedType.AddMember oddProperty
                providedType.AddMember loProperty
                providedType.AddMember hiProperty
            else
                let evenProperty = ProvidedProperty(propertyName = "even",
                                                    propertyType = typeof<FloatVector>, 
                                                    IsStatic = false,
                                                    GetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    FloatVector(Seq.toArray (seq { for index = 0 to casted.Count - 1 do if (index % 2 = 0) then yield casted.Get(index) })) 
                                                    @@>),                                         
                                                    SetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    let values = (%%args.[1]:FloatVector)
                                                                                    let values_index = ref 0
                                                                                    for index = 0 to casted.Count - 1 do
                                                                                        if(index % 2 = 0) then
                                                                                            casted.Set(index, values.Get(!values_index))
                                                                                            values_index := !values_index + 1
                                                    @@>))
                let oddProperty = ProvidedProperty(propertyName = "odd",
                                                   propertyType = typeof<FloatVector>, 
                                                   IsStatic = false,
                                                   GetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    FloatVector(Seq.toArray (seq { for index = 0 to casted.Count - 1 do if (index % 2 <> 0) then yield casted.Get(index) })) 
                                                   @@>),                                           
                                                   SetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    let values = (%%args.[1]:FloatVector)
                                                                                    let values_index = ref 0
                                                                                    for index = 0 to casted.Count - 1 do
                                                                                        if(index % 2 <> 0) then
                                                                                            casted.Set(index, values.Get(!values_index))
                                                                                            values_index := !values_index + 1
                                                   @@>))
                let halfCount = (int) (Math.Ceiling((float) componentCount / 2.0))
                let loProperty = ProvidedProperty(propertyName = "lo",
                                                  propertyType = typeof<FloatVector>, 
                                                  IsStatic = false,
                                                  GetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    FloatVector(Seq.toArray (seq { for index = 0 to halfCount - 1 do yield casted.Get(index) })) 
                                                  @@>),                                           
                                                  SetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    let values = (%%args.[1]:FloatVector)
                                                                                    let values_index = ref 0
                                                                                    for index = 0 to halfCount - 1 do
                                                                                        casted.Set(index, values.Get(!values_index))
                                                                                        values_index := !values_index + 1
                                                  @@>))
                let hiProperty = ProvidedProperty(propertyName = "hi",
                                                  propertyType = typeof<FloatVector>, 
                                                  IsStatic = false,                                              
                                                  GetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    FloatVector(Seq.toArray (seq { 
                                                                                                        for index = halfCount to (halfCount * 2) - 1 do 
                                                                                                            if(index < componentCount) then 
                                                                                                                yield casted.Get(index)
                                                                                                            else 
                                                                                                                yield 0.0f
                                                                                                 }))
                                                  @@>),                                           
                                                  SetterCode = (fun args -> <@@ 
                                                                                    let casted = (%%args.[0]:FloatVector)
                                                                                    let values = (%%args.[1]:FloatVector)
                                                                                    let values_index = ref 0
                                                                                    for index = halfCount to (halfCount * 2) - 1 do
                                                                                        if(index < componentCount) then
                                                                                            casted.Set(index, values.Get(!values_index))
                                                                                            values_index := !values_index + 1
                                                                                        else
                                                                                            casted.Set(index, 0.0f)
                                                  @@>))

                providedType.AddMember evenProperty
                providedType.AddMember oddProperty
                providedType.AddMember loProperty
                providedType.AddMember hiProperty
                (*
                providedType.AddMember(ProvidedMethod("op_Addition", 
                                               [ ProvidedParameter("v1", typeof<FloatVector>);
                                                 ProvidedParameter("v2", typeof<FloatVector>) ],
                                               typeof<FloatVector>,
                                               IsStaticMethod = true,
                                               InvokeCode = fun args -> <@@ 
                                                                            let v1 = (%%(args.[0]) : FloatVector)
                                                                            let v2 = (%%(args.[1]) : FloatVector)
                                                                            Array.ofSeq(seq {
                                                                                for i in 0 .. v1.Count - 1 do
                                                                                    yield (v1.Get(i) + v2.Get(i))
                                                                                }) @@>))                                                                               
                providedType.AddMember(ProvidedMethod("op_Subtraction", 
                                               [ ProvidedParameter("v1", typeof<FloatVector>);
                                                 ProvidedParameter("v2", typeof<FloatVector>) ],
                                               typeof<FloatVector>,
                                               IsStaticMethod = true,
                                               InvokeCode = fun args -> <@@ 
                                                                            let v1 = (%%(args.[0]) : FloatVector)
                                                                            let v2 = (%%(args.[1]) : FloatVector)
                                                                            Array.ofSeq(seq {
                                                                                for i in 0 .. v1.Count - 1 do
                                                                                    yield (v1.Get(i) - v2.Get(i))
                                                                                }) @@>))                                                                                              
                providedType.AddMember(ProvidedMethod("op_GreaterThan", 
                                               [ ProvidedParameter("v1", typeof<FloatVector>);
                                                 ProvidedParameter("v2", typeof<FloatVector>) ],
                                               typeof<IntVector>,
                                               IsStaticMethod = true,
                                               InvokeCode = fun args -> <@@ 
                                                                            let v1 = (%%(args.[0]) : FloatVector)
                                                                            let v2 = (%%(args.[1]) : FloatVector)
                                                                            IntVector(Array.ofSeq(seq {
                                                                                for i in 0 .. v1.Count - 1 do
                                                                                    yield (if v1.Get(i) > v2.Get(i) then -1 else 0)
                                                                                })) 
                                                                        @@>))
                providedType.AddMember(ProvidedMethod("op_LessThen", 
                                               [ ProvidedParameter("v1", typeof<FloatVector>);
                                                 ProvidedParameter("v2", typeof<FloatVector>) ],
                                               typeof<IntVector>,
                                               IsStaticMethod = true,
                                               InvokeCode = fun args -> <@@ 
                                                                            let v1 = (%%(args.[0]) : FloatVector)
                                                                            let v2 = (%%(args.[1]) : FloatVector)
                                                                            IntVector(Array.ofSeq(seq {
                                                                                for i in 0 .. v1.Count - 1 do
                                                                                    yield (if v1.Get(i) < v2.Get(i) then -1 else 0)
                                                                                }))
                                                                         @@>)) 
                providedType.AddMember(ProvidedMethod("op_GreaterThanOrEqual", 
                                               [ ProvidedParameter("v1", typeof<FloatVector>);
                                                 ProvidedParameter("v2", typeof<FloatVector>) ],
                                               typeof<IntVector>,
                                               IsStaticMethod = true,
                                               InvokeCode = fun args -> <@@ 
                                                                            let v1 = (%%(args.[0]) : FloatVector)
                                                                            let v2 = (%%(args.[1]) : FloatVector)
                                                                            IntVector(Array.ofSeq(seq {
                                                                                for i in 0 .. v1.Count - 1 do
                                                                                    yield (if v1.Get(i) >= v2.Get(i) then -1 else 0)
                                                                                }))
                                                                        @@>)) 
                providedType.AddMember(ProvidedMethod("op_LessThenOrEqual", 
                                               [ ProvidedParameter("v1", typeof<FloatVector>);
                                                 ProvidedParameter("v2", typeof<FloatVector>) ],
                                               typeof<IntVector>,
                                               IsStaticMethod = true,
                                               InvokeCode = fun args -> <@@ 
                                                                            let v1 = (%%(args.[0]) : FloatVector)
                                                                            let v2 = (%%(args.[1]) : FloatVector)
                                                                            IntVector(Array.ofSeq(seq {
                                                                                for i in 0 .. v1.Count - 1 do
                                                                                    yield (if v1.Get(i) <= v2.Get(i) then -1 else 0)
                                                                                }))
                                                                         @@>)) 
                providedType.AddMember(ProvidedMethod("op_Equality", 
                                               [ ProvidedParameter("v1", typeof<FloatVector>);
                                                 ProvidedParameter("v2", typeof<FloatVector>) ],
                                               typeof<IntVector>,
                                               IsStaticMethod = true,
                                               InvokeCode = fun args -> <@@ 
                                                                            let v1 = (%%(args.[0]) : FloatVector)
                                                                            let v2 = (%%(args.[1]) : FloatVector)
                                                                            IntVector(Array.ofSeq(seq {
                                                                                for i in 0 .. v1.Count - 1 do
                                                                                    yield (if v1.Get(i) = v2.Get(i) then -1 else 0)
                                                                                }))
                                                                        @@>)) 
               
            *)
            
            let ctor = ProvidedConstructor(parameters = [ ],
                                            InvokeCode= (fun args -> <@@ FloatVector(componentCount) @@>))
                                      
            // Add the provided constructor to the provided type.
            providedType.AddMember ctor
            let mutable provPars = List.ofSeq (seq { for i in 0 .. componentCount - 1 do yield ProvidedParameter("s" + i.ToString(), typeof<float32>) })
            if (provPars.Length = 2) then
                providedType.AddMember(ProvidedConstructor(parameters = provPars, 
                                                           InvokeCode= (fun args -> <@@ 
                                                                                        FloatVector([| (%%(args.[0]): float32); (%%(args.[1]): float32) |]) @@>)))
            if (provPars.Length = 3) then
                providedType.AddMember(ProvidedConstructor(parameters = provPars, 
                                                           InvokeCode= (fun args -> <@@ 
                                                                                        FloatVector([| (%%(args.[0]): float32); (%%(args.[1]): float32); (%%(args.[2]): float32) |]) @@>)))
            if (provPars.Length = 4) then
                providedType.AddMember(ProvidedConstructor(parameters = provPars, 
                                                           InvokeCode= (fun args -> <@@ 
                                                                                        FloatVector([| (%%(args.[0]): float32); (%%(args.[1]): float32); (%%(args.[2]): float32); (%%(args.[3]): float32) |]) @@>)))
            if (provPars.Length = 8) then
                providedType.AddMember(ProvidedConstructor(parameters = provPars, 
                                                           InvokeCode= (fun args -> <@@ 
                                                                                        FloatVector([| (%%(args.[0]): float32); (%%(args.[1]): float32); (%%(args.[2]): float32); (%%(args.[3]): float32); (%%(args.[4]): float32); (%%(args.[5]): float32); (%%(args.[6]): float32); (%%(args.[7]): float32) |]) @@>)))
            if (provPars.Length = 16) then
                providedType.AddMember(ProvidedConstructor(parameters = provPars, 
                                                           InvokeCode= (fun args -> <@@ 
                                                                                        FloatVector([| (%%(args.[0]): float32); (%%(args.[1]): float32); (%%(args.[2]): float32); (%%(args.[3]): float32); (%%(args.[4]): float32); (%%(args.[5]): float32); (%%(args.[6]): float32); (%%(args.[7]): float32); (%%(args.[8]): float32); (%%(args.[9]): float32); (%%(args.[10]): float32); (%%(args.[11]): float32); (%%(args.[12]): float32); (%%(args.[13]): float32); (%%(args.[14]): float32); (%%(args.[15]): float32) |]) @@>)))
                          
                
    // Now generate 100 types
    let zip = createVectorTypes(thisAssembly, namespaceName, [ 2; 4; 8 ])
    let types = zip |> List.unzip |> snd
    do
        zip |> initVectorTypes
        
    // And add them to the namespace
    do 
        this.AddNamespace(namespaceName, types)
                           
[<assembly:TypeProviderAssembly>] 
do() 