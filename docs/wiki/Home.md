At a glance, the FSCL Compiler generates OpenCL C99 kernel source code out of F# collection functions and F# **special** functions/methods/lambdas.

### Hello FSCL 

At first, we must open some namespaces:

	open FSCL.Compiler
	open FSCL.Language
	
Kernels are defined as F# functions marked with **ReflectedDefinition** and **Kernel** attributes and accepting an extra **WorkItemInfo** parameter, which holds the information about the work-item space size and the identifiers of the current work-item (global id, local id, space rank, etc.).

	[<ReflectedDefinition;Kernel>]
	let VecAdd(a:float32[], b:float32[], c:float32[], wi:WorkItemInfo) =
		let gid = wi.GlobalID(0)
		c.[gid] <- a.[gid] + b.[gid]
		
To compile the kernel, we instantiate the Compiler and we call the **Compile** method, passing a quoted call to the kernel.
While kernels take a **WorkItemInfo** parameter, the argument matching that parameter is of type **WorkSize**. This cause a WorkItemInfo contains the identifiers of the current work-items, which are established at runtime and not by the user, which has only to define the size of the work space.

	let a, b, c = [||], [||], [||]
	let ws = new WorkSize(1024L, 64L)
	let compiler = new Compiler()
	let compResult = compiler.Compile(<@ VecAdd(a, b, c, ws) @>)
	
Since the compilation pipeline is thought to be completely customisable and extensible, the compilation result is of type **Object**.
If using the default pipeline, which is the pipeline built through the parameterless Compiler constructor, you can safely cast the result as follows.

	let result = compResult :?> IKernelExpression

**IKernelExpression** is an interface that exposes the result of default compilation. The most interesting data it exposes is the **Kernel Flow Graph** (**KFG**), which is a representation of the flow of computations inside the given quoted expression. In our hello world we have a single computation, which is a kernel, so we can do the following to obtain the OpenCL source code generated out of it.

	let code = (e.KFGRoot :?> KFGKernelNode).Module.Code


