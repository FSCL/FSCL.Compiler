namespace FSCL.Compiler

open System
open System.Reflection
open System.Collections.Generic
open Microsoft.FSharp.Quotations
open System.Collections.ObjectModel

type CompilerStepResult =
| ContinueCompilation of obj
| StopCompilation of obj

///
///<summary>
///The base type of every compiler step processor
///</summary>
///<remarks>
///Developers of step processors should not inherit from this class but from the generic CompilerStepProcessor and provide an implementation to the method "Run"
///</remarks>
///
type [<AbstractClass>] ICompilerStepProcessor() = 
    ///
    ///<summary>
    ///The method to be called to execute the processor
    ///</summary>
    ///<remarks>
    ///This method looks for a method called "Run" in the runtime time definition of this instance and invokes it using the provided parameter
    ///</remarks>
    ///<param name="obj">The input of the processor</param>
    ///<param name="owner">The owner step</param>
    ///<returns>The output produced by this processor</returns>
    /// 
    abstract member Execute: obj * ICompilerStep * IReadOnlyDictionary<string, obj> -> obj

///
///<summary>
///The base type of every compiler step
///</summary>
///<remarks>
///Developers of steps should not inherit from this class but from the generic CompilerStep and provide an implementation to the method "Run"
///</remarks>
///
and [<AbstractClass>] ICompilerStep(tm: TypeManager, processors:ICompilerStepProcessor list) =
    ///
    ///<summary>
    ///The compiler type manager
    ///</summary>
    /// 
    member val TypeManager = tm with get    
    ///
    ///<summary>
    ///The set of step processors
    ///</summary>
    /// 
    member val Processors = processors with get
    ///
    ///<summary>
    ///The method to be called to execute the step
    ///</summary>
    ///<remarks>
    ///This method looks for a method called "Run" in the runtime time definition of this instance and invokes it using the provided parameter
    ///</remarks>
    ///<param name="obj">The input of the step</param>
    ///<returns>The output produced by this step</returns>
    /// 
    abstract member Execute: obj * IReadOnlyDictionary<string, obj> -> CompilerStepResult
        
        
///
///<summary>
///The generic base class of compiler steps
///</summary>
///<typeparam name="T">The type of the step input</typeparam>
///<typeparam name="U">The type of the step output</typeparam>
/// 
[<AbstractClass>]
type CompilerStep<'T,'U>(tm, processors) =
    inherit ICompilerStep(tm, processors)    
    ///
    ///<summary>
    ///The abstract method that every step must implement to define the behavior of the step
    ///</summary>
    ///<param name="param0">An instance of type 'T</param>
    ///<returns>An instance of type 'U</returns>
    /// 
    abstract member Run: 'T * IReadOnlyDictionary<string, obj> -> CompilerStepResult
    
    override this.Execute(obj, opts) =        
        this.Run(obj :?> 'T, opts)
    
///
///<summary>
///The generic base class of compiler step processors
///</summary>
/// 
type [<AbstractClass>] CompilerStepProcessor<'T,'U>() =
    inherit ICompilerStepProcessor()

    override this.Execute(obj, step, opts) =
        this.Run(obj :?> 'T, step, opts) :> obj
    ///
    ///<summary>
    ///The abstract method that every step processors must implement to define the behavior of the processor
    ///</summary>
    ///<param name="param0">An instance of type 'T</param>
    ///<param name="param1">The owner step</param>
    ///<returns>An instance of type 'U</returns>
    /// 
    abstract member Run: 'T * ICompilerStep * IReadOnlyDictionary<string, obj> -> 'U
    
///
///<summary>
///The generic base class of step processors that don't produce any output
///</summary>
/// 
type [<AbstractClass>] CompilerStepProcessor<'T>() =
    inherit ICompilerStepProcessor()

    override this.Execute(obj, owner, opts) =
        this.Run(obj :?> 'T, owner, opts) :> obj 
    ///
    ///<summary>
    ///The abstract method that the step processors must implement to define their behaviour
    ///</summary>
    ///<param name="param0">An instance of type 'T</param>
    ///<param name="param1">The owner step</param>
    /// 
    abstract member Run: 'T * ICompilerStep * IReadOnlyDictionary<string, obj> -> unit
       
    
///
///<summary>
///Alias of unit
///</summary>
/// 
type NoResult = unit
///
///<summary>
///The type of the processors of the module parsing step. Alias of CompilerStepProcessor&lt;obj, KernelModule option&gt;
///</summary>
/// 
type ModuleParsingProcessor = CompilerStepProcessor<obj * Var list, IKFGNode option>
///
///<summary>
///The type of the metadata processors of the module parsing step. 
///</summary>
/// 
type MetadataFinalizerProcessor = CompilerStepProcessor<KernelMetaCollection * ParamMetaCollection * List<ParamMetaCollection> * Dictionary<String, obj>, KernelMetaCollection * ParamMetaCollection * List<ParamMetaCollection>>
///
///<summary>
///The type of the processors of the module preprocessing step. Alias of CompilerStepProcessor&lt;KernelModule&gt;
///</summary>
/// 
type ModulePreprocessingProcessor = CompilerStepProcessor<KernelModule>
///
///<summary>
///The type of the processors of the module preprocessing step. Alias of CompilerStepProcessor&lt;KernelModule&gt;
///</summary>
/// 
type ModulePostprocessingProcessor = CompilerStepProcessor<KernelModule>
///
///<summary>
///The type of the processors of the function preprocessing step. Alias of CompilerStepProcessor&lt;FunctionInfo * ConnectionsWrapper, ConnectionWrapper&gt;
///</summary>
/// 
type FunctionPreprocessingProcessor = CompilerStepProcessor<FunctionInfo>
///
///<summary>
///The type of the processors of the function postprocessing step. Alias of CompilerStepProcessor&lt;FunctionInfo * ConnectionsWrapper, ConnectionWrapper&gt;
///</summary>
/// 
type FunctionPostprocessingProcessor = CompilerStepProcessor<FunctionInfo>
///
///<summary>
///The type of the processors of the function transformation step. Alias of CompilerStepProcessor&lt;Expr, Expr&gt;
///</summary>
/// 
type FunctionTransformationProcessor = CompilerStepProcessor<Expr, Expr>
///
///<summary>
///The type of the (signature) processors of the function codegen step. Alias of CompilerStepProcessor&lt;MethodInfo, String option&gt;
///</summary>
/// 
type FunctionSignatureCodegenProcessor = CompilerStepProcessor<String * FunctionParameter list, String option>
///
///<summary>
///The type of the (body) processors of the function codegen step. Alias of CompilerStepProcessor&lt;Expr, String option&gt;
///</summary>
/// 
type FunctionBodyCodegenProcessor = CompilerStepProcessor<Expr, String option>
///
///<summary>
///The type of the processors of the module codegen step. Alias of CompilerStepProcessor&lt;KernelModule * String, String&gt;
///</summary>
/// 
type ModuleCodegenProcessor = CompilerStepProcessor<KernelModule * String, String>
    
    
    
