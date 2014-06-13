namespace FSCL
open System.Runtime.InteropServices
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters
open System

// Float vector types
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type float2 =
    struct
        val mutable x: float32
        val mutable y: float32

        member internal this.Components
            with get() =
                [| this.x; this.y |]
                
        new(X: float32, Y: float32) =
            { x = X; y = Y }
                        
        new(v: float32) =
            { x = v; y = v }

        member this.xy 
            with get() =
                float2(this.x, this.y)
            and set (v: float2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.yx 
            with get() =
                float2(this.y, this.x)
            and set (v: float2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.xx 
            with get() =
                float2(this.x, this.x)
            
        member this.yy 
            with get() =
                float2(this.y, this.y)

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

        internal new(c: float32[]) =
            float2(c.[0], c.[1])

        static member (+) (f1: float2, f2: float2) =
            float2(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: float2, f2: float2) =
            float2(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: float2, f2: float2) =
            float2(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: float2, f2: float2) =
            float2(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: float2, f2: float2) =
            int2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: float2, f2: float2) =
            int2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: float2, f2: float2) =
            int2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: float2, f2: float2) =
            int2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
                        
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 2L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> float2
            stream.Close()
            data

        static member hypot(a:float2, b:float2) = 
            new float2(Math.Sqrt((a.x * a.x) + (b.x * b.x) |> float) |> float32, 
                       Math.Sqrt((a.y * a.y) + (b.y * b.y) |> float) |> float32)
    end               
                
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type float3 =
    struct
        val mutable x: float32
        val mutable y: float32
        val mutable z: float32

        member internal this.Components
            with get() =
                [| this.x; this.y; this.z |]
                
        new(X: float32, Y: float32, Z: float32) =
            { x = X; y = Y; z = Z }
   
        new(v: float32) =
            { x = v; y = v; z = v }

        member this.xy 
            with get() =
                float2(this.x, this.y)
            and set (v: float2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                float2(this.x, this.z)
            and set (v: float2) =
                this.x <- v.x
                this.z <- v.y

        member this.yx 
            with get() =
                float2(this.y, this.x)
            and set (v: float2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                float2(this.y, this.z)
            and set (v: float2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.zx 
            with get() =
                float2(this.z, this.x)
            and set (v: float2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                float2(this.z, this.y)
            and set (v: float2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.xx 
            with get() =
                float2(this.x, this.x)
            
        member this.yy 
            with get() =
                float2(this.y, this.y)
            
        member this.zz 
            with get() =
                float2(this.z, this.z)

        // 3-comps    
        member this.xyz 
            with get() =
                float3(this.x, this.y, this.z)
            and set (v: float3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                float3(this.x, this.z, this.y)
            and set (v: float3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.yxz 
            with get() =
                float3(this.y, this.x, this.z)
            and set (v: float3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                float3(this.y, this.z, this.x)
            and set (v: float3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.zxy 
            with get() =
                float3(this.z, this.x, this.y)
            and set (v: float3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                float3(this.z, this.y, this.x)
            and set (v: float3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.xxy 
            with get() =
                float3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                float3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                float3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                float3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                float3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                float3(this.z, this.x, this.x)
                        
        member this.yyx 
            with get() =
                float3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                float3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                float3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                float3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                float3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                float3(this.z, this.y, this.y)
            
        member this.zzx 
            with get() =
                float3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                float3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                float3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                float3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                float3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                float3(this.y, this.z, this.z)
            
        member this.xxx 
            with get() =
                float3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                float3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                float3(this.z, this.z, this.z)

        member this.lo 
            with get() =
                float2(this.x, this.y)
            and set (v:float2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                float2(this.y, 0.0f)
            and set (v:float2) =
                this.z <- v.x
            
        member this.even 
            with get() =
                float2(this.x, this.z)
            and set (v:float2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                float2(this.y, 0.0f)
            and set (v:float2) =
                this.y <- v.x

        internal new(c: float32[]) =
            float3(c.[0], c.[1], c.[2])

        static member (+) (f1: float3, f2: float3) =
            float3(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: float3, f2: float3) =
            float3(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: float3, f2: float3) =
            float3(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: float3, f2: float3) =
            float3(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: float3, f2: float3) =
            int3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: float3, f2: float3) =
            int3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: float3, f2: float3) =
            int3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: float3, f2: float3) =
            int3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> float3
            stream.Close()
            data

        static member hypot(a:float3, b:float3) = 
            new float3(Math.Sqrt((a.x * a.x) + (b.x * b.x) |> float) |> float32, 
                       Math.Sqrt((a.y * a.y) + (b.y * b.y) |> float) |> float32,
                       Math.Sqrt((a.z * a.z) + (b.z * b.z) |> float) |> float32)
    end
    
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type float4 =
    struct
        val mutable x: float32
        val mutable y: float32
        val mutable z: float32
        val mutable w: float32

        member internal this.Components
            with get() =
                [| this.x; this.y; this.z; this.w |]
                
        new(X: float32, Y: float32, Z: float32, W: float32) =
            { x = X; y = Y; z = Z; w = W }
                        
        new(v: float32) =
            { x = v; y = v; z = v; w = v }

        member this.xy 
            with get() =
                float2(this.x, this.y)
            and set (v: float2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                float2(this.x, this.z)
            and set (v: float2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.xw 
            with get() =
                float2(this.x, this.w)
            and set (v: float2) =
                this.x <- v.x
                this.w <- v.y

        member this.yx 
            with get() =
                float2(this.y, this.x)
            and set (v: float2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                float2(this.y, this.z)
            and set (v: float2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.yw 
            with get() =
                float2(this.y, this.w)
            and set (v: float2) =
                this.y <- v.x
                this.w <- v.y

        member this.zx 
            with get() =
                float2(this.z, this.x)
            and set (v: float2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                float2(this.z, this.y)
            and set (v: float2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.zw 
            with get() =
                float2(this.z, this.w)
            and set (v: float2) =
                this.z <- v.x
                this.w <- v.y
                          
        member this.wx 
            with get() =
                float2(this.w, this.x)
            and set (v: float2) =
                this.w <- v.x
                this.x <- v.y
            
        member this.wy 
            with get() =
                float2(this.w, this.y)
            and set (v: float2) =
                this.w <- v.x
                this.y <- v.y
            
        member this.wz 
            with get() =
                float2(this.w, this.z)
            and set (v: float2) =
                this.w <- v.x
                this.z <- v.y

        member this.xx 
            with get() =
                float2(this.x, this.x)
            
        member this.yy 
            with get() =
                float2(this.y, this.y)
            
        member this.zz 
            with get() =
                float2(this.z, this.z)
            
        member this.ww 
            with get() =
                float2(this.w, this.w)

        // 3-comps    
        member this.xyz 
            with get() =
                float3(this.x, this.y, this.z)
            and set (v: float3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                float3(this.x, this.z, this.y)
            and set (v: float3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.xyw
            with get() =
                float3(this.x, this.y, this.w)
            and set (v: float3) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.xwy
            with get() =
                float3(this.x, this.w, this.y)
            and set (v: float3) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
            
        member this.xzw
            with get() =
                float3(this.x, this.z, this.w)
            and set (v: float3) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.xwz
            with get() =
                float3(this.x, this.w, this.z)
            and set (v: float3) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.yxz 
            with get() =
                float3(this.y, this.x, this.z)
            and set (v: float3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                float3(this.y, this.z, this.x)
            and set (v: float3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.yxw 
            with get() =
                float3(this.y, this.x, this.w)
            and set (v: float3) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.ywx 
            with get() =
                float3(this.y, this.w, this.x)
            and set (v: float3) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.yzw 
            with get() =
                float3(this.y, this.z, this.w)
            and set (v: float3) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.ywz 
            with get() =
                float3(this.y, this.w, this.z)
            and set (v: float3) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.zxy 
            with get() =
                float3(this.z, this.x, this.y)
            and set (v: float3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                float3(this.z, this.y, this.x)
            and set (v: float3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.zxw 
            with get() =
                float3(this.z, this.x, this.w)
            and set (v: float3) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.zwx 
            with get() =
                float3(this.z, this.w, this.x)
            and set (v: float3) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.zyw 
            with get() =
                float3(this.z, this.y, this.w)
            and set (v: float3) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.zwy 
            with get() =
                float3(this.z, this.w, this.y)
            and set (v: float3) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                        
        member this.wxy 
            with get() =
                float3(this.w, this.x, this.y)
            and set (v: float3) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.wyx 
            with get() =
                float3(this.w, this.y, this.x)
            and set (v: float3) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.wxz 
            with get() =
                float3(this.w, this.x, this.z)
            and set (v: float3) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.wzx 
            with get() =
                float3(this.w, this.z, this.x)
            and set (v: float3) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.wyz 
            with get() =
                float3(this.w, this.y, this.z)
            and set (v: float3) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.wzy 
            with get() =
                float3(this.w, this.z, this.y)
            and set (v: float3) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z

        member this.xxy 
            with get() =
                float3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                float3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                float3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                float3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                float3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                float3(this.z, this.x, this.x)
                    
        member this.xxw 
            with get() =
                float3(this.x, this.x, this.w)
            
        member this.xwx 
            with get() =
                float3(this.x, this.w, this.x)

        member this.wxx 
            with get() =
                float3(this.w, this.x, this.x)
                            
        member this.yyx 
            with get() =
                float3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                float3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                float3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                float3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                float3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                float3(this.z, this.y, this.y)
            
        member this.yyw 
            with get() =
                float3(this.y, this.y, this.w)
            
        member this.ywy 
            with get() =
                float3(this.y, this.w, this.y)

        member this.wyy
            with get() =
                float3(this.w, this.y, this.y)

        member this.zzx 
            with get() =
                float3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                float3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                float3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                float3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                float3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                float3(this.y, this.z, this.z)
            
        member this.zzw 
            with get() =
                float3(this.z, this.z, this.w)
            
        member this.zwz 
            with get() =
                float3(this.z, this.w, this.z)
            
        member this.wzz 
            with get() =
                float3(this.w, this.z, this.z)
            
        member this.xxx 
            with get() =
                float3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                float3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                float3(this.z, this.z, this.z)
 
        member this.www 
            with get() =
                float3(this.w, this.w, this.w)
     
        // 4-comps       
        member this.xxxx
            with get() =
                float4(this.x, this.x, this.x, this.x)

        member this.xxxy
            with get() =
                float4(this.x, this.x, this.x, this.y)

        member this.xxxz
            with get() =
                float4(this.x, this.x, this.x, this.z)

        member this.xxxw
            with get() =
                float4(this.x, this.x, this.x, this.w)

        member this.xxyx
            with get() =
                float4(this.x, this.x, this.y, this.x)

        member this.xxyy
            with get() =
                float4(this.x, this.x, this.y, this.y)

        member this.xxyz
            with get() =
                float4(this.x, this.x, this.y, this.z)

        member this.xxyw
            with get() =
                float4(this.x, this.x, this.y, this.w)

        member this.xxzx
            with get() =
                float4(this.x, this.x, this.z, this.x)

        member this.xxzy
            with get() =
                float4(this.x, this.x, this.z, this.y)

        member this.xxzz
            with get() =
                float4(this.x, this.x, this.z, this.z)

        member this.xxzw
            with get() =
                float4(this.x, this.x, this.z, this.w)

        member this.xxwx
            with get() =
                float4(this.x, this.x, this.w, this.x)

        member this.xxwy
            with get() =
                float4(this.x, this.x, this.w, this.y)

        member this.xxwz
            with get() =
                float4(this.x, this.x, this.w, this.z)

        member this.xxww
            with get() =
                float4(this.x, this.x, this.w, this.w)

        member this.xyxx
            with get() =
                float4(this.x, this.y, this.x, this.x)

        member this.xyxy
            with get() =
                float4(this.x, this.y, this.x, this.y)

        member this.xyxz
            with get() =
                float4(this.x, this.y, this.x, this.z)

        member this.xyxw
            with get() =
                float4(this.x, this.y, this.x, this.w)

        member this.xyyx
            with get() =
                float4(this.x, this.y, this.y, this.x)

        member this.xyyy
            with get() =
                float4(this.x, this.y, this.y, this.y)

        member this.xyyz
            with get() =
                float4(this.x, this.y, this.y, this.z)

        member this.xyyw
            with get() =
                float4(this.x, this.y, this.y, this.w)

        member this.xyzx
            with get() =
                float4(this.x, this.y, this.z, this.x)

        member this.xyzy
            with get() =
                float4(this.x, this.y, this.z, this.y)

        member this.xyzz
            with get() =
                float4(this.x, this.y, this.z, this.z)

        member this.xyzw
            with get() =
                float4(this.x, this.y, this.z, this.w)
            and set(v: float4) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
                this.w <- v.w
            
        member this.xywx
            with get() =
                float4(this.x, this.y, this.w, this.x)

        member this.xywy
            with get() =
                float4(this.x, this.y, this.w, this.y)

        member this.xywz
            with get() =
                float4(this.x, this.y, this.w, this.z)
            and set(v: float4) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.xyww
            with get() =
                float4(this.x, this.y, this.w, this.w)

        member this.xzxx
            with get() =
                float4(this.x, this.z, this.x, this.x)

        member this.xzxy
            with get() =
                float4(this.x, this.z, this.x, this.y)

        member this.xzxz
            with get() =
                float4(this.x, this.z, this.x, this.z)

        member this.xzxw
            with get() =
                float4(this.x, this.z, this.x, this.w)

        member this.xzyx
            with get() =
                float4(this.x, this.z, this.y, this.x)

        member this.xzyy
            with get() =
                float4(this.x, this.z, this.y, this.y)

        member this.xzyz
            with get() =
                float4(this.x, this.z, this.y, this.z)

        member this.xzyw
            with get() =
                float4(this.x, this.z, this.y, this.w)
            and set(v: float4) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.xzzx
            with get() =
                float4(this.x, this.z, this.z, this.x)

        member this.xzzy
            with get() =
                float4(this.x, this.z, this.z, this.y)

        member this.xzzz
            with get() =
                float4(this.x, this.z, this.z, this.z)

        member this.xzzw
            with get() =
                float4(this.x, this.z, this.z, this.w)

        member this.xzwx
            with get() =
                float4(this.x, this.z, this.w, this.x)

        member this.xzwy
            with get() =
                float4(this.x, this.z, this.w, this.y)
            and set(v: float4) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.xzwz
            with get() =
                float4(this.x, this.z, this.w, this.z)

        member this.xzww
            with get() =
                float4(this.x, this.z, this.w, this.w)

        member this.xwxx
            with get() =
                float4(this.x, this.w, this.x, this.x)

        member this.xwxy
            with get() =
                float4(this.x, this.w, this.x, this.y)

        member this.xwxz
            with get() =
                float4(this.x, this.w, this.x, this.z)

        member this.xwxw
            with get() =
                float4(this.x, this.w, this.x, this.w)

        member this.xwyx
            with get() =
                float4(this.x, this.w, this.y, this.x)

        member this.xwyy
            with get() =
                float4(this.x, this.w, this.y, this.y)

        member this.xwyz
            with get() =
                float4(this.x, this.w, this.y, this.z)
            and set(v: float4) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.xwyw
            with get() =
                float4(this.x, this.w, this.y, this.w)

        member this.xwzx
            with get() =
                float4(this.x, this.w, this.z, this.x)

        member this.xwzy
            with get() =
                float4(this.x, this.w, this.z, this.y)
            and set(v: float4) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z
                this.y <- v.w
            
        member this.xwzz
            with get() =
                float4(this.x, this.w, this.z, this.z)

        member this.xwzw
            with get() =
                float4(this.x, this.w, this.z, this.w)

        member this.xwwx
            with get() =
                float4(this.x, this.w, this.w, this.x)

        member this.xwwy
            with get() =
                float4(this.x, this.w, this.w, this.y)

        member this.xwwz
            with get() =
                float4(this.x, this.w, this.w, this.z)

        member this.xwww
            with get() =
                float4(this.x, this.w, this.w, this.w)

        member this.yxxx
            with get() =
                float4(this.y, this.x, this.x, this.x)

        member this.yxxy
            with get() =
                float4(this.y, this.x, this.x, this.y)

        member this.yxxz
            with get() =
                float4(this.y, this.x, this.x, this.z)

        member this.yxxw
            with get() =
                float4(this.y, this.x, this.x, this.w)

        member this.yxyx
            with get() =
                float4(this.y, this.x, this.y, this.x)

        member this.yxyy
            with get() =
                float4(this.y, this.x, this.y, this.y)

        member this.yxyz
            with get() =
                float4(this.y, this.x, this.y, this.z)

        member this.yxyw
            with get() =
                float4(this.y, this.x, this.y, this.w)

        member this.yxzx
            with get() =
                float4(this.y, this.x, this.z, this.x)

        member this.yxzy
            with get() =
                float4(this.y, this.x, this.z, this.y)

        member this.yxzz
            with get() =
                float4(this.y, this.x, this.z, this.z)

        member this.yxzw
            with get() =
                float4(this.y, this.x, this.z, this.w)
            and set(v: float4) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
                this.w <- v.w


        member this.yxwx
            with get() =
                float4(this.y, this.x, this.w, this.x)

        member this.yxwy
            with get() =
                float4(this.y, this.x, this.w, this.y)

        member this.yxwz
            with get() =
                float4(this.y, this.x, this.w, this.z)
            and set(v: float4) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.yxww
            with get() =
                float4(this.y, this.x, this.w, this.w)

        member this.yyxx
            with get() =
                float4(this.y, this.y, this.x, this.x)

        member this.yyxy
            with get() =
                float4(this.y, this.y, this.x, this.y)

        member this.yyxz
            with get() =
                float4(this.y, this.y, this.x, this.z)

        member this.yyxw
            with get() =
                float4(this.y, this.y, this.x, this.w)

        member this.yyyx
            with get() =
                float4(this.y, this.y, this.y, this.x)

        member this.yyyy
            with get() =
                float4(this.y, this.y, this.y, this.y)

        member this.yyyz
            with get() =
                float4(this.y, this.y, this.y, this.z)

        member this.yyyw
            with get() =
                float4(this.y, this.y, this.y, this.w)

        member this.yyzx
            with get() =
                float4(this.y, this.y, this.z, this.x)

        member this.yyzy
            with get() =
                float4(this.y, this.y, this.z, this.y)

        member this.yyzz
            with get() =
                float4(this.y, this.y, this.z, this.z)

        member this.yyzw
            with get() =
                float4(this.y, this.y, this.z, this.w)

        member this.yywx
            with get() =
                float4(this.y, this.y, this.w, this.x)

        member this.yywy
            with get() =
                float4(this.y, this.y, this.w, this.y)

        member this.yywz
            with get() =
                float4(this.y, this.y, this.w, this.z)

        member this.yyww
            with get() =
                float4(this.y, this.y, this.w, this.w)

        member this.yzxx
            with get() =
                float4(this.y, this.z, this.x, this.x)

        member this.yzxy
            with get() =
                float4(this.y, this.z, this.x, this.y)

        member this.yzxz
            with get() =
                float4(this.y, this.z, this.x, this.z)

        member this.yzxw
            with get() =
                float4(this.y, this.z, this.x, this.w)
            and set(v: float4) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.yzyx
            with get() =
                float4(this.y, this.z, this.y, this.x)

        member this.yzyy
            with get() =
                float4(this.y, this.z, this.y, this.y)

        member this.yzyz
            with get() =
                float4(this.y, this.z, this.y, this.z)

        member this.yzyw
            with get() =
                float4(this.y, this.z, this.y, this.w)

        member this.yzzx
            with get() =
                float4(this.y, this.z, this.z, this.x)

        member this.yzzy
            with get() =
                float4(this.y, this.z, this.z, this.y)

        member this.yzzz
            with get() =
                float4(this.y, this.z, this.z, this.z)

        member this.yzzw
            with get() =
                float4(this.y, this.z, this.z, this.w)

        member this.yzwx
            with get() =
                float4(this.y, this.z, this.w, this.x)
            and set(v: float4) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.yzwy
            with get() =
                float4(this.y, this.z, this.w, this.y)

        member this.yzwz
            with get() =
                float4(this.y, this.z, this.w, this.z)

        member this.yzww
            with get() =
                float4(this.y, this.z, this.w, this.w)

        member this.ywxx
            with get() =
                float4(this.y, this.w, this.x, this.x)

        member this.ywxy
            with get() =
                float4(this.y, this.w, this.x, this.y)

        member this.ywxz
            with get() =
                float4(this.y, this.w, this.x, this.z)
            and set(v: float4) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
                this.z <- v.w


        member this.ywxw
            with get() =
                float4(this.y, this.w, this.x, this.w)

        member this.ywyx
            with get() =
                float4(this.y, this.w, this.y, this.x)

        member this.ywyy
            with get() =
                float4(this.y, this.w, this.y, this.y)

        member this.ywyz
            with get() =
                float4(this.y, this.w, this.y, this.z)

        member this.ywyw
            with get() =
                float4(this.y, this.w, this.y, this.w)

        member this.ywzx
            with get() =
                float4(this.y, this.w, this.z, this.x)
            and set(v: float4) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.ywzy
            with get() =
                float4(this.y, this.w, this.z, this.y)

        member this.ywzz
            with get() =
                float4(this.y, this.w, this.z, this.z)

        member this.ywzw
            with get() =
                float4(this.y, this.w, this.z, this.w)

        member this.ywwx
            with get() =
                float4(this.y, this.w, this.w, this.x)

        member this.ywwy
            with get() =
                float4(this.y, this.w, this.w, this.y)

        member this.ywwz
            with get() =
                float4(this.y, this.w, this.w, this.z)

        member this.ywww
            with get() =
                float4(this.y, this.w, this.w, this.w)

        member this.zxxx
            with get() =
                float4(this.z, this.x, this.x, this.x)

        member this.zxxy
            with get() =
                float4(this.z, this.x, this.x, this.y)

        member this.zxxz
            with get() =
                float4(this.z, this.x, this.x, this.z)

        member this.zxxw
            with get() =
                float4(this.z, this.x, this.x, this.w)

        member this.zxyx
            with get() =
                float4(this.z, this.x, this.y, this.x)

        member this.zxyy
            with get() =
                float4(this.z, this.x, this.y, this.y)

        member this.zxyz
            with get() =
                float4(this.z, this.x, this.y, this.z)

        member this.zxyw
            with get() =
                float4(this.z, this.x, this.y, this.w)
            and set(v: float4) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.zxzx
            with get() =
                float4(this.z, this.x, this.z, this.x)

        member this.zxzy
            with get() =
                float4(this.z, this.x, this.z, this.y)

        member this.zxzz
            with get() =
                float4(this.z, this.x, this.z, this.z)

        member this.zxzw
            with get() =
                float4(this.z, this.x, this.z, this.w)

        member this.zxwx
            with get() =
                float4(this.z, this.x, this.w, this.x)

        member this.zxwy
            with get() =
                float4(this.z, this.x, this.w, this.y)
            and set(v: float4) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.zxwz
            with get() =
                float4(this.z, this.x, this.w, this.z)

        member this.zxww
            with get() =
                float4(this.z, this.x, this.w, this.w)

        member this.zyxx
            with get() =
                float4(this.z, this.y, this.x, this.x)

        member this.zyxy
            with get() =
                float4(this.z, this.y, this.x, this.y)

        member this.zyxz
            with get() =
                float4(this.z, this.y, this.x, this.z)

        member this.zyxw
            with get() =
                float4(this.z, this.y, this.x, this.w)
            and set(v: float4) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.zyyx
            with get() =
                float4(this.z, this.y, this.y, this.x)

        member this.zyyy
            with get() =
                float4(this.z, this.y, this.y, this.y)

        member this.zyyz
            with get() =
                float4(this.z, this.y, this.y, this.z)

        member this.zyyw
            with get() =
                float4(this.z, this.y, this.y, this.w)

        member this.zyzx
            with get() =
                float4(this.z, this.y, this.z, this.x)

        member this.zyzy
            with get() =
                float4(this.z, this.y, this.z, this.y)

        member this.zyzz
            with get() =
                float4(this.z, this.y, this.z, this.z)

        member this.zyzw
            with get() =
                float4(this.z, this.y, this.z, this.w)

        member this.zywx
            with get() =
                float4(this.z, this.y, this.w, this.x)
            and set(v: float4) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.zywy
            with get() =
                float4(this.z, this.y, this.w, this.y)

        member this.zywz
            with get() =
                float4(this.z, this.y, this.w, this.z)

        member this.zyww
            with get() =
                float4(this.z, this.y, this.w, this.w)

        member this.zzxx
            with get() =
                float4(this.z, this.z, this.x, this.x)

        member this.zzxy
            with get() =
                float4(this.z, this.z, this.x, this.y)

        member this.zzxz
            with get() =
                float4(this.z, this.z, this.x, this.z)

        member this.zzxw
            with get() =
                float4(this.z, this.z, this.x, this.w)

        member this.zzyx
            with get() =
                float4(this.z, this.z, this.y, this.x)

        member this.zzyy
            with get() =
                float4(this.z, this.z, this.y, this.y)

        member this.zzyz
            with get() =
                float4(this.z, this.z, this.y, this.z)

        member this.zzyw
            with get() =
                float4(this.z, this.z, this.y, this.w)

        member this.zzzx
            with get() =
                float4(this.z, this.z, this.z, this.x)

        member this.zzzy
            with get() =
                float4(this.z, this.z, this.z, this.y)

        member this.zzzz
            with get() =
                float4(this.z, this.z, this.z, this.z)

        member this.zzzw
            with get() =
                float4(this.z, this.z, this.z, this.w)

        member this.zzwx
            with get() =
                float4(this.z, this.z, this.w, this.x)

        member this.zzwy
            with get() =
                float4(this.z, this.z, this.w, this.y)

        member this.zzwz
            with get() =
                float4(this.z, this.z, this.w, this.z)

        member this.zzww
            with get() =
                float4(this.z, this.z, this.w, this.w)

        member this.zwxx
            with get() =
                float4(this.z, this.w, this.x, this.x)

        member this.zwxy
            with get() =
                float4(this.z, this.w, this.x, this.y)
            and set(v: float4) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.zwxz
            with get() =
                float4(this.z, this.w, this.x, this.z)

        member this.zwxw
            with get() =
                float4(this.z, this.w, this.x, this.w)

        member this.zwyx
            with get() =
                float4(this.z, this.w, this.y, this.x)
            and set(v: float4) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.zwyy
            with get() =
                float4(this.z, this.w, this.y, this.y)

        member this.zwyz
            with get() =
                float4(this.z, this.w, this.y, this.z)

        member this.zwyw
            with get() =
                float4(this.z, this.w, this.y, this.w)

        member this.zwzx
            with get() =
                float4(this.z, this.w, this.z, this.x)

        member this.zwzy
            with get() =
                float4(this.z, this.w, this.z, this.y)

        member this.zwzz
            with get() =
                float4(this.z, this.w, this.z, this.z)

        member this.zwzw
            with get() =
                float4(this.z, this.w, this.z, this.w)

        member this.zwwx
            with get() =
                float4(this.z, this.w, this.w, this.x)

        member this.zwwy
            with get() =
                float4(this.z, this.w, this.w, this.y)

        member this.zwwz
            with get() =
                float4(this.z, this.w, this.w, this.z)

        member this.zwww
            with get() =
                float4(this.z, this.w, this.w, this.w)

        member this.wxxx
            with get() =
                float4(this.w, this.x, this.x, this.x)

        member this.wxxy
            with get() =
                float4(this.w, this.x, this.x, this.y)

        member this.wxxz
            with get() =
                float4(this.w, this.x, this.x, this.z)

        member this.wxxw
            with get() =
                float4(this.w, this.x, this.x, this.w)

        member this.wxyx
            with get() =
                float4(this.w, this.x, this.y, this.x)

        member this.wxyy
            with get() =
                float4(this.w, this.x, this.y, this.y)

        member this.wxyz
            with get() =
                float4(this.w, this.x, this.y, this.z)
            and set(v: float4) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.wxyw
            with get() =
                float4(this.w, this.x, this.y, this.w)

        member this.wxzx
            with get() =
                float4(this.w, this.x, this.z, this.x)

        member this.wxzy
            with get() =
                float4(this.w, this.x, this.z, this.y)
            and set(v: float4) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
                this.y <- v.w


        member this.wxzz
            with get() =
                float4(this.w, this.x, this.z, this.z)

        member this.wxzw
            with get() =
                float4(this.w, this.x, this.z, this.w)

        member this.wxwx
            with get() =
                float4(this.w, this.x, this.w, this.x)

        member this.wxwy
            with get() =
                float4(this.w, this.x, this.w, this.y)

        member this.wxwz
            with get() =
                float4(this.w, this.x, this.w, this.z)

        member this.wxww
            with get() =
                float4(this.w, this.x, this.w, this.w)

        member this.wyxx
            with get() =
                float4(this.w, this.y, this.x, this.x)

        member this.wyxy
            with get() =
                float4(this.w, this.y, this.x, this.y)

        member this.wyxz
            with get() =
                float4(this.w, this.y, this.x, this.z)
            and set(v: float4) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
                this.z <- v.w
            
        member this.wyxw
            with get() =
                float4(this.w, this.y, this.x, this.w)

        member this.wyyx
            with get() =
                float4(this.w, this.y, this.y, this.x)

        member this.wyyy
            with get() =
                float4(this.w, this.y, this.y, this.y)

        member this.wyyz
            with get() =
                float4(this.w, this.y, this.y, this.z)

        member this.wyyw
            with get() =
                float4(this.w, this.y, this.y, this.w)

        member this.wyzx
            with get() =
                float4(this.w, this.y, this.z, this.x)
            and set(v: float4) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.wyzy
            with get() =
                float4(this.w, this.y, this.z, this.y)

        member this.wyzz
            with get() =
                float4(this.w, this.y, this.z, this.z)

        member this.wyzw
            with get() =
                float4(this.w, this.y, this.z, this.w)

        member this.wywx
            with get() =
                float4(this.w, this.y, this.w, this.x)

        member this.wywy
            with get() =
                float4(this.w, this.y, this.w, this.y)

        member this.wywz
            with get() =
                float4(this.w, this.y, this.w, this.z)

        member this.wyww
            with get() =
                float4(this.w, this.y, this.w, this.w)

        member this.wzxx
            with get() =
                float4(this.w, this.z, this.x, this.x)

        member this.wzxy
            with get() =
                float4(this.w, this.z, this.x, this.y)
            and set(v: float4) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.wzxz
            with get() =
                float4(this.w, this.z, this.x, this.z)

        member this.wzxw
            with get() =
                float4(this.w, this.z, this.x, this.w)

        member this.wzyx
            with get() =
                float4(this.w, this.z, this.y, this.x)
            and set(v: float4) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.wzyy
            with get() =
                float4(this.w, this.z, this.y, this.y)

        member this.wzyz
            with get() =
                float4(this.w, this.z, this.y, this.z)

        member this.wzyw
            with get() =
                float4(this.w, this.z, this.y, this.w)

        member this.wzzx
            with get() =
                float4(this.w, this.z, this.z, this.x)

        member this.wzzy
            with get() =
                float4(this.w, this.z, this.z, this.y)

        member this.wzzz
            with get() =
                float4(this.w, this.z, this.z, this.z)

        member this.wzzw
            with get() =
                float4(this.w, this.z, this.z, this.w)

        member this.wzwx
            with get() =
                float4(this.w, this.z, this.w, this.x)

        member this.wzwy
            with get() =
                float4(this.w, this.z, this.w, this.y)

        member this.wzwz
            with get() =
                float4(this.w, this.z, this.w, this.z)

        member this.wzww
            with get() =
                float4(this.w, this.z, this.w, this.w)

        member this.wwxx
            with get() =
                float4(this.w, this.w, this.x, this.x)

        member this.wwxy
            with get() =
                float4(this.w, this.w, this.x, this.y)

        member this.wwxz
            with get() =
                float4(this.w, this.w, this.x, this.z)

        member this.wwxw
            with get() =
                float4(this.w, this.w, this.x, this.w)

        member this.wwyx
            with get() =
                float4(this.w, this.w, this.y, this.x)

        member this.wwyy
            with get() =
                float4(this.w, this.w, this.y, this.y)

        member this.wwyz
            with get() =
                float4(this.w, this.w, this.y, this.z)

        member this.wwyw
            with get() =
                float4(this.w, this.w, this.y, this.w)

        member this.wwzx
            with get() =
                float4(this.w, this.w, this.z, this.x)

        member this.wwzy
            with get() =
                float4(this.w, this.w, this.z, this.y)

        member this.wwzz
            with get() =
                float4(this.w, this.w, this.z, this.z)

        member this.wwzw
            with get() =
                float4(this.w, this.w, this.z, this.w)

        member this.wwwx
            with get() =
                float4(this.w, this.w, this.w, this.x)

        member this.wwwy
            with get() =
                float4(this.w, this.w, this.w, this.y)

        member this.wwwz
            with get() =
                float4(this.w, this.w, this.w, this.z)

        member this.wwww
            with get() =
                float4(this.w, this.w, this.w, this.w)

        member this.lo 
            with get() =
                float2(this.x, this.y)
            and set (v:float2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                float2(this.y, this.w)
            and set (v:float2) =
                this.z <- v.x
                this.w <- v.y
            
        member this.even 
            with get() =
                float2(this.x, this.z)
            and set (v:float2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                float2(this.y, this.w)
            and set (v:float2) =
                this.y <- v.x
                this.w <- v.y
                
        internal new(c: float32[]) =
            float4(c.[0], c.[1], c.[2], c.[3])

        static member (+) (f1: float4, f2: float4) =
            float4(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: float4, f2: float4) =
            float4(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: float4, f2: float4) =
            float4(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: float4, f2: float4) =
            float4(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: float4, f2: float4) =
            int4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<<=) (f1: float4, f2: float4) =
            int4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (===) (f1: float4, f2: float4) =
            int4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
        static member (<=>) (f1: float4, f2: float4) =
            int4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 4L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> float4
            stream.Close()
            data

        static member hypot(a:float4, b:float4) = 
            new float4(Math.Sqrt((a.x * a.x) + (b.x * b.x) |> float) |> float32, 
                       Math.Sqrt((a.y * a.y) + (b.y * b.y) |> float) |> float32,
                       Math.Sqrt((a.z * a.z) + (b.z * b.z) |> float) |> float32,
                       Math.Sqrt((a.w * a.w) + (b.w * b.w) |> float) |> float32)
    end