namespace FSCL.Compiler.Util

open FSCL.Compiler
open System.Collections.Generic
open System.IO
open System
open System.Diagnostics

module VerboseCompilationUtil =
    let StartVerboseStep(step: ICompilerStep, opts: IReadOnlyDictionary<string, obj>) =
        let verb = 
            if opts.ContainsKey(CompilerOptions.VerboseLevel) then
                opts.[CompilerOptions.VerboseLevel] :?> int
            else
                0
        if verb > 0 then
            let timer = new Stopwatch()
            Console.WriteLine("  -- " + String.Format("{0,-28}", step.GetType().Name.ToString()) + " [ " + String.Format("{0,3:###}", step.Processors.Length)  + " procs ] --") 
            timer.Start()
            Some(timer, step, opts)
        else
            None

    let StopVerboseStep(data: (Stopwatch * ICompilerStep * IReadOnlyDictionary<string, obj>) option) =
        if data.IsSome then
            let timer, step, opts = data.Value
            timer.Stop()
            Console.WriteLine("  -- Exec time:                    " + String.Format("{0,10:#########0}", timer.ElapsedMilliseconds) + "ms --") 

    let StartVerboseProcessor(proc: ICompilerStepProcessor, opts: IReadOnlyDictionary<string, obj>) =    
        let verb = 
            if opts.ContainsKey(CompilerOptions.VerboseLevel) then
                opts.[CompilerOptions.VerboseLevel] :?> int
            else
                0
        if verb > 1 then
            let timer = new Stopwatch()
            Console.Write("      -- " +  String.Format("{0,-30}", proc.GetType().Name.ToString()))
            timer.Start()
            Some(timer, proc, opts)
        else
            None
            
    let StopVerboseProcessor(data: (Stopwatch * ICompilerStepProcessor * IReadOnlyDictionary<string, obj>) option) =
        if data.IsSome then
            let timer, proc, opts = data.Value
            timer.Stop()
            Console.WriteLine("    -- Exec time:                        " + String.Format("{0,10:#########0}", timer.ElapsedMilliseconds) + "ms --") 
                    
    let StartVerboseCompiler(stepsCount: int, opts: IReadOnlyDictionary<string, obj>) =    
        let verb = 
            if opts.ContainsKey(CompilerOptions.VerboseLevel) then
                opts.[CompilerOptions.VerboseLevel] :?> int
            else
                0
        if verb > 0 then
            let timer = new Stopwatch()
            Console.WriteLine("--------------------------------------------------")
            Console.WriteLine("-- FSCL Compiler                  [ " + String.Format("{0,3:###}", stepsCount) + " steps ] --") 
            timer.Start()
            Some(timer, stepsCount, opts)
        else
            None
            
    let StopVerboseCompiler(data: (Stopwatch * int * IReadOnlyDictionary<string, obj>) option) =
        if data.IsSome then
            let timer, dteps, opts = data.Value
            timer.Stop()
            Console.WriteLine("-- Total Exec time:                " + String.Format("{0,10:#########0}", timer.ElapsedMilliseconds) + "ms --") 
            Console.WriteLine("--------------------------------------------------")

        