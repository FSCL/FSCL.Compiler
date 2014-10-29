namespace FSCL.Compiler.Types

open System
open FSCL.Compiler
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Reflection
open FSCL.Compiler.Util.ReflectionUtil

[<TypeHandler("FSCL_OPTION_TYPE_HANDLER")>]
type OptionTypeHandler() =       
    inherit TypeHandler() with

    override this.Print(t:Type) =
        let arrayStar = if t.IsArray then "*" else ""
        let plainType = if t.IsArray then t.GetElementType() else t
        let genT = plainType.GetGenericArguments()
        "struct Option_" + genT.[0].Name + "" + arrayStar

    override this.ManagedGenericInstances
        with get() = 
            []
            
    override this.CanHandle(t) = 
        let plainType = if t.IsArray then t.GetElementType() else t
        plainType.IsOption