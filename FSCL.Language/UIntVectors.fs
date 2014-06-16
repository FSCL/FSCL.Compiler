namespace FSCL
open System.Runtime.InteropServices
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters
open System

type uint = System.UInt32

// Integer vector types
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type uint2 =
    struct 
        val mutable x: uint
        val mutable y: uint

        member internal this.Components
            with get() =
                [| this.x; this.y |]
            
        member this.xy 
            with get() =
                uint2(this.x, this.y)
            and set (v: uint2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.yx 
            with get() =
                uint2(this.y, this.x)
            and set (v: uint2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.xx 
            with get() =
                uint2(this.x, this.x)
            
        member this.yy 
            with get() =
                uint2(this.y, this.y)

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

        new(X: uint32, Y: uint32) =
            { x = X; y = Y }
            
        new(v: uint32) =
            { x = v; y = v }

        internal new(c: uint32[]) =
            uint2(c.[0], c.[1])

        static member (+) (f1: uint2, f2: uint2) =
            uint2(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: uint2, f2: uint2) =
            uint2(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: uint2, f2: uint2) =
            uint2(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: uint2, f2: uint2) =
            uint2(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: uint2, f2: uint2) =
            int2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: uint2, f2: uint2) =
            int2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: uint2, f2: uint2) =
            int2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: uint2, f2: uint2) =
            int2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 2L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> uint2
            stream.Close()
            data

        static member hypot(a:uint2, b:uint2) = 
            new uint2(Math.Sqrt((a.x * a.x) + (b.x * b.x) |> float) |> uint32, 
                      Math.Sqrt((a.y * a.y) + (b.y * b.y) |> float) |> uint32)
    end
    
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]  
[<VectorType>]             
type uint3 =
    struct
        val mutable x: uint
        val mutable y: uint
        val mutable z: uint

        member internal this.Components
            with get() =
                [| this.x; this.y; this.z |]
                
        new(X: uint32, Y: uint32, Z: uint32) =
            { x = X; y = Y; z = Z }
            
        new(v: uint32) =
            { x = v; y = v; z = v }

        member this.xy 
            with get() =
                uint2(this.x, this.y)
            and set (v: uint2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                uint2(this.x, this.z)
            and set (v: uint2) =
                this.x <- v.x
                this.z <- v.y

        member this.yx 
            with get() =
                uint2(this.y, this.x)
            and set (v: uint2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                uint2(this.y, this.z)
            and set (v: uint2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.zx 
            with get() =
                uint2(this.z, this.x)
            and set (v: uint2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                uint2(this.z, this.y)
            and set (v: uint2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.xx 
            with get() =
                uint2(this.x, this.x)
            
        member this.yy 
            with get() =
                uint2(this.y, this.y)
            
        member this.zz 
            with get() =
                uint2(this.z, this.z)

        // 3-comps    
        member this.xyz 
            with get() =
                uint3(this.x, this.y, this.z)
            and set (v: uint3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                uint3(this.x, this.z, this.y)
            and set (v: uint3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.yxz 
            with get() =
                uint3(this.y, this.x, this.z)
            and set (v: uint3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                uint3(this.y, this.z, this.x)
            and set (v: uint3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.zxy 
            with get() =
                uint3(this.z, this.x, this.y)
            and set (v: uint3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                uint3(this.z, this.y, this.x)
            and set (v: uint3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.xxy 
            with get() =
                uint3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                uint3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                uint3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                uint3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                uint3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                uint3(this.z, this.x, this.x)
                        
        member this.yyx 
            with get() =
                uint3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                uint3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                uint3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                uint3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                uint3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                uint3(this.z, this.y, this.y)
            
        member this.zzx 
            with get() =
                uint3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                uint3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                uint3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                uint3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                uint3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                uint3(this.y, this.z, this.z)
            
        member this.xxx 
            with get() =
                uint3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                uint3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                uint3(this.z, this.z, this.z)

        member this.lo 
            with get() =
                uint2(this.x, this.y)
            and set (v:uint2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                uint2(this.y, 0u)
            and set (v:uint2) =
                this.z <- v.x
            
        member this.even 
            with get() =
                uint2(this.x, this.z)
            and set (v:uint2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                uint2(this.y, 0u)
            and set (v:uint2) =
                this.y <- v.x

        internal new(c: uint32[]) =
            uint3(c.[0], c.[1], c.[2])

        static member (+) (f1: uint3, f2: uint3) =
            uint3(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: uint3, f2: uint3) =
            uint3(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: uint3, f2: uint3) =
            uint3(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: uint3, f2: uint3) =
            uint3(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: uint3, f2: uint3) =
            int3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: uint3, f2: uint3) =
            int3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: uint3, f2: uint3) =
            int3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: uint3, f2: uint3) =
            int3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> uint3
            stream.Close()
            data

        static member hypot(a:uint3, b:uint3) = 
            new uint3(Math.Sqrt((a.x * a.x) + (b.x * b.x) |> float) |> uint32, 
                      Math.Sqrt((a.y * a.y) + (b.y * b.y) |> float) |> uint32,
                      Math.Sqrt((a.z * a.z) + (b.z * b.z) |> float) |> uint32)
    end

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]      
[<VectorType>]      
type uint4 =
    struct
        val mutable x: uint
        val mutable y: uint
        val mutable z: uint
        val mutable w: uint

        member internal this.Components
            with get() =
                [| this.x; this.y; this.z; this.w |]
                
        new(X: uint32, Y: uint32, Z: uint32, W: uint32) =
            { x = X; y = Y; z = Z; w = W }
            
        new(v: uint32) =
            { x = v; y = v; z = v; w = v }

        member this.xy 
            with get() =
                uint2(this.x, this.y)
            and set (v: uint2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                uint2(this.x, this.z)
            and set (v: uint2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.xw 
            with get() =
                uint2(this.x, this.w)
            and set (v: uint2) =
                this.x <- v.x
                this.w <- v.y

        member this.yx 
            with get() =
                uint2(this.y, this.x)
            and set (v: uint2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                uint2(this.y, this.z)
            and set (v: uint2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.yw 
            with get() =
                uint2(this.y, this.w)
            and set (v: uint2) =
                this.y <- v.x
                this.w <- v.y

        member this.zx 
            with get() =
                uint2(this.z, this.x)
            and set (v: uint2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                uint2(this.z, this.y)
            and set (v: uint2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.zw 
            with get() =
                uint2(this.z, this.w)
            and set (v: uint2) =
                this.z <- v.x
                this.w <- v.y
                          
        member this.wx 
            with get() =
                uint2(this.w, this.x)
            and set (v: uint2) =
                this.w <- v.x
                this.x <- v.y
            
        member this.wy 
            with get() =
                uint2(this.w, this.y)
            and set (v: uint2) =
                this.w <- v.x
                this.y <- v.y
            
        member this.wz 
            with get() =
                uint2(this.w, this.z)
            and set (v: uint2) =
                this.w <- v.x
                this.z <- v.y

        member this.xx 
            with get() =
                uint2(this.x, this.x)
            
        member this.yy 
            with get() =
                uint2(this.y, this.y)
            
        member this.zz 
            with get() =
                uint2(this.z, this.z)
            
        member this.ww 
            with get() =
                uint2(this.w, this.w)

        // 3-comps    
        member this.xyz 
            with get() =
                uint3(this.x, this.y, this.z)
            and set (v: uint3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                uint3(this.x, this.z, this.y)
            and set (v: uint3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.xyw
            with get() =
                uint3(this.x, this.y, this.w)
            and set (v: uint3) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.xwy
            with get() =
                uint3(this.x, this.w, this.y)
            and set (v: uint3) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
            
        member this.xzw
            with get() =
                uint3(this.x, this.z, this.w)
            and set (v: uint3) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.xwz
            with get() =
                uint3(this.x, this.w, this.z)
            and set (v: uint3) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.yxz 
            with get() =
                uint3(this.y, this.x, this.z)
            and set (v: uint3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                uint3(this.y, this.z, this.x)
            and set (v: uint3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.yxw 
            with get() =
                uint3(this.y, this.x, this.w)
            and set (v: uint3) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.ywx 
            with get() =
                uint3(this.y, this.w, this.x)
            and set (v: uint3) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.yzw 
            with get() =
                uint3(this.y, this.z, this.w)
            and set (v: uint3) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.ywz 
            with get() =
                uint3(this.y, this.w, this.z)
            and set (v: uint3) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.zxy 
            with get() =
                uint3(this.z, this.x, this.y)
            and set (v: uint3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                uint3(this.z, this.y, this.x)
            and set (v: uint3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.zxw 
            with get() =
                uint3(this.z, this.x, this.w)
            and set (v: uint3) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.zwx 
            with get() =
                uint3(this.z, this.w, this.x)
            and set (v: uint3) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.zyw 
            with get() =
                uint3(this.z, this.y, this.w)
            and set (v: uint3) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.zwy 
            with get() =
                uint3(this.z, this.w, this.y)
            and set (v: uint3) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                        
        member this.wxy 
            with get() =
                uint3(this.w, this.x, this.y)
            and set (v: uint3) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.wyx 
            with get() =
                uint3(this.w, this.y, this.x)
            and set (v: uint3) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.wxz 
            with get() =
                uint3(this.w, this.x, this.z)
            and set (v: uint3) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.wzx 
            with get() =
                uint3(this.w, this.z, this.x)
            and set (v: uint3) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.wyz 
            with get() =
                uint3(this.w, this.y, this.z)
            and set (v: uint3) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.wzy 
            with get() =
                uint3(this.w, this.z, this.y)
            and set (v: uint3) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z

        member this.xxy 
            with get() =
                uint3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                uint3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                uint3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                uint3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                uint3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                uint3(this.z, this.x, this.x)
                    
        member this.xxw 
            with get() =
                uint3(this.x, this.x, this.w)
            
        member this.xwx 
            with get() =
                uint3(this.x, this.w, this.x)

        member this.wxx 
            with get() =
                uint3(this.w, this.x, this.x)
                            
        member this.yyx 
            with get() =
                uint3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                uint3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                uint3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                uint3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                uint3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                uint3(this.z, this.y, this.y)
            
        member this.yyw 
            with get() =
                uint3(this.y, this.y, this.w)
            
        member this.ywy 
            with get() =
                uint3(this.y, this.w, this.y)

        member this.wyy
            with get() =
                uint3(this.w, this.y, this.y)

        member this.zzx 
            with get() =
                uint3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                uint3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                uint3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                uint3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                uint3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                uint3(this.y, this.z, this.z)
            
        member this.zzw 
            with get() =
                uint3(this.z, this.z, this.w)
            
        member this.zwz 
            with get() =
                uint3(this.z, this.w, this.z)
            
        member this.wzz 
            with get() =
                uint3(this.w, this.z, this.z)
            
        member this.xxx 
            with get() =
                uint3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                uint3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                uint3(this.z, this.z, this.z)
 
        member this.www 
            with get() =
                uint3(this.w, this.w, this.w)
     
        // 4-comps       
        member this.xxxx
            with get() =
                uint4(this.x, this.x, this.x, this.x)

        member this.xxxy
            with get() =
                uint4(this.x, this.x, this.x, this.y)

        member this.xxxz
            with get() =
                uint4(this.x, this.x, this.x, this.z)

        member this.xxxw
            with get() =
                uint4(this.x, this.x, this.x, this.w)

        member this.xxyx
            with get() =
                uint4(this.x, this.x, this.y, this.x)

        member this.xxyy
            with get() =
                uint4(this.x, this.x, this.y, this.y)

        member this.xxyz
            with get() =
                uint4(this.x, this.x, this.y, this.z)

        member this.xxyw
            with get() =
                uint4(this.x, this.x, this.y, this.w)

        member this.xxzx
            with get() =
                uint4(this.x, this.x, this.z, this.x)

        member this.xxzy
            with get() =
                uint4(this.x, this.x, this.z, this.y)

        member this.xxzz
            with get() =
                uint4(this.x, this.x, this.z, this.z)

        member this.xxzw
            with get() =
                uint4(this.x, this.x, this.z, this.w)

        member this.xxwx
            with get() =
                uint4(this.x, this.x, this.w, this.x)

        member this.xxwy
            with get() =
                uint4(this.x, this.x, this.w, this.y)

        member this.xxwz
            with get() =
                uint4(this.x, this.x, this.w, this.z)

        member this.xxww
            with get() =
                uint4(this.x, this.x, this.w, this.w)

        member this.xyxx
            with get() =
                uint4(this.x, this.y, this.x, this.x)

        member this.xyxy
            with get() =
                uint4(this.x, this.y, this.x, this.y)

        member this.xyxz
            with get() =
                uint4(this.x, this.y, this.x, this.z)

        member this.xyxw
            with get() =
                uint4(this.x, this.y, this.x, this.w)

        member this.xyyx
            with get() =
                uint4(this.x, this.y, this.y, this.x)

        member this.xyyy
            with get() =
                uint4(this.x, this.y, this.y, this.y)

        member this.xyyz
            with get() =
                uint4(this.x, this.y, this.y, this.z)

        member this.xyyw
            with get() =
                uint4(this.x, this.y, this.y, this.w)

        member this.xyzx
            with get() =
                uint4(this.x, this.y, this.z, this.x)

        member this.xyzy
            with get() =
                uint4(this.x, this.y, this.z, this.y)

        member this.xyzz
            with get() =
                uint4(this.x, this.y, this.z, this.z)

        member this.xyzw
            with get() =
                uint4(this.x, this.y, this.z, this.w)
            and set(v: uint4) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
                this.w <- v.w
            
        member this.xywx
            with get() =
                uint4(this.x, this.y, this.w, this.x)

        member this.xywy
            with get() =
                uint4(this.x, this.y, this.w, this.y)

        member this.xywz
            with get() =
                uint4(this.x, this.y, this.w, this.z)
            and set(v: uint4) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.xyww
            with get() =
                uint4(this.x, this.y, this.w, this.w)

        member this.xzxx
            with get() =
                uint4(this.x, this.z, this.x, this.x)

        member this.xzxy
            with get() =
                uint4(this.x, this.z, this.x, this.y)

        member this.xzxz
            with get() =
                uint4(this.x, this.z, this.x, this.z)

        member this.xzxw
            with get() =
                uint4(this.x, this.z, this.x, this.w)

        member this.xzyx
            with get() =
                uint4(this.x, this.z, this.y, this.x)

        member this.xzyy
            with get() =
                uint4(this.x, this.z, this.y, this.y)

        member this.xzyz
            with get() =
                uint4(this.x, this.z, this.y, this.z)

        member this.xzyw
            with get() =
                uint4(this.x, this.z, this.y, this.w)
            and set(v: uint4) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.xzzx
            with get() =
                uint4(this.x, this.z, this.z, this.x)

        member this.xzzy
            with get() =
                uint4(this.x, this.z, this.z, this.y)

        member this.xzzz
            with get() =
                uint4(this.x, this.z, this.z, this.z)

        member this.xzzw
            with get() =
                uint4(this.x, this.z, this.z, this.w)

        member this.xzwx
            with get() =
                uint4(this.x, this.z, this.w, this.x)

        member this.xzwy
            with get() =
                uint4(this.x, this.z, this.w, this.y)
            and set(v: uint4) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.xzwz
            with get() =
                uint4(this.x, this.z, this.w, this.z)

        member this.xzww
            with get() =
                uint4(this.x, this.z, this.w, this.w)

        member this.xwxx
            with get() =
                uint4(this.x, this.w, this.x, this.x)

        member this.xwxy
            with get() =
                uint4(this.x, this.w, this.x, this.y)

        member this.xwxz
            with get() =
                uint4(this.x, this.w, this.x, this.z)

        member this.xwxw
            with get() =
                uint4(this.x, this.w, this.x, this.w)

        member this.xwyx
            with get() =
                uint4(this.x, this.w, this.y, this.x)

        member this.xwyy
            with get() =
                uint4(this.x, this.w, this.y, this.y)

        member this.xwyz
            with get() =
                uint4(this.x, this.w, this.y, this.z)
            and set(v: uint4) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.xwyw
            with get() =
                uint4(this.x, this.w, this.y, this.w)

        member this.xwzx
            with get() =
                uint4(this.x, this.w, this.z, this.x)

        member this.xwzy
            with get() =
                uint4(this.x, this.w, this.z, this.y)
            and set(v: uint4) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z
                this.y <- v.w
            
        member this.xwzz
            with get() =
                uint4(this.x, this.w, this.z, this.z)

        member this.xwzw
            with get() =
                uint4(this.x, this.w, this.z, this.w)

        member this.xwwx
            with get() =
                uint4(this.x, this.w, this.w, this.x)

        member this.xwwy
            with get() =
                uint4(this.x, this.w, this.w, this.y)

        member this.xwwz
            with get() =
                uint4(this.x, this.w, this.w, this.z)

        member this.xwww
            with get() =
                uint4(this.x, this.w, this.w, this.w)

        member this.yxxx
            with get() =
                uint4(this.y, this.x, this.x, this.x)

        member this.yxxy
            with get() =
                uint4(this.y, this.x, this.x, this.y)

        member this.yxxz
            with get() =
                uint4(this.y, this.x, this.x, this.z)

        member this.yxxw
            with get() =
                uint4(this.y, this.x, this.x, this.w)

        member this.yxyx
            with get() =
                uint4(this.y, this.x, this.y, this.x)

        member this.yxyy
            with get() =
                uint4(this.y, this.x, this.y, this.y)

        member this.yxyz
            with get() =
                uint4(this.y, this.x, this.y, this.z)

        member this.yxyw
            with get() =
                uint4(this.y, this.x, this.y, this.w)

        member this.yxzx
            with get() =
                uint4(this.y, this.x, this.z, this.x)

        member this.yxzy
            with get() =
                uint4(this.y, this.x, this.z, this.y)

        member this.yxzz
            with get() =
                uint4(this.y, this.x, this.z, this.z)

        member this.yxzw
            with get() =
                uint4(this.y, this.x, this.z, this.w)
            and set(v: uint4) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
                this.w <- v.w


        member this.yxwx
            with get() =
                uint4(this.y, this.x, this.w, this.x)

        member this.yxwy
            with get() =
                uint4(this.y, this.x, this.w, this.y)

        member this.yxwz
            with get() =
                uint4(this.y, this.x, this.w, this.z)
            and set(v: uint4) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.yxww
            with get() =
                uint4(this.y, this.x, this.w, this.w)

        member this.yyxx
            with get() =
                uint4(this.y, this.y, this.x, this.x)

        member this.yyxy
            with get() =
                uint4(this.y, this.y, this.x, this.y)

        member this.yyxz
            with get() =
                uint4(this.y, this.y, this.x, this.z)

        member this.yyxw
            with get() =
                uint4(this.y, this.y, this.x, this.w)

        member this.yyyx
            with get() =
                uint4(this.y, this.y, this.y, this.x)

        member this.yyyy
            with get() =
                uint4(this.y, this.y, this.y, this.y)

        member this.yyyz
            with get() =
                uint4(this.y, this.y, this.y, this.z)

        member this.yyyw
            with get() =
                uint4(this.y, this.y, this.y, this.w)

        member this.yyzx
            with get() =
                uint4(this.y, this.y, this.z, this.x)

        member this.yyzy
            with get() =
                uint4(this.y, this.y, this.z, this.y)

        member this.yyzz
            with get() =
                uint4(this.y, this.y, this.z, this.z)

        member this.yyzw
            with get() =
                uint4(this.y, this.y, this.z, this.w)

        member this.yywx
            with get() =
                uint4(this.y, this.y, this.w, this.x)

        member this.yywy
            with get() =
                uint4(this.y, this.y, this.w, this.y)

        member this.yywz
            with get() =
                uint4(this.y, this.y, this.w, this.z)

        member this.yyww
            with get() =
                uint4(this.y, this.y, this.w, this.w)

        member this.yzxx
            with get() =
                uint4(this.y, this.z, this.x, this.x)

        member this.yzxy
            with get() =
                uint4(this.y, this.z, this.x, this.y)

        member this.yzxz
            with get() =
                uint4(this.y, this.z, this.x, this.z)

        member this.yzxw
            with get() =
                uint4(this.y, this.z, this.x, this.w)
            and set(v: uint4) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.yzyx
            with get() =
                uint4(this.y, this.z, this.y, this.x)

        member this.yzyy
            with get() =
                uint4(this.y, this.z, this.y, this.y)

        member this.yzyz
            with get() =
                uint4(this.y, this.z, this.y, this.z)

        member this.yzyw
            with get() =
                uint4(this.y, this.z, this.y, this.w)

        member this.yzzx
            with get() =
                uint4(this.y, this.z, this.z, this.x)

        member this.yzzy
            with get() =
                uint4(this.y, this.z, this.z, this.y)

        member this.yzzz
            with get() =
                uint4(this.y, this.z, this.z, this.z)

        member this.yzzw
            with get() =
                uint4(this.y, this.z, this.z, this.w)

        member this.yzwx
            with get() =
                uint4(this.y, this.z, this.w, this.x)
            and set(v: uint4) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.yzwy
            with get() =
                uint4(this.y, this.z, this.w, this.y)

        member this.yzwz
            with get() =
                uint4(this.y, this.z, this.w, this.z)

        member this.yzww
            with get() =
                uint4(this.y, this.z, this.w, this.w)

        member this.ywxx
            with get() =
                uint4(this.y, this.w, this.x, this.x)

        member this.ywxy
            with get() =
                uint4(this.y, this.w, this.x, this.y)

        member this.ywxz
            with get() =
                uint4(this.y, this.w, this.x, this.z)
            and set(v: uint4) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
                this.z <- v.w


        member this.ywxw
            with get() =
                uint4(this.y, this.w, this.x, this.w)

        member this.ywyx
            with get() =
                uint4(this.y, this.w, this.y, this.x)

        member this.ywyy
            with get() =
                uint4(this.y, this.w, this.y, this.y)

        member this.ywyz
            with get() =
                uint4(this.y, this.w, this.y, this.z)

        member this.ywyw
            with get() =
                uint4(this.y, this.w, this.y, this.w)

        member this.ywzx
            with get() =
                uint4(this.y, this.w, this.z, this.x)
            and set(v: uint4) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.ywzy
            with get() =
                uint4(this.y, this.w, this.z, this.y)

        member this.ywzz
            with get() =
                uint4(this.y, this.w, this.z, this.z)

        member this.ywzw
            with get() =
                uint4(this.y, this.w, this.z, this.w)

        member this.ywwx
            with get() =
                uint4(this.y, this.w, this.w, this.x)

        member this.ywwy
            with get() =
                uint4(this.y, this.w, this.w, this.y)

        member this.ywwz
            with get() =
                uint4(this.y, this.w, this.w, this.z)

        member this.ywww
            with get() =
                uint4(this.y, this.w, this.w, this.w)

        member this.zxxx
            with get() =
                uint4(this.z, this.x, this.x, this.x)

        member this.zxxy
            with get() =
                uint4(this.z, this.x, this.x, this.y)

        member this.zxxz
            with get() =
                uint4(this.z, this.x, this.x, this.z)

        member this.zxxw
            with get() =
                uint4(this.z, this.x, this.x, this.w)

        member this.zxyx
            with get() =
                uint4(this.z, this.x, this.y, this.x)

        member this.zxyy
            with get() =
                uint4(this.z, this.x, this.y, this.y)

        member this.zxyz
            with get() =
                uint4(this.z, this.x, this.y, this.z)

        member this.zxyw
            with get() =
                uint4(this.z, this.x, this.y, this.w)
            and set(v: uint4) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.zxzx
            with get() =
                uint4(this.z, this.x, this.z, this.x)

        member this.zxzy
            with get() =
                uint4(this.z, this.x, this.z, this.y)

        member this.zxzz
            with get() =
                uint4(this.z, this.x, this.z, this.z)

        member this.zxzw
            with get() =
                uint4(this.z, this.x, this.z, this.w)

        member this.zxwx
            with get() =
                uint4(this.z, this.x, this.w, this.x)

        member this.zxwy
            with get() =
                uint4(this.z, this.x, this.w, this.y)
            and set(v: uint4) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.zxwz
            with get() =
                uint4(this.z, this.x, this.w, this.z)

        member this.zxww
            with get() =
                uint4(this.z, this.x, this.w, this.w)

        member this.zyxx
            with get() =
                uint4(this.z, this.y, this.x, this.x)

        member this.zyxy
            with get() =
                uint4(this.z, this.y, this.x, this.y)

        member this.zyxz
            with get() =
                uint4(this.z, this.y, this.x, this.z)

        member this.zyxw
            with get() =
                uint4(this.z, this.y, this.x, this.w)
            and set(v: uint4) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.zyyx
            with get() =
                uint4(this.z, this.y, this.y, this.x)

        member this.zyyy
            with get() =
                uint4(this.z, this.y, this.y, this.y)

        member this.zyyz
            with get() =
                uint4(this.z, this.y, this.y, this.z)

        member this.zyyw
            with get() =
                uint4(this.z, this.y, this.y, this.w)

        member this.zyzx
            with get() =
                uint4(this.z, this.y, this.z, this.x)

        member this.zyzy
            with get() =
                uint4(this.z, this.y, this.z, this.y)

        member this.zyzz
            with get() =
                uint4(this.z, this.y, this.z, this.z)

        member this.zyzw
            with get() =
                uint4(this.z, this.y, this.z, this.w)

        member this.zywx
            with get() =
                uint4(this.z, this.y, this.w, this.x)
            and set(v: uint4) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.zywy
            with get() =
                uint4(this.z, this.y, this.w, this.y)

        member this.zywz
            with get() =
                uint4(this.z, this.y, this.w, this.z)

        member this.zyww
            with get() =
                uint4(this.z, this.y, this.w, this.w)

        member this.zzxx
            with get() =
                uint4(this.z, this.z, this.x, this.x)

        member this.zzxy
            with get() =
                uint4(this.z, this.z, this.x, this.y)

        member this.zzxz
            with get() =
                uint4(this.z, this.z, this.x, this.z)

        member this.zzxw
            with get() =
                uint4(this.z, this.z, this.x, this.w)

        member this.zzyx
            with get() =
                uint4(this.z, this.z, this.y, this.x)

        member this.zzyy
            with get() =
                uint4(this.z, this.z, this.y, this.y)

        member this.zzyz
            with get() =
                uint4(this.z, this.z, this.y, this.z)

        member this.zzyw
            with get() =
                uint4(this.z, this.z, this.y, this.w)

        member this.zzzx
            with get() =
                uint4(this.z, this.z, this.z, this.x)

        member this.zzzy
            with get() =
                uint4(this.z, this.z, this.z, this.y)

        member this.zzzz
            with get() =
                uint4(this.z, this.z, this.z, this.z)

        member this.zzzw
            with get() =
                uint4(this.z, this.z, this.z, this.w)

        member this.zzwx
            with get() =
                uint4(this.z, this.z, this.w, this.x)

        member this.zzwy
            with get() =
                uint4(this.z, this.z, this.w, this.y)

        member this.zzwz
            with get() =
                uint4(this.z, this.z, this.w, this.z)

        member this.zzww
            with get() =
                uint4(this.z, this.z, this.w, this.w)

        member this.zwxx
            with get() =
                uint4(this.z, this.w, this.x, this.x)

        member this.zwxy
            with get() =
                uint4(this.z, this.w, this.x, this.y)
            and set(v: uint4) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.zwxz
            with get() =
                uint4(this.z, this.w, this.x, this.z)

        member this.zwxw
            with get() =
                uint4(this.z, this.w, this.x, this.w)

        member this.zwyx
            with get() =
                uint4(this.z, this.w, this.y, this.x)
            and set(v: uint4) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.zwyy
            with get() =
                uint4(this.z, this.w, this.y, this.y)

        member this.zwyz
            with get() =
                uint4(this.z, this.w, this.y, this.z)

        member this.zwyw
            with get() =
                uint4(this.z, this.w, this.y, this.w)

        member this.zwzx
            with get() =
                uint4(this.z, this.w, this.z, this.x)

        member this.zwzy
            with get() =
                uint4(this.z, this.w, this.z, this.y)

        member this.zwzz
            with get() =
                uint4(this.z, this.w, this.z, this.z)

        member this.zwzw
            with get() =
                uint4(this.z, this.w, this.z, this.w)

        member this.zwwx
            with get() =
                uint4(this.z, this.w, this.w, this.x)

        member this.zwwy
            with get() =
                uint4(this.z, this.w, this.w, this.y)

        member this.zwwz
            with get() =
                uint4(this.z, this.w, this.w, this.z)

        member this.zwww
            with get() =
                uint4(this.z, this.w, this.w, this.w)

        member this.wxxx
            with get() =
                uint4(this.w, this.x, this.x, this.x)

        member this.wxxy
            with get() =
                uint4(this.w, this.x, this.x, this.y)

        member this.wxxz
            with get() =
                uint4(this.w, this.x, this.x, this.z)

        member this.wxxw
            with get() =
                uint4(this.w, this.x, this.x, this.w)

        member this.wxyx
            with get() =
                uint4(this.w, this.x, this.y, this.x)

        member this.wxyy
            with get() =
                uint4(this.w, this.x, this.y, this.y)

        member this.wxyz
            with get() =
                uint4(this.w, this.x, this.y, this.z)
            and set(v: uint4) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.wxyw
            with get() =
                uint4(this.w, this.x, this.y, this.w)

        member this.wxzx
            with get() =
                uint4(this.w, this.x, this.z, this.x)

        member this.wxzy
            with get() =
                uint4(this.w, this.x, this.z, this.y)
            and set(v: uint4) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
                this.y <- v.w


        member this.wxzz
            with get() =
                uint4(this.w, this.x, this.z, this.z)

        member this.wxzw
            with get() =
                uint4(this.w, this.x, this.z, this.w)

        member this.wxwx
            with get() =
                uint4(this.w, this.x, this.w, this.x)

        member this.wxwy
            with get() =
                uint4(this.w, this.x, this.w, this.y)

        member this.wxwz
            with get() =
                uint4(this.w, this.x, this.w, this.z)

        member this.wxww
            with get() =
                uint4(this.w, this.x, this.w, this.w)

        member this.wyxx
            with get() =
                uint4(this.w, this.y, this.x, this.x)

        member this.wyxy
            with get() =
                uint4(this.w, this.y, this.x, this.y)

        member this.wyxz
            with get() =
                uint4(this.w, this.y, this.x, this.z)
            and set(v: uint4) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
                this.z <- v.w
            
        member this.wyxw
            with get() =
                uint4(this.w, this.y, this.x, this.w)

        member this.wyyx
            with get() =
                uint4(this.w, this.y, this.y, this.x)

        member this.wyyy
            with get() =
                uint4(this.w, this.y, this.y, this.y)

        member this.wyyz
            with get() =
                uint4(this.w, this.y, this.y, this.z)

        member this.wyyw
            with get() =
                uint4(this.w, this.y, this.y, this.w)

        member this.wyzx
            with get() =
                uint4(this.w, this.y, this.z, this.x)
            and set(v: uint4) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.wyzy
            with get() =
                uint4(this.w, this.y, this.z, this.y)

        member this.wyzz
            with get() =
                uint4(this.w, this.y, this.z, this.z)

        member this.wyzw
            with get() =
                uint4(this.w, this.y, this.z, this.w)

        member this.wywx
            with get() =
                uint4(this.w, this.y, this.w, this.x)

        member this.wywy
            with get() =
                uint4(this.w, this.y, this.w, this.y)

        member this.wywz
            with get() =
                uint4(this.w, this.y, this.w, this.z)

        member this.wyww
            with get() =
                uint4(this.w, this.y, this.w, this.w)

        member this.wzxx
            with get() =
                uint4(this.w, this.z, this.x, this.x)

        member this.wzxy
            with get() =
                uint4(this.w, this.z, this.x, this.y)
            and set(v: uint4) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.wzxz
            with get() =
                uint4(this.w, this.z, this.x, this.z)

        member this.wzxw
            with get() =
                uint4(this.w, this.z, this.x, this.w)

        member this.wzyx
            with get() =
                uint4(this.w, this.z, this.y, this.x)
            and set(v: uint4) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.wzyy
            with get() =
                uint4(this.w, this.z, this.y, this.y)

        member this.wzyz
            with get() =
                uint4(this.w, this.z, this.y, this.z)

        member this.wzyw
            with get() =
                uint4(this.w, this.z, this.y, this.w)

        member this.wzzx
            with get() =
                uint4(this.w, this.z, this.z, this.x)

        member this.wzzy
            with get() =
                uint4(this.w, this.z, this.z, this.y)

        member this.wzzz
            with get() =
                uint4(this.w, this.z, this.z, this.z)

        member this.wzzw
            with get() =
                uint4(this.w, this.z, this.z, this.w)

        member this.wzwx
            with get() =
                uint4(this.w, this.z, this.w, this.x)

        member this.wzwy
            with get() =
                uint4(this.w, this.z, this.w, this.y)

        member this.wzwz
            with get() =
                uint4(this.w, this.z, this.w, this.z)

        member this.wzww
            with get() =
                uint4(this.w, this.z, this.w, this.w)

        member this.wwxx
            with get() =
                uint4(this.w, this.w, this.x, this.x)

        member this.wwxy
            with get() =
                uint4(this.w, this.w, this.x, this.y)

        member this.wwxz
            with get() =
                uint4(this.w, this.w, this.x, this.z)

        member this.wwxw
            with get() =
                uint4(this.w, this.w, this.x, this.w)

        member this.wwyx
            with get() =
                uint4(this.w, this.w, this.y, this.x)

        member this.wwyy
            with get() =
                uint4(this.w, this.w, this.y, this.y)

        member this.wwyz
            with get() =
                uint4(this.w, this.w, this.y, this.z)

        member this.wwyw
            with get() =
                uint4(this.w, this.w, this.y, this.w)

        member this.wwzx
            with get() =
                uint4(this.w, this.w, this.z, this.x)

        member this.wwzy
            with get() =
                uint4(this.w, this.w, this.z, this.y)

        member this.wwzz
            with get() =
                uint4(this.w, this.w, this.z, this.z)

        member this.wwzw
            with get() =
                uint4(this.w, this.w, this.z, this.w)

        member this.wwwx
            with get() =
                uint4(this.w, this.w, this.w, this.x)

        member this.wwwy
            with get() =
                uint4(this.w, this.w, this.w, this.y)

        member this.wwwz
            with get() =
                uint4(this.w, this.w, this.w, this.z)

        member this.wwww
            with get() =
                uint4(this.w, this.w, this.w, this.w)

        member this.lo 
            with get() =
                uint2(this.x, this.y)
            and set (v:uint2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                uint2(this.y, this.w)
            and set (v:uint2) =
                this.z <- v.x
                this.w <- v.y
            
        member this.even 
            with get() =
                uint2(this.x, this.z)
            and set (v:uint2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                uint2(this.y, this.w)
            and set (v:uint2) =
                this.y <- v.x
                this.w <- v.y

        internal new(c: uint32[]) =
            uint4(c.[0], c.[1], c.[2], c.[3])

        static member (+) (f1: uint4, f2: uint4) =
            uint4(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: uint4, f2: uint4) =
            uint4(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: uint4, f2: uint4) =
            uint4(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: uint4, f2: uint4) =
            uint4(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: uint4, f2: uint4) =
            int4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: uint4, f2: uint4) =
            int4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: uint4, f2: uint4) =
            int4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: uint4, f2: uint4) =
            int4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 4L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> uint4
            stream.Close()
            data

        static member hypot(a:uint4, b:uint4) = 
            new uint4(Math.Sqrt((a.x * a.x) + (b.x * b.x) |> float) |> uint32, 
                      Math.Sqrt((a.y * a.y) + (b.y * b.y) |> float) |> uint32,
                      Math.Sqrt((a.z * a.z) + (b.z * b.z) |> float) |> uint32,
                      Math.Sqrt((a.w * a.w) + (b.w * b.w) |> float) |> uint32)
    end    
// **************************************************************************************************************
