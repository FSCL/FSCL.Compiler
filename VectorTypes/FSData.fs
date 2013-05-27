// This is a generated file; the original input is 'C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt'
namespace FSData
            
open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
open Microsoft.FSharp.Reflection
open System.Reflection
// (namespaces below for specific case of using the tool to compile FSharp.Core itself)
open Microsoft.FSharp.Core
open Microsoft.FSharp.Core.Operators
open Microsoft.FSharp.Text
open Microsoft.FSharp.Collections
open Printf

type internal SR private() =
            
    // BEGIN BOILERPLATE        
    static let resources = lazy (new System.Resources.ResourceManager("FSData", System.Reflection.Assembly.GetExecutingAssembly()))

    static let GetString(name:string) =        
        let s = resources.Value.GetString(name, System.Globalization.CultureInfo.CurrentUICulture)
#if DEBUG
        if null = s then
            System.Diagnostics.Debug.Assert(false, sprintf "**RESOURCE ERROR**: Resource token %s does not exist!" name)
#endif
        s

    static let mkFunctionValue (tys: System.Type[]) (impl:obj->obj) = 
        FSharpValue.MakeFunction(FSharpType.MakeFunctionType(tys.[0],tys.[1]), impl)
        
    static let funTyC = typeof<(obj -> obj)>.GetGenericTypeDefinition()  

    static let isNamedType(ty:System.Type) = not (ty.IsArray ||  ty.IsByRef ||  ty.IsPointer)
    static let isFunctionType (ty1:System.Type)  = 
        isNamedType(ty1) && ty1.IsGenericType && (ty1.GetGenericTypeDefinition()).Equals(funTyC)

    static let rec destFunTy (ty:System.Type) =
        if isFunctionType ty then 
            ty, ty.GetGenericArguments() 
        else
            match ty.BaseType with 
            | null -> failwith "destFunTy: not a function type" 
            | b -> destFunTy b 

    static let buildFunctionForOneArgPat (ty: System.Type) impl = 
        let _,tys = destFunTy ty 
        let rty = tys.[1]
        // PERF: this technique is a bit slow (e.g. in simple cases, like 'sprintf "%x"') 
        mkFunctionValue tys (fun inp -> impl rty inp)
                
    static let capture1 (fmt:string) i args ty (go : obj list -> System.Type -> int -> obj) : obj = 
        match fmt.[i] with
        | '%' -> go args ty (i+1) 
        | 'd'
        | 'f'
        | 's' -> buildFunctionForOneArgPat ty (fun rty n -> go (n::args) rty (i+1))
        | _ -> failwith "bad format specifier"
        
    // newlines and tabs get converted to strings when read from a resource file
    // this will preserve their original intention    
    static let postProcessString (s : string) =
        s.Replace("\\n","\n").Replace("\\t","\t").Replace("\\r","\r").Replace("\\\"", "\"")
        
    static let createMessageString (messageString : string) (fmt : Printf.StringFormat<'T>) : 'T = 
        let fmt = fmt.Value // here, we use the actual error string, as opposed to the one stored as fmt
        let len = fmt.Length 

        /// Function to capture the arguments and then run.
        let rec capture args ty i = 
            if i >= len ||  (fmt.[i] = '%' && i+1 >= len) then 
                let b = new System.Text.StringBuilder()    
                b.AppendFormat(messageString, [| for x in List.rev args -> x |]) |> ignore
                box(b.ToString())
            // REVIEW: For these purposes, this should be a nop, but I'm leaving it
            // in incase we ever decide to support labels for the error format string
            // E.g., "<name>%s<foo>%d"
            elif System.Char.IsSurrogatePair(fmt,i) then 
               capture args ty (i+2)
            else
                match fmt.[i] with
                | '%' ->
                    let i = i+1 
                    capture1 fmt i args ty capture
                | _ ->
                    capture args ty (i+1) 

        (unbox (capture [] (typeof<'T>) 0) : 'T)

    static let mutable swallowResourceText = false
    
    static let GetStringFunc((messageID : string),(fmt : Printf.StringFormat<'T>)) : 'T =
        if swallowResourceText then
            sprintf fmt
        else
            let mutable messageString = GetString(messageID)
            messageString <- postProcessString messageString                            
            createMessageString messageString fmt
        
    /// If set to true, then all error messages will just return the filled 'holes' delimited by ',,,'s - this is for language-neutral testing (e.g. localization-invariant baselines).
    static member SwallowResourceText with get () = swallowResourceText
                                      and set (b) = swallowResourceText <- b
    // END BOILERPLATE        
    
    /// The .NET SDK 4.0 or 4.5 tools could not be found
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:7)
    static member unsupportedFramework() = (GetStringFunc("unsupportedFramework",",,,") )
    /// The operation '%s' on item '%s' should not be called on provided type, member or parameter
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:13)
    static member invalidOperationOnProvidedType(a0 : System.String, a1 : System.String) = (GetStringFunc("invalidOperationOnProvidedType",",,,%s,,,%s,,,") a0 a1)
    /// constructor for %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:14)
    static member constructorFor(a0 : System.String) = (GetStringFunc("constructorFor",",,,%s,,,") a0)
    /// <not yet known type>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:15)
    static member notYetKnownType() = (GetStringFunc("notYetKnownType",",,,") )
    /// ProvidedConstructor: declaringType already set on '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:16)
    static member declaringTypeAlreadySet(a0 : System.String) = (GetStringFunc("declaringTypeAlreadySet",",,,%s,,,") a0)
    /// ProvidedConstructor: no invoker for '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:17)
    static member pcNoInvoker(a0 : System.String) = (GetStringFunc("pcNoInvoker",",,,%s,,,") a0)
    /// ProvidedConstructor: code already given for '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:18)
    static member pcCodeAlreadyGiven(a0 : System.String) = (GetStringFunc("pcCodeAlreadyGiven",",,,%s,,,") a0)
    /// ProvidedMethod: no invoker for %s on type %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:19)
    static member pmNoInvokerName(a0 : System.String, a1 : System.String) = (GetStringFunc("pmNoInvokerName",",,,%s,,,%s,,,") a0 a1)
    /// ProvidedConstructor: code already given for %s on type %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:20)
    static member pcNoInvokerName(a0 : System.String, a1 : System.String) = (GetStringFunc("pcNoInvokerName",",,,%s,,,%s,,,") a0 a1)
    /// ProvidedProperty: getter MethodInfo has already been created
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:21)
    static member ppGetterAlreadyCreated() = (GetStringFunc("ppGetterAlreadyCreated",",,,") )
    /// ProvidedProperty: setter MethodInfo has already been created
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:22)
    static member ppSetterAlreadyCreated() = (GetStringFunc("ppSetterAlreadyCreated",",,,") )
    /// unreachable
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:23)
    static member unreachable() = (GetStringFunc("unreachable",",,,") )
    /// non-array type
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:24)
    static member nonArrayType() = (GetStringFunc("nonArrayType",",,,") )
    /// non-generic type
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:25)
    static member nonGenericType() = (GetStringFunc("nonGenericType",",,,") )
    /// not an array, pointer or byref type
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:26)
    static member notAnArrayPointerOrByref() = (GetStringFunc("notAnArrayPointerOrByref",",,,") )
    /// Unit '%s' not found in FSharp.Core SI module
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:27)
    static member unitNotFound(a0 : System.String) = (GetStringFunc("unitNotFound",",,,%s,,,") a0)
    /// Use 'null' for global namespace
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:28)
    static member useNullForGlobalNamespace() = (GetStringFunc("useNullForGlobalNamespace",",,,") )
    /// type '%s' was not added as a member to a declaring type
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:29)
    static member typeNotAddedAsAMember(a0 : System.String) = (GetStringFunc("typeNotAddedAsAMember",",,,%s,,,") a0)
    /// ProvidedTypeDefinition: expecting %d static parameters but given %d for type %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:30)
    static member pdErrorExpectingStaticParameters(a0 : System.Int32, a1 : System.Int32, a2 : System.String) = (GetStringFunc("pdErrorExpectingStaticParameters",",,,%d,,,%d,,,%s,,,") a0 a1 a2)
    /// ProvidedTypeDefinition: DefineStaticParameters was not called
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:31)
    static member pdDefineStaticParametersNotCalled() = (GetStringFunc("pdDefineStaticParametersNotCalled",",,,") )
    /// ProvidedTypeDefinition: static parameters supplied but not expected for %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:32)
    static member ptdStaticParametersSuppliedButNotExpected(a0 : System.String) = (GetStringFunc("ptdStaticParametersSuppliedButNotExpected",",,,%s,,,") a0)
    /// container type for '%s' was already set to '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:33)
    static member containerTypeAlreadySet(a0 : System.String, a1 : System.String) = (GetStringFunc("containerTypeAlreadySet",",,,%s,,,%s,,,") a0 a1)
    /// GetMethodImpl does not support overloads
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:34)
    static member getMethodImplDoesNotSupportOverloads() = (GetStringFunc("getMethodImplDoesNotSupportOverloads",",,,") )
    /// Need to handle specified return type in GetPropertyImpl
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:35)
    static member gpiNeedToHandleSpecifiedReturnType() = (GetStringFunc("gpiNeedToHandleSpecifiedReturnType",",,,") )
    /// Need to handle specified parameter types in GetPropertyImpl
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:36)
    static member gpiNeedToHandleSpecifiedParameterTypes() = (GetStringFunc("gpiNeedToHandleSpecifiedParameterTypes",",,,") )
    /// Need to handle specified modifiers in GetPropertyImpl
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:37)
    static member gpiNeedToHandleSpecifiedModifiers() = (GetStringFunc("gpiNeedToHandleSpecifiedModifiers",",,,") )
    /// Need to handle binder in GetPropertyImpl
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:38)
    static member gpiNeedToHandleBinder() = (GetStringFunc("gpiNeedToHandleBinder",",,,") )
    /// There is more than one nested type called '%s' in type '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:39)
    static member moreThanOneNestedType(a0 : System.String, a1 : System.String) = (GetStringFunc("moreThanOneNestedType",",,,%s,,,%s,,,") a0 a1)
    /// Error writing to local schema file. %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:45)
    static member errorWritingLocalSchemaFile(a0 : System.String) = (GetStringFunc("errorWritingLocalSchemaFile",",,,%s,,,") a0)
    /// Error reading schema. %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:46)
    static member errorReadingSchema(a0 : System.String) = (GetStringFunc("errorReadingSchema",",,,%s,,,") a0)
    /// The extension of the given LocalSchema file '%s' is not valid. The required extension is '%s'.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:47)
    static member errorInvalidExtensionSchema(a0 : System.String, a1 : System.String) = (GetStringFunc("errorInvalidExtensionSchema",",,,%s,,,%s,,,") a0 a1)
    /// The file '%s' doesn't contain XML element '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:48)
    static member fileDoesNotContainXMLElement(a0 : System.String, a1 : System.String) = (GetStringFunc("fileDoesNotContainXMLElement",",,,%s,,,%s,,,") a0 a1)
    /// Failed to load the file '%s' as XML
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:49)
    static member failedToLoadFileAsXML(a0 : System.String) = (GetStringFunc("failedToLoadFileAsXML",",,,%s,,,") a0)
    /// Contains the simplified context types for the %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:55)
    static member xmlDocContainsTheSimplifiedContextTypes(a0 : System.String) = (GetStringFunc("xmlDocContainsTheSimplifiedContextTypes",",,,%s,,,") a0)
    /// <summary><para>The full API to the %s.</para><para>To use the service via the full API, create an instance of one of the types %s.</para><para>You may need to set the Credentials property on the instance.</para></summary>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:56)
    static member xmlDocFullServiceTypesAPI(a0 : System.String, a1 : System.String) = (GetStringFunc("xmlDocFullServiceTypesAPI",",,,%s,,,%s,,,") a0 a1)
    /// <summary><para>The full API to the %s.</para><para>To use the service via the full API, create an instance of one of the types %s.</para></summary>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:57)
    static member xmlDocFullServiceTypesAPINoCredentials(a0 : System.String, a1 : System.String) = (GetStringFunc("xmlDocFullServiceTypesAPINoCredentials",",,,%s,,,%s,,,") a0 a1)
    /// A simplified data context for the %s. The full data context object is available via the DataContext property.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:58)
    static member xmlDocSimplifiedDataContext(a0 : System.String) = (GetStringFunc("xmlDocSimplifiedDataContext",",,,%s,,,") a0)
    /// Execute the '%s' procedure
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:59)
    static member xmlDocExecuteProcedure(a0 : System.String) = (GetStringFunc("xmlDocExecuteProcedure",",,,%s,,,") a0)
    /// Gets the '%s' entities from the %s. This property may be used as the source in a query expression.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:60)
    static member xmlDocGetEntities(a0 : System.String, a1 : System.String) = (GetStringFunc("xmlDocGetEntities",",,,%s,,,%s,,,") a0 a1)
    /// Gets the full data context object for this %s
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:61)
    static member xmlDocGetFullContext(a0 : System.String) = (GetStringFunc("xmlDocGetFullContext",",,,%s,,,") a0)
    /// Get a simplified data context for this %s. By default, no credentials are set
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:62)
    static member xmlDocGetSimplifiedContext(a0 : System.String) = (GetStringFunc("xmlDocGetSimplifiedContext",",,,%s,,,") a0)
    /// Construct a simplified data context for this %s. By default, no credentials are set
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:63)
    static member xmlDocConstructSimplifiedContext(a0 : System.String) = (GetStringFunc("xmlDocConstructSimplifiedContext",",,,%s,,,") a0)
    /// <summary>Provides the types to access a database with the schema in a DBML file, using a LINQ-to-SQL mapping</summary><param name='File'>The DBML file containing the schema description</param><param name='ResolutionFolder'>The folder used to resolve relative file paths at compile-time (default: folder containing the project or script)</param><param name='ContextTypeName'>The name of data context class (default: derived from database name)</param><param name='Serializable'>Generate uni-directional serializable classes (default: false, which means no serialization)</param>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:64)
    static member dbmlFileTypeHelp() = (GetStringFunc("dbmlFileTypeHelp",",,,") )
    /// SQL connection
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:65)
    static member sqlDataConnection() = (GetStringFunc("sqlDataConnection",",,,") )
    /// Gets the connection used by the framework
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:66)
    static member sqlDataConnectionInfo() = (GetStringFunc("sqlDataConnectionInfo",",,,") )
    /// <summary>Provides the types to access a database, using a LINQ-to-SQL mapping</summary><param name='ConnectionString'>The connection string for the database connection. If using Visual Studio, a connection string can be found in database properties in the Server Explorer window.</param><param name='ConnectionStringName'>The name of the connection string for the database connection in the configuration file.</param><param name='LocalSchemaFile'>The local .dbml file for the database schema (default: no local schema file)</param><param name='ForceUpdate'>Require that a direct connection to the database be available at design-time and force the refresh of the local schema file (default: true)</param><param name='Pluralize'>Automatically pluralize or singularize class and member names using English language rules (default: false)</param><param name='Views'>Extract database views (default: true)</param><param name='Functions'>Extract database functions (default: true)</param><param name='ConfigFile'>The name of the configuration file used for connection strings (default: app.config or web.config is used)</param><param name='DataDirectory'>The name of the data directory, used to replace |DataDirectory| in connection strings (default: the project or script directory)</param><param name='ResolutionFolder'>The folder used to resolve relative file paths at compile-time (default: folder containing the project or script)</param><param name='StoredProcedures'>Extract stored procedures (default: true)</param><param name='Timeout'>Timeout value in seconds to use when SqlMetal accesses the database (default: 0, which means infinite)</param><param name='ContextTypeName'>The name of data context class (default: derived from database name)</param><param name='Serializable'>Generate uni-directional serializable classes (default: false, which means no serialization)</param>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:67)
    static member sqlDataConnectionTypeHelp() = (GetStringFunc("sqlDataConnectionTypeHelp",",,,") )
    /// <summary>Provides the types to access a database with the schema in an EDMX file, using a LINQ-to-Entities mapping</summary><param name='File'>The EDMX file containing the conceptual, storage and mapping schema descriptions</param><param name='ResolutionFolder'>The folder used to resolve relative file paths at compile-time (default: folder containing the project or script)</param>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:68)
    static member edmxFileTypeHelp() = (GetStringFunc("edmxFileTypeHelp",",,,") )
    /// SQL Entity connection
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:69)
    static member sqlEntityConnection() = (GetStringFunc("sqlEntityConnection",",,,") )
    /// <summary>Provides the types to access a database, using a LINQ-to-Entities mapping</summary><param name='ConnectionString'>The connection string for the database connection</param><param name='ConnectionStringName'>The name of the connection string for the database connection in the configuration file.</param><param name='LocalSchemaFile'>The local file for the database schema</param><param name='Provider'>The name of the ADO.NET data provider to be used for ssdl generation (default: System.Data.SqlClient)</param><param name='EntityContainer'>The name to use for the EntityContainer in the conceptual model</param><param name='ConfigFile'>The name of the configuration file used for connection strings (default: app.config or web.config is used)</param><param name='DataDirectory'>The name of the data directory, used to replace |DataDirectory| in connection strings (default: the project or script directory)</param><param name='ResolutionFolder'>The folder used to resolve relative file paths at compile-time (default: folder containing the project or script)</param><param name='ForceUpdate'>Require that a direct connection to the database be available at design-time and force the refresh of the local schema file (default: true)</param><param name='Pluralize'>Automatically pluralize or singularize class and member names using English language rules (default: false)</param><param name='SuppressForeignKeyProperties'>Exclude foreign key properties in entity type definitions (default: false)</param>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:70)
    static member sqlEntityConnectionTypeHelp() = (GetStringFunc("sqlEntityConnectionTypeHelp",",,,") )
    /// Gets the connection used by the object context
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:71)
    static member connectionInfo() = (GetStringFunc("connectionInfo",",,,") )
    /// Gets or sets the authentication information used by each query for this data context object
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:72)
    static member odataServiceCredentialsInfo() = (GetStringFunc("odataServiceCredentialsInfo",",,,") )
    /// <summary>Provides the types to access an OData service</summary><param name="ServiceUri">The Uri for the OData service</param><param name='LocalSchemaFile'>The local .csdl file for the service schema</param><param name='ForceUpdate'>Require that a direct connection to the service be available at design-time and force the refresh of the local schema file (default: true)</param><param name='ResolutionFolder'>The folder used to resolve relative file paths at compile-time (default: folder containing the project or script)</param><param name='DataServiceCollection'>Generate collections derived from DataServiceCollection (default: false)</param>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:73)
    static member odataServiceTypeHelp() = (GetStringFunc("odataServiceTypeHelp",",,,") )
    /// <summary>Provides the types to access a WSDL web service</summary><param name='ServiceUri'>The Uri for the WSDL service</param><param name='LocalSchemaFile'>The .wsdlschema file to store locally cached service schema</param><param name='ForceUpdate'>Require that a direct connection to the service be available at design-time and force the refresh of the local schema file (default: true)</param><param name='ResolutionFolder'>The folder used to resolve relative file paths at compile-time (default: folder containing the project or script)</param><param name='MessageContract'>Generate Message Contract types (default: false)</param><param name='EnableDataBinding'>Implement the System.ComponentModel.INotifyPropertyChanged interface on all DataContract types to enable data binding (default: false)</param><param name='Serializable'>Generate classes marked with the Serializable Attribute (default: false)</param><param name='Async'>Generate both synchronous and asynchronous method signatures (default: false, which means generate only synchronous method signatures)</param><param name='CollectionType'>A fully-qualified or assembly-qualified name of the type to use as a collection data type when code is generated from schemas</param>
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:74)
    static member wsdlServiceTypeHelp() = (GetStringFunc("wsdlServiceTypeHelp",",,,") )
    /// static parameter '%s' not found for type '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:75)
    static member staticParameterNotFoundForType(a0 : System.String, a1 : System.String) = (GetStringFunc("staticParameterNotFoundForType",",,,%s,,,%s,,,") a0 a1)
    /// unexpected MethodBase
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:76)
    static member unexpectedMethodBase() = (GetStringFunc("unexpectedMethodBase",",,,") )
    /// Disposes the given context
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:77)
    static member xmlDocDisposeSimplifiedContext() = (GetStringFunc("xmlDocDisposeSimplifiedContext",",,,") )
    /// %s is not valid name for data context class
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:78)
    static member invalidDataContextClassName(a0 : System.String) = (GetStringFunc("invalidDataContextClassName",",,,%s,,,") a0)
    /// The provided ServiceUri is for a data service that supports fixed queries. The OData type provider does not support such services.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:79)
    static member fixedQueriesNotSupported() = (GetStringFunc("fixedQueriesNotSupported",",,,") )
    /// Services that implement the Data Quality Services API are not supported.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:80)
    static member dqsServicesNotSupported() = (GetStringFunc("dqsServicesNotSupported",",,,") )
    /// The supplied connection string should be either a valid provider-specific connection string or a valid connection string accepted by the EntityClient.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:81)
    static member invalidConnectionString() = (GetStringFunc("invalidConnectionString",",,,") )
    /// Connection string presented in EntityClient format can differ only in provider-specific part.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:82)
    static member nonEquivalentConnectionString() = (GetStringFunc("nonEquivalentConnectionString",",,,") )
    /// A configuration string name was specified but no configuration file was found. Neither app.config nor web.config found in project or script directory.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:83)
    static member noConfigFileFound1() = (GetStringFunc("noConfigFileFound1",",,,") )
    /// A configuration string name was specified but the configuration file '%s' was not found
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:84)
    static member noConfigFileFound2(a0 : System.String) = (GetStringFunc("noConfigFileFound2",",,,%s,,,") a0)
    /// When using this provider you must specify either a connection string or a connection string name. To specify a connection string, use %s<\"...connection string...\">.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:85)
    static member noConnectionStringOrConnectionStringName(a0 : System.String) = (GetStringFunc("noConnectionStringOrConnectionStringName",",,,%s,,,") a0)
    /// When using this provider you must specify either a connection string or a connection string name, but not both. To specify a connection string, use SqlDataConnection<\"...connection string...\">.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:86)
    static member notBothConnectionStringOrConnectionStringName() = (GetStringFunc("notBothConnectionStringOrConnectionStringName",",,,") )
    /// Invalid provider '%s' in connection string entry '%s' in config file '%s'. SqlDataConnection can only be used with provider 'System.Data.SqlClient'.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:87)
    static member invalidProviderInConfigFile(a0 : System.String, a1 : System.String, a2 : System.String) = (GetStringFunc("invalidProviderInConfigFile",",,,%s,,,%s,,,%s,,,") a0 a1 a2)
    /// Invalid empty connection string '%s' for the connection string name '%s' in config file '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:88)
    static member invalidConnectionStringInConfigFile(a0 : System.String, a1 : System.String, a2 : System.String) = (GetStringFunc("invalidConnectionStringInConfigFile",",,,%s,,,%s,,,%s,,,") a0 a1 a2)
    /// An error occured while reading connection string '%s' from the config file '%s': '%s'
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:89)
    static member errorWhileReadingConnectionStringInConfigFile(a0 : System.String, a1 : System.String, a2 : System.String) = (GetStringFunc("errorWhileReadingConnectionStringInConfigFile",",,,%s,,,%s,,,%s,,,") a0 a1 a2)
    /// ServiceMetadataFile element cannot be empty
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:90)
    static member serviceMetadataFileElementIsEmpty() = (GetStringFunc("serviceMetadataFileElementIsEmpty",",,,") )
    /// The parameter 'ServiceUri' cannot be an empty string.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:91)
    static member invalidWsdlUri() = (GetStringFunc("invalidWsdlUri",",,,") )
    /// The required tool '%s' could not be found.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:92)
    static member requiredToolNotFound(a0 : System.String) = (GetStringFunc("requiredToolNotFound",",,,%s,,,") a0)
    /// The data directory '%s' did not exist.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:93)
    static member dataDirectoryNotFound(a0 : System.String) = (GetStringFunc("dataDirectoryNotFound",",,,%s,,,") a0)
    /// File '%s' requires .NET 4.5. To use this file please change project target framework to .NET 4.5.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:94)
    static member edmxFileRequiresDotNet45(a0 : System.String) = (GetStringFunc("edmxFileRequiresDotNet45",",,,%s,,,") a0)
    /// Connection string '%s' not found in configuration file.
    /// (Originally from C:\Users\Gabriele\Desktop\Sep2012\src\fsharp\FSharp.Data.TypeProviders\FSData.txt:95)
    static member connectionStringNotFound(a0 : System.String) = (GetStringFunc("connectionStringNotFound",",,,%s,,,") a0)

    /// Call this method once to validate that all known resources are valid; throws if not
    static member RunStartupValidation() =
        ignore(GetString("unsupportedFramework"))
        ignore(GetString("invalidOperationOnProvidedType"))
        ignore(GetString("constructorFor"))
        ignore(GetString("notYetKnownType"))
        ignore(GetString("declaringTypeAlreadySet"))
        ignore(GetString("pcNoInvoker"))
        ignore(GetString("pcCodeAlreadyGiven"))
        ignore(GetString("pmNoInvokerName"))
        ignore(GetString("pcNoInvokerName"))
        ignore(GetString("ppGetterAlreadyCreated"))
        ignore(GetString("ppSetterAlreadyCreated"))
        ignore(GetString("unreachable"))
        ignore(GetString("nonArrayType"))
        ignore(GetString("nonGenericType"))
        ignore(GetString("notAnArrayPointerOrByref"))
        ignore(GetString("unitNotFound"))
        ignore(GetString("useNullForGlobalNamespace"))
        ignore(GetString("typeNotAddedAsAMember"))
        ignore(GetString("pdErrorExpectingStaticParameters"))
        ignore(GetString("pdDefineStaticParametersNotCalled"))
        ignore(GetString("ptdStaticParametersSuppliedButNotExpected"))
        ignore(GetString("containerTypeAlreadySet"))
        ignore(GetString("getMethodImplDoesNotSupportOverloads"))
        ignore(GetString("gpiNeedToHandleSpecifiedReturnType"))
        ignore(GetString("gpiNeedToHandleSpecifiedParameterTypes"))
        ignore(GetString("gpiNeedToHandleSpecifiedModifiers"))
        ignore(GetString("gpiNeedToHandleBinder"))
        ignore(GetString("moreThanOneNestedType"))
        ignore(GetString("errorWritingLocalSchemaFile"))
        ignore(GetString("errorReadingSchema"))
        ignore(GetString("errorInvalidExtensionSchema"))
        ignore(GetString("fileDoesNotContainXMLElement"))
        ignore(GetString("failedToLoadFileAsXML"))
        ignore(GetString("xmlDocContainsTheSimplifiedContextTypes"))
        ignore(GetString("xmlDocFullServiceTypesAPI"))
        ignore(GetString("xmlDocFullServiceTypesAPINoCredentials"))
        ignore(GetString("xmlDocSimplifiedDataContext"))
        ignore(GetString("xmlDocExecuteProcedure"))
        ignore(GetString("xmlDocGetEntities"))
        ignore(GetString("xmlDocGetFullContext"))
        ignore(GetString("xmlDocGetSimplifiedContext"))
        ignore(GetString("xmlDocConstructSimplifiedContext"))
        ignore(GetString("dbmlFileTypeHelp"))
        ignore(GetString("sqlDataConnection"))
        ignore(GetString("sqlDataConnectionInfo"))
        ignore(GetString("sqlDataConnectionTypeHelp"))
        ignore(GetString("edmxFileTypeHelp"))
        ignore(GetString("sqlEntityConnection"))
        ignore(GetString("sqlEntityConnectionTypeHelp"))
        ignore(GetString("connectionInfo"))
        ignore(GetString("odataServiceCredentialsInfo"))
        ignore(GetString("odataServiceTypeHelp"))
        ignore(GetString("wsdlServiceTypeHelp"))
        ignore(GetString("staticParameterNotFoundForType"))
        ignore(GetString("unexpectedMethodBase"))
        ignore(GetString("xmlDocDisposeSimplifiedContext"))
        ignore(GetString("invalidDataContextClassName"))
        ignore(GetString("fixedQueriesNotSupported"))
        ignore(GetString("dqsServicesNotSupported"))
        ignore(GetString("invalidConnectionString"))
        ignore(GetString("nonEquivalentConnectionString"))
        ignore(GetString("noConfigFileFound1"))
        ignore(GetString("noConfigFileFound2"))
        ignore(GetString("noConnectionStringOrConnectionStringName"))
        ignore(GetString("notBothConnectionStringOrConnectionStringName"))
        ignore(GetString("invalidProviderInConfigFile"))
        ignore(GetString("invalidConnectionStringInConfigFile"))
        ignore(GetString("errorWhileReadingConnectionStringInConfigFile"))
        ignore(GetString("serviceMetadataFileElementIsEmpty"))
        ignore(GetString("invalidWsdlUri"))
        ignore(GetString("requiredToolNotFound"))
        ignore(GetString("dataDirectoryNotFound"))
        ignore(GetString("edmxFileRequiresDotNet45"))
        ignore(GetString("connectionStringNotFound"))
        ()
