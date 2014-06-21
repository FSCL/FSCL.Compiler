namespace FSCL
open System.Runtime.InteropServices
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters
open System

// Double vector types
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type double2 =
    struct
        val mutable x: double
        val mutable y: double

        member this.Components
            with get() =
                [| this.x; this.y |]
                
        new(X: double, Y: double) =
            { x = X; y = Y }
            
        new(v: double) =
            { x = v; y = v }

        member this.xy 
            with get() =
                double2(this.x, this.y)
            and set (v: double2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.yx 
            with get() =
                double2(this.y, this.x)
            and set (v: double2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.xx 
            with get() =
                double2(this.x, this.x)
            
        member this.yy 
            with get() =
                double2(this.y, this.y)

        member this.lo 
            with get() =
                this.x
            and set v =
                this.x <- v
            
        member this.hi 
            with get() =
                this.y
            and set v =
                this.y <- v
            
        member this.even 
            with get() =
                this.x
            and set v =
                this.x <- v
            
        member this.odd 
            with get() =
                this.y
            and set v =
                this.y <- v

        internal new(c: double[]) =
            double2(c.[0], c.[1])

        static member (+) (f1: double2, f2: double2) =
            double2(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: double2, f2: double2) =
            double2(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: double2, f2: double2) =
            double2(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: double2, f2: double2) =
            double2(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: double2, f2: double2) =
            int2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: double2, f2: double2) =
            int2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: double2, f2: double2) =
            int2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: double2, f2: double2) =
            int2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 2L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> double2
            stream.Close()
            data

        static member hypot(a:double2, b:double2) = 
            new double2(Math.Sqrt((a.x * a.x) + (b.x * b.x)), 
                        Math.Sqrt((a.y * a.y) + (b.y * b.y)))

        member this.pown(n: int) =
            double2(this.x ** (n |> double), 
                    this.y ** (n |> double))
        
        member this.sqrt() =
            double2(Math.Sqrt(this.x), 
                    Math.Sqrt(this.y))
    end
    
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]   
[<VectorType>]                 
type double3 =
    struct
        val mutable x: double
        val mutable y: double
        val mutable z: double

        member this.Components
            with get() =
                [| this.x; this.y; this.z |]
                
        new(X: double, Y: double, Z: double) =
            { x = X; y = Y; z = Z }
    
        new(v: double) =
            { x = v; y = v; z = v }

        member this.xy 
            with get() =
                double2(this.x, this.y)
            and set (v: double2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                double2(this.x, this.z)
            and set (v: double2) =
                this.x <- v.x
                this.z <- v.y

        member this.yx 
            with get() =
                double2(this.y, this.x)
            and set (v: double2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                double2(this.y, this.z)
            and set (v: double2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.zx 
            with get() =
                double2(this.z, this.x)
            and set (v: double2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                double2(this.z, this.y)
            and set (v: double2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.xx 
            with get() =
                double2(this.x, this.x)
            
        member this.yy 
            with get() =
                double2(this.y, this.y)
            
        member this.zz 
            with get() =
                double2(this.z, this.z)

        // 3-comps    
        member this.xyz 
            with get() =
                double3(this.x, this.y, this.z)
            and set (v: double3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                double3(this.x, this.z, this.y)
            and set (v: double3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.yxz 
            with get() =
                double3(this.y, this.x, this.z)
            and set (v: double3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                double3(this.y, this.z, this.x)
            and set (v: double3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.zxy 
            with get() =
                double3(this.z, this.x, this.y)
            and set (v: double3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                double3(this.z, this.y, this.x)
            and set (v: double3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.xxy 
            with get() =
                double3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                double3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                double3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                double3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                double3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                double3(this.z, this.x, this.x)
                        
        member this.yyx 
            with get() =
                double3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                double3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                double3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                double3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                double3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                double3(this.z, this.y, this.y)
            
        member this.zzx 
            with get() =
                double3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                double3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                double3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                double3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                double3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                double3(this.y, this.z, this.z)
            
        member this.xxx 
            with get() =
                double3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                double3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                double3(this.z, this.z, this.z)

        member this.lo 
            with get() =
                double2(this.x, this.y)
            and set (v:double2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                double2(this.y, 0.0)
            and set (v:double2) =
                this.z <- v.x
            
        member this.even 
            with get() =
                double2(this.x, this.z)
            and set (v:double2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                double2(this.y, 0.0)
            and set (v:double2) =
                this.y <- v.x
                
        internal new(c: double[]) =
            double3(c.[0], c.[1], c.[2])

        static member (+) (f1: double3, f2: double3) =
            double3(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: double3, f2: double3) =
            double3(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: double3, f2: double3) =
            double3(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: double3, f2: double3) =
            double3(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: double3, f2: double3) =
            int3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: double3, f2: double3) =
            int3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: double3, f2: double3) =
            int3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: double3, f2: double3) =
            int3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
                        
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> double3
            stream.Close()
            data

        static member hypot(a:double3, b:double3) = 
            new double3(Math.Sqrt((a.x * a.x) + (b.x * b.x)), 
                        Math.Sqrt((a.y * a.y) + (b.y * b.y)),
                        Math.Sqrt((a.z * a.z) + (b.z * b.z)))

        member this.pown(n: int) =
            double3(this.x ** (n |> double), 
                    this.y ** (n |> double), 
                    this.z ** (n |> double))
        
        member this.sqrt() =
            double3(Math.Sqrt(this.x), 
                    Math.Sqrt(this.y), 
                    Math.Sqrt(this.z))
    end
 
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]      
[<VectorType>]  
type double4 =
    struct
        val mutable x: double
        val mutable y: double
        val mutable z: double
        val mutable w: double

        member this.Components
            with get() =
                [| this.x; this.y; this.z; this.w |]
                
        new(X: double, Y: double, Z: double, W: double) =
            { x = X; y = Y; z = Z; w = W }
            
        new(v: double) =
            { x = v; y = v; z = v; w = v }

        member this.xy 
            with get() =
                double2(this.x, this.y)
            and set (v: double2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                double2(this.x, this.z)
            and set (v: double2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.xw 
            with get() =
                double2(this.x, this.w)
            and set (v: double2) =
                this.x <- v.x
                this.w <- v.y

        member this.yx 
            with get() =
                double2(this.y, this.x)
            and set (v: double2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                double2(this.y, this.z)
            and set (v: double2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.yw 
            with get() =
                double2(this.y, this.w)
            and set (v: double2) =
                this.y <- v.x
                this.w <- v.y

        member this.zx 
            with get() =
                double2(this.z, this.x)
            and set (v: double2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                double2(this.z, this.y)
            and set (v: double2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.zw 
            with get() =
                double2(this.z, this.w)
            and set (v: double2) =
                this.z <- v.x
                this.w <- v.y
                          
        member this.wx 
            with get() =
                double2(this.w, this.x)
            and set (v: double2) =
                this.w <- v.x
                this.x <- v.y
            
        member this.wy 
            with get() =
                double2(this.w, this.y)
            and set (v: double2) =
                this.w <- v.x
                this.y <- v.y
            
        member this.wz 
            with get() =
                double2(this.w, this.z)
            and set (v: double2) =
                this.w <- v.x
                this.z <- v.y

        member this.xx 
            with get() =
                double2(this.x, this.x)
            
        member this.yy 
            with get() =
                double2(this.y, this.y)
            
        member this.zz 
            with get() =
                double2(this.z, this.z)
            
        member this.ww 
            with get() =
                double2(this.w, this.w)

        // 3-comps    
        member this.xyz 
            with get() =
                double3(this.x, this.y, this.z)
            and set (v: double3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                double3(this.x, this.z, this.y)
            and set (v: double3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.xyw
            with get() =
                double3(this.x, this.y, this.w)
            and set (v: double3) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.xwy
            with get() =
                double3(this.x, this.w, this.y)
            and set (v: double3) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
            
        member this.xzw
            with get() =
                double3(this.x, this.z, this.w)
            and set (v: double3) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.xwz
            with get() =
                double3(this.x, this.w, this.z)
            and set (v: double3) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.yxz 
            with get() =
                double3(this.y, this.x, this.z)
            and set (v: double3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                double3(this.y, this.z, this.x)
            and set (v: double3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.yxw 
            with get() =
                double3(this.y, this.x, this.w)
            and set (v: double3) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.ywx 
            with get() =
                double3(this.y, this.w, this.x)
            and set (v: double3) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.yzw 
            with get() =
                double3(this.y, this.z, this.w)
            and set (v: double3) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.ywz 
            with get() =
                double3(this.y, this.w, this.z)
            and set (v: double3) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.zxy 
            with get() =
                double3(this.z, this.x, this.y)
            and set (v: double3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                double3(this.z, this.y, this.x)
            and set (v: double3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.zxw 
            with get() =
                double3(this.z, this.x, this.w)
            and set (v: double3) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.zwx 
            with get() =
                double3(this.z, this.w, this.x)
            and set (v: double3) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.zyw 
            with get() =
                double3(this.z, this.y, this.w)
            and set (v: double3) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.zwy 
            with get() =
                double3(this.z, this.w, this.y)
            and set (v: double3) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                        
        member this.wxy 
            with get() =
                double3(this.w, this.x, this.y)
            and set (v: double3) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.wyx 
            with get() =
                double3(this.w, this.y, this.x)
            and set (v: double3) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.wxz 
            with get() =
                double3(this.w, this.x, this.z)
            and set (v: double3) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.wzx 
            with get() =
                double3(this.w, this.z, this.x)
            and set (v: double3) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.wyz 
            with get() =
                double3(this.w, this.y, this.z)
            and set (v: double3) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.wzy 
            with get() =
                double3(this.w, this.z, this.y)
            and set (v: double3) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z

        member this.xxy 
            with get() =
                double3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                double3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                double3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                double3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                double3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                double3(this.z, this.x, this.x)
                    
        member this.xxw 
            with get() =
                double3(this.x, this.x, this.w)
            
        member this.xwx 
            with get() =
                double3(this.x, this.w, this.x)

        member this.wxx 
            with get() =
                double3(this.w, this.x, this.x)
                            
        member this.yyx 
            with get() =
                double3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                double3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                double3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                double3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                double3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                double3(this.z, this.y, this.y)
            
        member this.yyw 
            with get() =
                double3(this.y, this.y, this.w)
            
        member this.ywy 
            with get() =
                double3(this.y, this.w, this.y)

        member this.wyy
            with get() =
                double3(this.w, this.y, this.y)

        member this.zzx 
            with get() =
                double3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                double3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                double3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                double3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                double3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                double3(this.y, this.z, this.z)
            
        member this.zzw 
            with get() =
                double3(this.z, this.z, this.w)
            
        member this.zwz 
            with get() =
                double3(this.z, this.w, this.z)
            
        member this.wzz 
            with get() =
                double3(this.w, this.z, this.z)
            
        member this.xxx 
            with get() =
                double3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                double3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                double3(this.z, this.z, this.z)
 
        member this.www 
            with get() =
                double3(this.w, this.w, this.w)
     
        // 4-comps       
        member this.xxxx
            with get() =
                double4(this.x, this.x, this.x, this.x)

        member this.xxxy
            with get() =
                double4(this.x, this.x, this.x, this.y)

        member this.xxxz
            with get() =
                double4(this.x, this.x, this.x, this.z)

        member this.xxxw
            with get() =
                double4(this.x, this.x, this.x, this.w)

        member this.xxyx
            with get() =
                double4(this.x, this.x, this.y, this.x)

        member this.xxyy
            with get() =
                double4(this.x, this.x, this.y, this.y)

        member this.xxyz
            with get() =
                double4(this.x, this.x, this.y, this.z)

        member this.xxyw
            with get() =
                double4(this.x, this.x, this.y, this.w)

        member this.xxzx
            with get() =
                double4(this.x, this.x, this.z, this.x)

        member this.xxzy
            with get() =
                double4(this.x, this.x, this.z, this.y)

        member this.xxzz
            with get() =
                double4(this.x, this.x, this.z, this.z)

        member this.xxzw
            with get() =
                double4(this.x, this.x, this.z, this.w)

        member this.xxwx
            with get() =
                double4(this.x, this.x, this.w, this.x)

        member this.xxwy
            with get() =
                double4(this.x, this.x, this.w, this.y)

        member this.xxwz
            with get() =
                double4(this.x, this.x, this.w, this.z)

        member this.xxww
            with get() =
                double4(this.x, this.x, this.w, this.w)

        member this.xyxx
            with get() =
                double4(this.x, this.y, this.x, this.x)

        member this.xyxy
            with get() =
                double4(this.x, this.y, this.x, this.y)

        member this.xyxz
            with get() =
                double4(this.x, this.y, this.x, this.z)

        member this.xyxw
            with get() =
                double4(this.x, this.y, this.x, this.w)

        member this.xyyx
            with get() =
                double4(this.x, this.y, this.y, this.x)

        member this.xyyy
            with get() =
                double4(this.x, this.y, this.y, this.y)

        member this.xyyz
            with get() =
                double4(this.x, this.y, this.y, this.z)

        member this.xyyw
            with get() =
                double4(this.x, this.y, this.y, this.w)

        member this.xyzx
            with get() =
                double4(this.x, this.y, this.z, this.x)

        member this.xyzy
            with get() =
                double4(this.x, this.y, this.z, this.y)

        member this.xyzz
            with get() =
                double4(this.x, this.y, this.z, this.z)

        member this.xyzw
            with get() =
                double4(this.x, this.y, this.z, this.w)
            and set(v: double4) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
                this.w <- v.w
            
        member this.xywx
            with get() =
                double4(this.x, this.y, this.w, this.x)

        member this.xywy
            with get() =
                double4(this.x, this.y, this.w, this.y)

        member this.xywz
            with get() =
                double4(this.x, this.y, this.w, this.z)
            and set(v: double4) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.xyww
            with get() =
                double4(this.x, this.y, this.w, this.w)

        member this.xzxx
            with get() =
                double4(this.x, this.z, this.x, this.x)

        member this.xzxy
            with get() =
                double4(this.x, this.z, this.x, this.y)

        member this.xzxz
            with get() =
                double4(this.x, this.z, this.x, this.z)

        member this.xzxw
            with get() =
                double4(this.x, this.z, this.x, this.w)

        member this.xzyx
            with get() =
                double4(this.x, this.z, this.y, this.x)

        member this.xzyy
            with get() =
                double4(this.x, this.z, this.y, this.y)

        member this.xzyz
            with get() =
                double4(this.x, this.z, this.y, this.z)

        member this.xzyw
            with get() =
                double4(this.x, this.z, this.y, this.w)
            and set(v: double4) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.xzzx
            with get() =
                double4(this.x, this.z, this.z, this.x)

        member this.xzzy
            with get() =
                double4(this.x, this.z, this.z, this.y)

        member this.xzzz
            with get() =
                double4(this.x, this.z, this.z, this.z)

        member this.xzzw
            with get() =
                double4(this.x, this.z, this.z, this.w)

        member this.xzwx
            with get() =
                double4(this.x, this.z, this.w, this.x)

        member this.xzwy
            with get() =
                double4(this.x, this.z, this.w, this.y)
            and set(v: double4) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.xzwz
            with get() =
                double4(this.x, this.z, this.w, this.z)

        member this.xzww
            with get() =
                double4(this.x, this.z, this.w, this.w)

        member this.xwxx
            with get() =
                double4(this.x, this.w, this.x, this.x)

        member this.xwxy
            with get() =
                double4(this.x, this.w, this.x, this.y)

        member this.xwxz
            with get() =
                double4(this.x, this.w, this.x, this.z)

        member this.xwxw
            with get() =
                double4(this.x, this.w, this.x, this.w)

        member this.xwyx
            with get() =
                double4(this.x, this.w, this.y, this.x)

        member this.xwyy
            with get() =
                double4(this.x, this.w, this.y, this.y)

        member this.xwyz
            with get() =
                double4(this.x, this.w, this.y, this.z)
            and set(v: double4) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.xwyw
            with get() =
                double4(this.x, this.w, this.y, this.w)

        member this.xwzx
            with get() =
                double4(this.x, this.w, this.z, this.x)

        member this.xwzy
            with get() =
                double4(this.x, this.w, this.z, this.y)
            and set(v: double4) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z
                this.y <- v.w
            
        member this.xwzz
            with get() =
                double4(this.x, this.w, this.z, this.z)

        member this.xwzw
            with get() =
                double4(this.x, this.w, this.z, this.w)

        member this.xwwx
            with get() =
                double4(this.x, this.w, this.w, this.x)

        member this.xwwy
            with get() =
                double4(this.x, this.w, this.w, this.y)

        member this.xwwz
            with get() =
                double4(this.x, this.w, this.w, this.z)

        member this.xwww
            with get() =
                double4(this.x, this.w, this.w, this.w)

        member this.yxxx
            with get() =
                double4(this.y, this.x, this.x, this.x)

        member this.yxxy
            with get() =
                double4(this.y, this.x, this.x, this.y)

        member this.yxxz
            with get() =
                double4(this.y, this.x, this.x, this.z)

        member this.yxxw
            with get() =
                double4(this.y, this.x, this.x, this.w)

        member this.yxyx
            with get() =
                double4(this.y, this.x, this.y, this.x)

        member this.yxyy
            with get() =
                double4(this.y, this.x, this.y, this.y)

        member this.yxyz
            with get() =
                double4(this.y, this.x, this.y, this.z)

        member this.yxyw
            with get() =
                double4(this.y, this.x, this.y, this.w)

        member this.yxzx
            with get() =
                double4(this.y, this.x, this.z, this.x)

        member this.yxzy
            with get() =
                double4(this.y, this.x, this.z, this.y)

        member this.yxzz
            with get() =
                double4(this.y, this.x, this.z, this.z)

        member this.yxzw
            with get() =
                double4(this.y, this.x, this.z, this.w)
            and set(v: double4) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
                this.w <- v.w


        member this.yxwx
            with get() =
                double4(this.y, this.x, this.w, this.x)

        member this.yxwy
            with get() =
                double4(this.y, this.x, this.w, this.y)

        member this.yxwz
            with get() =
                double4(this.y, this.x, this.w, this.z)
            and set(v: double4) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.yxww
            with get() =
                double4(this.y, this.x, this.w, this.w)

        member this.yyxx
            with get() =
                double4(this.y, this.y, this.x, this.x)

        member this.yyxy
            with get() =
                double4(this.y, this.y, this.x, this.y)

        member this.yyxz
            with get() =
                double4(this.y, this.y, this.x, this.z)

        member this.yyxw
            with get() =
                double4(this.y, this.y, this.x, this.w)

        member this.yyyx
            with get() =
                double4(this.y, this.y, this.y, this.x)

        member this.yyyy
            with get() =
                double4(this.y, this.y, this.y, this.y)

        member this.yyyz
            with get() =
                double4(this.y, this.y, this.y, this.z)

        member this.yyyw
            with get() =
                double4(this.y, this.y, this.y, this.w)

        member this.yyzx
            with get() =
                double4(this.y, this.y, this.z, this.x)

        member this.yyzy
            with get() =
                double4(this.y, this.y, this.z, this.y)

        member this.yyzz
            with get() =
                double4(this.y, this.y, this.z, this.z)

        member this.yyzw
            with get() =
                double4(this.y, this.y, this.z, this.w)

        member this.yywx
            with get() =
                double4(this.y, this.y, this.w, this.x)

        member this.yywy
            with get() =
                double4(this.y, this.y, this.w, this.y)

        member this.yywz
            with get() =
                double4(this.y, this.y, this.w, this.z)

        member this.yyww
            with get() =
                double4(this.y, this.y, this.w, this.w)

        member this.yzxx
            with get() =
                double4(this.y, this.z, this.x, this.x)

        member this.yzxy
            with get() =
                double4(this.y, this.z, this.x, this.y)

        member this.yzxz
            with get() =
                double4(this.y, this.z, this.x, this.z)

        member this.yzxw
            with get() =
                double4(this.y, this.z, this.x, this.w)
            and set(v: double4) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.yzyx
            with get() =
                double4(this.y, this.z, this.y, this.x)

        member this.yzyy
            with get() =
                double4(this.y, this.z, this.y, this.y)

        member this.yzyz
            with get() =
                double4(this.y, this.z, this.y, this.z)

        member this.yzyw
            with get() =
                double4(this.y, this.z, this.y, this.w)

        member this.yzzx
            with get() =
                double4(this.y, this.z, this.z, this.x)

        member this.yzzy
            with get() =
                double4(this.y, this.z, this.z, this.y)

        member this.yzzz
            with get() =
                double4(this.y, this.z, this.z, this.z)

        member this.yzzw
            with get() =
                double4(this.y, this.z, this.z, this.w)

        member this.yzwx
            with get() =
                double4(this.y, this.z, this.w, this.x)
            and set(v: double4) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.yzwy
            with get() =
                double4(this.y, this.z, this.w, this.y)

        member this.yzwz
            with get() =
                double4(this.y, this.z, this.w, this.z)

        member this.yzww
            with get() =
                double4(this.y, this.z, this.w, this.w)

        member this.ywxx
            with get() =
                double4(this.y, this.w, this.x, this.x)

        member this.ywxy
            with get() =
                double4(this.y, this.w, this.x, this.y)

        member this.ywxz
            with get() =
                double4(this.y, this.w, this.x, this.z)
            and set(v: double4) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
                this.z <- v.w


        member this.ywxw
            with get() =
                double4(this.y, this.w, this.x, this.w)

        member this.ywyx
            with get() =
                double4(this.y, this.w, this.y, this.x)

        member this.ywyy
            with get() =
                double4(this.y, this.w, this.y, this.y)

        member this.ywyz
            with get() =
                double4(this.y, this.w, this.y, this.z)

        member this.ywyw
            with get() =
                double4(this.y, this.w, this.y, this.w)

        member this.ywzx
            with get() =
                double4(this.y, this.w, this.z, this.x)
            and set(v: double4) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.ywzy
            with get() =
                double4(this.y, this.w, this.z, this.y)

        member this.ywzz
            with get() =
                double4(this.y, this.w, this.z, this.z)

        member this.ywzw
            with get() =
                double4(this.y, this.w, this.z, this.w)

        member this.ywwx
            with get() =
                double4(this.y, this.w, this.w, this.x)

        member this.ywwy
            with get() =
                double4(this.y, this.w, this.w, this.y)

        member this.ywwz
            with get() =
                double4(this.y, this.w, this.w, this.z)

        member this.ywww
            with get() =
                double4(this.y, this.w, this.w, this.w)

        member this.zxxx
            with get() =
                double4(this.z, this.x, this.x, this.x)

        member this.zxxy
            with get() =
                double4(this.z, this.x, this.x, this.y)

        member this.zxxz
            with get() =
                double4(this.z, this.x, this.x, this.z)

        member this.zxxw
            with get() =
                double4(this.z, this.x, this.x, this.w)

        member this.zxyx
            with get() =
                double4(this.z, this.x, this.y, this.x)

        member this.zxyy
            with get() =
                double4(this.z, this.x, this.y, this.y)

        member this.zxyz
            with get() =
                double4(this.z, this.x, this.y, this.z)

        member this.zxyw
            with get() =
                double4(this.z, this.x, this.y, this.w)
            and set(v: double4) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.zxzx
            with get() =
                double4(this.z, this.x, this.z, this.x)

        member this.zxzy
            with get() =
                double4(this.z, this.x, this.z, this.y)

        member this.zxzz
            with get() =
                double4(this.z, this.x, this.z, this.z)

        member this.zxzw
            with get() =
                double4(this.z, this.x, this.z, this.w)

        member this.zxwx
            with get() =
                double4(this.z, this.x, this.w, this.x)

        member this.zxwy
            with get() =
                double4(this.z, this.x, this.w, this.y)
            and set(v: double4) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.zxwz
            with get() =
                double4(this.z, this.x, this.w, this.z)

        member this.zxww
            with get() =
                double4(this.z, this.x, this.w, this.w)

        member this.zyxx
            with get() =
                double4(this.z, this.y, this.x, this.x)

        member this.zyxy
            with get() =
                double4(this.z, this.y, this.x, this.y)

        member this.zyxz
            with get() =
                double4(this.z, this.y, this.x, this.z)

        member this.zyxw
            with get() =
                double4(this.z, this.y, this.x, this.w)
            and set(v: double4) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.zyyx
            with get() =
                double4(this.z, this.y, this.y, this.x)

        member this.zyyy
            with get() =
                double4(this.z, this.y, this.y, this.y)

        member this.zyyz
            with get() =
                double4(this.z, this.y, this.y, this.z)

        member this.zyyw
            with get() =
                double4(this.z, this.y, this.y, this.w)

        member this.zyzx
            with get() =
                double4(this.z, this.y, this.z, this.x)

        member this.zyzy
            with get() =
                double4(this.z, this.y, this.z, this.y)

        member this.zyzz
            with get() =
                double4(this.z, this.y, this.z, this.z)

        member this.zyzw
            with get() =
                double4(this.z, this.y, this.z, this.w)

        member this.zywx
            with get() =
                double4(this.z, this.y, this.w, this.x)
            and set(v: double4) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.zywy
            with get() =
                double4(this.z, this.y, this.w, this.y)

        member this.zywz
            with get() =
                double4(this.z, this.y, this.w, this.z)

        member this.zyww
            with get() =
                double4(this.z, this.y, this.w, this.w)

        member this.zzxx
            with get() =
                double4(this.z, this.z, this.x, this.x)

        member this.zzxy
            with get() =
                double4(this.z, this.z, this.x, this.y)

        member this.zzxz
            with get() =
                double4(this.z, this.z, this.x, this.z)

        member this.zzxw
            with get() =
                double4(this.z, this.z, this.x, this.w)

        member this.zzyx
            with get() =
                double4(this.z, this.z, this.y, this.x)

        member this.zzyy
            with get() =
                double4(this.z, this.z, this.y, this.y)

        member this.zzyz
            with get() =
                double4(this.z, this.z, this.y, this.z)

        member this.zzyw
            with get() =
                double4(this.z, this.z, this.y, this.w)

        member this.zzzx
            with get() =
                double4(this.z, this.z, this.z, this.x)

        member this.zzzy
            with get() =
                double4(this.z, this.z, this.z, this.y)

        member this.zzzz
            with get() =
                double4(this.z, this.z, this.z, this.z)

        member this.zzzw
            with get() =
                double4(this.z, this.z, this.z, this.w)

        member this.zzwx
            with get() =
                double4(this.z, this.z, this.w, this.x)

        member this.zzwy
            with get() =
                double4(this.z, this.z, this.w, this.y)

        member this.zzwz
            with get() =
                double4(this.z, this.z, this.w, this.z)

        member this.zzww
            with get() =
                double4(this.z, this.z, this.w, this.w)

        member this.zwxx
            with get() =
                double4(this.z, this.w, this.x, this.x)

        member this.zwxy
            with get() =
                double4(this.z, this.w, this.x, this.y)
            and set(v: double4) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.zwxz
            with get() =
                double4(this.z, this.w, this.x, this.z)

        member this.zwxw
            with get() =
                double4(this.z, this.w, this.x, this.w)

        member this.zwyx
            with get() =
                double4(this.z, this.w, this.y, this.x)
            and set(v: double4) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.zwyy
            with get() =
                double4(this.z, this.w, this.y, this.y)

        member this.zwyz
            with get() =
                double4(this.z, this.w, this.y, this.z)

        member this.zwyw
            with get() =
                double4(this.z, this.w, this.y, this.w)

        member this.zwzx
            with get() =
                double4(this.z, this.w, this.z, this.x)

        member this.zwzy
            with get() =
                double4(this.z, this.w, this.z, this.y)

        member this.zwzz
            with get() =
                double4(this.z, this.w, this.z, this.z)

        member this.zwzw
            with get() =
                double4(this.z, this.w, this.z, this.w)

        member this.zwwx
            with get() =
                double4(this.z, this.w, this.w, this.x)

        member this.zwwy
            with get() =
                double4(this.z, this.w, this.w, this.y)

        member this.zwwz
            with get() =
                double4(this.z, this.w, this.w, this.z)

        member this.zwww
            with get() =
                double4(this.z, this.w, this.w, this.w)

        member this.wxxx
            with get() =
                double4(this.w, this.x, this.x, this.x)

        member this.wxxy
            with get() =
                double4(this.w, this.x, this.x, this.y)

        member this.wxxz
            with get() =
                double4(this.w, this.x, this.x, this.z)

        member this.wxxw
            with get() =
                double4(this.w, this.x, this.x, this.w)

        member this.wxyx
            with get() =
                double4(this.w, this.x, this.y, this.x)

        member this.wxyy
            with get() =
                double4(this.w, this.x, this.y, this.y)

        member this.wxyz
            with get() =
                double4(this.w, this.x, this.y, this.z)
            and set(v: double4) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.wxyw
            with get() =
                double4(this.w, this.x, this.y, this.w)

        member this.wxzx
            with get() =
                double4(this.w, this.x, this.z, this.x)

        member this.wxzy
            with get() =
                double4(this.w, this.x, this.z, this.y)
            and set(v: double4) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
                this.y <- v.w


        member this.wxzz
            with get() =
                double4(this.w, this.x, this.z, this.z)

        member this.wxzw
            with get() =
                double4(this.w, this.x, this.z, this.w)

        member this.wxwx
            with get() =
                double4(this.w, this.x, this.w, this.x)

        member this.wxwy
            with get() =
                double4(this.w, this.x, this.w, this.y)

        member this.wxwz
            with get() =
                double4(this.w, this.x, this.w, this.z)

        member this.wxww
            with get() =
                double4(this.w, this.x, this.w, this.w)

        member this.wyxx
            with get() =
                double4(this.w, this.y, this.x, this.x)

        member this.wyxy
            with get() =
                double4(this.w, this.y, this.x, this.y)

        member this.wyxz
            with get() =
                double4(this.w, this.y, this.x, this.z)
            and set(v: double4) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
                this.z <- v.w
            
        member this.wyxw
            with get() =
                double4(this.w, this.y, this.x, this.w)

        member this.wyyx
            with get() =
                double4(this.w, this.y, this.y, this.x)

        member this.wyyy
            with get() =
                double4(this.w, this.y, this.y, this.y)

        member this.wyyz
            with get() =
                double4(this.w, this.y, this.y, this.z)

        member this.wyyw
            with get() =
                double4(this.w, this.y, this.y, this.w)

        member this.wyzx
            with get() =
                double4(this.w, this.y, this.z, this.x)
            and set(v: double4) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.wyzy
            with get() =
                double4(this.w, this.y, this.z, this.y)

        member this.wyzz
            with get() =
                double4(this.w, this.y, this.z, this.z)

        member this.wyzw
            with get() =
                double4(this.w, this.y, this.z, this.w)

        member this.wywx
            with get() =
                double4(this.w, this.y, this.w, this.x)

        member this.wywy
            with get() =
                double4(this.w, this.y, this.w, this.y)

        member this.wywz
            with get() =
                double4(this.w, this.y, this.w, this.z)

        member this.wyww
            with get() =
                double4(this.w, this.y, this.w, this.w)

        member this.wzxx
            with get() =
                double4(this.w, this.z, this.x, this.x)

        member this.wzxy
            with get() =
                double4(this.w, this.z, this.x, this.y)
            and set(v: double4) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.wzxz
            with get() =
                double4(this.w, this.z, this.x, this.z)

        member this.wzxw
            with get() =
                double4(this.w, this.z, this.x, this.w)

        member this.wzyx
            with get() =
                double4(this.w, this.z, this.y, this.x)
            and set(v: double4) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.wzyy
            with get() =
                double4(this.w, this.z, this.y, this.y)

        member this.wzyz
            with get() =
                double4(this.w, this.z, this.y, this.z)

        member this.wzyw
            with get() =
                double4(this.w, this.z, this.y, this.w)

        member this.wzzx
            with get() =
                double4(this.w, this.z, this.z, this.x)

        member this.wzzy
            with get() =
                double4(this.w, this.z, this.z, this.y)

        member this.wzzz
            with get() =
                double4(this.w, this.z, this.z, this.z)

        member this.wzzw
            with get() =
                double4(this.w, this.z, this.z, this.w)

        member this.wzwx
            with get() =
                double4(this.w, this.z, this.w, this.x)

        member this.wzwy
            with get() =
                double4(this.w, this.z, this.w, this.y)

        member this.wzwz
            with get() =
                double4(this.w, this.z, this.w, this.z)

        member this.wzww
            with get() =
                double4(this.w, this.z, this.w, this.w)

        member this.wwxx
            with get() =
                double4(this.w, this.w, this.x, this.x)

        member this.wwxy
            with get() =
                double4(this.w, this.w, this.x, this.y)

        member this.wwxz
            with get() =
                double4(this.w, this.w, this.x, this.z)

        member this.wwxw
            with get() =
                double4(this.w, this.w, this.x, this.w)

        member this.wwyx
            with get() =
                double4(this.w, this.w, this.y, this.x)

        member this.wwyy
            with get() =
                double4(this.w, this.w, this.y, this.y)

        member this.wwyz
            with get() =
                double4(this.w, this.w, this.y, this.z)

        member this.wwyw
            with get() =
                double4(this.w, this.w, this.y, this.w)

        member this.wwzx
            with get() =
                double4(this.w, this.w, this.z, this.x)

        member this.wwzy
            with get() =
                double4(this.w, this.w, this.z, this.y)

        member this.wwzz
            with get() =
                double4(this.w, this.w, this.z, this.z)

        member this.wwzw
            with get() =
                double4(this.w, this.w, this.z, this.w)

        member this.wwwx
            with get() =
                double4(this.w, this.w, this.w, this.x)

        member this.wwwy
            with get() =
                double4(this.w, this.w, this.w, this.y)

        member this.wwwz
            with get() =
                double4(this.w, this.w, this.w, this.z)

        member this.wwww
            with get() =
                double4(this.w, this.w, this.w, this.w)

        member this.lo 
            with get() =
                double2(this.x, this.y)
            and set (v:double2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                double2(this.y, this.w)
            and set (v:double2) =
                this.z <- v.x
                this.w <- v.y
            
        member this.even 
            with get() =
                double2(this.x, this.z)
            and set (v:double2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                double2(this.y, this.w)
            and set (v:double2) =
                this.y <- v.x
                this.w <- v.y
                
        internal new(c: double[]) =
            double4(c.[0], c.[1], c.[2], c.[3])

        static member (+) (f1: double4, f2: double4) =
            double4(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: double4, f2: double4) =
            double4(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: double4, f2: double4) =
            double4(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: double4, f2: double4) =
            double4(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: double4, f2: double4) =
            int4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: double4, f2: double4) =
            int4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: double4, f2: double4) =
            int4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: double4, f2: double4) =
            int4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
                        
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 4L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> double4
            stream.Close()
            data

        static member hypot(a:double4, b:double4) = 
            new double4(Math.Sqrt((a.x * a.x) + (b.x * b.x)), 
                        Math.Sqrt((a.y * a.y) + (b.y * b.y)),
                        Math.Sqrt((a.z * a.z) + (b.z * b.z)),
                        Math.Sqrt((a.w * a.w) + (b.w * b.w)))

        member this.pown(n: int) =
            double4(this.x ** (n |> double), 
                    this.y ** (n |> double), 
                    this.z ** (n |> double), 
                    this.w ** (n |> double))
        
        member this.sqrt() =
            double4(Math.Sqrt(this.x), 
                    Math.Sqrt(this.y), 
                    Math.Sqrt(this.z),
                    Math.Sqrt(this.w))  
    end