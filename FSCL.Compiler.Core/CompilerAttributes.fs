namespace FSCL.Compiler

open System

[<AllowNullLiteral>]
type StepAttribute =
    inherit Attribute

    val id : string
    val dependencies: string[]
    val before: string[]
    val replace: string option

    new(i: string) =  {
        id = i
        dependencies = [||]
        before = [||]
        replace = None
    }    
    new(i: string, d: string[]) =  {
        id = i
        dependencies = d
        before = [||]
        replace = None
    }
    new(i: string, d: string[], b: string[]) =  {
        id = i
        dependencies = d
        before = b
        replace = None
    }
    new(i: string, r: string) =  {
        id = i
        dependencies = [||]
        before = [||]
        replace = Some(r)
    }
    new(i: string, r: string, d: string[]) =  {
        id = i
        dependencies = d
        before = [||]
        replace = Some(r)
    }
    new(i: string, r: string, d: string[], b: string[]) =  {
        id = i
        dependencies = d
        before = b
        replace = Some(r)
    }

    member this.Id 
        with get() = 
            this.id
    member this.Dependencies
        with get() = 
            this.dependencies
    member this.Before
        with get() = 
            this.before
    member this.Replace
        with get() = 
            this.replace
         
[<AllowNullLiteral>]   
type StepProcessorAttribute =
    inherit Attribute

    val step: string
    val id : string
    val dependencies: string[]
    val before: string[]
    val replace: string option

    new(i: string, s: string) =  {
        id = i
        step = s
        dependencies = [||]
        before = [||]
        replace = None
    }    
    new(i: string, s:string, d: string[]) =  {
        id = i
        step = s
        dependencies = d
        before = [||]
        replace = None
    }
    new(i: string, s:string, d: string[], b: string[]) =  {
        id = i
        step = s
        dependencies = d
        before = b
        replace = None
    }
    new(i: string, s:string, r: string) =  {
        id = i
        step = s
        dependencies = [||]
        before = [||]
        replace = Some(r)
    }
    new(i: string, s:string, r: string, d: string[]) =  {
        id = i
        step = s
        dependencies = d
        before = [||]
        replace = Some(r)
    }
    new(i: string, s:string, r: string, d: string[], b: string[]) =  {
        id = i
        step = s
        dependencies = d
        before = b
        replace = Some(r)
    }

    member this.Id 
        with get() = 
            this.id
    member this.Step 
        with get() = 
            this.step
    member this.Dependencies
        with get() = 
            this.dependencies
    member this.Before
        with get() = 
            this.before
    member this.Replace
        with get() = 
            this.replace
  
[<AllowNullLiteral>]          
type TypeHandlerAttribute(i: string) =
    inherit Attribute()
    
    member val Id = i with get


