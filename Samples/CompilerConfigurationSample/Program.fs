// Begin sample
open FSCL.Compiler.FunctionPreprocessing
open FSCL.Compiler.FunctionCodegen
open FSCL.Compiler.FunctionTransformation
open FSCL.Compiler.ModuleParsing
open FSCL.Compiler.ModulePreprocessing
open FSCL.Compiler.ModuleCodegen
open FSCL.Compiler.Types

open FSCL.Compiler
open FSCL.Compiler.Configuration
open System.IO

[<EntryPoint>]
let main argv = 
    // ***************************************************************************************************************
    //
    // 01: File-based compiler configuration (storage)
    //
    // ***************************************************************************************************************    
    printf("01) Test file-based compiler configuration (storage)\n")
    if Directory.Exists(CompilerConfigurationManager.ConfigurationRoot) then
        printf "    Compiler configuration root exists: %s\n" CompilerConfigurationManager.ConfigurationRoot
    else
        printf "    Compiler configuration root doesn't exist: %s %s\n" CompilerConfigurationManager.ConfigurationRoot 
               "\n    Please create it, copy some components into it and restart the sample\n"
    let compiler = Compiler()
            
    // ***************************************************************************************************************
    //
    // 01: File-based compiler configuration (configuration file implicit)
    //
    // ***************************************************************************************************************    
    printf("02) Test file-based compiler configuration (configuration file)\n")
    printf "    Using configuration file: ImplicitConfiguration.xml"
    let compiler2 = Compiler("ImplicitConfiguration.xml")

    // ***************************************************************************************************************
    //
    // 03: Object-based compiler configuration (implicit file sources)
    //
    // ***************************************************************************************************************
    printf("03) Test object-based compiler configuration (implicit file sources)\n")
    let configuration = CompilerConfiguration(false, // false = Do not load core sources (explicitely listed as second parameter)
                                              [
                                                SourceConfiguration(
                                                    FileSource(Path.Combine(
                                                                CompilerConfigurationManager.ComponentsRoot,
                                                                "FSCL.Compiler.Core.FunctionPreprocessing.dll")));
                                                SourceConfiguration(
                                                    FileSource(Path.Combine(
                                                                CompilerConfigurationManager.ComponentsRoot,
                                                                "FSCL.Compiler.Core.FunctionPrettyPrinting.dll")));
                                                SourceConfiguration(
                                                    FileSource(Path.Combine(
                                                                CompilerConfigurationManager.ComponentsRoot,
                                                                "FSCL.Compiler.Core.FunctionTransformation.dll")));
                                                SourceConfiguration(
                                                    FileSource(Path.Combine(
                                                                CompilerConfigurationManager.ComponentsRoot,
                                                                "FSCL.Compiler.Core.ModuleParsing.dll")));
                                                SourceConfiguration(
                                                    FileSource(Path.Combine(
                                                                CompilerConfigurationManager.ComponentsRoot,
                                                                "FSCL.Compiler.Core.ModulePreprocessing.dll")));
                                                SourceConfiguration(
                                                    FileSource(Path.Combine(
                                                                CompilerConfigurationManager.ComponentsRoot,
                                                                "FSCL.Compiler.Core.ModulePrettyPrinting.dll")));
                                                SourceConfiguration(
                                                    FileSource(Path.Combine(
                                                                CompilerConfigurationManager.ComponentsRoot,
                                                                "FSCL.Compiler.Core.Types.dll")))
                                              ])
    let compiler3 = Compiler(configuration)
                                              
    // ***************************************************************************************************************
    //
    // 04: Object-based compiler configuration (implicit assembly sources)
    //
    // ***************************************************************************************************************
    printf("04) Test object-based compiler configuration (implicit assembly sources)\n")
    let configuration = CompilerConfiguration(false, // false = Do not load core sources (explicitely listed as second parameter)
                                              [
                                                SourceConfiguration(
                                                    AssemblySource(typeof<FunctionPreprocessingStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<FunctionTransformationStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<FunctionCodegenStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<ModuleParsingStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<ModulePreprocessingStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<ModuleCodegenStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<DefaultTypeHandler>.Assembly))
                                              ])
    let compiler4 = Compiler(configuration)
                                                                                      
    // ***************************************************************************************************************
    //
    // 05: Object-based compiler configuration (implicit assembly sources except one source, explicit)
    //     All the sources could be made explicit (much verbose for all the core components)
    //
    // ***************************************************************************************************************
    printf("05) Test object-based compiler configuration (explicit assembly source)\n")
    let configuration = CompilerConfiguration(false, // false = Do not load core sources (explicitely listed as second parameter)
                                              [
                                                SourceConfiguration(                                                                // Explicit source
                                                    AssemblySource(typeof<FunctionPreprocessingStep>.Assembly),                     // The assembly
                                                    [],                                                                             // No type handlers
                                                    [ StepConfiguration("FSCL_FUNCTION_PREPROCESSING_STEP",                         // Explicitely define steps
                                                                        typeof<FunctionPreprocessingStep>,
                                                                        [ "FSCL_MODULE_PREPROCESSING_STEP"; "FSCL_MODULE_PARSING_STEP" ]) ],
                                                    [ StepProcessorConfiguration("FSCL_RTTOA_PREPROCESSING_PROCESSOR",     // Explicitely define processors
                                                                                 "FSCL_FUNCTION_PREPROCESSING_STEP",
                                                                                 typeof<ReturnTypeToOutputArgProcessor>);
                                                      StepProcessorConfiguration("FSCL_ALAI_PREPROCESSING_PROCESSOR", 
                                                                                 "FSCL_FUNCTION_PREPROCESSING_STEP",
                                                                                 typeof<ArrayLengthArgsInsertionProcessor>,
                                                                                 ["FSCL_RTTOA_PREPROCESSING_PROCESSOR"]);
                                                      StepProcessorConfiguration("FSCL_RTTAR_PREPROCESSING_PROCESSOR", 
                                                                                 "FSCL_FUNCTION_PREPROCESSING_STEP",
                                                                                 typeof<RefTypeToArrayReplacingProcessor>,
                                                                                 ["FSCL_ALAI_PREPROCESSING_PROCESSOR"])]);
                                                SourceConfiguration(
                                                    AssemblySource(typeof<FunctionTransformationStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<FunctionCodegenStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<ModuleParsingStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<ModulePreprocessingStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<ModuleCodegenStep>.Assembly));
                                                SourceConfiguration(
                                                    AssemblySource(typeof<DefaultTypeHandler>.Assembly))
                                              ])
                                              
    let compiler5 = Compiler(configuration)

    0