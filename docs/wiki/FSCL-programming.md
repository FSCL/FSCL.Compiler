FSCL works at incremental levels of abstractions, so that programmers can place themeselves at the most appropriate level depending on the particular abstraction vs flexibility blance required.

##### 1) Use collection functions to express parallel skeletons

In many cases the computation to express is a common pattern (e.g. map, reduce, scan). In such a case, we can use pre-existing F# collection functions. For example, we can use **Array.map** to express a parallel map over an input array.

	let myArr = [||]
	let mycomp = <@ Array.map(fun a -> a * 2) myArr @>
	let result = compiler.Compile(mycomp)
	
##### 2) If collection functions are not flexible enough, use custom kernels

If none of the F# collection functions can express a particular parallel computation, FSCL allows to define a custom kernel as a (user-defined) F# function. In the following, each thread (work-item) averages each element of a vector with the neighbouring elements.

	[ReflectedDefinition;Kernel]
	let avg3(input:float32[], output:float32[], wi:WorkItemInfo) =
		let gid = wi.GlobalID(0)
		if gid > 0 && gid < input.Length - 1 then
			output.[gid] <- (input.[gid - 1] + input.[gid] + input.[gid + 1])/3.0f
	
	let myArr, myOut = [||], [||]
	let mycomp = <@ avg3(myArr, myOut) @>
	let result = compiler.Compile(mycomp)

##### 3) Use custom kernel return type to enable composition
Well, in OpenCL 1.2 kernels must return void, which is a strong limit to the compositionality of multiple kernels. In FSCL, kernels can return arrays. The compiler handles lifting return instructions and generating additional parameters to hold the return values. 
For example, we can rewrite the **avg3** function to return the output array.

	[ReflectedDefinition;Kernel]
	let avg3(input:float32[]) =
		// Declare the output (done once)
		let output = Array.zeroCreate<float32> (input.Length)
		
		// Per-work-item code (executed once for each work-item)
		let gid = wi.GlobalID(0)
		if gid > 0 && gid < input.Length - 1 then
			output.[gid] <- (input.[gid - 1] + input.[gid] + input.[gid + 1])/3.0f
			
		// Return (done once)
		output

Easy, right? To return a value, a kernel must at first create the output array as local variable using the **zeroCreate** function and then return it after the work-item code has been executed.

##### 4) Compose kernels using functions and high-order collections functions

In F#, we can compose functions using the handy operators **|>**, **||>** and **|||>**. We can use the exact same operator in FSCL to compose kernels. In the following, we express a map kernel followed by a reduce kernel (it looks exactly as in F#).

	let myArr = [||]
	let mycomp = <@ 
					myArr |>
					Array.map(fun a -> a * 2) |> 
					Array.reduce (*) 
		         @>
	let result = compiler.Compile(mycomp)
	
You can also use higher-order collection functions to express the coordination of kernels. For example, let's say we want to apply the above processing to a set of input arrays (instead to one only). We can use an higher-order map, where the functional argument is the above computation.

	let mySetOfArrs = [| [||], [||] |]
	let myComp = <@
					mySetOfArrs |>
					Array.map(fun myArr ->
						myArr |>
						Array.map(fun a -> a * 2) |> 
						Array.reduce (*))
				 @>
				 
The 0-order collection functions (i.e. the collection functions that do not contain other collection functions or custom kernels) are treated as OpenCL kernels, while the higher-order function (the outermost Array.map) is treated as executor/coordinator of the computations contained inside its functional argument.

##### 5) If functional and collection composition are not flexible enough, divide the expression

Sometimes we want to compose and execute kernels in a way that can be difficult to express using only functional and high-order collection composition.
In such a case, especially when **executing** the computation (instead of compiling it), we can divide the expression into multiple sub-expressions and execute each of them separately.
We refer to this as **imperative composition**.

For example, we can apply this sort of composition to execute each kernel of the computation in 4) separately, coordinating the execution using an imperative loop instead of an higher-order collection function.

	let mySetOfArrs = [| [||], [||] |]
	let myOutSet = [| [||], [||] |]
	for i = 0 to mySetOfArrs.Length - 1 do
		let myArr = mySetOfArrs.[i]
		
		let map = <@ Array.map(fun a -> a * 2) myArr @>
		let mapres = mapcomp.Run()
		let red = <@ Array.reduce (*) mapres @>
		let redres = redcomp.Run()
		
		myOutSet.[i] <- redres
	
