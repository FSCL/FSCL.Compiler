(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
Compiler Configuration
========================

The FSCL compiler pipeline is composed of a set of steps that progressively transform F# expressions into OpenCL code.
Even if the compiler is thought to work straight out-of-the-box by simply instantiating it using the parameterless constructor `new Compiler()`,
the pipeline can be fully changed and customised as needed.
In this page we give an overview on how the compiler pipeline configuration works.

###Steps, processors and type managers

Before digging into the configuration infrastructure, a small introduction and a little terminology is needed.
There are three types of *components* that contribute to the compiler behavior.

+ **Steps**: a step is a stage of the compiler pipeline. Generally, the step is logically represents a particular 
transformation or processing
+ **Processors**: each step of the pipeline is made of one or more processors, each of which is contributing to realize
a particular subset of the whole funcionality of the step
+ **Type handlers**: a type handler is resposible to validate and generate the target representation of types

For example, the *parsing step* (or parser) is the first step of the native pipeline whose task is to extract/create a `MethodInfo` for the
kernel and to determine the set of kernel parameters starting from a quoted expression.
To do this, the parser orchestrates a set of processors, each of which is responsible of parsing a particular quoted
construct, such as kernel calls, kernel references and collection function (e.g. `Array.map`) calls.

An example of type handler is the *Array type handler*, whose purpose is to determine is a particular array type is
valid and to generate the target code (e.g. `float*`) from the .NET type (e.g. `float32[]`).

Steps, processors and type handlers are characterised by some properties that allow to determine their interrelated order
of execution.
For example, each step is decribed by:

+ **ID**: the step unique string identifier
+ **Dependencies**: the list of IDs of steps that should execute before the current one
+ **Before**: the list of IDs of steps that should execute after the current one

Processors and type handlers have the same set of properties. In addition, each processor has a *Step* property that holds the ID of the
step the processor belongs to.
Having both *dependencies* and *before* may sound redundant, but it comes in handy whenever various dlls containing components 
(generally called *plugins* ) are published, shared and used in different environments.
In fact, plugin developers cannot take into account all the past, present and future components that may populate
the compiler pipeline in the target environment.
Given this, developers generally specify only the dependencies of custom components from the native compiler components.
Sometimes it happens that two plugins (potentially from different authors) cannot be evaluated in arbitrary order, but instead
have an inter-dependency. For example, a programmer may create a custom step that converts objects into structs to be plugged into a pipeline
already containing another custom step that performs some optimisations on structs. Since the programmer cannot modify the pre-existing custom step
(it's a dll placed somewhere), he will add the ID of the structs-optimisation step to the *Before* list of the step he's developing to guarantee
the pre-existing step is executed only after his one.

###Native configuration

Whenever you instantiate the compiler using the parameterless constructor, the pipeline is populated with the native, built-in FSCL compiler components (which, by the way, are contained in the `FSCL.Compiler.NativeComponents.dll` library).

###Custom configuration

There are mainly three aspects to consider when configuring the compiler pipeline:

+ **Component source**: the compiler configuration is specified as a set of compoents containers (or sources), from which to load the 
steps, processors and type handlers that will form the pipeline. There are two kinds of components sources:
    + **File source**: a (string) path to a dll
    + **Assembly source**: an `Assembly` object 
+ **Loading mode**: the loading mode determines the way a components source is inspected to find compiler components. There are two kinds of loading modes:
    + **Automatic loading mode**: each compiler component is marked with a custom attribute reporting the type of component (step, processor, etc.), the ID and the dependencies/before list. 
    The configuration system is inspecting the set of types declared in the dll/assembly and extract all the types marked with that specific custom attribute.
    + **Explicit loading mode**: the user has to explicitely tell the set of types (together with the ID, the dependencies, etc.) contained in the source that should
    be loaded as compiler components
+ **Configuration mode**: the configuration mode tells which kind on object you use to represent the pipeline configuration. It can be:
    + **Object-based**: you use an object of type `PipelineConfiguration`, containing the list of components sources to consider
    + **File-based**: you specify the path to an XML file that describes the pipeline configuration

In a prototyping environment, object-base configuration, assembly source and automatic loading mode is the suggested combination, since makes it fast to create and change
the configuration.

When moving to a testing/tweaking environment, you may want to play with the order of execution of come steps or try to disable/enable some processors for sake of performance.
This is a case where object-based configuration and assembly sources are still the best choice, but automatic locading mode is not anymore, cause changing inter-dependencies or
enabling-disabling a components with such a loading mode requires to change/add/remove components custom attributes (and recompile the components library).
Therefore, you should go for explicit loading mode, that allows you to perform some changes in the compiler pipeline without the need to touch your components code.

Finally, in a production environment you'll very likely store the dlls containing your components somewhere and you won't touch the
compiler instantiation anymore (you may have some production code using the FSCL compiler that you don't want to change, recompile and redeploy).
This is the perfect situation to use file sources and file-based configurations.
For example, consider that you create an appplication that uses the FSCL compiler. After proper testing and before deploying, you create a configuration file, you store it somewhere,
and you change the compiler constructor used in your application from the one taking a list of assemblies to one taking a file path (a.k.a. you move from object-based configuration to file-based).
From this moment on, every change to the compiler pipeline (e.g. fixing a bug in a step, adding a custom step, change some order of execution) can be done without the need to
change your application but instead working on the configuration file only.

###Prototyping environment: assembly sources and automatic loading

The following code shows how to configure the compiler using assembly sources and automatic component loading.
The `PipelineConfiguration` object represents the entire pipeline configuration. In this particular case, we instantiate
it passing a list of `AssemblySource`, one of each different assembly containing components that we want to add to the pipeline (most of the time you have one only assembly containing all the components of your plugin).
The first bool value is used to tell the configuration whether the native steps have to be loaded together we the ones specified.
If you build a plugin you generally want to insert it in the default pipeline, so the first argument will be `true`. If you pass `false`, you're creating the pipeline from scratch.

With the configuration shown, the configuration system automatically loads the components from the specified assemblies, establishes the execution order based on inter-dependencies, and finally builds a ready-to-use instance of the FSCL compiler.
*)

(*** hide ***)
type MyCustomStepInAnAssembly() =
    inherit System.Object()
type MyCustomProcessorInAnotherAssembly() =
    inherit System.Object()
type MyCustomStepA() =
    inherit System.Object()
type MyCustomStepB() =
    inherit System.Object()

(*** hide ***)
#r "FSCL.Compiler.dll"
#r "FSCL.Compiler.Core.dll"
#r "FSCL.Compiler.Language.dll"
(**
*)
open System
open System.IO
open FSCL
open FSCL.Compiler
open FSCL.Compiler.Configuration

let simpleConf = PipelineConfiguration(true, // true = Load native components
                                        [|
                                        SourceConfiguration(
                                            AssemblySource(typeof<MyCustomStepInAnAssembly>.Assembly));
                                        SourceConfiguration(
                                            AssemblySource(typeof<MyCustomProcessorInAnotherAssembly>.Assembly));
                                        |])
let compilerWithSimpleConf = Compiler(simpleConf)

(**
Every source can be configured independently from each other, so if you wanted to load some components from a file you could
change an assembly source to file source. For example, in the fillowing code the first source is changed from assembly to file.
*)
let simpleConfWithAFileSource = PipelineConfiguration(true, // true = Load native components
                                                        [|
                                                        SourceConfiguration(
                                                            FileSource("MyCustomComponentContainer.dll"));
                                                        SourceConfiguration(
                                                            AssemblySource(typeof<MyCustomProcessorInAnotherAssembly>.Assembly));
                                                        |])
let compilerWithFileSource = Compiler(simpleConfWithAFileSource)

(**
###Testing and tweaking environment: assembly sources and explicit loading

Let's consider we have the two components sources of the previous example, each of which is containing a step, and we want to test if performances or correctness change
if we switch the order of execution.
Instead of changing the custom attributes marking the steps in the two libraries we can explicitely set (override) the attributes of the steps
in the configuration object.

In the following example we use explicit components loading to load these two steps, where the first (MY_CUSTOM_STEP_A) must be executed after (depends on) the second (MY_CUSTOM_STEP_B).
*)
let explicitConf = PipelineConfiguration(true,
                                            [|
                                            SourceConfiguration(                                                            
                                                FileSource("MyCustomComponentContainer.dll"),                    
                                                [||], // No type handlers                                                            
                                                [| StepConfiguration("MY_CUSTOM_STEP_A", // ID                         
                                                                    typeof<MyCustomStepA>, // Object type
                                                                    [| "MY_CUSTOM_STEP_B" |]) |], // Depends on B
                                                [||] // No processors
                                                );
                                                
                                            SourceConfiguration(                                                            
                                                AssemblySource(typeof<MyCustomProcessorInAnotherAssembly>.Assembly),                    
                                                [||], // No type handlers                                                            
                                                [| StepConfiguration("MY_CUSTOM_STEP_B", // ID                         
                                                                     typeof<MyCustomStepB>) |], // Object type
                                                [||] // No processors
                                                );
                                            |])
                                              
let compilerStepAAfterB = Compiler(explicitConf)
    
(**
To change the order of execution of the two steps, you can change the inter-dependency so that the second is 
depending from the first, as shown below.
*)
let explicitConfDiffOrder = PipelineConfiguration(true,
                                                [|
                                                SourceConfiguration(                                                            
                                                    FileSource("MyCustomComponentContainer.dll"),                    
                                                    [||], // No type handlers                                                            
                                                    [| StepConfiguration("MY_CUSTOM_STEP_A", // ID                         
                                                                        typeof<MyCustomStepA>) |], // Object type
                                                    [||] // No processors
                                                    );
                                                
                                                SourceConfiguration(                                                            
                                                    AssemblySource(typeof<MyCustomProcessorInAnotherAssembly>.Assembly),                    
                                                    [||], // No type handlers                                                            
                                                    [| StepConfiguration("MY_CUSTOM_STEP_B", // ID                         
                                                                         typeof<MyCustomStepB>, // Object type
                                                                         [| "MY_CUSTOM_STEP_A" |]) |], // Now is B that depends from A
                                                    [||] // No processors
                                                    )
                                                |])
                                              
let compilerStepABeforeB = Compiler(explicitConfDiffOrder)

(**
###Production environment: file sources and configuration file

In a production environment, you may place the dlls or your custom components in a specific folder and instantiate the compiler
using file sources.

*)
let confWithOnlyFileSource = PipelineConfiguration(false, 
                                                    [|
                                                    SourceConfiguration(
                                                        FileSource(Path.Combine(
                                                                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                                    "MyCustomComponentContainerA.dll")));
                                                    SourceConfiguration(
                                                        FileSource(Path.Combine(
                                                                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                                                    "MyCustomComponentContainerB.dll")))
                                                    |])

(**
Every change to the components (e.g. bug fixing) can be performed without the need to change the app that uses the FSCL compiler but simply replacing the dlls in the folder.

The highest flexibility in production is definitely obtained moving from object-based configuration to file-based, that is creating the compiler specifying the path to configuration file
instead of an object. 
A configuration file is an XML file that strictly resebles a `PipelineConfiguration` object (actually, it's the exact result you obtain by XML marshalling an object of such type).

For example, the configuration of the previous example can be put into the configuration file as follows:
<code>
<?xml version="1.0" encoding="utf-8" ?>
<PipelineConfiguration LoadDefaultSteps="false">
  <Sources>
    <SourceConfiguration FileSource="C:\Documents and Settings\Administrator\Application Data\MyCustomComponentContainerA.dll"/>
    <SourceConfiguration FileSource="C:\Documents and Settings\Administrator\Application Data\MyCustomComponentContainerB.dll"/>
  </Sources>  
</PipelineConfiguration>
</code>

Instead of passing an object to configure the compiler, you can now pass the path to the configuration file.
*)      
let compilerWithConfFile = Compiler("MyConfigurationFile.xml")