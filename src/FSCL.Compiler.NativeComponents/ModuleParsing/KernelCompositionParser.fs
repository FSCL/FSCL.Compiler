namespace FSCL.Compiler.ModuleParsing

open FSCL.Compiler
open FSCL.Compiler.Util
open System.Collections.Generic
open System.Reflection
open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open Microsoft.FSharp.Linq.RuntimeHelpers

open QuotationAnalysis.FunctionsManipulation
open QuotationAnalysis.KernelParsing
open QuotationAnalysis.MetadataExtraction
open ReflectionUtil

[<StepProcessor("FSCL_COMPOSITION_PARSING_PROCESSOR", "FSCL_MODULE_PARSING_STEP")>]
type KernelCompositionExpressionParser() =      
    inherit ModuleParsingProcessor()        
    
    let PipelineMethods = 
        [ ExtractMethodInfo(<@ (|>) @>).Value.TryGetGenericMethodDefinition(); 
            ExtractMethodInfo(<@ (||>) @>).Value.TryGetGenericMethodDefinition();
            ExtractMethodInfo(<@ (|||>) @>).Value.TryGetGenericMethodDefinition() ]
            
                    
    override this.Run(e, s, opts) =
        let step = s :?> ModuleParsingStep
        if e :? Expr then
            let norm = CompositionToCallOrApplication (e :?> Expr)
            Some(step.Process(norm))
        else
            None
            