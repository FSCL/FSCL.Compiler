namespace FSCL
open System.Runtime.InteropServices
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters
open System

// byteeger vector types
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type byte2 =
    struct 
        val mutable x: byte
        val mutable y: byte

        member internal this.Components
            with get() =
                [| this.x; this.y |]
            
        member this.xy 
            with get() =
                byte2(this.x, this.y)
            and set (v: byte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.yx 
            with get() =
                byte2(this.y, this.x)
            and set (v: byte2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.xx 
            with get() =
                byte2(this.x, this.x)
            
        member this.yy 
            with get() =
                byte2(this.y, this.y)

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
            byte2(c.[0], c.[1])

        static member (+) (f1: byte2, f2: byte2) =
            byte2(Array.map2 (+) (f1.Components) (f2.Components))
        static member (+) (f1: byte, f2: byte2) =
            byte2(Array.map2 (+) ([| f1;f1 |]) (f2.Components))
        static member (+) (f1: byte2, f2: byte) =
            byte2(Array.map2 (+) (f1.Components) ([| f2;f2 |]))
        static member (-) (f1: byte2, f2: byte2) =
            byte2(Array.map2 (-) (f1.Components) (f2.Components))
        static member (-) (f1: byte, f2: byte2) =
            byte2(Array.map2 (-) ([| f1;f1 |]) (f2.Components))
        static member (-) (f1: byte2, f2: byte) =
            byte2(Array.map2 (-) (f1.Components) ([| f2;f2 |]))
        static member (*) (f1: byte2, f2: byte2) =
            byte2(Array.map2 (*) (f1.Components) (f2.Components))
        static member (*) (f1: byte, f2: byte2) =
            byte2(Array.map2 (*) ([| f1;f1;|]) (f2.Components))
        static member (*) (f1: byte2, f2: byte) =
            byte2(Array.map2 (*) ([| f2;f2 |]) (f1.Components))
        static member (/) (f1: byte2, f2: byte2) =
            byte2(Array.map2 (/) (f1.Components) (f2.Components))
        static member (/) (f1: byte, f2: byte2) =
            byte2(Array.map2 (/) ([| f1;f1 |]) (f2.Components))
        static member (/) (f1: byte2, f2: byte) =
            byte2(Array.map2 (/) (f1.Components) ([| f2;f2 |]))
        
        static member (>>=) (f1: byte2, f2: byte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: byte2, f2: byte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: byte2, f2: byte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: byte2, f2: byte2) =
            sbyte2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))

        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> byte2
            stream.Close()
            data

    end
    
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]      
[<VectorType>]         
type byte3 =
    struct
        val mutable x: byte
        val mutable y: byte
        val mutable z: byte

        member internal this.Components
            with get() =
                [| this.x; this.y; this.z |]
                
        new(X: byte, Y: byte, Z: byte) =
            { x = X; y = Y; z = Z }
            
        new(v: byte) =
            { x = v; y = v; z = v }

        member this.xy 
            with get() =
                byte2(this.x, this.y)
            and set (v: byte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                byte2(this.x, this.z)
            and set (v: byte2) =
                this.x <- v.x
                this.z <- v.y

        member this.yx 
            with get() =
                byte2(this.y, this.x)
            and set (v: byte2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                byte2(this.y, this.z)
            and set (v: byte2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.zx 
            with get() =
                byte2(this.z, this.x)
            and set (v: byte2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                byte2(this.z, this.y)
            and set (v: byte2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.xx 
            with get() =
                byte2(this.x, this.x)
            
        member this.yy 
            with get() =
                byte2(this.y, this.y)
            
        member this.zz 
            with get() =
                byte2(this.z, this.z)

        // 3-comps    
        member this.xyz 
            with get() =
                byte3(this.x, this.y, this.z)
            and set (v: byte3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                byte3(this.x, this.z, this.y)
            and set (v: byte3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.yxz 
            with get() =
                byte3(this.y, this.x, this.z)
            and set (v: byte3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                byte3(this.y, this.z, this.x)
            and set (v: byte3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.zxy 
            with get() =
                byte3(this.z, this.x, this.y)
            and set (v: byte3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                byte3(this.z, this.y, this.x)
            and set (v: byte3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.xxy 
            with get() =
                byte3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                byte3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                byte3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                byte3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                byte3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                byte3(this.z, this.x, this.x)
                        
        member this.yyx 
            with get() =
                byte3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                byte3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                byte3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                byte3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                byte3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                byte3(this.z, this.y, this.y)
            
        member this.zzx 
            with get() =
                byte3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                byte3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                byte3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                byte3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                byte3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                byte3(this.y, this.z, this.z)
            
        member this.xxx 
            with get() =
                byte3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                byte3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                byte3(this.z, this.z, this.z)

        member this.lo 
            with get() =
                byte2(this.x, this.y)
            and set (v:byte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                byte2(this.y, 0uy)
            and set (v:byte2) =
                this.z <- v.x
            
        member this.even 
            with get() =
                byte2(this.x, this.z)
            and set (v:byte2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                byte2(this.y, 0uy)
            and set (v:byte2) =
                this.y <- v.x

        internal new(c: byte[]) =
            byte3(c.[0], c.[1], c.[2])

        static member (+) (f1: byte3, f2: byte3) =
            byte3(Array.map2 (+) (f1.Components) (f2.Components))
        static member (+) (f1: byte, f2: byte3) =
            byte3(Array.map2 (+) ([| f1;f1;f1 |]) (f2.Components))
        static member (+) (f1: byte3, f2: byte) =
            byte3(Array.map2 (+) (f1.Components) ([| f2;f2;f2 |]))
        static member (-) (f1: byte3, f2: byte3) =
            byte3(Array.map2 (-) (f1.Components) (f2.Components))
        static member (-) (f1: byte, f2: byte3) =
            byte3(Array.map2 (-) ([| f1;f1;f1 |]) (f2.Components))
        static member (-) (f1: byte3, f2: byte) =
            byte3(Array.map2 (-) (f1.Components) ([| f2;f2;f2 |]))
        static member (*) (f1: byte3, f2: byte3) =
            byte3(Array.map2 (*) (f1.Components) (f2.Components))
        static member (*) (f1: byte, f2: byte3) =
            byte3(Array.map2 (*) ([| f1;f1;f1 |]) (f2.Components))
        static member (*) (f1: byte3, f2: byte) =
            byte3(Array.map2 (*) ([| f2;f2;f2 |]) (f1.Components))
        static member (/) (f1: byte3, f2: byte3) =
            byte3(Array.map2 (/) (f1.Components) (f2.Components))
        static member (/) (f1: byte, f2: byte3) =
            byte3(Array.map2 (/) ([| f1;f1;f1 |]) (f2.Components))
        static member (/) (f1: byte3, f2: byte) =
            byte3(Array.map2 (/) (f1.Components) ([| f2;f2;f2 |]))
        
        static member (>>=) (f1: byte3, f2: byte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: byte3, f2: byte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: byte3, f2: byte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: byte3, f2: byte3) =
            sbyte3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> byte3
            stream.Close()
            data
    end

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]    
[<VectorType>]        
type byte4 =
    struct
        val mutable x: byte
        val mutable y: byte
        val mutable z: byte
        val mutable w: byte

        member internal this.Components
            with get() =
                [| this.x; this.y; this.z; this.w |]
                
        new(X: byte, Y: byte, Z: byte, W: byte) =
            { x = X; y = Y; z = Z; w = W }
            
        new(v: byte) =
            { x = v; y = v; z = v; w = v }

        member this.xy 
            with get() =
                byte2(this.x, this.y)
            and set (v: byte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                byte2(this.x, this.z)
            and set (v: byte2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.xw 
            with get() =
                byte2(this.x, this.w)
            and set (v: byte2) =
                this.x <- v.x
                this.w <- v.y

        member this.yx 
            with get() =
                byte2(this.y, this.x)
            and set (v: byte2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                byte2(this.y, this.z)
            and set (v: byte2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.yw 
            with get() =
                byte2(this.y, this.w)
            and set (v: byte2) =
                this.y <- v.x
                this.w <- v.y

        member this.zx 
            with get() =
                byte2(this.z, this.x)
            and set (v: byte2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                byte2(this.z, this.y)
            and set (v: byte2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.zw 
            with get() =
                byte2(this.z, this.w)
            and set (v: byte2) =
                this.z <- v.x
                this.w <- v.y
                          
        member this.wx 
            with get() =
                byte2(this.w, this.x)
            and set (v: byte2) =
                this.w <- v.x
                this.x <- v.y
            
        member this.wy 
            with get() =
                byte2(this.w, this.y)
            and set (v: byte2) =
                this.w <- v.x
                this.y <- v.y
            
        member this.wz 
            with get() =
                byte2(this.w, this.z)
            and set (v: byte2) =
                this.w <- v.x
                this.z <- v.y

        member this.xx 
            with get() =
                byte2(this.x, this.x)
            
        member this.yy 
            with get() =
                byte2(this.y, this.y)
            
        member this.zz 
            with get() =
                byte2(this.z, this.z)
            
        member this.ww 
            with get() =
                byte2(this.w, this.w)

        // 3-comps    
        member this.xyz 
            with get() =
                byte3(this.x, this.y, this.z)
            and set (v: byte3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                byte3(this.x, this.z, this.y)
            and set (v: byte3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.xyw
            with get() =
                byte3(this.x, this.y, this.w)
            and set (v: byte3) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.xwy
            with get() =
                byte3(this.x, this.w, this.y)
            and set (v: byte3) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
            
        member this.xzw
            with get() =
                byte3(this.x, this.z, this.w)
            and set (v: byte3) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.xwz
            with get() =
                byte3(this.x, this.w, this.z)
            and set (v: byte3) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.yxz 
            with get() =
                byte3(this.y, this.x, this.z)
            and set (v: byte3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                byte3(this.y, this.z, this.x)
            and set (v: byte3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.yxw 
            with get() =
                byte3(this.y, this.x, this.w)
            and set (v: byte3) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.ywx 
            with get() =
                byte3(this.y, this.w, this.x)
            and set (v: byte3) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.yzw 
            with get() =
                byte3(this.y, this.z, this.w)
            and set (v: byte3) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.ywz 
            with get() =
                byte3(this.y, this.w, this.z)
            and set (v: byte3) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.zxy 
            with get() =
                byte3(this.z, this.x, this.y)
            and set (v: byte3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                byte3(this.z, this.y, this.x)
            and set (v: byte3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.zxw 
            with get() =
                byte3(this.z, this.x, this.w)
            and set (v: byte3) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.zwx 
            with get() =
                byte3(this.z, this.w, this.x)
            and set (v: byte3) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.zyw 
            with get() =
                byte3(this.z, this.y, this.w)
            and set (v: byte3) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.zwy 
            with get() =
                byte3(this.z, this.w, this.y)
            and set (v: byte3) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                        
        member this.wxy 
            with get() =
                byte3(this.w, this.x, this.y)
            and set (v: byte3) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.wyx 
            with get() =
                byte3(this.w, this.y, this.x)
            and set (v: byte3) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.wxz 
            with get() =
                byte3(this.w, this.x, this.z)
            and set (v: byte3) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.wzx 
            with get() =
                byte3(this.w, this.z, this.x)
            and set (v: byte3) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.wyz 
            with get() =
                byte3(this.w, this.y, this.z)
            and set (v: byte3) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.wzy 
            with get() =
                byte3(this.w, this.z, this.y)
            and set (v: byte3) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z

        member this.xxy 
            with get() =
                byte3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                byte3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                byte3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                byte3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                byte3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                byte3(this.z, this.x, this.x)
                    
        member this.xxw 
            with get() =
                byte3(this.x, this.x, this.w)
            
        member this.xwx 
            with get() =
                byte3(this.x, this.w, this.x)

        member this.wxx 
            with get() =
                byte3(this.w, this.x, this.x)
                            
        member this.yyx 
            with get() =
                byte3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                byte3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                byte3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                byte3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                byte3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                byte3(this.z, this.y, this.y)
            
        member this.yyw 
            with get() =
                byte3(this.y, this.y, this.w)
            
        member this.ywy 
            with get() =
                byte3(this.y, this.w, this.y)

        member this.wyy
            with get() =
                byte3(this.w, this.y, this.y)

        member this.zzx 
            with get() =
                byte3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                byte3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                byte3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                byte3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                byte3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                byte3(this.y, this.z, this.z)
            
        member this.zzw 
            with get() =
                byte3(this.z, this.z, this.w)
            
        member this.zwz 
            with get() =
                byte3(this.z, this.w, this.z)
            
        member this.wzz 
            with get() =
                byte3(this.w, this.z, this.z)
            
        member this.xxx 
            with get() =
                byte3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                byte3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                byte3(this.z, this.z, this.z)
 
        member this.www 
            with get() =
                byte3(this.w, this.w, this.w)
     
        // 4-comps       
        member this.xxxx
            with get() =
                byte4(this.x, this.x, this.x, this.x)

        member this.xxxy
            with get() =
                byte4(this.x, this.x, this.x, this.y)

        member this.xxxz
            with get() =
                byte4(this.x, this.x, this.x, this.z)

        member this.xxxw
            with get() =
                byte4(this.x, this.x, this.x, this.w)

        member this.xxyx
            with get() =
                byte4(this.x, this.x, this.y, this.x)

        member this.xxyy
            with get() =
                byte4(this.x, this.x, this.y, this.y)

        member this.xxyz
            with get() =
                byte4(this.x, this.x, this.y, this.z)

        member this.xxyw
            with get() =
                byte4(this.x, this.x, this.y, this.w)

        member this.xxzx
            with get() =
                byte4(this.x, this.x, this.z, this.x)

        member this.xxzy
            with get() =
                byte4(this.x, this.x, this.z, this.y)

        member this.xxzz
            with get() =
                byte4(this.x, this.x, this.z, this.z)

        member this.xxzw
            with get() =
                byte4(this.x, this.x, this.z, this.w)

        member this.xxwx
            with get() =
                byte4(this.x, this.x, this.w, this.x)

        member this.xxwy
            with get() =
                byte4(this.x, this.x, this.w, this.y)

        member this.xxwz
            with get() =
                byte4(this.x, this.x, this.w, this.z)

        member this.xxww
            with get() =
                byte4(this.x, this.x, this.w, this.w)

        member this.xyxx
            with get() =
                byte4(this.x, this.y, this.x, this.x)

        member this.xyxy
            with get() =
                byte4(this.x, this.y, this.x, this.y)

        member this.xyxz
            with get() =
                byte4(this.x, this.y, this.x, this.z)

        member this.xyxw
            with get() =
                byte4(this.x, this.y, this.x, this.w)

        member this.xyyx
            with get() =
                byte4(this.x, this.y, this.y, this.x)

        member this.xyyy
            with get() =
                byte4(this.x, this.y, this.y, this.y)

        member this.xyyz
            with get() =
                byte4(this.x, this.y, this.y, this.z)

        member this.xyyw
            with get() =
                byte4(this.x, this.y, this.y, this.w)

        member this.xyzx
            with get() =
                byte4(this.x, this.y, this.z, this.x)

        member this.xyzy
            with get() =
                byte4(this.x, this.y, this.z, this.y)

        member this.xyzz
            with get() =
                byte4(this.x, this.y, this.z, this.z)

        member this.xyzw
            with get() =
                byte4(this.x, this.y, this.z, this.w)
            and set(v: byte4) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
                this.w <- v.w
            
        member this.xywx
            with get() =
                byte4(this.x, this.y, this.w, this.x)

        member this.xywy
            with get() =
                byte4(this.x, this.y, this.w, this.y)

        member this.xywz
            with get() =
                byte4(this.x, this.y, this.w, this.z)
            and set(v: byte4) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.xyww
            with get() =
                byte4(this.x, this.y, this.w, this.w)

        member this.xzxx
            with get() =
                byte4(this.x, this.z, this.x, this.x)

        member this.xzxy
            with get() =
                byte4(this.x, this.z, this.x, this.y)

        member this.xzxz
            with get() =
                byte4(this.x, this.z, this.x, this.z)

        member this.xzxw
            with get() =
                byte4(this.x, this.z, this.x, this.w)

        member this.xzyx
            with get() =
                byte4(this.x, this.z, this.y, this.x)

        member this.xzyy
            with get() =
                byte4(this.x, this.z, this.y, this.y)

        member this.xzyz
            with get() =
                byte4(this.x, this.z, this.y, this.z)

        member this.xzyw
            with get() =
                byte4(this.x, this.z, this.y, this.w)
            and set(v: byte4) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.xzzx
            with get() =
                byte4(this.x, this.z, this.z, this.x)

        member this.xzzy
            with get() =
                byte4(this.x, this.z, this.z, this.y)

        member this.xzzz
            with get() =
                byte4(this.x, this.z, this.z, this.z)

        member this.xzzw
            with get() =
                byte4(this.x, this.z, this.z, this.w)

        member this.xzwx
            with get() =
                byte4(this.x, this.z, this.w, this.x)

        member this.xzwy
            with get() =
                byte4(this.x, this.z, this.w, this.y)
            and set(v: byte4) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.xzwz
            with get() =
                byte4(this.x, this.z, this.w, this.z)

        member this.xzww
            with get() =
                byte4(this.x, this.z, this.w, this.w)

        member this.xwxx
            with get() =
                byte4(this.x, this.w, this.x, this.x)

        member this.xwxy
            with get() =
                byte4(this.x, this.w, this.x, this.y)

        member this.xwxz
            with get() =
                byte4(this.x, this.w, this.x, this.z)

        member this.xwxw
            with get() =
                byte4(this.x, this.w, this.x, this.w)

        member this.xwyx
            with get() =
                byte4(this.x, this.w, this.y, this.x)

        member this.xwyy
            with get() =
                byte4(this.x, this.w, this.y, this.y)

        member this.xwyz
            with get() =
                byte4(this.x, this.w, this.y, this.z)
            and set(v: byte4) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.xwyw
            with get() =
                byte4(this.x, this.w, this.y, this.w)

        member this.xwzx
            with get() =
                byte4(this.x, this.w, this.z, this.x)

        member this.xwzy
            with get() =
                byte4(this.x, this.w, this.z, this.y)
            and set(v: byte4) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z
                this.y <- v.w
            
        member this.xwzz
            with get() =
                byte4(this.x, this.w, this.z, this.z)

        member this.xwzw
            with get() =
                byte4(this.x, this.w, this.z, this.w)

        member this.xwwx
            with get() =
                byte4(this.x, this.w, this.w, this.x)

        member this.xwwy
            with get() =
                byte4(this.x, this.w, this.w, this.y)

        member this.xwwz
            with get() =
                byte4(this.x, this.w, this.w, this.z)

        member this.xwww
            with get() =
                byte4(this.x, this.w, this.w, this.w)

        member this.yxxx
            with get() =
                byte4(this.y, this.x, this.x, this.x)

        member this.yxxy
            with get() =
                byte4(this.y, this.x, this.x, this.y)

        member this.yxxz
            with get() =
                byte4(this.y, this.x, this.x, this.z)

        member this.yxxw
            with get() =
                byte4(this.y, this.x, this.x, this.w)

        member this.yxyx
            with get() =
                byte4(this.y, this.x, this.y, this.x)

        member this.yxyy
            with get() =
                byte4(this.y, this.x, this.y, this.y)

        member this.yxyz
            with get() =
                byte4(this.y, this.x, this.y, this.z)

        member this.yxyw
            with get() =
                byte4(this.y, this.x, this.y, this.w)

        member this.yxzx
            with get() =
                byte4(this.y, this.x, this.z, this.x)

        member this.yxzy
            with get() =
                byte4(this.y, this.x, this.z, this.y)

        member this.yxzz
            with get() =
                byte4(this.y, this.x, this.z, this.z)

        member this.yxzw
            with get() =
                byte4(this.y, this.x, this.z, this.w)
            and set(v: byte4) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
                this.w <- v.w


        member this.yxwx
            with get() =
                byte4(this.y, this.x, this.w, this.x)

        member this.yxwy
            with get() =
                byte4(this.y, this.x, this.w, this.y)

        member this.yxwz
            with get() =
                byte4(this.y, this.x, this.w, this.z)
            and set(v: byte4) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.yxww
            with get() =
                byte4(this.y, this.x, this.w, this.w)

        member this.yyxx
            with get() =
                byte4(this.y, this.y, this.x, this.x)

        member this.yyxy
            with get() =
                byte4(this.y, this.y, this.x, this.y)

        member this.yyxz
            with get() =
                byte4(this.y, this.y, this.x, this.z)

        member this.yyxw
            with get() =
                byte4(this.y, this.y, this.x, this.w)

        member this.yyyx
            with get() =
                byte4(this.y, this.y, this.y, this.x)

        member this.yyyy
            with get() =
                byte4(this.y, this.y, this.y, this.y)

        member this.yyyz
            with get() =
                byte4(this.y, this.y, this.y, this.z)

        member this.yyyw
            with get() =
                byte4(this.y, this.y, this.y, this.w)

        member this.yyzx
            with get() =
                byte4(this.y, this.y, this.z, this.x)

        member this.yyzy
            with get() =
                byte4(this.y, this.y, this.z, this.y)

        member this.yyzz
            with get() =
                byte4(this.y, this.y, this.z, this.z)

        member this.yyzw
            with get() =
                byte4(this.y, this.y, this.z, this.w)

        member this.yywx
            with get() =
                byte4(this.y, this.y, this.w, this.x)

        member this.yywy
            with get() =
                byte4(this.y, this.y, this.w, this.y)

        member this.yywz
            with get() =
                byte4(this.y, this.y, this.w, this.z)

        member this.yyww
            with get() =
                byte4(this.y, this.y, this.w, this.w)

        member this.yzxx
            with get() =
                byte4(this.y, this.z, this.x, this.x)

        member this.yzxy
            with get() =
                byte4(this.y, this.z, this.x, this.y)

        member this.yzxz
            with get() =
                byte4(this.y, this.z, this.x, this.z)

        member this.yzxw
            with get() =
                byte4(this.y, this.z, this.x, this.w)
            and set(v: byte4) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.yzyx
            with get() =
                byte4(this.y, this.z, this.y, this.x)

        member this.yzyy
            with get() =
                byte4(this.y, this.z, this.y, this.y)

        member this.yzyz
            with get() =
                byte4(this.y, this.z, this.y, this.z)

        member this.yzyw
            with get() =
                byte4(this.y, this.z, this.y, this.w)

        member this.yzzx
            with get() =
                byte4(this.y, this.z, this.z, this.x)

        member this.yzzy
            with get() =
                byte4(this.y, this.z, this.z, this.y)

        member this.yzzz
            with get() =
                byte4(this.y, this.z, this.z, this.z)

        member this.yzzw
            with get() =
                byte4(this.y, this.z, this.z, this.w)

        member this.yzwx
            with get() =
                byte4(this.y, this.z, this.w, this.x)
            and set(v: byte4) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.yzwy
            with get() =
                byte4(this.y, this.z, this.w, this.y)

        member this.yzwz
            with get() =
                byte4(this.y, this.z, this.w, this.z)

        member this.yzww
            with get() =
                byte4(this.y, this.z, this.w, this.w)

        member this.ywxx
            with get() =
                byte4(this.y, this.w, this.x, this.x)

        member this.ywxy
            with get() =
                byte4(this.y, this.w, this.x, this.y)

        member this.ywxz
            with get() =
                byte4(this.y, this.w, this.x, this.z)
            and set(v: byte4) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
                this.z <- v.w


        member this.ywxw
            with get() =
                byte4(this.y, this.w, this.x, this.w)

        member this.ywyx
            with get() =
                byte4(this.y, this.w, this.y, this.x)

        member this.ywyy
            with get() =
                byte4(this.y, this.w, this.y, this.y)

        member this.ywyz
            with get() =
                byte4(this.y, this.w, this.y, this.z)

        member this.ywyw
            with get() =
                byte4(this.y, this.w, this.y, this.w)

        member this.ywzx
            with get() =
                byte4(this.y, this.w, this.z, this.x)
            and set(v: byte4) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.ywzy
            with get() =
                byte4(this.y, this.w, this.z, this.y)

        member this.ywzz
            with get() =
                byte4(this.y, this.w, this.z, this.z)

        member this.ywzw
            with get() =
                byte4(this.y, this.w, this.z, this.w)

        member this.ywwx
            with get() =
                byte4(this.y, this.w, this.w, this.x)

        member this.ywwy
            with get() =
                byte4(this.y, this.w, this.w, this.y)

        member this.ywwz
            with get() =
                byte4(this.y, this.w, this.w, this.z)

        member this.ywww
            with get() =
                byte4(this.y, this.w, this.w, this.w)

        member this.zxxx
            with get() =
                byte4(this.z, this.x, this.x, this.x)

        member this.zxxy
            with get() =
                byte4(this.z, this.x, this.x, this.y)

        member this.zxxz
            with get() =
                byte4(this.z, this.x, this.x, this.z)

        member this.zxxw
            with get() =
                byte4(this.z, this.x, this.x, this.w)

        member this.zxyx
            with get() =
                byte4(this.z, this.x, this.y, this.x)

        member this.zxyy
            with get() =
                byte4(this.z, this.x, this.y, this.y)

        member this.zxyz
            with get() =
                byte4(this.z, this.x, this.y, this.z)

        member this.zxyw
            with get() =
                byte4(this.z, this.x, this.y, this.w)
            and set(v: byte4) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.zxzx
            with get() =
                byte4(this.z, this.x, this.z, this.x)

        member this.zxzy
            with get() =
                byte4(this.z, this.x, this.z, this.y)

        member this.zxzz
            with get() =
                byte4(this.z, this.x, this.z, this.z)

        member this.zxzw
            with get() =
                byte4(this.z, this.x, this.z, this.w)

        member this.zxwx
            with get() =
                byte4(this.z, this.x, this.w, this.x)

        member this.zxwy
            with get() =
                byte4(this.z, this.x, this.w, this.y)
            and set(v: byte4) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.zxwz
            with get() =
                byte4(this.z, this.x, this.w, this.z)

        member this.zxww
            with get() =
                byte4(this.z, this.x, this.w, this.w)

        member this.zyxx
            with get() =
                byte4(this.z, this.y, this.x, this.x)

        member this.zyxy
            with get() =
                byte4(this.z, this.y, this.x, this.y)

        member this.zyxz
            with get() =
                byte4(this.z, this.y, this.x, this.z)

        member this.zyxw
            with get() =
                byte4(this.z, this.y, this.x, this.w)
            and set(v: byte4) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.zyyx
            with get() =
                byte4(this.z, this.y, this.y, this.x)

        member this.zyyy
            with get() =
                byte4(this.z, this.y, this.y, this.y)

        member this.zyyz
            with get() =
                byte4(this.z, this.y, this.y, this.z)

        member this.zyyw
            with get() =
                byte4(this.z, this.y, this.y, this.w)

        member this.zyzx
            with get() =
                byte4(this.z, this.y, this.z, this.x)

        member this.zyzy
            with get() =
                byte4(this.z, this.y, this.z, this.y)

        member this.zyzz
            with get() =
                byte4(this.z, this.y, this.z, this.z)

        member this.zyzw
            with get() =
                byte4(this.z, this.y, this.z, this.w)

        member this.zywx
            with get() =
                byte4(this.z, this.y, this.w, this.x)
            and set(v: byte4) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.zywy
            with get() =
                byte4(this.z, this.y, this.w, this.y)

        member this.zywz
            with get() =
                byte4(this.z, this.y, this.w, this.z)

        member this.zyww
            with get() =
                byte4(this.z, this.y, this.w, this.w)

        member this.zzxx
            with get() =
                byte4(this.z, this.z, this.x, this.x)

        member this.zzxy
            with get() =
                byte4(this.z, this.z, this.x, this.y)

        member this.zzxz
            with get() =
                byte4(this.z, this.z, this.x, this.z)

        member this.zzxw
            with get() =
                byte4(this.z, this.z, this.x, this.w)

        member this.zzyx
            with get() =
                byte4(this.z, this.z, this.y, this.x)

        member this.zzyy
            with get() =
                byte4(this.z, this.z, this.y, this.y)

        member this.zzyz
            with get() =
                byte4(this.z, this.z, this.y, this.z)

        member this.zzyw
            with get() =
                byte4(this.z, this.z, this.y, this.w)

        member this.zzzx
            with get() =
                byte4(this.z, this.z, this.z, this.x)

        member this.zzzy
            with get() =
                byte4(this.z, this.z, this.z, this.y)

        member this.zzzz
            with get() =
                byte4(this.z, this.z, this.z, this.z)

        member this.zzzw
            with get() =
                byte4(this.z, this.z, this.z, this.w)

        member this.zzwx
            with get() =
                byte4(this.z, this.z, this.w, this.x)

        member this.zzwy
            with get() =
                byte4(this.z, this.z, this.w, this.y)

        member this.zzwz
            with get() =
                byte4(this.z, this.z, this.w, this.z)

        member this.zzww
            with get() =
                byte4(this.z, this.z, this.w, this.w)

        member this.zwxx
            with get() =
                byte4(this.z, this.w, this.x, this.x)

        member this.zwxy
            with get() =
                byte4(this.z, this.w, this.x, this.y)
            and set(v: byte4) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.zwxz
            with get() =
                byte4(this.z, this.w, this.x, this.z)

        member this.zwxw
            with get() =
                byte4(this.z, this.w, this.x, this.w)

        member this.zwyx
            with get() =
                byte4(this.z, this.w, this.y, this.x)
            and set(v: byte4) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.zwyy
            with get() =
                byte4(this.z, this.w, this.y, this.y)

        member this.zwyz
            with get() =
                byte4(this.z, this.w, this.y, this.z)

        member this.zwyw
            with get() =
                byte4(this.z, this.w, this.y, this.w)

        member this.zwzx
            with get() =
                byte4(this.z, this.w, this.z, this.x)

        member this.zwzy
            with get() =
                byte4(this.z, this.w, this.z, this.y)

        member this.zwzz
            with get() =
                byte4(this.z, this.w, this.z, this.z)

        member this.zwzw
            with get() =
                byte4(this.z, this.w, this.z, this.w)

        member this.zwwx
            with get() =
                byte4(this.z, this.w, this.w, this.x)

        member this.zwwy
            with get() =
                byte4(this.z, this.w, this.w, this.y)

        member this.zwwz
            with get() =
                byte4(this.z, this.w, this.w, this.z)

        member this.zwww
            with get() =
                byte4(this.z, this.w, this.w, this.w)

        member this.wxxx
            with get() =
                byte4(this.w, this.x, this.x, this.x)

        member this.wxxy
            with get() =
                byte4(this.w, this.x, this.x, this.y)

        member this.wxxz
            with get() =
                byte4(this.w, this.x, this.x, this.z)

        member this.wxxw
            with get() =
                byte4(this.w, this.x, this.x, this.w)

        member this.wxyx
            with get() =
                byte4(this.w, this.x, this.y, this.x)

        member this.wxyy
            with get() =
                byte4(this.w, this.x, this.y, this.y)

        member this.wxyz
            with get() =
                byte4(this.w, this.x, this.y, this.z)
            and set(v: byte4) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.wxyw
            with get() =
                byte4(this.w, this.x, this.y, this.w)

        member this.wxzx
            with get() =
                byte4(this.w, this.x, this.z, this.x)

        member this.wxzy
            with get() =
                byte4(this.w, this.x, this.z, this.y)
            and set(v: byte4) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
                this.y <- v.w


        member this.wxzz
            with get() =
                byte4(this.w, this.x, this.z, this.z)

        member this.wxzw
            with get() =
                byte4(this.w, this.x, this.z, this.w)

        member this.wxwx
            with get() =
                byte4(this.w, this.x, this.w, this.x)

        member this.wxwy
            with get() =
                byte4(this.w, this.x, this.w, this.y)

        member this.wxwz
            with get() =
                byte4(this.w, this.x, this.w, this.z)

        member this.wxww
            with get() =
                byte4(this.w, this.x, this.w, this.w)

        member this.wyxx
            with get() =
                byte4(this.w, this.y, this.x, this.x)

        member this.wyxy
            with get() =
                byte4(this.w, this.y, this.x, this.y)

        member this.wyxz
            with get() =
                byte4(this.w, this.y, this.x, this.z)
            and set(v: byte4) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
                this.z <- v.w
            
        member this.wyxw
            with get() =
                byte4(this.w, this.y, this.x, this.w)

        member this.wyyx
            with get() =
                byte4(this.w, this.y, this.y, this.x)

        member this.wyyy
            with get() =
                byte4(this.w, this.y, this.y, this.y)

        member this.wyyz
            with get() =
                byte4(this.w, this.y, this.y, this.z)

        member this.wyyw
            with get() =
                byte4(this.w, this.y, this.y, this.w)

        member this.wyzx
            with get() =
                byte4(this.w, this.y, this.z, this.x)
            and set(v: byte4) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.wyzy
            with get() =
                byte4(this.w, this.y, this.z, this.y)

        member this.wyzz
            with get() =
                byte4(this.w, this.y, this.z, this.z)

        member this.wyzw
            with get() =
                byte4(this.w, this.y, this.z, this.w)

        member this.wywx
            with get() =
                byte4(this.w, this.y, this.w, this.x)

        member this.wywy
            with get() =
                byte4(this.w, this.y, this.w, this.y)

        member this.wywz
            with get() =
                byte4(this.w, this.y, this.w, this.z)

        member this.wyww
            with get() =
                byte4(this.w, this.y, this.w, this.w)

        member this.wzxx
            with get() =
                byte4(this.w, this.z, this.x, this.x)

        member this.wzxy
            with get() =
                byte4(this.w, this.z, this.x, this.y)
            and set(v: byte4) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.wzxz
            with get() =
                byte4(this.w, this.z, this.x, this.z)

        member this.wzxw
            with get() =
                byte4(this.w, this.z, this.x, this.w)

        member this.wzyx
            with get() =
                byte4(this.w, this.z, this.y, this.x)
            and set(v: byte4) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.wzyy
            with get() =
                byte4(this.w, this.z, this.y, this.y)

        member this.wzyz
            with get() =
                byte4(this.w, this.z, this.y, this.z)

        member this.wzyw
            with get() =
                byte4(this.w, this.z, this.y, this.w)

        member this.wzzx
            with get() =
                byte4(this.w, this.z, this.z, this.x)

        member this.wzzy
            with get() =
                byte4(this.w, this.z, this.z, this.y)

        member this.wzzz
            with get() =
                byte4(this.w, this.z, this.z, this.z)

        member this.wzzw
            with get() =
                byte4(this.w, this.z, this.z, this.w)

        member this.wzwx
            with get() =
                byte4(this.w, this.z, this.w, this.x)

        member this.wzwy
            with get() =
                byte4(this.w, this.z, this.w, this.y)

        member this.wzwz
            with get() =
                byte4(this.w, this.z, this.w, this.z)

        member this.wzww
            with get() =
                byte4(this.w, this.z, this.w, this.w)

        member this.wwxx
            with get() =
                byte4(this.w, this.w, this.x, this.x)

        member this.wwxy
            with get() =
                byte4(this.w, this.w, this.x, this.y)

        member this.wwxz
            with get() =
                byte4(this.w, this.w, this.x, this.z)

        member this.wwxw
            with get() =
                byte4(this.w, this.w, this.x, this.w)

        member this.wwyx
            with get() =
                byte4(this.w, this.w, this.y, this.x)

        member this.wwyy
            with get() =
                byte4(this.w, this.w, this.y, this.y)

        member this.wwyz
            with get() =
                byte4(this.w, this.w, this.y, this.z)

        member this.wwyw
            with get() =
                byte4(this.w, this.w, this.y, this.w)

        member this.wwzx
            with get() =
                byte4(this.w, this.w, this.z, this.x)

        member this.wwzy
            with get() =
                byte4(this.w, this.w, this.z, this.y)

        member this.wwzz
            with get() =
                byte4(this.w, this.w, this.z, this.z)

        member this.wwzw
            with get() =
                byte4(this.w, this.w, this.z, this.w)

        member this.wwwx
            with get() =
                byte4(this.w, this.w, this.w, this.x)

        member this.wwwy
            with get() =
                byte4(this.w, this.w, this.w, this.y)

        member this.wwwz
            with get() =
                byte4(this.w, this.w, this.w, this.z)

        member this.wwww
            with get() =
                byte4(this.w, this.w, this.w, this.w)

        member this.lo 
            with get() =
                byte2(this.x, this.y)
            and set (v:byte2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                byte2(this.y, this.w)
            and set (v:byte2) =
                this.z <- v.x
                this.w <- v.y
            
        member this.even 
            with get() =
                byte2(this.x, this.z)
            and set (v:byte2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                byte2(this.y, this.w)
            and set (v:byte2) =
                this.y <- v.x
                this.w <- v.y

        internal new(c: byte[]) =
            byte4(c.[0], c.[1], c.[2], c.[3])

        static member (+) (f1: byte4, f2: byte4) =
            byte4(Array.map2 (+) (f1.Components) (f2.Components))
        static member (+) (f1: byte, f2: byte4) =
            byte4(Array.map2 (+) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (+) (f1: byte4, f2: byte) =
            byte4(Array.map2 (+) (f1.Components) ([| f2;f2;f2;f2 |]))
        static member (-) (f1: byte4, f2: byte4) =
            byte4(Array.map2 (-) (f1.Components) (f2.Components))
        static member (-) (f1: byte, f2: byte4) =
            byte4(Array.map2 (-) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (-) (f1: byte4, f2: byte) =
            byte4(Array.map2 (-) (f1.Components) ([| f2;f2;f2;f2 |]))
        static member (*) (f1: byte4, f2: byte4) =
            byte4(Array.map2 (*) (f1.Components) (f2.Components))
        static member (*) (f1: byte, f2: byte4) =
            byte4(Array.map2 (*) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (*) (f1: byte4, f2: byte) =
            byte4(Array.map2 (*) ([| f2;f2;f2;f2 |]) (f1.Components))
        static member (/) (f1: byte4, f2: byte4) =
            byte4(Array.map2 (/) (f1.Components) (f2.Components))
        static member (/) (f1: byte, f2: byte4) =
            byte4(Array.map2 (/) ([| f1;f1;f1;f1 |]) (f2.Components))
        static member (/) (f1: byte4, f2: byte) =
            byte4(Array.map2 (/) (f1.Components) ([| f2;f2;f2;f2 |]))
        
        static member (>>=) (f1: byte4, f2: byte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<<=) (f1: byte4, f2: byte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (===) (f1: byte4, f2: byte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.Components) (f2.Components))
        static member (<=>) (f1: byte4, f2: byte4) =
            sbyte4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.Components) (f2.Components))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 4L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> byte4
            stream.Close()
            data          
    end    

// **************************************************************************************************************
