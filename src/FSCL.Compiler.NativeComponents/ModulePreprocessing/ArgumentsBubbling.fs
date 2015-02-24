namespace FSCL.Compiler.ModulePreprocessing

open FSCL.Compiler
open FSCL
open FSCL.Compiler.ModulePreprocessing
open System.Reflection.Emit
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Reflection
open System
    
type OutsiderReference =
| PropertyGetOutsider of Expr * Expr option * PropertyInfo * Expr list
| ValueOutsider of Expr * obj * Type

[<StepProcessor("FSCL_ARGS_BUBBLING_PROCESSOR", 
                "FSCL_MODULE_PREPROCESSING_STEP",
                Dependencies = [| "FSCL_FUNCTIONS_DISCOVERY_PROCESSOR" |])>] 
type ArgumentsBubblingProcessor() = 
    inherit ModulePreprocessingProcessor()

    let mutable OutsiderIndex = 0

    let GenerateOutsiderName() = 
        let v = "outsider_arg_" + OutsiderIndex.ToString()
        OutsiderIndex <- OutsiderIndex + 1
        v

    let FindOutsider (outsiders: List<Var * OutsiderReference>, o: OutsiderReference) = 
        outsiders |> Seq.tryFind(fun (_, oth) ->
                                    match oth, o with
                                    | PropertyGetOutsider(_, o1, p1, l1), PropertyGetOutsider(_, o2, p2, l2) ->
                                        if (o1.IsNone && o2.IsSome) || (o1.IsSome && o2.IsNone) then
                                            false
                                        else if (o1.IsSome && o2.IsSome) then
                                            if not(o1.Value.Equals(o2.Value)) then
                                                false
                                            else if p1 <> p2 then
                                                false
                                            else
                                                l1 = l2
                                        else
                                           if p1 <> p2 then
                                                false
                                            else
                                                l1 = l2
                                    | ValueOutsider(_, o1, t1), ValueOutsider(_, o2, t2) ->
                                        o1.Equals(o2)
                                    | _, _ ->
                                        false)

    let rec CollectOutsidersAndFixSignature(m: KernelModule, f:FunctionInfo) =
        let rec CollectReferencesToOutsiderValues(e:Expr, outsiders: List<Var * OutsiderReference>) =
            match e with
            | Patterns.Value(o, t) when t.IsArray ->
                let outs = ValueOutsider(e, o, t)
                let existing = FindOutsider(outsiders, outs)
                if existing.IsNone then
                    let name = GenerateOutsiderName()
                    let newVar = new Var(name, t)
                    outsiders.Add(newVar, outs)
                    Expr.Var(newVar)
                else
                    Expr.Var(existing.Value |> fst)             
            | Patterns.PropertyGet(o, p, l) when p.PropertyType.IsArray ->
                let outs = PropertyGetOutsider(e, o, p, l)
                let existing = FindOutsider(outsiders, outs)
                if existing.IsNone then
                    let name = GenerateOutsiderName()
                    let newVar = new Var(name, p.PropertyType)
                    outsiders.Add(newVar, outs)
                    Expr.Var(newVar)
                else
                    Expr.Var(existing.Value |> fst)    
            | ExprShape.ShapeVar(v) ->
                e
            | ExprShape.ShapeLambda(v, b) ->
                Expr.Lambda(v, CollectReferencesToOutsiderValues(b, outsiders))
            | ExprShape.ShapeCombination(o, l) ->
                ExprShape.RebuildShapeCombination(o, l |> List.map(fun e -> CollectReferencesToOutsiderValues(e, outsiders)))                
        
        let subOutsiders = new List<Var * OutsiderReference>() 
        for subf in f.CalledFunctions do
            // Process called functions recursively
            let outs = CollectOutsidersAndFixSignature(m, m.Functions.[subf] :?> FunctionInfo)

            // Add outsiders from this subfunction
            for v, o in outs do
                let existing = FindOutsider(subOutsiders, o)
                if existing.IsNone then
                    subOutsiders.Add(v, o)
                   
        // Check this function outsiders
        f.Body <- CollectReferencesToOutsiderValues(f.Body, subOutsiders)
        // Fix signature
        for v, o in subOutsiders do
            f.GeneratedParameters.Add(new FunctionParameter(v.Name, v, FunctionParameterType.OutsiderParameter, None))
        
        subOutsiders

    override this.Run(km, s, opts) =
        let step = s :?> ModulePreprocessingStep
        let outsiders = CollectOutsidersAndFixSignature(km, km.Kernel)
        // Bubbled up argument
        for o in outsiders do
            match o |> snd with
            | ValueOutsider(e, _, _) ->
                km.OutsiderArgs.Add(e)
            | PropertyGetOutsider(e, _, _, _) ->
                km.OutsiderArgs.Add(e)
                
            
             
            