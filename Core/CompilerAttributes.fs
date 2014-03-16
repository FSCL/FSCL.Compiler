namespace FSCL.Compiler

open System

///
///<summary>
///The attribute used to describe a compiler step
///</summary>
/// 
[<AllowNullLiteral>]
type StepAttribute(i: string) =
    inherit Attribute()    
    ///
    ///<summary>
    ///The global ID of the step
    ///</summary>
    /// 
    member val ID = i with get
    ///
    ///<summary>
    ///The set of dependencies (i.e. the set of steps that must be executed before this one)
    ///</summary>
    /// 
    member val Dependencies: string array = [||] with get, set
    ///
    ///<summary>
    ///The set of steps that must be executed after this one
    ///</summary>
    /// 
    member val Before: string array = [||] with get, set
    ///
    ///<summary>
    ///The set of metadata that affect the result of this step
    ///</summary>
    /// 
    member val MetadataAffectingResult: Type array = [||] with get, set
        
///
///<summary>
///The attribute used to describe a compiler step processor
///</summary>
/// 
[<AllowNullLiteral>]   
type StepProcessorAttribute(i: string, s: string) =
    inherit Attribute()    
    ///
    ///<summary>
    ///The global ID of the processor
    ///</summary>
    /// 
    member val ID = i with get
    ///
    ///<summary>
    ///The global ID of the owner processor
    ///</summary>
    /// 
    member val Step = s with get
    ///
    ///<summary>
    ///The set of dependencies (i.e. the set of steps that must be executed before this one)
    ///</summary>
    ///      
    member val Dependencies: string array = [||] with get, set
    ///
    ///<summary>
    ///The set of steps that must be executed after this one
    ///</summary>
    /// 
    member val Before: string array = [||] with get, set
    ///
    ///<summary>
    ///The set of metadata that affect the result of this step
    ///</summary>
    /// 
    member val MetadataAffectingResult: Type array = [||] with get, set
  
[<AllowNullLiteral>]          
type TypeHandlerAttribute(i: string) =
    inherit Attribute()
    
    member val ID = i with get
    member val Dependencies: string array = [||] with get, set
    member val Before: string array = [||] with get, set
    
[<AllowNullLiteral>]          
[<AttributeUsage(AttributeTargets.Assembly)>]
type DefaultComponentAssembly() =
    inherit Attribute()
  
