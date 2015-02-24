namespace FSCL.Compiler
open System.Collections.Generic
open System.Reflection
open Microsoft.FSharp.Quotations
open System
open System.Collections.Generic
open System.Collections.ObjectModel
   
// Kernel Call Graph types
type KFGNodeType =
| DataNode
| OutValNode
| EnvVarNode
| KernelNode
| CollectionCompositionNode
| SequentialFunctionNode

[<AllowNullLiteral; AbstractClass>]
type IKFGNode(t: KFGNodeType) =
    member val Type = t with get      
    abstract member Input: IReadOnlyList<IKFGNode>
    
[<AllowNullLiteral>]
type KFGNode(t: KFGNodeType) =
    inherit IKFGNode(t) 
    member val Type = t with get      
    override this.Input 
        with get() =
            this.InputNodes :> IReadOnlyList<IKFGNode>
    member val InputNodes = new List<IKFGNode>()
        with get
    
type KFGKernelNode(kernel: IKernelModule) =
    inherit KFGNode(KFGNodeType.KernelNode)   
    member val Module = kernel 
        with get
                
type KFGDataNode(e: Expr) =
    inherit KFGNode(KFGNodeType.DataNode)  
    member val Data = e 
        with get 
                        
type KFGOutValNode(e:Expr) =
    inherit KFGNode(KFGNodeType.OutValNode)  
    member val Expr = e
        with get
                
type KFGEnvVarNode(v:Var) =
    inherit KFGNode(KFGNodeType.EnvVarNode)  
    member val Var = v
        with get

type KFGSequentialFunctionNode(o: obj option, mi: MethodInfo option, e: Expr) =
    inherit KFGNode(KFGNodeType.SequentialFunctionNode)  
    member val Instance = o with get
    member val MethodInfo = mi with get
    member val Expr = e with get
        
type KFGCollectionCompositionNode(mi: MethodInfo, compositionRoot: IKFGNode) =
    inherit KFGNode(KFGNodeType.CollectionCompositionNode)  
    member val CompositionID = mi 
        with get
    member val CompositionGraph = compositionRoot 
        with get 

[<AllowNullLiteral>]
[<AbstractClass>]
type IComputingExpressionModule(root: IKFGNode) =
    member val KFGRoot = root 
        with get    

    abstract member KernelModules: IReadOnlyList<IKernelModule>
    
[<AllowNullLiteral>]
type ComputingExpressionModule(root: IKFGNode, 
                               metadataVerifier: ReadOnlyMetaCollection * ReadOnlyMetaCollection -> bool) =
    inherit IComputingExpressionModule(root)

    let kmods = new Dictionary<FunctionInfoID, List<ReadOnlyMetaCollection * KernelModule>>()
    let compileKmList = new List<KernelModule>()
    let copyKmList = new List<KernelModule>()
    let fullKmList = new List<IKernelModule>()
    let rec graphSearch(r: IKFGNode) =
        match r with
        | :? KFGKernelNode ->
            let km = (r :?> KFGKernelNode).Module :?> KernelModule
            if kmods.ContainsKey(km.Kernel.ID) then
                let potentialKernels = kmods.[km.Kernel.ID]
                let item = Seq.tryFind(fun (cachedMeta: ReadOnlyMetaCollection, cachedKernel: KernelModule) ->
                                            metadataVerifier(cachedMeta, km.Kernel.Meta)) potentialKernels
                match item with
                | Some(m, k) ->
                    copyKmList.Add(km)
                | _ ->
                    kmods.[km.Kernel.ID].Add(km.Kernel.Meta, km)
                    compileKmList.Add(km)
            else
                kmods.Add(km.Kernel.ID, new List<ReadOnlyMetaCollection * KernelModule>())
                kmods.[km.Kernel.ID].Add(km.Kernel.Meta, km)
                compileKmList.Add(km)
            fullKmList.Add(km)
        | :? KFGSequentialFunctionNode ->
            (r :?> KFGSequentialFunctionNode).Input |> Seq.iter(fun i -> graphSearch(i))
        | :? KFGCollectionCompositionNode ->
            (r :?> KFGCollectionCompositionNode).Input |> Seq.iter(fun i -> graphSearch(i))
            (r :?> KFGCollectionCompositionNode).CompositionGraph |> graphSearch 
        | _ ->
            ()
    do
        graphSearch(root)
                 
    override val KernelModules = fullKmList :> IReadOnlyList<IKernelModule>
        with get
    
    member val KernelModulesToCompile = compileKmList :> IReadOnlyList<KernelModule>
        with get   
    member val KernelModulesToCopy = copyKmList :> IReadOnlyList<KernelModule>
        with get   