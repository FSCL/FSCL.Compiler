namespace FSCL.Compiler.FunctionPreprocessing

open FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open System.Reflection.Emit
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System

[<StepProcessor("FSCL_REF_TYPE_TO_ARRAY_REPLACING_PREPROCESSING_PROCESSOR", "FSCL_FUNCTION_PREPROCESSING_STEP",
                Dependencies = [|"FSCL_ARRAY_PARAMETERS_MANIPULATION_PREPROCESSING_PROCESSOR"|])>] 
type RefTypeToArrayReplacingProcessor() =        
    inherit FunctionPreprocessingProcessor()
    let IsRef(t:Type) =
        if (t.IsGenericType && (t.GetGenericTypeDefinition() = typeof<Ref<_>>.GetGenericTypeDefinition())) then
            true
        else 
            false

    override this.Run(fi, en) =
        // Get kernel info
        let kernelInfo = fi

        // Transform each ref variable in an array of 1 element
        for p in fi.Parameters do
            let t = p.Type
            if IsRef(t) then
                let newType = (FSharpType.GetRecordFields(t)).[0].PropertyType.MakeArrayType()
                // Generate new placeholder
                p.Placeholder <- Some(Quotations.Var(p.Name, newType, false))                
                p.Type <- (FSharpType.GetRecordFields(t)).[0].PropertyType.MakeArrayType()

            
