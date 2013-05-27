namespace FSCL.Compiler

open System

[<AllowNullLiteral>]
type StepAttribute(i: string) =
    inherit Attribute()

    member val ID = i with get
    member val Dependencies: string array = [||] with get, set
    member val Before: string array = [||] with get, set
         
[<AllowNullLiteral>]   
type StepProcessorAttribute(i: string, s: string) =
    inherit Attribute()
    
    member val ID = i with get
    member val Step = s with get
    member val Dependencies: string array = [||] with get, set
    member val Before: string array = [||] with get, set
  
[<AllowNullLiteral>]          
type TypeHandlerAttribute(i: string) =
    inherit Attribute()
    
    member val ID = i with get
    member val Before: string array = [||] with get, set
    
[<AllowNullLiteral>]          
[<AttributeUsage(AttributeTargets.Assembly)>]
type DefaultComponentAssembly() =
    inherit Attribute()
  
