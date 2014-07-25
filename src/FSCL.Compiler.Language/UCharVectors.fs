namespace FSCL
open System.Runtime.InteropServices
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters
open System

// uchareger vector types
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type uchar2 =
    struct 
        val mutable x: byte
        val mutable y: byte

        member this.Components
            with get() =
                [| this.x; this.y |]
            
        member this.xy 
            with get() =
                uchar2(this.x, this.y)
            and set (v: uchar2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.yx 
            with get() =
                uchar2(this.y, this.x)
            and set (v: uchar2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.xx 
            with get() =
                uchar2(this.x, this.x)
            
        member this.yy 
            with get() =
                uchar2(this.y, this.y)

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

        new(X: byte, Y: byte) =
            { x = X; y = Y }
            
        new(v: byte) =
            { x = v; y = v; }

        internal new(c: byte[]) =
            uchar2(c.[0], c.[1])

        static member (+) (f1: uchar2, f2: uchar2) =
            uchar2(Array.map2 (+) (f1.Components) (f2.Components))
        static member (+) (f1: byte, f2: uchar2) =
            uchar2(Array.map2 (+) ([| f1;f1 |]) (f2.Components))
        static member (+) (f1: uchar2, f2: byte) =
            uchar2(Array.map2 (+) (f1.Components) ([| f2;f2 |]))
        static member (-) (f1: uchar2, f2: uchar2) =
            uchar2(Array.map2 (-) (f1.Components) (f2.Components))
        static member (-) (f1: byte, f2: uchar2) =
            uchar2(Array.map2 (-) ([| f1;f1 |]) (f2.Components))
        static member (-) (f1: uchar2, f2: byte) =
            uchar2(Array.map2 (-) (f1.Components) ([| f2;f2 |]))
        static member (*) (f1: uchar2, f2: uchar2) =
            uchar2(Array.map2 (*) (f1.Components) (f2.Components))
        static member (*) (f1: byte, f2: uchar2) =
            uchar2(Array.map2 (*) ([| f1;f1;|]) (f2.Components))
        static member (*) (f1: uchar2, f2: byte) =
            uchar2(Array.map2 (*) ([| f2;f2 |]) (f1.Components))
        static member (/) (f1: uchar2, f2: uchar2) =
            uchar2(Array.map2 (/) (f1.Components) (f2.Components))
        static member (/) (f1: byte, f2: uchar2) =
            uchar2(Array.map2 (/) ([| f1;f1 |]) (f2.Components))
        static member (/) (f1: uchar2, f2: byte) =
            uchar2(Array.map2 (/) (f1.Components) ([| f2;f2 |]))
        
        static member (>>=) (f1: uchar2, f2: uchar2) =
            char2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: uchar2, f2: uchar2) =
            char2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: uchar2, f2: uchar2) =
            char2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: uchar2, f2: uchar2) =
            char2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> uchar2
            stream.Close()
            data

    end
    
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]      
[<VectorType>]         
type uchar3 =
    struct
        val mutable x: byte
        val mutable y: byte
        val mutable z: byte

        member this.Components
            with get() =
                [| this.x; this.y; this.z |]
                
        new(X: byte, Y: byte, Z: byte) =
            { x = X; y = Y; z = Z }
            
        new(v: byte) =
            { x = v; y = v; z = v }

        member this.xy 
            with get() =
                uchar2(this.x, this.y)
            and set (v: uchar2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                uchar2(this.x, this.z)
            and set (v: uchar2) =
                this.x <- v.x
                this.z <- v.y

        member this.yx 
            with get() =
                uchar2(this.y, this.x)
            and set (v: uchar2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                uchar2(this.y, this.z)
            and set (v: uchar2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.zx 
            with get() =
                uchar2(this.z, this.x)
            and set (v: uchar2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                uchar2(this.z, this.y)
            and set (v: uchar2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.xx 
            with get() =
                uchar2(this.x, this.x)
            
        member this.yy 
            with get() =
                uchar2(this.y, this.y)
            
        member this.zz 
            with get() =
                uchar2(this.z, this.z)

        // 3-comps    
        member this.xyz 
            with get() =
                uchar3(this.x, this.y, this.z)
            and set (v: uchar3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                uchar3(this.x, this.z, this.y)
            and set (v: uchar3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.yxz 
            with get() =
                uchar3(this.y, this.x, this.z)
            and set (v: uchar3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                uchar3(this.y, this.z, this.x)
            and set (v: uchar3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.zxy 
            with get() =
                uchar3(this.z, this.x, this.y)
            and set (v: uchar3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                uchar3(this.z, this.y, this.x)
            and set (v: uchar3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.xxy 
            with get() =
                uchar3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                uchar3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                uchar3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                uchar3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                uchar3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                uchar3(this.z, this.x, this.x)
                        
        member this.yyx 
            with get() =
                uchar3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                uchar3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                uchar3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                uchar3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                uchar3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                uchar3(this.z, this.y, this.y)
            
        member this.zzx 
            with get() =
                uchar3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                uchar3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                uchar3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                uchar3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                uchar3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                uchar3(this.y, this.z, this.z)
            
        member this.xxx 
            with get() =
                uchar3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                uchar3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                uchar3(this.z, this.z, this.z)

        member this.lo 
            with get() =
                uchar2(this.x, this.y)
            and set (v:uchar2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                uchar2(this.y, 0uy)
            and set (v:uchar2) =
                this.z <- v.x
            
        member this.even 
            with get() =
                uchar2(this.x, this.z)
            and set (v:uchar2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                uchar2(this.y, 0uy)
            and set (v:uchar2) =
                this.y <- v.x

        internal new(c: byte[]) =
            uchar3(c.[0], c.[1], c.[2])

        static member (+) (f1: uchar3, f2: uchar3) =
            uchar3(Array.map2 (+) (f1.Components) (f2.Components))
        static member (+) (f1: byte, f2: uchar3) =
            uchar3(Array.map2 (+) ([| f1;f1;f1 |]) (f2.Components))
        static member (+) (f1: uchar3, f2: byte) =
            uchar3(Array.map2 (+) (f1.Components) ([| f2;f2;f2 |]))
        static member (-) (f1: uchar3, f2: uchar3) =
            uchar3(Array.map2 (-) (f1.Components) (f2.Components))
        static member (-) (f1: byte, f2: uchar3) =
            uchar3(Array.map2 (-) ([| f1;f1;f1 |]) (f2.Components))
        static member (-) (f1: uchar3, f2: byte) =
            uchar3(Array.map2 (-) (f1.Components) ([| f2;f2;f2 |]))
        static member (*) (f1: uchar3, f2: uchar3) =
            uchar3(Array.map2 (*) (f1.Components) (f2.Components))
        static member (*) (f1: byte, f2: uchar3) =
            uchar3(Array.map2 (*) ([| f1;f1;f1 |]) (f2.Components))
        static member (*) (f1: uchar3, f2: byte) =
            uchar3(Array.map2 (*) ([| f2;f2;f2 |]) (f1.Components))
        static member (/) (f1: uchar3, f2: uchar3) =
            uchar3(Array.map2 (/) (f1.Components) (f2.Components))
        static member (/) (f1: byte, f2: uchar3) =
            uchar3(Array.map2 (/) ([| f1;f1;f1 |]) (f2.Components))
        static member (/) (f1: uchar3, f2: byte) =
            uchar3(Array.map2 (/) (f1.Components) ([| f2;f2;f2 |]))
        
        static member (>>=) (f1: uchar3, f2: uchar3) =
            char3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: uchar3, f2: uchar3) =
            char3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: uchar3, f2: uchar3) =
            char3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: uchar3, f2: uchar3) =
            char3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> uchar3
            stream.Close()
            data
    end

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]    
[<VectorType>]        
type uchar4 =
    struct
        val mutable x: byte
        val mutable y: byte
        val mutable z: byte
        val mutable w: byte

        member this.Components
            with get() =
                [| this.x; this.y; this.z; this.w |]
                
        new(X: byte, Y: byte, Z: byte, W: byte) =
            { x = X; y = Y; z = Z; w = W }
            
        new(v: byte) =
            { x = v; y = v; z = v; w = v }

        member this.xy 
            with get() =
                uchar2(this.x, this.y)
            and set (v: uchar2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                uchar2(this.x, this.z)
            and set (v: uchar2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.xw 
            with get() =
                uchar2(this.x, this.w)
            and set (v: uchar2) =
                this.x <- v.x
                this.w <- v.y

        member this.yx 
            with get() =
                uchar2(this.y, this.x)
            and set (v: uchar2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                uchar2(this.y, this.z)
            and set (v: uchar2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.yw 
            with get() =
                uchar2(this.y, this.w)
            and set (v: uchar2) =
                this.y <- v.x
                this.w <- v.y

        member this.zx 
            with get() =
                uchar2(this.z, this.x)
            and set (v: uchar2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                uchar2(this.z, this.y)
            and set (v: uchar2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.zw 
            with get() =
                uchar2(this.z, this.w)
            and set (v: uchar2) =
                this.z <- v.x
                this.w <- v.y
                          
        member this.wx 
            with get() =
                uchar2(this.w, this.x)
            and set (v: uchar2) =
                this.w <- v.x
                this.x <- v.y
            
        member this.wy 
            with get() =
                uchar2(this.w, this.y)
            and set (v: uchar2) =
                this.w <- v.x
                this.y <- v.y
            
        member this.wz 
            with get() =
                uchar2(this.w, this.z)
            and set (v: uchar2) =
                this.w <- v.x
                this.z <- v.y

        member this.xx 
            with get() =
                uchar2(this.x, this.x)
            
        member this.yy 
            with get() =
                uchar2(this.y, this.y)
            
        member this.zz 
            with get() =
                uchar2(this.z, this.z)
            
        member this.ww 
            with get() =
                uchar2(this.w, this.w)

        // 3-comps    
        member this.xyz 
            with get() =
                uchar3(this.x, this.y, this.z)
            and set (v: uchar3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                uchar3(this.x, this.z, this.y)
            and set (v: uchar3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.xyw
            with get() =
                uchar3(this.x, this.y, this.w)
            and set (v: uchar3) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.xwy
            with get() =
                uchar3(this.x, this.w, this.y)
            and set (v: uchar3) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
            
        member this.xzw
            with get() =
                uchar3(this.x, this.z, this.w)
            and set (v: uchar3) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.xwz
            with get() =
                uchar3(this.x, this.w, this.z)
            and set (v: uchar3) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.yxz 
            with get() =
                uchar3(this.y, this.x, this.z)
            and set (v: uchar3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                uchar3(this.y, this.z, this.x)
            and set (v: uchar3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.yxw 
            with get() =
                uchar3(this.y, this.x, this.w)
            and set (v: uchar3) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.ywx 
            with get() =
                uchar3(this.y, this.w, this.x)
            and set (v: uchar3) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.yzw 
            with get() =
                uchar3(this.y, this.z, this.w)
            and set (v: uchar3) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.ywz 
            with get() =
                uchar3(this.y, this.w, this.z)
            and set (v: uchar3) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.zxy 
            with get() =
                uchar3(this.z, this.x, this.y)
            and set (v: uchar3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                uchar3(this.z, this.y, this.x)
            and set (v: uchar3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.zxw 
            with get() =
                uchar3(this.z, this.x, this.w)
            and set (v: uchar3) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.zwx 
            with get() =
                uchar3(this.z, this.w, this.x)
            and set (v: uchar3) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.zyw 
            with get() =
                uchar3(this.z, this.y, this.w)
            and set (v: uchar3) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.zwy 
            with get() =
                uchar3(this.z, this.w, this.y)
            and set (v: uchar3) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                        
        member this.wxy 
            with get() =
                uchar3(this.w, this.x, this.y)
            and set (v: uchar3) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.wyx 
            with get() =
                uchar3(this.w, this.y, this.x)
            and set (v: uchar3) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.wxz 
            with get() =
                uchar3(this.w, this.x, this.z)
            and set (v: uchar3) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.wzx 
            with get() =
                uchar3(this.w, this.z, this.x)
            and set (v: uchar3) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.wyz 
            with get() =
                uchar3(this.w, this.y, this.z)
            and set (v: uchar3) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.wzy 
            with get() =
                uchar3(this.w, this.z, this.y)
            and set (v: uchar3) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z

        member this.xxy 
            with get() =
                uchar3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                uchar3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                uchar3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                uchar3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                uchar3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                uchar3(this.z, this.x, this.x)
                    
        member this.xxw 
            with get() =
                uchar3(this.x, this.x, this.w)
            
        member this.xwx 
            with get() =
                uchar3(this.x, this.w, this.x)

        member this.wxx 
            with get() =
                uchar3(this.w, this.x, this.x)
                            
        member this.yyx 
            with get() =
                uchar3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                uchar3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                uchar3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                uchar3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                uchar3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                uchar3(this.z, this.y, this.y)
            
        member this.yyw 
            with get() =
                uchar3(this.y, this.y, this.w)
            
        member this.ywy 
            with get() =
                uchar3(this.y, this.w, this.y)

        member this.wyy
            with get() =
                uchar3(this.w, this.y, this.y)

        member this.zzx 
            with get() =
                uchar3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                uchar3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                uchar3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                uchar3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                uchar3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                uchar3(this.y, this.z, this.z)
            
        member this.zzw 
            with get() =
                uchar3(this.z, this.z, this.w)
            
        member this.zwz 
            with get() =
                uchar3(this.z, this.w, this.z)
            
        member this.wzz 
            with get() =
                uchar3(this.w, this.z, this.z)
            
        member this.xxx 
            with get() =
                uchar3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                uchar3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                uchar3(this.z, this.z, this.z)
 
        member this.www 
            with get() =
                uchar3(this.w, this.w, this.w)
     
        // 4-comps       
        member this.xxxx
            with get() =
                uchar4(this.x, this.x, this.x, this.x)

        member this.xxxy
            with get() =
                uchar4(this.x, this.x, this.x, this.y)

        member this.xxxz
            with get() =
                uchar4(this.x, this.x, this.x, this.z)

        member this.xxxw
            with get() =
                uchar4(this.x, this.x, this.x, this.w)

        member this.xxyx
            with get() =
                uchar4(this.x, this.x, this.y, this.x)

        member this.xxyy
            with get() =
                uchar4(this.x, this.x, this.y, this.y)

        member this.xxyz
            with get() =
                uchar4(this.x, this.x, this.y, this.z)

        member this.xxyw
            with get() =
                uchar4(this.x, this.x, this.y, this.w)

        member this.xxzx
            with get() =
                uchar4(this.x, this.x, this.z, this.x)

        member this.xxzy
            with get() =
                uchar4(this.x, this.x, this.z, this.y)

        member this.xxzz
            with get() =
                uchar4(this.x, this.x, this.z, this.z)

        member this.xxzw
            with get() =
                uchar4(this.x, this.x, this.z, this.w)

        member this.xxwx
            with get() =
                uchar4(this.x, this.x, this.w, this.x)

        member this.xxwy
            with get() =
                uchar4(this.x, this.x, this.w, this.y)

        member this.xxwz
            with get() =
                uchar4(this.x, this.x, this.w, this.z)

        member this.xxww
            with get() =
                uchar4(this.x, this.x, this.w, this.w)

        member this.xyxx
            with get() =
                uchar4(this.x, this.y, this.x, this.x)

        member this.xyxy
            with get() =
                uchar4(this.x, this.y, this.x, this.y)

        member this.xyxz
            with get() =
                uchar4(this.x, this.y, this.x, this.z)

        member this.xyxw
            with get() =
                uchar4(this.x, this.y, this.x, this.w)

        member this.xyyx
            with get() =
                uchar4(this.x, this.y, this.y, this.x)

        member this.xyyy
            with get() =
                uchar4(this.x, this.y, this.y, this.y)

        member this.xyyz
            with get() =
                uchar4(this.x, this.y, this.y, this.z)

        member this.xyyw
            with get() =
                uchar4(this.x, this.y, this.y, this.w)

        member this.xyzx
            with get() =
                uchar4(this.x, this.y, this.z, this.x)

        member this.xyzy
            with get() =
                uchar4(this.x, this.y, this.z, this.y)

        member this.xyzz
            with get() =
                uchar4(this.x, this.y, this.z, this.z)

        member this.xyzw
            with get() =
                uchar4(this.x, this.y, this.z, this.w)
            and set(v: uchar4) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
                this.w <- v.w
            
        member this.xywx
            with get() =
                uchar4(this.x, this.y, this.w, this.x)

        member this.xywy
            with get() =
                uchar4(this.x, this.y, this.w, this.y)

        member this.xywz
            with get() =
                uchar4(this.x, this.y, this.w, this.z)
            and set(v: uchar4) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.xyww
            with get() =
                uchar4(this.x, this.y, this.w, this.w)

        member this.xzxx
            with get() =
                uchar4(this.x, this.z, this.x, this.x)

        member this.xzxy
            with get() =
                uchar4(this.x, this.z, this.x, this.y)

        member this.xzxz
            with get() =
                uchar4(this.x, this.z, this.x, this.z)

        member this.xzxw
            with get() =
                uchar4(this.x, this.z, this.x, this.w)

        member this.xzyx
            with get() =
                uchar4(this.x, this.z, this.y, this.x)

        member this.xzyy
            with get() =
                uchar4(this.x, this.z, this.y, this.y)

        member this.xzyz
            with get() =
                uchar4(this.x, this.z, this.y, this.z)

        member this.xzyw
            with get() =
                uchar4(this.x, this.z, this.y, this.w)
            and set(v: uchar4) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.xzzx
            with get() =
                uchar4(this.x, this.z, this.z, this.x)

        member this.xzzy
            with get() =
                uchar4(this.x, this.z, this.z, this.y)

        member this.xzzz
            with get() =
                uchar4(this.x, this.z, this.z, this.z)

        member this.xzzw
            with get() =
                uchar4(this.x, this.z, this.z, this.w)

        member this.xzwx
            with get() =
                uchar4(this.x, this.z, this.w, this.x)

        member this.xzwy
            with get() =
                uchar4(this.x, this.z, this.w, this.y)
            and set(v: uchar4) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.xzwz
            with get() =
                uchar4(this.x, this.z, this.w, this.z)

        member this.xzww
            with get() =
                uchar4(this.x, this.z, this.w, this.w)

        member this.xwxx
            with get() =
                uchar4(this.x, this.w, this.x, this.x)

        member this.xwxy
            with get() =
                uchar4(this.x, this.w, this.x, this.y)

        member this.xwxz
            with get() =
                uchar4(this.x, this.w, this.x, this.z)

        member this.xwxw
            with get() =
                uchar4(this.x, this.w, this.x, this.w)

        member this.xwyx
            with get() =
                uchar4(this.x, this.w, this.y, this.x)

        member this.xwyy
            with get() =
                uchar4(this.x, this.w, this.y, this.y)

        member this.xwyz
            with get() =
                uchar4(this.x, this.w, this.y, this.z)
            and set(v: uchar4) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.xwyw
            with get() =
                uchar4(this.x, this.w, this.y, this.w)

        member this.xwzx
            with get() =
                uchar4(this.x, this.w, this.z, this.x)

        member this.xwzy
            with get() =
                uchar4(this.x, this.w, this.z, this.y)
            and set(v: uchar4) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z
                this.y <- v.w
            
        member this.xwzz
            with get() =
                uchar4(this.x, this.w, this.z, this.z)

        member this.xwzw
            with get() =
                uchar4(this.x, this.w, this.z, this.w)

        member this.xwwx
            with get() =
                uchar4(this.x, this.w, this.w, this.x)

        member this.xwwy
            with get() =
                uchar4(this.x, this.w, this.w, this.y)

        member this.xwwz
            with get() =
                uchar4(this.x, this.w, this.w, this.z)

        member this.xwww
            with get() =
                uchar4(this.x, this.w, this.w, this.w)

        member this.yxxx
            with get() =
                uchar4(this.y, this.x, this.x, this.x)

        member this.yxxy
            with get() =
                uchar4(this.y, this.x, this.x, this.y)

        member this.yxxz
            with get() =
                uchar4(this.y, this.x, this.x, this.z)

        member this.yxxw
            with get() =
                uchar4(this.y, this.x, this.x, this.w)

        member this.yxyx
            with get() =
                uchar4(this.y, this.x, this.y, this.x)

        member this.yxyy
            with get() =
                uchar4(this.y, this.x, this.y, this.y)

        member this.yxyz
            with get() =
                uchar4(this.y, this.x, this.y, this.z)

        member this.yxyw
            with get() =
                uchar4(this.y, this.x, this.y, this.w)

        member this.yxzx
            with get() =
                uchar4(this.y, this.x, this.z, this.x)

        member this.yxzy
            with get() =
                uchar4(this.y, this.x, this.z, this.y)

        member this.yxzz
            with get() =
                uchar4(this.y, this.x, this.z, this.z)

        member this.yxzw
            with get() =
                uchar4(this.y, this.x, this.z, this.w)
            and set(v: uchar4) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
                this.w <- v.w


        member this.yxwx
            with get() =
                uchar4(this.y, this.x, this.w, this.x)

        member this.yxwy
            with get() =
                uchar4(this.y, this.x, this.w, this.y)

        member this.yxwz
            with get() =
                uchar4(this.y, this.x, this.w, this.z)
            and set(v: uchar4) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.yxww
            with get() =
                uchar4(this.y, this.x, this.w, this.w)

        member this.yyxx
            with get() =
                uchar4(this.y, this.y, this.x, this.x)

        member this.yyxy
            with get() =
                uchar4(this.y, this.y, this.x, this.y)

        member this.yyxz
            with get() =
                uchar4(this.y, this.y, this.x, this.z)

        member this.yyxw
            with get() =
                uchar4(this.y, this.y, this.x, this.w)

        member this.yyyx
            with get() =
                uchar4(this.y, this.y, this.y, this.x)

        member this.yyyy
            with get() =
                uchar4(this.y, this.y, this.y, this.y)

        member this.yyyz
            with get() =
                uchar4(this.y, this.y, this.y, this.z)

        member this.yyyw
            with get() =
                uchar4(this.y, this.y, this.y, this.w)

        member this.yyzx
            with get() =
                uchar4(this.y, this.y, this.z, this.x)

        member this.yyzy
            with get() =
                uchar4(this.y, this.y, this.z, this.y)

        member this.yyzz
            with get() =
                uchar4(this.y, this.y, this.z, this.z)

        member this.yyzw
            with get() =
                uchar4(this.y, this.y, this.z, this.w)

        member this.yywx
            with get() =
                uchar4(this.y, this.y, this.w, this.x)

        member this.yywy
            with get() =
                uchar4(this.y, this.y, this.w, this.y)

        member this.yywz
            with get() =
                uchar4(this.y, this.y, this.w, this.z)

        member this.yyww
            with get() =
                uchar4(this.y, this.y, this.w, this.w)

        member this.yzxx
            with get() =
                uchar4(this.y, this.z, this.x, this.x)

        member this.yzxy
            with get() =
                uchar4(this.y, this.z, this.x, this.y)

        member this.yzxz
            with get() =
                uchar4(this.y, this.z, this.x, this.z)

        member this.yzxw
            with get() =
                uchar4(this.y, this.z, this.x, this.w)
            and set(v: uchar4) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.yzyx
            with get() =
                uchar4(this.y, this.z, this.y, this.x)

        member this.yzyy
            with get() =
                uchar4(this.y, this.z, this.y, this.y)

        member this.yzyz
            with get() =
                uchar4(this.y, this.z, this.y, this.z)

        member this.yzyw
            with get() =
                uchar4(this.y, this.z, this.y, this.w)

        member this.yzzx
            with get() =
                uchar4(this.y, this.z, this.z, this.x)

        member this.yzzy
            with get() =
                uchar4(this.y, this.z, this.z, this.y)

        member this.yzzz
            with get() =
                uchar4(this.y, this.z, this.z, this.z)

        member this.yzzw
            with get() =
                uchar4(this.y, this.z, this.z, this.w)

        member this.yzwx
            with get() =
                uchar4(this.y, this.z, this.w, this.x)
            and set(v: uchar4) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.yzwy
            with get() =
                uchar4(this.y, this.z, this.w, this.y)

        member this.yzwz
            with get() =
                uchar4(this.y, this.z, this.w, this.z)

        member this.yzww
            with get() =
                uchar4(this.y, this.z, this.w, this.w)

        member this.ywxx
            with get() =
                uchar4(this.y, this.w, this.x, this.x)

        member this.ywxy
            with get() =
                uchar4(this.y, this.w, this.x, this.y)

        member this.ywxz
            with get() =
                uchar4(this.y, this.w, this.x, this.z)
            and set(v: uchar4) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
                this.z <- v.w


        member this.ywxw
            with get() =
                uchar4(this.y, this.w, this.x, this.w)

        member this.ywyx
            with get() =
                uchar4(this.y, this.w, this.y, this.x)

        member this.ywyy
            with get() =
                uchar4(this.y, this.w, this.y, this.y)

        member this.ywyz
            with get() =
                uchar4(this.y, this.w, this.y, this.z)

        member this.ywyw
            with get() =
                uchar4(this.y, this.w, this.y, this.w)

        member this.ywzx
            with get() =
                uchar4(this.y, this.w, this.z, this.x)
            and set(v: uchar4) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.ywzy
            with get() =
                uchar4(this.y, this.w, this.z, this.y)

        member this.ywzz
            with get() =
                uchar4(this.y, this.w, this.z, this.z)

        member this.ywzw
            with get() =
                uchar4(this.y, this.w, this.z, this.w)

        member this.ywwx
            with get() =
                uchar4(this.y, this.w, this.w, this.x)

        member this.ywwy
            with get() =
                uchar4(this.y, this.w, this.w, this.y)

        member this.ywwz
            with get() =
                uchar4(this.y, this.w, this.w, this.z)

        member this.ywww
            with get() =
                uchar4(this.y, this.w, this.w, this.w)

        member this.zxxx
            with get() =
                uchar4(this.z, this.x, this.x, this.x)

        member this.zxxy
            with get() =
                uchar4(this.z, this.x, this.x, this.y)

        member this.zxxz
            with get() =
                uchar4(this.z, this.x, this.x, this.z)

        member this.zxxw
            with get() =
                uchar4(this.z, this.x, this.x, this.w)

        member this.zxyx
            with get() =
                uchar4(this.z, this.x, this.y, this.x)

        member this.zxyy
            with get() =
                uchar4(this.z, this.x, this.y, this.y)

        member this.zxyz
            with get() =
                uchar4(this.z, this.x, this.y, this.z)

        member this.zxyw
            with get() =
                uchar4(this.z, this.x, this.y, this.w)
            and set(v: uchar4) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.zxzx
            with get() =
                uchar4(this.z, this.x, this.z, this.x)

        member this.zxzy
            with get() =
                uchar4(this.z, this.x, this.z, this.y)

        member this.zxzz
            with get() =
                uchar4(this.z, this.x, this.z, this.z)

        member this.zxzw
            with get() =
                uchar4(this.z, this.x, this.z, this.w)

        member this.zxwx
            with get() =
                uchar4(this.z, this.x, this.w, this.x)

        member this.zxwy
            with get() =
                uchar4(this.z, this.x, this.w, this.y)
            and set(v: uchar4) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.zxwz
            with get() =
                uchar4(this.z, this.x, this.w, this.z)

        member this.zxww
            with get() =
                uchar4(this.z, this.x, this.w, this.w)

        member this.zyxx
            with get() =
                uchar4(this.z, this.y, this.x, this.x)

        member this.zyxy
            with get() =
                uchar4(this.z, this.y, this.x, this.y)

        member this.zyxz
            with get() =
                uchar4(this.z, this.y, this.x, this.z)

        member this.zyxw
            with get() =
                uchar4(this.z, this.y, this.x, this.w)
            and set(v: uchar4) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.zyyx
            with get() =
                uchar4(this.z, this.y, this.y, this.x)

        member this.zyyy
            with get() =
                uchar4(this.z, this.y, this.y, this.y)

        member this.zyyz
            with get() =
                uchar4(this.z, this.y, this.y, this.z)

        member this.zyyw
            with get() =
                uchar4(this.z, this.y, this.y, this.w)

        member this.zyzx
            with get() =
                uchar4(this.z, this.y, this.z, this.x)

        member this.zyzy
            with get() =
                uchar4(this.z, this.y, this.z, this.y)

        member this.zyzz
            with get() =
                uchar4(this.z, this.y, this.z, this.z)

        member this.zyzw
            with get() =
                uchar4(this.z, this.y, this.z, this.w)

        member this.zywx
            with get() =
                uchar4(this.z, this.y, this.w, this.x)
            and set(v: uchar4) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.zywy
            with get() =
                uchar4(this.z, this.y, this.w, this.y)

        member this.zywz
            with get() =
                uchar4(this.z, this.y, this.w, this.z)

        member this.zyww
            with get() =
                uchar4(this.z, this.y, this.w, this.w)

        member this.zzxx
            with get() =
                uchar4(this.z, this.z, this.x, this.x)

        member this.zzxy
            with get() =
                uchar4(this.z, this.z, this.x, this.y)

        member this.zzxz
            with get() =
                uchar4(this.z, this.z, this.x, this.z)

        member this.zzxw
            with get() =
                uchar4(this.z, this.z, this.x, this.w)

        member this.zzyx
            with get() =
                uchar4(this.z, this.z, this.y, this.x)

        member this.zzyy
            with get() =
                uchar4(this.z, this.z, this.y, this.y)

        member this.zzyz
            with get() =
                uchar4(this.z, this.z, this.y, this.z)

        member this.zzyw
            with get() =
                uchar4(this.z, this.z, this.y, this.w)

        member this.zzzx
            with get() =
                uchar4(this.z, this.z, this.z, this.x)

        member this.zzzy
            with get() =
                uchar4(this.z, this.z, this.z, this.y)

        member this.zzzz
            with get() =
                uchar4(this.z, this.z, this.z, this.z)

        member this.zzzw
            with get() =
                uchar4(this.z, this.z, this.z, this.w)

        member this.zzwx
            with get() =
                uchar4(this.z, this.z, this.w, this.x)

        member this.zzwy
            with get() =
                uchar4(this.z, this.z, this.w, this.y)

        member this.zzwz
            with get() =
                uchar4(this.z, this.z, this.w, this.z)

        member this.zzww
            with get() =
                uchar4(this.z, this.z, this.w, this.w)

        member this.zwxx
            with get() =
                uchar4(this.z, this.w, this.x, this.x)

        member this.zwxy
            with get() =
                uchar4(this.z, this.w, this.x, this.y)
            and set(v: uchar4) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.zwxz
            with get() =
                uchar4(this.z, this.w, this.x, this.z)

        member this.zwxw
            with get() =
                uchar4(this.z, this.w, this.x, this.w)

        member this.zwyx
            with get() =
                uchar4(this.z, this.w, this.y, this.x)
            and set(v: uchar4) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.zwyy
            with get() =
                uchar4(this.z, this.w, this.y, this.y)

        member this.zwyz
            with get() =
                uchar4(this.z, this.w, this.y, this.z)

        member this.zwyw
            with get() =
                uchar4(this.z, this.w, this.y, this.w)

        member this.zwzx
            with get() =
                uchar4(this.z, this.w, this.z, this.x)

        member this.zwzy
            with get() =
                uchar4(this.z, this.w, this.z, this.y)

        member this.zwzz
            with get() =
                uchar4(this.z, this.w, this.z, this.z)

        member this.zwzw
            with get() =
                uchar4(this.z, this.w, this.z, this.w)

        member this.zwwx
            with get() =
                uchar4(this.z, this.w, this.w, this.x)

        member this.zwwy
            with get() =
                uchar4(this.z, this.w, this.w, this.y)

        member this.zwwz
            with get() =
                uchar4(this.z, this.w, this.w, this.z)

        member this.zwww
            with get() =
                uchar4(this.z, this.w, this.w, this.w)

        member this.wxxx
            with get() =
                uchar4(this.w, this.x, this.x, this.x)

        member this.wxxy
            with get() =
                uchar4(this.w, this.x, this.x, this.y)

        member this.wxxz
            with get() =
                uchar4(this.w, this.x, this.x, this.z)

        member this.wxxw
            with get() =
                uchar4(this.w, this.x, this.x, this.w)

        member this.wxyx
            with get() =
                uchar4(this.w, this.x, this.y, this.x)

        member this.wxyy
            with get() =
                uchar4(this.w, this.x, this.y, this.y)

        member this.wxyz
            with get() =
                uchar4(this.w, this.x, this.y, this.z)
            and set(v: uchar4) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.wxyw
            with get() =
                uchar4(this.w, this.x, this.y, this.w)

        member this.wxzx
            with get() =
                uchar4(this.w, this.x, this.z, this.x)

        member this.wxzy
            with get() =
                uchar4(this.w, this.x, this.z, this.y)
            and set(v: uchar4) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
                this.y <- v.w


        member this.wxzz
            with get() =
                uchar4(this.w, this.x, this.z, this.z)

        member this.wxzw
            with get() =
                uchar4(this.w, this.x, this.z, this.w)

        member this.wxwx
            with get() =
                uchar4(this.w, this.x, this.w, this.x)

        member this.wxwy
            with get() =
                uchar4(this.w, this.x, this.w, this.y)

        member this.wxwz
            with get() =
                uchar4(this.w, this.x, this.w, this.z)

        member this.wxww
            with get() =
                uchar4(this.w, this.x, this.w, this.w)

        member this.wyxx
            with get() =
                uchar4(this.w, this.y, this.x, this.x)

        member this.wyxy
            with get() =
                uchar4(this.w, this.y, this.x, this.y)

        member this.wyxz
            with get() =
                uchar4(this.w, this.y, this.x, this.z)
            and set(v: uchar4) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
                this.z <- v.w
            
        member this.wyxw
            with get() =
                uchar4(this.w, this.y, this.x, this.w)

        member this.wyyx
            with get() =
                uchar4(this.w, this.y, this.y, this.x)

        member this.wyyy
            with get() =
                uchar4(this.w, this.y, this.y, this.y)

        member this.wyyz
            with get() =
                uchar4(this.w, this.y, this.y, this.z)

        member this.wyyw
            with get() =
                uchar4(this.w, this.y, this.y, this.w)

        member this.wyzx
            with get() =
                uchar4(this.w, this.y, this.z, this.x)
            and set(v: uchar4) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.wyzy
            with get() =
                uchar4(this.w, this.y, this.z, this.y)

        member this.wyzz
            with get() =
                uchar4(this.w, this.y, this.z, this.z)

        member this.wyzw
            with get() =
                uchar4(this.w, this.y, this.z, this.w)

        member this.wywx
            with get() =
                uchar4(this.w, this.y, this.w, this.x)

        member this.wywy
            with get() =
                uchar4(this.w, this.y, this.w, this.y)

        member this.wywz
            with get() =
                uchar4(this.w, this.y, this.w, this.z)

        member this.wyww
            with get() =
                uchar4(this.w, this.y, this.w, this.w)

        member this.wzxx
            with get() =
                uchar4(this.w, this.z, this.x, this.x)

        member this.wzxy
            with get() =
                uchar4(this.w, this.z, this.x, this.y)
            and set(v: uchar4) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.wzxz
            with get() =
                uchar4(this.w, this.z, this.x, this.z)

        member this.wzxw
            with get() =
                uchar4(this.w, this.z, this.x, this.w)

        member this.wzyx
            with get() =
                uchar4(this.w, this.z, this.y, this.x)
            and set(v: uchar4) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.wzyy
            with get() =
                uchar4(this.w, this.z, this.y, this.y)

        member this.wzyz
            with get() =
                uchar4(this.w, this.z, this.y, this.z)

        member this.wzyw
            with get() =
                uchar4(this.w, this.z, this.y, this.w)

        member this.wzzx
            with get() =
                uchar4(this.w, this.z, this.z, this.x)

        member this.wzzy
            with get() =
                uchar4(this.w, this.z, this.z, this.y)

        member this.wzzz
            with get() =
                uchar4(this.w, this.z, this.z, this.z)

        member this.wzzw
            with get() =
                uchar4(this.w, this.z, this.z, this.w)

        member this.wzwx
            with get() =
                uchar4(this.w, this.z, this.w, this.x)

        member this.wzwy
            with get() =
                uchar4(this.w, this.z, this.w, this.y)

        member this.wzwz
            with get() =
                uchar4(this.w, this.z, this.w, this.z)

        member this.wzww
            with get() =
                uchar4(this.w, this.z, this.w, this.w)

        member this.wwxx
            with get() =
                uchar4(this.w, this.w, this.x, this.x)

        member this.wwxy
            with get() =
                uchar4(this.w, this.w, this.x, this.y)

        member this.wwxz
            with get() =
                uchar4(this.w, this.w, this.x, this.z)

        member this.wwxw
            with get() =
                uchar4(this.w, this.w, this.x, this.w)

        member this.wwyx
            with get() =
                uchar4(this.w, this.w, this.y, this.x)

        member this.wwyy
            with get() =
                uchar4(this.w, this.w, this.y, this.y)

        member this.wwyz
            with get() =
                uchar4(this.w, this.w, this.y, this.z)

        member this.wwyw
            with get() =
                uchar4(this.w, this.w, this.y, this.w)

        member this.wwzx
            with get() =
                uchar4(this.w, this.w, this.z, this.x)

        member this.wwzy
            with get() =
                uchar4(this.w, this.w, this.z, this.y)

        member this.wwzz
            with get() =
                uchar4(this.w, this.w, this.z, this.z)

        member this.wwzw
            with get() =
                uchar4(this.w, this.w, this.z, this.w)

        member this.wwwx
            with get() =
                uchar4(this.w, this.w, this.w, this.x)

        member this.wwwy
            with get() =
                uchar4(this.w, this.w, this.w, this.y)

        member this.wwwz
            with get() =
                uchar4(this.w, this.w, this.w, this.z)

        member this.wwww
            with get() =
                uchar4(this.w, this.w, this.w, this.w)

        member this.lo 
            with get() =
                uchar2(this.x, this.y)
            and set (v:uchar2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                uchar2(this.y, this.w)
            and set (v:uchar2) =
                this.z <- v.x
                this.w <- v.y
            
        member this.even 
            with get() =
                uchar2(this.x, this.z)
            and set (v:uchar2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                uchar2(this.y, this.w)
            and set (v:uchar2) =
                this.y <- v.x
                this.w <- v.y

        internal new(c: byte[]) =
            uchar4(c.[0], c.[1], c.[2], c.[3])

        static member (+) (f1: uchar4, f2: uchar4) =
            uchar4(Array.map2 (+) (f1.Components) (f2.Components))
        static member (+) (f1: byte, f2: uchar4) =
            uchar4(Array.map2 (+) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (+) (f1: uchar4, f2: byte) =
            uchar4(Array.map2 (+) (f1.Components) ([| f2;f2;f2;f2 |]))
        static member (-) (f1: uchar4, f2: uchar4) =
            uchar4(Array.map2 (-) (f1.Components) (f2.Components))
        static member (-) (f1: byte, f2: uchar4) =
            uchar4(Array.map2 (-) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (-) (f1: uchar4, f2: byte) =
            uchar4(Array.map2 (-) (f1.Components) ([| f2;f2;f2;f2 |]))
        static member (*) (f1: uchar4, f2: uchar4) =
            uchar4(Array.map2 (*) (f1.Components) (f2.Components))
        static member (*) (f1: byte, f2: uchar4) =
            uchar4(Array.map2 (*) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (*) (f1: uchar4, f2: byte) =
            uchar4(Array.map2 (*) ([| f2;f2;f2;f2 |]) (f1.Components))
        static member (/) (f1: uchar4, f2: uchar4) =
            uchar4(Array.map2 (/) (f1.Components) (f2.Components))
        static member (/) (f1: byte, f2: uchar4) =
            uchar4(Array.map2 (/) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (/) (f1: uchar4, f2: byte) =
            uchar4(Array.map2 (/) (f1.Components) ([| f2;f2;f2;f2 |]))
        
        static member (>>=) (f1: uchar4, f2: uchar4) =
            char4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: uchar4, f2: uchar4) =
            char4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: uchar4, f2: uchar4) =
            char4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: uchar4, f2: uchar4) =
            char4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 4L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> uchar4
            stream.Close()
            data          
    end    

// **************************************************************************************************************
