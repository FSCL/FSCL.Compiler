namespace FSCL
open System.Runtime.InteropServices
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters
open System

// sbyteeger vector types
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type sbyte2 =
    struct 
        val mutable x: sbyte
        val mutable y: sbyte

        member this.Components
            with get() =
                [| this.x; this.y |]
            
        member this.xy 
            with get() =
                sbyte2(this.x, this.y)
            and set (v: sbyte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.yx 
            with get() =
                sbyte2(this.y, this.x)
            and set (v: sbyte2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.xx 
            with get() =
                sbyte2(this.x, this.x)
            
        member this.yy 
            with get() =
                sbyte2(this.y, this.y)

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

        new(X: sbyte, Y: sbyte) =
            { x = X; y = Y }
            
        new(v: sbyte) =
            { x = v; y = v }

        internal new(c: sbyte[]) =
            sbyte2(c.[0], c.[1])

        static member (+) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: sbyte2, f2: sbyte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 2L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> sbyte2
            stream.Close()
            data
    end
    
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]  
[<VectorType>]             
type sbyte3 =
    struct
        val mutable x: sbyte
        val mutable y: sbyte
        val mutable z: sbyte

        member this.Components
            with get() =
                [| this.x; this.y; this.z |]
                
        new(X: sbyte, Y: sbyte, Z: sbyte) =
            { x = X; y = Y; z = Z }
            
        new(v: sbyte) =
            { x = v; y = v; z = v }

        member this.xy 
            with get() =
                sbyte2(this.x, this.y)
            and set (v: sbyte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                sbyte2(this.x, this.z)
            and set (v: sbyte2) =
                this.x <- v.x
                this.z <- v.y

        member this.yx 
            with get() =
                sbyte2(this.y, this.x)
            and set (v: sbyte2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                sbyte2(this.y, this.z)
            and set (v: sbyte2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.zx 
            with get() =
                sbyte2(this.z, this.x)
            and set (v: sbyte2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                sbyte2(this.z, this.y)
            and set (v: sbyte2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.xx 
            with get() =
                sbyte2(this.x, this.x)
            
        member this.yy 
            with get() =
                sbyte2(this.y, this.y)
            
        member this.zz 
            with get() =
                sbyte2(this.z, this.z)

        // 3-comps    
        member this.xyz 
            with get() =
                sbyte3(this.x, this.y, this.z)
            and set (v: sbyte3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                sbyte3(this.x, this.z, this.y)
            and set (v: sbyte3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.yxz 
            with get() =
                sbyte3(this.y, this.x, this.z)
            and set (v: sbyte3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                sbyte3(this.y, this.z, this.x)
            and set (v: sbyte3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.zxy 
            with get() =
                sbyte3(this.z, this.x, this.y)
            and set (v: sbyte3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                sbyte3(this.z, this.y, this.x)
            and set (v: sbyte3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.xxy 
            with get() =
                sbyte3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                sbyte3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                sbyte3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                sbyte3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                sbyte3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                sbyte3(this.z, this.x, this.x)
                        
        member this.yyx 
            with get() =
                sbyte3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                sbyte3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                sbyte3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                sbyte3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                sbyte3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                sbyte3(this.z, this.y, this.y)
            
        member this.zzx 
            with get() =
                sbyte3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                sbyte3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                sbyte3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                sbyte3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                sbyte3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                sbyte3(this.y, this.z, this.z)
            
        member this.xxx 
            with get() =
                sbyte3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                sbyte3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                sbyte3(this.z, this.z, this.z)

        member this.lo 
            with get() =
                sbyte2(this.x, this.y)
            and set (v:sbyte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                sbyte2(this.y, 0y)
            and set (v:sbyte2) =
                this.z <- v.x
            
        member this.even 
            with get() =
                sbyte2(this.x, this.z)
            and set (v:sbyte2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                sbyte2(this.y, 0y)
            and set (v:sbyte2) =
                this.y <- v.x

        internal new(c: sbyte[]) =
            sbyte3(c.[0], c.[1], c.[2])

        static member (+) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: sbyte3, f2: sbyte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> sbyte3
            stream.Close()
            data
    end

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]     
[<VectorType>]       
type sbyte4 =
    struct
        val mutable x: sbyte
        val mutable y: sbyte
        val mutable z: sbyte
        val mutable w: sbyte

        member this.Components
            with get() =
                [| this.x; this.y; this.z; this.w |]
                
        new(X: sbyte, Y: sbyte, Z: sbyte, W: sbyte) =
            { x = X; y = Y; z = Z; w = W }
            
        new(v: sbyte) =
            { x = v; y = v; z = v; w = v }

        member this.xy 
            with get() =
                sbyte2(this.x, this.y)
            and set (v: sbyte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                sbyte2(this.x, this.z)
            and set (v: sbyte2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.xw 
            with get() =
                sbyte2(this.x, this.w)
            and set (v: sbyte2) =
                this.x <- v.x
                this.w <- v.y

        member this.yx 
            with get() =
                sbyte2(this.y, this.x)
            and set (v: sbyte2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                sbyte2(this.y, this.z)
            and set (v: sbyte2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.yw 
            with get() =
                sbyte2(this.y, this.w)
            and set (v: sbyte2) =
                this.y <- v.x
                this.w <- v.y

        member this.zx 
            with get() =
                sbyte2(this.z, this.x)
            and set (v: sbyte2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                sbyte2(this.z, this.y)
            and set (v: sbyte2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.zw 
            with get() =
                sbyte2(this.z, this.w)
            and set (v: sbyte2) =
                this.z <- v.x
                this.w <- v.y
                          
        member this.wx 
            with get() =
                sbyte2(this.w, this.x)
            and set (v: sbyte2) =
                this.w <- v.x
                this.x <- v.y
            
        member this.wy 
            with get() =
                sbyte2(this.w, this.y)
            and set (v: sbyte2) =
                this.w <- v.x
                this.y <- v.y
            
        member this.wz 
            with get() =
                sbyte2(this.w, this.z)
            and set (v: sbyte2) =
                this.w <- v.x
                this.z <- v.y

        member this.xx 
            with get() =
                sbyte2(this.x, this.x)
            
        member this.yy 
            with get() =
                sbyte2(this.y, this.y)
            
        member this.zz 
            with get() =
                sbyte2(this.z, this.z)
            
        member this.ww 
            with get() =
                sbyte2(this.w, this.w)

        // 3-comps    
        member this.xyz 
            with get() =
                sbyte3(this.x, this.y, this.z)
            and set (v: sbyte3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                sbyte3(this.x, this.z, this.y)
            and set (v: sbyte3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.xyw
            with get() =
                sbyte3(this.x, this.y, this.w)
            and set (v: sbyte3) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.xwy
            with get() =
                sbyte3(this.x, this.w, this.y)
            and set (v: sbyte3) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
            
        member this.xzw
            with get() =
                sbyte3(this.x, this.z, this.w)
            and set (v: sbyte3) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.xwz
            with get() =
                sbyte3(this.x, this.w, this.z)
            and set (v: sbyte3) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.yxz 
            with get() =
                sbyte3(this.y, this.x, this.z)
            and set (v: sbyte3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                sbyte3(this.y, this.z, this.x)
            and set (v: sbyte3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.yxw 
            with get() =
                sbyte3(this.y, this.x, this.w)
            and set (v: sbyte3) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.ywx 
            with get() =
                sbyte3(this.y, this.w, this.x)
            and set (v: sbyte3) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.yzw 
            with get() =
                sbyte3(this.y, this.z, this.w)
            and set (v: sbyte3) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.ywz 
            with get() =
                sbyte3(this.y, this.w, this.z)
            and set (v: sbyte3) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.zxy 
            with get() =
                sbyte3(this.z, this.x, this.y)
            and set (v: sbyte3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                sbyte3(this.z, this.y, this.x)
            and set (v: sbyte3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.zxw 
            with get() =
                sbyte3(this.z, this.x, this.w)
            and set (v: sbyte3) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.zwx 
            with get() =
                sbyte3(this.z, this.w, this.x)
            and set (v: sbyte3) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.zyw 
            with get() =
                sbyte3(this.z, this.y, this.w)
            and set (v: sbyte3) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.zwy 
            with get() =
                sbyte3(this.z, this.w, this.y)
            and set (v: sbyte3) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                        
        member this.wxy 
            with get() =
                sbyte3(this.w, this.x, this.y)
            and set (v: sbyte3) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.wyx 
            with get() =
                sbyte3(this.w, this.y, this.x)
            and set (v: sbyte3) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.wxz 
            with get() =
                sbyte3(this.w, this.x, this.z)
            and set (v: sbyte3) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.wzx 
            with get() =
                sbyte3(this.w, this.z, this.x)
            and set (v: sbyte3) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.wyz 
            with get() =
                sbyte3(this.w, this.y, this.z)
            and set (v: sbyte3) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.wzy 
            with get() =
                sbyte3(this.w, this.z, this.y)
            and set (v: sbyte3) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z

        member this.xxy 
            with get() =
                sbyte3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                sbyte3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                sbyte3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                sbyte3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                sbyte3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                sbyte3(this.z, this.x, this.x)
                    
        member this.xxw 
            with get() =
                sbyte3(this.x, this.x, this.w)
            
        member this.xwx 
            with get() =
                sbyte3(this.x, this.w, this.x)

        member this.wxx 
            with get() =
                sbyte3(this.w, this.x, this.x)
                            
        member this.yyx 
            with get() =
                sbyte3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                sbyte3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                sbyte3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                sbyte3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                sbyte3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                sbyte3(this.z, this.y, this.y)
            
        member this.yyw 
            with get() =
                sbyte3(this.y, this.y, this.w)
            
        member this.ywy 
            with get() =
                sbyte3(this.y, this.w, this.y)

        member this.wyy
            with get() =
                sbyte3(this.w, this.y, this.y)

        member this.zzx 
            with get() =
                sbyte3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                sbyte3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                sbyte3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                sbyte3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                sbyte3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                sbyte3(this.y, this.z, this.z)
            
        member this.zzw 
            with get() =
                sbyte3(this.z, this.z, this.w)
            
        member this.zwz 
            with get() =
                sbyte3(this.z, this.w, this.z)
            
        member this.wzz 
            with get() =
                sbyte3(this.w, this.z, this.z)
            
        member this.xxx 
            with get() =
                sbyte3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                sbyte3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                sbyte3(this.z, this.z, this.z)
 
        member this.www 
            with get() =
                sbyte3(this.w, this.w, this.w)
     
        // 4-comps       
        member this.xxxx
            with get() =
                sbyte4(this.x, this.x, this.x, this.x)

        member this.xxxy
            with get() =
                sbyte4(this.x, this.x, this.x, this.y)

        member this.xxxz
            with get() =
                sbyte4(this.x, this.x, this.x, this.z)

        member this.xxxw
            with get() =
                sbyte4(this.x, this.x, this.x, this.w)

        member this.xxyx
            with get() =
                sbyte4(this.x, this.x, this.y, this.x)

        member this.xxyy
            with get() =
                sbyte4(this.x, this.x, this.y, this.y)

        member this.xxyz
            with get() =
                sbyte4(this.x, this.x, this.y, this.z)

        member this.xxyw
            with get() =
                sbyte4(this.x, this.x, this.y, this.w)

        member this.xxzx
            with get() =
                sbyte4(this.x, this.x, this.z, this.x)

        member this.xxzy
            with get() =
                sbyte4(this.x, this.x, this.z, this.y)

        member this.xxzz
            with get() =
                sbyte4(this.x, this.x, this.z, this.z)

        member this.xxzw
            with get() =
                sbyte4(this.x, this.x, this.z, this.w)

        member this.xxwx
            with get() =
                sbyte4(this.x, this.x, this.w, this.x)

        member this.xxwy
            with get() =
                sbyte4(this.x, this.x, this.w, this.y)

        member this.xxwz
            with get() =
                sbyte4(this.x, this.x, this.w, this.z)

        member this.xxww
            with get() =
                sbyte4(this.x, this.x, this.w, this.w)

        member this.xyxx
            with get() =
                sbyte4(this.x, this.y, this.x, this.x)

        member this.xyxy
            with get() =
                sbyte4(this.x, this.y, this.x, this.y)

        member this.xyxz
            with get() =
                sbyte4(this.x, this.y, this.x, this.z)

        member this.xyxw
            with get() =
                sbyte4(this.x, this.y, this.x, this.w)

        member this.xyyx
            with get() =
                sbyte4(this.x, this.y, this.y, this.x)

        member this.xyyy
            with get() =
                sbyte4(this.x, this.y, this.y, this.y)

        member this.xyyz
            with get() =
                sbyte4(this.x, this.y, this.y, this.z)

        member this.xyyw
            with get() =
                sbyte4(this.x, this.y, this.y, this.w)

        member this.xyzx
            with get() =
                sbyte4(this.x, this.y, this.z, this.x)

        member this.xyzy
            with get() =
                sbyte4(this.x, this.y, this.z, this.y)

        member this.xyzz
            with get() =
                sbyte4(this.x, this.y, this.z, this.z)

        member this.xyzw
            with get() =
                sbyte4(this.x, this.y, this.z, this.w)
            and set(v: sbyte4) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
                this.w <- v.w
            
        member this.xywx
            with get() =
                sbyte4(this.x, this.y, this.w, this.x)

        member this.xywy
            with get() =
                sbyte4(this.x, this.y, this.w, this.y)

        member this.xywz
            with get() =
                sbyte4(this.x, this.y, this.w, this.z)
            and set(v: sbyte4) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.xyww
            with get() =
                sbyte4(this.x, this.y, this.w, this.w)

        member this.xzxx
            with get() =
                sbyte4(this.x, this.z, this.x, this.x)

        member this.xzxy
            with get() =
                sbyte4(this.x, this.z, this.x, this.y)

        member this.xzxz
            with get() =
                sbyte4(this.x, this.z, this.x, this.z)

        member this.xzxw
            with get() =
                sbyte4(this.x, this.z, this.x, this.w)

        member this.xzyx
            with get() =
                sbyte4(this.x, this.z, this.y, this.x)

        member this.xzyy
            with get() =
                sbyte4(this.x, this.z, this.y, this.y)

        member this.xzyz
            with get() =
                sbyte4(this.x, this.z, this.y, this.z)

        member this.xzyw
            with get() =
                sbyte4(this.x, this.z, this.y, this.w)
            and set(v: sbyte4) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.xzzx
            with get() =
                sbyte4(this.x, this.z, this.z, this.x)

        member this.xzzy
            with get() =
                sbyte4(this.x, this.z, this.z, this.y)

        member this.xzzz
            with get() =
                sbyte4(this.x, this.z, this.z, this.z)

        member this.xzzw
            with get() =
                sbyte4(this.x, this.z, this.z, this.w)

        member this.xzwx
            with get() =
                sbyte4(this.x, this.z, this.w, this.x)

        member this.xzwy
            with get() =
                sbyte4(this.x, this.z, this.w, this.y)
            and set(v: sbyte4) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.xzwz
            with get() =
                sbyte4(this.x, this.z, this.w, this.z)

        member this.xzww
            with get() =
                sbyte4(this.x, this.z, this.w, this.w)

        member this.xwxx
            with get() =
                sbyte4(this.x, this.w, this.x, this.x)

        member this.xwxy
            with get() =
                sbyte4(this.x, this.w, this.x, this.y)

        member this.xwxz
            with get() =
                sbyte4(this.x, this.w, this.x, this.z)

        member this.xwxw
            with get() =
                sbyte4(this.x, this.w, this.x, this.w)

        member this.xwyx
            with get() =
                sbyte4(this.x, this.w, this.y, this.x)

        member this.xwyy
            with get() =
                sbyte4(this.x, this.w, this.y, this.y)

        member this.xwyz
            with get() =
                sbyte4(this.x, this.w, this.y, this.z)
            and set(v: sbyte4) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.xwyw
            with get() =
                sbyte4(this.x, this.w, this.y, this.w)

        member this.xwzx
            with get() =
                sbyte4(this.x, this.w, this.z, this.x)

        member this.xwzy
            with get() =
                sbyte4(this.x, this.w, this.z, this.y)
            and set(v: sbyte4) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z
                this.y <- v.w
            
        member this.xwzz
            with get() =
                sbyte4(this.x, this.w, this.z, this.z)

        member this.xwzw
            with get() =
                sbyte4(this.x, this.w, this.z, this.w)

        member this.xwwx
            with get() =
                sbyte4(this.x, this.w, this.w, this.x)

        member this.xwwy
            with get() =
                sbyte4(this.x, this.w, this.w, this.y)

        member this.xwwz
            with get() =
                sbyte4(this.x, this.w, this.w, this.z)

        member this.xwww
            with get() =
                sbyte4(this.x, this.w, this.w, this.w)

        member this.yxxx
            with get() =
                sbyte4(this.y, this.x, this.x, this.x)

        member this.yxxy
            with get() =
                sbyte4(this.y, this.x, this.x, this.y)

        member this.yxxz
            with get() =
                sbyte4(this.y, this.x, this.x, this.z)

        member this.yxxw
            with get() =
                sbyte4(this.y, this.x, this.x, this.w)

        member this.yxyx
            with get() =
                sbyte4(this.y, this.x, this.y, this.x)

        member this.yxyy
            with get() =
                sbyte4(this.y, this.x, this.y, this.y)

        member this.yxyz
            with get() =
                sbyte4(this.y, this.x, this.y, this.z)

        member this.yxyw
            with get() =
                sbyte4(this.y, this.x, this.y, this.w)

        member this.yxzx
            with get() =
                sbyte4(this.y, this.x, this.z, this.x)

        member this.yxzy
            with get() =
                sbyte4(this.y, this.x, this.z, this.y)

        member this.yxzz
            with get() =
                sbyte4(this.y, this.x, this.z, this.z)

        member this.yxzw
            with get() =
                sbyte4(this.y, this.x, this.z, this.w)
            and set(v: sbyte4) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
                this.w <- v.w


        member this.yxwx
            with get() =
                sbyte4(this.y, this.x, this.w, this.x)

        member this.yxwy
            with get() =
                sbyte4(this.y, this.x, this.w, this.y)

        member this.yxwz
            with get() =
                sbyte4(this.y, this.x, this.w, this.z)
            and set(v: sbyte4) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.yxww
            with get() =
                sbyte4(this.y, this.x, this.w, this.w)

        member this.yyxx
            with get() =
                sbyte4(this.y, this.y, this.x, this.x)

        member this.yyxy
            with get() =
                sbyte4(this.y, this.y, this.x, this.y)

        member this.yyxz
            with get() =
                sbyte4(this.y, this.y, this.x, this.z)

        member this.yyxw
            with get() =
                sbyte4(this.y, this.y, this.x, this.w)

        member this.yyyx
            with get() =
                sbyte4(this.y, this.y, this.y, this.x)

        member this.yyyy
            with get() =
                sbyte4(this.y, this.y, this.y, this.y)

        member this.yyyz
            with get() =
                sbyte4(this.y, this.y, this.y, this.z)

        member this.yyyw
            with get() =
                sbyte4(this.y, this.y, this.y, this.w)

        member this.yyzx
            with get() =
                sbyte4(this.y, this.y, this.z, this.x)

        member this.yyzy
            with get() =
                sbyte4(this.y, this.y, this.z, this.y)

        member this.yyzz
            with get() =
                sbyte4(this.y, this.y, this.z, this.z)

        member this.yyzw
            with get() =
                sbyte4(this.y, this.y, this.z, this.w)

        member this.yywx
            with get() =
                sbyte4(this.y, this.y, this.w, this.x)

        member this.yywy
            with get() =
                sbyte4(this.y, this.y, this.w, this.y)

        member this.yywz
            with get() =
                sbyte4(this.y, this.y, this.w, this.z)

        member this.yyww
            with get() =
                sbyte4(this.y, this.y, this.w, this.w)

        member this.yzxx
            with get() =
                sbyte4(this.y, this.z, this.x, this.x)

        member this.yzxy
            with get() =
                sbyte4(this.y, this.z, this.x, this.y)

        member this.yzxz
            with get() =
                sbyte4(this.y, this.z, this.x, this.z)

        member this.yzxw
            with get() =
                sbyte4(this.y, this.z, this.x, this.w)
            and set(v: sbyte4) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.yzyx
            with get() =
                sbyte4(this.y, this.z, this.y, this.x)

        member this.yzyy
            with get() =
                sbyte4(this.y, this.z, this.y, this.y)

        member this.yzyz
            with get() =
                sbyte4(this.y, this.z, this.y, this.z)

        member this.yzyw
            with get() =
                sbyte4(this.y, this.z, this.y, this.w)

        member this.yzzx
            with get() =
                sbyte4(this.y, this.z, this.z, this.x)

        member this.yzzy
            with get() =
                sbyte4(this.y, this.z, this.z, this.y)

        member this.yzzz
            with get() =
                sbyte4(this.y, this.z, this.z, this.z)

        member this.yzzw
            with get() =
                sbyte4(this.y, this.z, this.z, this.w)

        member this.yzwx
            with get() =
                sbyte4(this.y, this.z, this.w, this.x)
            and set(v: sbyte4) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.yzwy
            with get() =
                sbyte4(this.y, this.z, this.w, this.y)

        member this.yzwz
            with get() =
                sbyte4(this.y, this.z, this.w, this.z)

        member this.yzww
            with get() =
                sbyte4(this.y, this.z, this.w, this.w)

        member this.ywxx
            with get() =
                sbyte4(this.y, this.w, this.x, this.x)

        member this.ywxy
            with get() =
                sbyte4(this.y, this.w, this.x, this.y)

        member this.ywxz
            with get() =
                sbyte4(this.y, this.w, this.x, this.z)
            and set(v: sbyte4) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
                this.z <- v.w


        member this.ywxw
            with get() =
                sbyte4(this.y, this.w, this.x, this.w)

        member this.ywyx
            with get() =
                sbyte4(this.y, this.w, this.y, this.x)

        member this.ywyy
            with get() =
                sbyte4(this.y, this.w, this.y, this.y)

        member this.ywyz
            with get() =
                sbyte4(this.y, this.w, this.y, this.z)

        member this.ywyw
            with get() =
                sbyte4(this.y, this.w, this.y, this.w)

        member this.ywzx
            with get() =
                sbyte4(this.y, this.w, this.z, this.x)
            and set(v: sbyte4) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.ywzy
            with get() =
                sbyte4(this.y, this.w, this.z, this.y)

        member this.ywzz
            with get() =
                sbyte4(this.y, this.w, this.z, this.z)

        member this.ywzw
            with get() =
                sbyte4(this.y, this.w, this.z, this.w)

        member this.ywwx
            with get() =
                sbyte4(this.y, this.w, this.w, this.x)

        member this.ywwy
            with get() =
                sbyte4(this.y, this.w, this.w, this.y)

        member this.ywwz
            with get() =
                sbyte4(this.y, this.w, this.w, this.z)

        member this.ywww
            with get() =
                sbyte4(this.y, this.w, this.w, this.w)

        member this.zxxx
            with get() =
                sbyte4(this.z, this.x, this.x, this.x)

        member this.zxxy
            with get() =
                sbyte4(this.z, this.x, this.x, this.y)

        member this.zxxz
            with get() =
                sbyte4(this.z, this.x, this.x, this.z)

        member this.zxxw
            with get() =
                sbyte4(this.z, this.x, this.x, this.w)

        member this.zxyx
            with get() =
                sbyte4(this.z, this.x, this.y, this.x)

        member this.zxyy
            with get() =
                sbyte4(this.z, this.x, this.y, this.y)

        member this.zxyz
            with get() =
                sbyte4(this.z, this.x, this.y, this.z)

        member this.zxyw
            with get() =
                sbyte4(this.z, this.x, this.y, this.w)
            and set(v: sbyte4) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.zxzx
            with get() =
                sbyte4(this.z, this.x, this.z, this.x)

        member this.zxzy
            with get() =
                sbyte4(this.z, this.x, this.z, this.y)

        member this.zxzz
            with get() =
                sbyte4(this.z, this.x, this.z, this.z)

        member this.zxzw
            with get() =
                sbyte4(this.z, this.x, this.z, this.w)

        member this.zxwx
            with get() =
                sbyte4(this.z, this.x, this.w, this.x)

        member this.zxwy
            with get() =
                sbyte4(this.z, this.x, this.w, this.y)
            and set(v: sbyte4) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.zxwz
            with get() =
                sbyte4(this.z, this.x, this.w, this.z)

        member this.zxww
            with get() =
                sbyte4(this.z, this.x, this.w, this.w)

        member this.zyxx
            with get() =
                sbyte4(this.z, this.y, this.x, this.x)

        member this.zyxy
            with get() =
                sbyte4(this.z, this.y, this.x, this.y)

        member this.zyxz
            with get() =
                sbyte4(this.z, this.y, this.x, this.z)

        member this.zyxw
            with get() =
                sbyte4(this.z, this.y, this.x, this.w)
            and set(v: sbyte4) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.zyyx
            with get() =
                sbyte4(this.z, this.y, this.y, this.x)

        member this.zyyy
            with get() =
                sbyte4(this.z, this.y, this.y, this.y)

        member this.zyyz
            with get() =
                sbyte4(this.z, this.y, this.y, this.z)

        member this.zyyw
            with get() =
                sbyte4(this.z, this.y, this.y, this.w)

        member this.zyzx
            with get() =
                sbyte4(this.z, this.y, this.z, this.x)

        member this.zyzy
            with get() =
                sbyte4(this.z, this.y, this.z, this.y)

        member this.zyzz
            with get() =
                sbyte4(this.z, this.y, this.z, this.z)

        member this.zyzw
            with get() =
                sbyte4(this.z, this.y, this.z, this.w)

        member this.zywx
            with get() =
                sbyte4(this.z, this.y, this.w, this.x)
            and set(v: sbyte4) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.zywy
            with get() =
                sbyte4(this.z, this.y, this.w, this.y)

        member this.zywz
            with get() =
                sbyte4(this.z, this.y, this.w, this.z)

        member this.zyww
            with get() =
                sbyte4(this.z, this.y, this.w, this.w)

        member this.zzxx
            with get() =
                sbyte4(this.z, this.z, this.x, this.x)

        member this.zzxy
            with get() =
                sbyte4(this.z, this.z, this.x, this.y)

        member this.zzxz
            with get() =
                sbyte4(this.z, this.z, this.x, this.z)

        member this.zzxw
            with get() =
                sbyte4(this.z, this.z, this.x, this.w)

        member this.zzyx
            with get() =
                sbyte4(this.z, this.z, this.y, this.x)

        member this.zzyy
            with get() =
                sbyte4(this.z, this.z, this.y, this.y)

        member this.zzyz
            with get() =
                sbyte4(this.z, this.z, this.y, this.z)

        member this.zzyw
            with get() =
                sbyte4(this.z, this.z, this.y, this.w)

        member this.zzzx
            with get() =
                sbyte4(this.z, this.z, this.z, this.x)

        member this.zzzy
            with get() =
                sbyte4(this.z, this.z, this.z, this.y)

        member this.zzzz
            with get() =
                sbyte4(this.z, this.z, this.z, this.z)

        member this.zzzw
            with get() =
                sbyte4(this.z, this.z, this.z, this.w)

        member this.zzwx
            with get() =
                sbyte4(this.z, this.z, this.w, this.x)

        member this.zzwy
            with get() =
                sbyte4(this.z, this.z, this.w, this.y)

        member this.zzwz
            with get() =
                sbyte4(this.z, this.z, this.w, this.z)

        member this.zzww
            with get() =
                sbyte4(this.z, this.z, this.w, this.w)

        member this.zwxx
            with get() =
                sbyte4(this.z, this.w, this.x, this.x)

        member this.zwxy
            with get() =
                sbyte4(this.z, this.w, this.x, this.y)
            and set(v: sbyte4) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.zwxz
            with get() =
                sbyte4(this.z, this.w, this.x, this.z)

        member this.zwxw
            with get() =
                sbyte4(this.z, this.w, this.x, this.w)

        member this.zwyx
            with get() =
                sbyte4(this.z, this.w, this.y, this.x)
            and set(v: sbyte4) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.zwyy
            with get() =
                sbyte4(this.z, this.w, this.y, this.y)

        member this.zwyz
            with get() =
                sbyte4(this.z, this.w, this.y, this.z)

        member this.zwyw
            with get() =
                sbyte4(this.z, this.w, this.y, this.w)

        member this.zwzx
            with get() =
                sbyte4(this.z, this.w, this.z, this.x)

        member this.zwzy
            with get() =
                sbyte4(this.z, this.w, this.z, this.y)

        member this.zwzz
            with get() =
                sbyte4(this.z, this.w, this.z, this.z)

        member this.zwzw
            with get() =
                sbyte4(this.z, this.w, this.z, this.w)

        member this.zwwx
            with get() =
                sbyte4(this.z, this.w, this.w, this.x)

        member this.zwwy
            with get() =
                sbyte4(this.z, this.w, this.w, this.y)

        member this.zwwz
            with get() =
                sbyte4(this.z, this.w, this.w, this.z)

        member this.zwww
            with get() =
                sbyte4(this.z, this.w, this.w, this.w)

        member this.wxxx
            with get() =
                sbyte4(this.w, this.x, this.x, this.x)

        member this.wxxy
            with get() =
                sbyte4(this.w, this.x, this.x, this.y)

        member this.wxxz
            with get() =
                sbyte4(this.w, this.x, this.x, this.z)

        member this.wxxw
            with get() =
                sbyte4(this.w, this.x, this.x, this.w)

        member this.wxyx
            with get() =
                sbyte4(this.w, this.x, this.y, this.x)

        member this.wxyy
            with get() =
                sbyte4(this.w, this.x, this.y, this.y)

        member this.wxyz
            with get() =
                sbyte4(this.w, this.x, this.y, this.z)
            and set(v: sbyte4) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.wxyw
            with get() =
                sbyte4(this.w, this.x, this.y, this.w)

        member this.wxzx
            with get() =
                sbyte4(this.w, this.x, this.z, this.x)

        member this.wxzy
            with get() =
                sbyte4(this.w, this.x, this.z, this.y)
            and set(v: sbyte4) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
                this.y <- v.w


        member this.wxzz
            with get() =
                sbyte4(this.w, this.x, this.z, this.z)

        member this.wxzw
            with get() =
                sbyte4(this.w, this.x, this.z, this.w)

        member this.wxwx
            with get() =
                sbyte4(this.w, this.x, this.w, this.x)

        member this.wxwy
            with get() =
                sbyte4(this.w, this.x, this.w, this.y)

        member this.wxwz
            with get() =
                sbyte4(this.w, this.x, this.w, this.z)

        member this.wxww
            with get() =
                sbyte4(this.w, this.x, this.w, this.w)

        member this.wyxx
            with get() =
                sbyte4(this.w, this.y, this.x, this.x)

        member this.wyxy
            with get() =
                sbyte4(this.w, this.y, this.x, this.y)

        member this.wyxz
            with get() =
                sbyte4(this.w, this.y, this.x, this.z)
            and set(v: sbyte4) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
                this.z <- v.w
            
        member this.wyxw
            with get() =
                sbyte4(this.w, this.y, this.x, this.w)

        member this.wyyx
            with get() =
                sbyte4(this.w, this.y, this.y, this.x)

        member this.wyyy
            with get() =
                sbyte4(this.w, this.y, this.y, this.y)

        member this.wyyz
            with get() =
                sbyte4(this.w, this.y, this.y, this.z)

        member this.wyyw
            with get() =
                sbyte4(this.w, this.y, this.y, this.w)

        member this.wyzx
            with get() =
                sbyte4(this.w, this.y, this.z, this.x)
            and set(v: sbyte4) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.wyzy
            with get() =
                sbyte4(this.w, this.y, this.z, this.y)

        member this.wyzz
            with get() =
                sbyte4(this.w, this.y, this.z, this.z)

        member this.wyzw
            with get() =
                sbyte4(this.w, this.y, this.z, this.w)

        member this.wywx
            with get() =
                sbyte4(this.w, this.y, this.w, this.x)

        member this.wywy
            with get() =
                sbyte4(this.w, this.y, this.w, this.y)

        member this.wywz
            with get() =
                sbyte4(this.w, this.y, this.w, this.z)

        member this.wyww
            with get() =
                sbyte4(this.w, this.y, this.w, this.w)

        member this.wzxx
            with get() =
                sbyte4(this.w, this.z, this.x, this.x)

        member this.wzxy
            with get() =
                sbyte4(this.w, this.z, this.x, this.y)
            and set(v: sbyte4) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.wzxz
            with get() =
                sbyte4(this.w, this.z, this.x, this.z)

        member this.wzxw
            with get() =
                sbyte4(this.w, this.z, this.x, this.w)

        member this.wzyx
            with get() =
                sbyte4(this.w, this.z, this.y, this.x)
            and set(v: sbyte4) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.wzyy
            with get() =
                sbyte4(this.w, this.z, this.y, this.y)

        member this.wzyz
            with get() =
                sbyte4(this.w, this.z, this.y, this.z)

        member this.wzyw
            with get() =
                sbyte4(this.w, this.z, this.y, this.w)

        member this.wzzx
            with get() =
                sbyte4(this.w, this.z, this.z, this.x)

        member this.wzzy
            with get() =
                sbyte4(this.w, this.z, this.z, this.y)

        member this.wzzz
            with get() =
                sbyte4(this.w, this.z, this.z, this.z)

        member this.wzzw
            with get() =
                sbyte4(this.w, this.z, this.z, this.w)

        member this.wzwx
            with get() =
                sbyte4(this.w, this.z, this.w, this.x)

        member this.wzwy
            with get() =
                sbyte4(this.w, this.z, this.w, this.y)

        member this.wzwz
            with get() =
                sbyte4(this.w, this.z, this.w, this.z)

        member this.wzww
            with get() =
                sbyte4(this.w, this.z, this.w, this.w)

        member this.wwxx
            with get() =
                sbyte4(this.w, this.w, this.x, this.x)

        member this.wwxy
            with get() =
                sbyte4(this.w, this.w, this.x, this.y)

        member this.wwxz
            with get() =
                sbyte4(this.w, this.w, this.x, this.z)

        member this.wwxw
            with get() =
                sbyte4(this.w, this.w, this.x, this.w)

        member this.wwyx
            with get() =
                sbyte4(this.w, this.w, this.y, this.x)

        member this.wwyy
            with get() =
                sbyte4(this.w, this.w, this.y, this.y)

        member this.wwyz
            with get() =
                sbyte4(this.w, this.w, this.y, this.z)

        member this.wwyw
            with get() =
                sbyte4(this.w, this.w, this.y, this.w)

        member this.wwzx
            with get() =
                sbyte4(this.w, this.w, this.z, this.x)

        member this.wwzy
            with get() =
                sbyte4(this.w, this.w, this.z, this.y)

        member this.wwzz
            with get() =
                sbyte4(this.w, this.w, this.z, this.z)

        member this.wwzw
            with get() =
                sbyte4(this.w, this.w, this.z, this.w)

        member this.wwwx
            with get() =
                sbyte4(this.w, this.w, this.w, this.x)

        member this.wwwy
            with get() =
                sbyte4(this.w, this.w, this.w, this.y)

        member this.wwwz
            with get() =
                sbyte4(this.w, this.w, this.w, this.z)

        member this.wwww
            with get() =
                sbyte4(this.w, this.w, this.w, this.w)

        member this.lo 
            with get() =
                sbyte2(this.x, this.y)
            and set (v:sbyte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                sbyte2(this.y, this.w)
            and set (v:sbyte2) =
                this.z <- v.x
                this.w <- v.y
            
        member this.even 
            with get() =
                sbyte2(this.x, this.z)
            and set (v:sbyte2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                sbyte2(this.y, this.w)
            and set (v:sbyte2) =
                this.y <- v.x
                this.w <- v.y

        internal new(c: sbyte[]) =
            sbyte4(c.[0], c.[1], c.[2], c.[3])

        static member (+) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (+) (f1.Components) (f2.Components))
        static member (-) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (-) (f1.Components) (f2.Components))
        static member (*) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (*) (f1.Components) (f2.Components))
        static member (/) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (/) (f1.Components) (f2.Components))
        
        static member (>>=) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: sbyte4, f2: sbyte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 4L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> sbyte4
            stream.Close()
            data
    end    
  
// **************************************************************************************************************
