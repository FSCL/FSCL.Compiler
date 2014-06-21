
namespace FSCL
open System.Runtime.InteropServices
open System.IO
open System.Runtime.Serialization
open System.Runtime.Serialization.Formatters
open System

// chareger vector types
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]
[<VectorType>]
type char2 =
    struct 
        val mutable x: char
        val mutable y: char

        member this.Components
            with get() =
                [| this.x; this.y |]

        member this.ByteComponents
            with get() =
                [| this.x |> sbyte; this.y |> sbyte |]
            
        member this.xy 
            with get() =
                char2(this.x, this.y)
            and set (v: char2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.yx 
            with get() =
                char2(this.y, this.x)
            and set (v: char2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.xx 
            with get() =
                char2(this.x, this.x)
            
        member this.yy 
            with get() =
                char2(this.y, this.y)

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

        new(X: char, Y: char) =
            { x = X; y = Y }
            
        new(v: char) =
            { x = v; y = v }

        internal new(c: char[]) =
            char2(c.[0], c.[1])

        internal new(c: sbyte[]) =
            char2(c.[0] |> char, c.[1] |> char)

        static member (+) (f1: char2, f2: char2) =
            char2(Array.map2 (+) (f1.ByteComponents) (f2.ByteComponents))
        static member (-) (f1: char2, f2: char2) =
            char2(Array.map2 (-) (f1.ByteComponents) (f2.ByteComponents))
        static member (*) (f1: char2, f2: char2) =
            char2(Array.map2 (*) (f1.ByteComponents) (f2.ByteComponents))
        static member (/) (f1: char2, f2: char2) =
            char2(Array.map2 (/) (f1.ByteComponents) (f2.ByteComponents))
        
        static member (>>=) (f1: char2, f2: char2) =
            char2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (<<=) (f1: char2, f2: char2) =
            char2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (===) (f1: char2, f2: char2) =
            char2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (<=>) (f1: char2, f2: char2) =
            char2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 2L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> char2
            stream.Close()
            data
    end
    
[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]  
[<VectorType>]             
type char3 =
    struct
        val mutable x: char
        val mutable y: char
        val mutable z: char

        member this.Components
            with get() =
                [| this.x; this.y; this.z |]

        member this.ByteComponents
            with get() =
                [| this.x |> sbyte; this.y |> sbyte |]

        new(X: char, Y: char, Z: char) =
            { x = X; y = Y; z = Z }
            
        new(v: char) =
            { x = v; y = v; z = v }

        member this.xy 
            with get() =
                char2(this.x, this.y)
            and set (v: char2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                char2(this.x, this.z)
            and set (v: char2) =
                this.x <- v.x
                this.z <- v.y

        member this.yx 
            with get() =
                char2(this.y, this.x)
            and set (v: char2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                char2(this.y, this.z)
            and set (v: char2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.zx 
            with get() =
                char2(this.z, this.x)
            and set (v: char2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                char2(this.z, this.y)
            and set (v: char2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.xx 
            with get() =
                char2(this.x, this.x)
            
        member this.yy 
            with get() =
                char2(this.y, this.y)
            
        member this.zz 
            with get() =
                char2(this.z, this.z)

        // 3-comps    
        member this.xyz 
            with get() =
                char3(this.x, this.y, this.z)
            and set (v: char3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                char3(this.x, this.z, this.y)
            and set (v: char3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.yxz 
            with get() =
                char3(this.y, this.x, this.z)
            and set (v: char3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                char3(this.y, this.z, this.x)
            and set (v: char3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.zxy 
            with get() =
                char3(this.z, this.x, this.y)
            and set (v: char3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                char3(this.z, this.y, this.x)
            and set (v: char3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.xxy 
            with get() =
                char3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                char3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                char3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                char3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                char3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                char3(this.z, this.x, this.x)
                        
        member this.yyx 
            with get() =
                char3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                char3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                char3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                char3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                char3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                char3(this.z, this.y, this.y)
            
        member this.zzx 
            with get() =
                char3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                char3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                char3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                char3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                char3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                char3(this.y, this.z, this.z)
            
        member this.xxx 
            with get() =
                char3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                char3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                char3(this.z, this.z, this.z)

        member this.lo 
            with get() =
                char2(this.x, this.y)
            and set (v:char2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                char2(this.y, 0y |> char)
            and set (v:char2) =
                this.z <- v.x
            
        member this.even 
            with get() =
                char2(this.x, this.z)
            and set (v:char2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                char2(this.y, 0y |> char)
            and set (v:char2) =
                this.y <- v.x

        internal new(c: char[]) =
            char3(c.[0], c.[1], c.[2])

        internal new(c: sbyte[]) =
            char3(c.[0] |> char, c.[1] |> char, c.[2] |> char)

        static member (+) (f1: char3, f2: char3) =
            char3(Array.map2 (+) (f1.ByteComponents) (f2.ByteComponents))
        static member (-) (f1: char3, f2: char3) =
            char3(Array.map2 (-) (f1.ByteComponents) (f2.ByteComponents))
        static member (*) (f1: char3, f2: char3) =
            char3(Array.map2 (*) (f1.ByteComponents) (f2.ByteComponents))
        static member (/) (f1: char3, f2: char3) =
            char3(Array.map2 (/) (f1.ByteComponents) (f2.ByteComponents))
        
        static member (>>=) (f1: char3, f2: char3) =
            char3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (<<=) (f1: char3, f2: char3) =
            char3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (===) (f1: char3, f2: char3) =
            char3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (<=>) (f1: char3, f2: char3) =
            char3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 3L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> char3
            stream.Close()
            data
    end

[<Struct>]
[<StructLayout(LayoutKind.Sequential)>]     
[<VectorType>]       
type char4 =
    struct
        val mutable x: char
        val mutable y: char
        val mutable z: char
        val mutable w: char

        member this.Components
            with get() =
                [| this.x; this.y; this.z; this.w |]

        member this.ByteComponents
            with get() =
                [| this.x |> sbyte; this.y |> sbyte; this.z |> sbyte; this.w |> sbyte |]
                
        new(X: char, Y: char, Z: char, W: char) =
            { x = X; y = Y; z = Z; w = W }
            
        new(v: char) =
            { x = v; y = v; z = v; w = v }

        member this.xy 
            with get() =
                char2(this.x, this.y)
            and set (v: char2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.xz 
            with get() =
                char2(this.x, this.z)
            and set (v: char2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.xw 
            with get() =
                char2(this.x, this.w)
            and set (v: char2) =
                this.x <- v.x
                this.w <- v.y

        member this.yx 
            with get() =
                char2(this.y, this.x)
            and set (v: char2) =
                this.x <- v.y
                this.y <- v.x
            
        member this.yz 
            with get() =
                char2(this.y, this.z)
            and set (v: char2) =
                this.y <- v.x
                this.z <- v.y
                        
        member this.yw 
            with get() =
                char2(this.y, this.w)
            and set (v: char2) =
                this.y <- v.x
                this.w <- v.y

        member this.zx 
            with get() =
                char2(this.z, this.x)
            and set (v: char2) =
                this.z <- v.x
                this.x <- v.y

        member this.zy 
            with get() =
                char2(this.z, this.y)
            and set (v: char2) =
                this.z <- v.x
                this.y <- v.y
            
        member this.zw 
            with get() =
                char2(this.z, this.w)
            and set (v: char2) =
                this.z <- v.x
                this.w <- v.y
                          
        member this.wx 
            with get() =
                char2(this.w, this.x)
            and set (v: char2) =
                this.w <- v.x
                this.x <- v.y
            
        member this.wy 
            with get() =
                char2(this.w, this.y)
            and set (v: char2) =
                this.w <- v.x
                this.y <- v.y
            
        member this.wz 
            with get() =
                char2(this.w, this.z)
            and set (v: char2) =
                this.w <- v.x
                this.z <- v.y

        member this.xx 
            with get() =
                char2(this.x, this.x)
            
        member this.yy 
            with get() =
                char2(this.y, this.y)
            
        member this.zz 
            with get() =
                char2(this.z, this.z)
            
        member this.ww 
            with get() =
                char2(this.w, this.w)

        // 3-comps    
        member this.xyz 
            with get() =
                char3(this.x, this.y, this.z)
            and set (v: char3) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.xzy
            with get() =
                char3(this.x, this.z, this.y)
            and set (v: char3) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
            
        member this.xyw
            with get() =
                char3(this.x, this.y, this.w)
            and set (v: char3) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.xwy
            with get() =
                char3(this.x, this.w, this.y)
            and set (v: char3) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
            
        member this.xzw
            with get() =
                char3(this.x, this.z, this.w)
            and set (v: char3) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.xwz
            with get() =
                char3(this.x, this.w, this.z)
            and set (v: char3) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.yxz 
            with get() =
                char3(this.y, this.x, this.z)
            and set (v: char3) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.yzx 
            with get() =
                char3(this.y, this.z, this.x)
            and set (v: char3) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.yxw 
            with get() =
                char3(this.y, this.x, this.w)
            and set (v: char3) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.ywx 
            with get() =
                char3(this.y, this.w, this.x)
            and set (v: char3) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.yzw 
            with get() =
                char3(this.y, this.z, this.w)
            and set (v: char3) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
            
        member this.ywz 
            with get() =
                char3(this.y, this.w, this.z)
            and set (v: char3) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z

        member this.zxy 
            with get() =
                char3(this.z, this.x, this.y)
            and set (v: char3) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.zyx 
            with get() =
                char3(this.z, this.y, this.x)
            and set (v: char3) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.zxw 
            with get() =
                char3(this.z, this.x, this.w)
            and set (v: char3) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
            
        member this.zwx 
            with get() =
                char3(this.z, this.w, this.x)
            and set (v: char3) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
            
        member this.zyw 
            with get() =
                char3(this.z, this.y, this.w)
            and set (v: char3) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
            
        member this.zwy 
            with get() =
                char3(this.z, this.w, this.y)
            and set (v: char3) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                        
        member this.wxy 
            with get() =
                char3(this.w, this.x, this.y)
            and set (v: char3) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
            
        member this.wyx 
            with get() =
                char3(this.w, this.y, this.x)
            and set (v: char3) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
            
        member this.wxz 
            with get() =
                char3(this.w, this.x, this.z)
            and set (v: char3) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
            
        member this.wzx 
            with get() =
                char3(this.w, this.z, this.x)
            and set (v: char3) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
            
        member this.wyz 
            with get() =
                char3(this.w, this.y, this.z)
            and set (v: char3) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
            
        member this.wzy 
            with get() =
                char3(this.w, this.z, this.y)
            and set (v: char3) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z

        member this.xxy 
            with get() =
                char3(this.x, this.x, this.y)
            
        member this.xyx 
            with get() =
                char3(this.x, this.y, this.x)
            
        member this.yxx 
            with get() =
                char3(this.y, this.x, this.x)
            
        member this.xxz 
            with get() =
                char3(this.x, this.x, this.z)
            
        member this.xzx 
            with get() =
                char3(this.x, this.z, this.x)

        member this.zxx 
            with get() =
                char3(this.z, this.x, this.x)
                    
        member this.xxw 
            with get() =
                char3(this.x, this.x, this.w)
            
        member this.xwx 
            with get() =
                char3(this.x, this.w, this.x)

        member this.wxx 
            with get() =
                char3(this.w, this.x, this.x)
                            
        member this.yyx 
            with get() =
                char3(this.y, this.y, this.x)
            
        member this.yxy 
            with get() =
                char3(this.y, this.x, this.y)
            
        member this.xyy 
            with get() =
                char3(this.x, this.y, this.y)
            
        member this.yyz 
            with get() =
                char3(this.y, this.y, this.z)
            
        member this.yzy 
            with get() =
                char3(this.y, this.z, this.y)

        member this.zyy 
            with get() =
                char3(this.z, this.y, this.y)
            
        member this.yyw 
            with get() =
                char3(this.y, this.y, this.w)
            
        member this.ywy 
            with get() =
                char3(this.y, this.w, this.y)

        member this.wyy
            with get() =
                char3(this.w, this.y, this.y)

        member this.zzx 
            with get() =
                char3(this.z, this.z, this.x)
            
        member this.zxz 
            with get() =
                char3(this.z, this.x, this.z)
            
        member this.xzz 
            with get() =
                char3(this.x, this.z, this.z)
            
        member this.zzy 
            with get() =
                char3(this.z, this.z, this.y)
            
        member this.zyz 
            with get() =
                char3(this.z, this.y, this.z)

        member this.yzz 
            with get() =
                char3(this.y, this.z, this.z)
            
        member this.zzw 
            with get() =
                char3(this.z, this.z, this.w)
            
        member this.zwz 
            with get() =
                char3(this.z, this.w, this.z)
            
        member this.wzz 
            with get() =
                char3(this.w, this.z, this.z)
            
        member this.xxx 
            with get() =
                char3(this.x, this.x, this.x)
            
        member this.yyy 
            with get() =
                char3(this.y, this.y, this.y)
            
        member this.zzz 
            with get() =
                char3(this.z, this.z, this.z)
 
        member this.www 
            with get() =
                char3(this.w, this.w, this.w)
     
        // 4-comps       
        member this.xxxx
            with get() =
                char4(this.x, this.x, this.x, this.x)

        member this.xxxy
            with get() =
                char4(this.x, this.x, this.x, this.y)

        member this.xxxz
            with get() =
                char4(this.x, this.x, this.x, this.z)

        member this.xxxw
            with get() =
                char4(this.x, this.x, this.x, this.w)

        member this.xxyx
            with get() =
                char4(this.x, this.x, this.y, this.x)

        member this.xxyy
            with get() =
                char4(this.x, this.x, this.y, this.y)

        member this.xxyz
            with get() =
                char4(this.x, this.x, this.y, this.z)

        member this.xxyw
            with get() =
                char4(this.x, this.x, this.y, this.w)

        member this.xxzx
            with get() =
                char4(this.x, this.x, this.z, this.x)

        member this.xxzy
            with get() =
                char4(this.x, this.x, this.z, this.y)

        member this.xxzz
            with get() =
                char4(this.x, this.x, this.z, this.z)

        member this.xxzw
            with get() =
                char4(this.x, this.x, this.z, this.w)

        member this.xxwx
            with get() =
                char4(this.x, this.x, this.w, this.x)

        member this.xxwy
            with get() =
                char4(this.x, this.x, this.w, this.y)

        member this.xxwz
            with get() =
                char4(this.x, this.x, this.w, this.z)

        member this.xxww
            with get() =
                char4(this.x, this.x, this.w, this.w)

        member this.xyxx
            with get() =
                char4(this.x, this.y, this.x, this.x)

        member this.xyxy
            with get() =
                char4(this.x, this.y, this.x, this.y)

        member this.xyxz
            with get() =
                char4(this.x, this.y, this.x, this.z)

        member this.xyxw
            with get() =
                char4(this.x, this.y, this.x, this.w)

        member this.xyyx
            with get() =
                char4(this.x, this.y, this.y, this.x)

        member this.xyyy
            with get() =
                char4(this.x, this.y, this.y, this.y)

        member this.xyyz
            with get() =
                char4(this.x, this.y, this.y, this.z)

        member this.xyyw
            with get() =
                char4(this.x, this.y, this.y, this.w)

        member this.xyzx
            with get() =
                char4(this.x, this.y, this.z, this.x)

        member this.xyzy
            with get() =
                char4(this.x, this.y, this.z, this.y)

        member this.xyzz
            with get() =
                char4(this.x, this.y, this.z, this.z)

        member this.xyzw
            with get() =
                char4(this.x, this.y, this.z, this.w)
            and set(v: char4) =
                this.x <- v.x
                this.y <- v.y
                this.z <- v.z
                this.w <- v.w
            
        member this.xywx
            with get() =
                char4(this.x, this.y, this.w, this.x)

        member this.xywy
            with get() =
                char4(this.x, this.y, this.w, this.y)

        member this.xywz
            with get() =
                char4(this.x, this.y, this.w, this.z)
            and set(v: char4) =
                this.x <- v.x
                this.y <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.xyww
            with get() =
                char4(this.x, this.y, this.w, this.w)

        member this.xzxx
            with get() =
                char4(this.x, this.z, this.x, this.x)

        member this.xzxy
            with get() =
                char4(this.x, this.z, this.x, this.y)

        member this.xzxz
            with get() =
                char4(this.x, this.z, this.x, this.z)

        member this.xzxw
            with get() =
                char4(this.x, this.z, this.x, this.w)

        member this.xzyx
            with get() =
                char4(this.x, this.z, this.y, this.x)

        member this.xzyy
            with get() =
                char4(this.x, this.z, this.y, this.y)

        member this.xzyz
            with get() =
                char4(this.x, this.z, this.y, this.z)

        member this.xzyw
            with get() =
                char4(this.x, this.z, this.y, this.w)
            and set(v: char4) =
                this.x <- v.x
                this.z <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.xzzx
            with get() =
                char4(this.x, this.z, this.z, this.x)

        member this.xzzy
            with get() =
                char4(this.x, this.z, this.z, this.y)

        member this.xzzz
            with get() =
                char4(this.x, this.z, this.z, this.z)

        member this.xzzw
            with get() =
                char4(this.x, this.z, this.z, this.w)

        member this.xzwx
            with get() =
                char4(this.x, this.z, this.w, this.x)

        member this.xzwy
            with get() =
                char4(this.x, this.z, this.w, this.y)
            and set(v: char4) =
                this.x <- v.x
                this.z <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.xzwz
            with get() =
                char4(this.x, this.z, this.w, this.z)

        member this.xzww
            with get() =
                char4(this.x, this.z, this.w, this.w)

        member this.xwxx
            with get() =
                char4(this.x, this.w, this.x, this.x)

        member this.xwxy
            with get() =
                char4(this.x, this.w, this.x, this.y)

        member this.xwxz
            with get() =
                char4(this.x, this.w, this.x, this.z)

        member this.xwxw
            with get() =
                char4(this.x, this.w, this.x, this.w)

        member this.xwyx
            with get() =
                char4(this.x, this.w, this.y, this.x)

        member this.xwyy
            with get() =
                char4(this.x, this.w, this.y, this.y)

        member this.xwyz
            with get() =
                char4(this.x, this.w, this.y, this.z)
            and set(v: char4) =
                this.x <- v.x
                this.w <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.xwyw
            with get() =
                char4(this.x, this.w, this.y, this.w)

        member this.xwzx
            with get() =
                char4(this.x, this.w, this.z, this.x)

        member this.xwzy
            with get() =
                char4(this.x, this.w, this.z, this.y)
            and set(v: char4) =
                this.x <- v.x
                this.w <- v.y
                this.z <- v.z
                this.y <- v.w
            
        member this.xwzz
            with get() =
                char4(this.x, this.w, this.z, this.z)

        member this.xwzw
            with get() =
                char4(this.x, this.w, this.z, this.w)

        member this.xwwx
            with get() =
                char4(this.x, this.w, this.w, this.x)

        member this.xwwy
            with get() =
                char4(this.x, this.w, this.w, this.y)

        member this.xwwz
            with get() =
                char4(this.x, this.w, this.w, this.z)

        member this.xwww
            with get() =
                char4(this.x, this.w, this.w, this.w)

        member this.yxxx
            with get() =
                char4(this.y, this.x, this.x, this.x)

        member this.yxxy
            with get() =
                char4(this.y, this.x, this.x, this.y)

        member this.yxxz
            with get() =
                char4(this.y, this.x, this.x, this.z)

        member this.yxxw
            with get() =
                char4(this.y, this.x, this.x, this.w)

        member this.yxyx
            with get() =
                char4(this.y, this.x, this.y, this.x)

        member this.yxyy
            with get() =
                char4(this.y, this.x, this.y, this.y)

        member this.yxyz
            with get() =
                char4(this.y, this.x, this.y, this.z)

        member this.yxyw
            with get() =
                char4(this.y, this.x, this.y, this.w)

        member this.yxzx
            with get() =
                char4(this.y, this.x, this.z, this.x)

        member this.yxzy
            with get() =
                char4(this.y, this.x, this.z, this.y)

        member this.yxzz
            with get() =
                char4(this.y, this.x, this.z, this.z)

        member this.yxzw
            with get() =
                char4(this.y, this.x, this.z, this.w)
            and set(v: char4) =
                this.y <- v.x
                this.x <- v.y
                this.z <- v.z
                this.w <- v.w


        member this.yxwx
            with get() =
                char4(this.y, this.x, this.w, this.x)

        member this.yxwy
            with get() =
                char4(this.y, this.x, this.w, this.y)

        member this.yxwz
            with get() =
                char4(this.y, this.x, this.w, this.z)
            and set(v: char4) =
                this.y <- v.x
                this.x <- v.y
                this.w <- v.z
                this.z <- v.w
            
        member this.yxww
            with get() =
                char4(this.y, this.x, this.w, this.w)

        member this.yyxx
            with get() =
                char4(this.y, this.y, this.x, this.x)

        member this.yyxy
            with get() =
                char4(this.y, this.y, this.x, this.y)

        member this.yyxz
            with get() =
                char4(this.y, this.y, this.x, this.z)

        member this.yyxw
            with get() =
                char4(this.y, this.y, this.x, this.w)

        member this.yyyx
            with get() =
                char4(this.y, this.y, this.y, this.x)

        member this.yyyy
            with get() =
                char4(this.y, this.y, this.y, this.y)

        member this.yyyz
            with get() =
                char4(this.y, this.y, this.y, this.z)

        member this.yyyw
            with get() =
                char4(this.y, this.y, this.y, this.w)

        member this.yyzx
            with get() =
                char4(this.y, this.y, this.z, this.x)

        member this.yyzy
            with get() =
                char4(this.y, this.y, this.z, this.y)

        member this.yyzz
            with get() =
                char4(this.y, this.y, this.z, this.z)

        member this.yyzw
            with get() =
                char4(this.y, this.y, this.z, this.w)

        member this.yywx
            with get() =
                char4(this.y, this.y, this.w, this.x)

        member this.yywy
            with get() =
                char4(this.y, this.y, this.w, this.y)

        member this.yywz
            with get() =
                char4(this.y, this.y, this.w, this.z)

        member this.yyww
            with get() =
                char4(this.y, this.y, this.w, this.w)

        member this.yzxx
            with get() =
                char4(this.y, this.z, this.x, this.x)

        member this.yzxy
            with get() =
                char4(this.y, this.z, this.x, this.y)

        member this.yzxz
            with get() =
                char4(this.y, this.z, this.x, this.z)

        member this.yzxw
            with get() =
                char4(this.y, this.z, this.x, this.w)
            and set(v: char4) =
                this.y <- v.x
                this.z <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.yzyx
            with get() =
                char4(this.y, this.z, this.y, this.x)

        member this.yzyy
            with get() =
                char4(this.y, this.z, this.y, this.y)

        member this.yzyz
            with get() =
                char4(this.y, this.z, this.y, this.z)

        member this.yzyw
            with get() =
                char4(this.y, this.z, this.y, this.w)

        member this.yzzx
            with get() =
                char4(this.y, this.z, this.z, this.x)

        member this.yzzy
            with get() =
                char4(this.y, this.z, this.z, this.y)

        member this.yzzz
            with get() =
                char4(this.y, this.z, this.z, this.z)

        member this.yzzw
            with get() =
                char4(this.y, this.z, this.z, this.w)

        member this.yzwx
            with get() =
                char4(this.y, this.z, this.w, this.x)
            and set(v: char4) =
                this.y <- v.x
                this.z <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.yzwy
            with get() =
                char4(this.y, this.z, this.w, this.y)

        member this.yzwz
            with get() =
                char4(this.y, this.z, this.w, this.z)

        member this.yzww
            with get() =
                char4(this.y, this.z, this.w, this.w)

        member this.ywxx
            with get() =
                char4(this.y, this.w, this.x, this.x)

        member this.ywxy
            with get() =
                char4(this.y, this.w, this.x, this.y)

        member this.ywxz
            with get() =
                char4(this.y, this.w, this.x, this.z)
            and set(v: char4) =
                this.y <- v.x
                this.w <- v.y
                this.x <- v.z
                this.z <- v.w


        member this.ywxw
            with get() =
                char4(this.y, this.w, this.x, this.w)

        member this.ywyx
            with get() =
                char4(this.y, this.w, this.y, this.x)

        member this.ywyy
            with get() =
                char4(this.y, this.w, this.y, this.y)

        member this.ywyz
            with get() =
                char4(this.y, this.w, this.y, this.z)

        member this.ywyw
            with get() =
                char4(this.y, this.w, this.y, this.w)

        member this.ywzx
            with get() =
                char4(this.y, this.w, this.z, this.x)
            and set(v: char4) =
                this.y <- v.x
                this.w <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.ywzy
            with get() =
                char4(this.y, this.w, this.z, this.y)

        member this.ywzz
            with get() =
                char4(this.y, this.w, this.z, this.z)

        member this.ywzw
            with get() =
                char4(this.y, this.w, this.z, this.w)

        member this.ywwx
            with get() =
                char4(this.y, this.w, this.w, this.x)

        member this.ywwy
            with get() =
                char4(this.y, this.w, this.w, this.y)

        member this.ywwz
            with get() =
                char4(this.y, this.w, this.w, this.z)

        member this.ywww
            with get() =
                char4(this.y, this.w, this.w, this.w)

        member this.zxxx
            with get() =
                char4(this.z, this.x, this.x, this.x)

        member this.zxxy
            with get() =
                char4(this.z, this.x, this.x, this.y)

        member this.zxxz
            with get() =
                char4(this.z, this.x, this.x, this.z)

        member this.zxxw
            with get() =
                char4(this.z, this.x, this.x, this.w)

        member this.zxyx
            with get() =
                char4(this.z, this.x, this.y, this.x)

        member this.zxyy
            with get() =
                char4(this.z, this.x, this.y, this.y)

        member this.zxyz
            with get() =
                char4(this.z, this.x, this.y, this.z)

        member this.zxyw
            with get() =
                char4(this.z, this.x, this.y, this.w)
            and set(v: char4) =
                this.z <- v.x
                this.x <- v.y
                this.y <- v.z
                this.w <- v.w
            
        member this.zxzx
            with get() =
                char4(this.z, this.x, this.z, this.x)

        member this.zxzy
            with get() =
                char4(this.z, this.x, this.z, this.y)

        member this.zxzz
            with get() =
                char4(this.z, this.x, this.z, this.z)

        member this.zxzw
            with get() =
                char4(this.z, this.x, this.z, this.w)

        member this.zxwx
            with get() =
                char4(this.z, this.x, this.w, this.x)

        member this.zxwy
            with get() =
                char4(this.z, this.x, this.w, this.y)
            and set(v: char4) =
                this.z <- v.x
                this.x <- v.y
                this.w <- v.z
                this.y <- v.w
            
        member this.zxwz
            with get() =
                char4(this.z, this.x, this.w, this.z)

        member this.zxww
            with get() =
                char4(this.z, this.x, this.w, this.w)

        member this.zyxx
            with get() =
                char4(this.z, this.y, this.x, this.x)

        member this.zyxy
            with get() =
                char4(this.z, this.y, this.x, this.y)

        member this.zyxz
            with get() =
                char4(this.z, this.y, this.x, this.z)

        member this.zyxw
            with get() =
                char4(this.z, this.y, this.x, this.w)
            and set(v: char4) =
                this.z <- v.x
                this.y <- v.y
                this.x <- v.z
                this.w <- v.w
            
        member this.zyyx
            with get() =
                char4(this.z, this.y, this.y, this.x)

        member this.zyyy
            with get() =
                char4(this.z, this.y, this.y, this.y)

        member this.zyyz
            with get() =
                char4(this.z, this.y, this.y, this.z)

        member this.zyyw
            with get() =
                char4(this.z, this.y, this.y, this.w)

        member this.zyzx
            with get() =
                char4(this.z, this.y, this.z, this.x)

        member this.zyzy
            with get() =
                char4(this.z, this.y, this.z, this.y)

        member this.zyzz
            with get() =
                char4(this.z, this.y, this.z, this.z)

        member this.zyzw
            with get() =
                char4(this.z, this.y, this.z, this.w)

        member this.zywx
            with get() =
                char4(this.z, this.y, this.w, this.x)
            and set(v: char4) =
                this.z <- v.x
                this.y <- v.y
                this.w <- v.z
                this.x <- v.w
            
        member this.zywy
            with get() =
                char4(this.z, this.y, this.w, this.y)

        member this.zywz
            with get() =
                char4(this.z, this.y, this.w, this.z)

        member this.zyww
            with get() =
                char4(this.z, this.y, this.w, this.w)

        member this.zzxx
            with get() =
                char4(this.z, this.z, this.x, this.x)

        member this.zzxy
            with get() =
                char4(this.z, this.z, this.x, this.y)

        member this.zzxz
            with get() =
                char4(this.z, this.z, this.x, this.z)

        member this.zzxw
            with get() =
                char4(this.z, this.z, this.x, this.w)

        member this.zzyx
            with get() =
                char4(this.z, this.z, this.y, this.x)

        member this.zzyy
            with get() =
                char4(this.z, this.z, this.y, this.y)

        member this.zzyz
            with get() =
                char4(this.z, this.z, this.y, this.z)

        member this.zzyw
            with get() =
                char4(this.z, this.z, this.y, this.w)

        member this.zzzx
            with get() =
                char4(this.z, this.z, this.z, this.x)

        member this.zzzy
            with get() =
                char4(this.z, this.z, this.z, this.y)

        member this.zzzz
            with get() =
                char4(this.z, this.z, this.z, this.z)

        member this.zzzw
            with get() =
                char4(this.z, this.z, this.z, this.w)

        member this.zzwx
            with get() =
                char4(this.z, this.z, this.w, this.x)

        member this.zzwy
            with get() =
                char4(this.z, this.z, this.w, this.y)

        member this.zzwz
            with get() =
                char4(this.z, this.z, this.w, this.z)

        member this.zzww
            with get() =
                char4(this.z, this.z, this.w, this.w)

        member this.zwxx
            with get() =
                char4(this.z, this.w, this.x, this.x)

        member this.zwxy
            with get() =
                char4(this.z, this.w, this.x, this.y)
            and set(v: char4) =
                this.z <- v.x
                this.w <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.zwxz
            with get() =
                char4(this.z, this.w, this.x, this.z)

        member this.zwxw
            with get() =
                char4(this.z, this.w, this.x, this.w)

        member this.zwyx
            with get() =
                char4(this.z, this.w, this.y, this.x)
            and set(v: char4) =
                this.z <- v.x
                this.w <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.zwyy
            with get() =
                char4(this.z, this.w, this.y, this.y)

        member this.zwyz
            with get() =
                char4(this.z, this.w, this.y, this.z)

        member this.zwyw
            with get() =
                char4(this.z, this.w, this.y, this.w)

        member this.zwzx
            with get() =
                char4(this.z, this.w, this.z, this.x)

        member this.zwzy
            with get() =
                char4(this.z, this.w, this.z, this.y)

        member this.zwzz
            with get() =
                char4(this.z, this.w, this.z, this.z)

        member this.zwzw
            with get() =
                char4(this.z, this.w, this.z, this.w)

        member this.zwwx
            with get() =
                char4(this.z, this.w, this.w, this.x)

        member this.zwwy
            with get() =
                char4(this.z, this.w, this.w, this.y)

        member this.zwwz
            with get() =
                char4(this.z, this.w, this.w, this.z)

        member this.zwww
            with get() =
                char4(this.z, this.w, this.w, this.w)

        member this.wxxx
            with get() =
                char4(this.w, this.x, this.x, this.x)

        member this.wxxy
            with get() =
                char4(this.w, this.x, this.x, this.y)

        member this.wxxz
            with get() =
                char4(this.w, this.x, this.x, this.z)

        member this.wxxw
            with get() =
                char4(this.w, this.x, this.x, this.w)

        member this.wxyx
            with get() =
                char4(this.w, this.x, this.y, this.x)

        member this.wxyy
            with get() =
                char4(this.w, this.x, this.y, this.y)

        member this.wxyz
            with get() =
                char4(this.w, this.x, this.y, this.z)
            and set(v: char4) =
                this.w <- v.x
                this.x <- v.y
                this.y <- v.z
                this.z <- v.w
            
        member this.wxyw
            with get() =
                char4(this.w, this.x, this.y, this.w)

        member this.wxzx
            with get() =
                char4(this.w, this.x, this.z, this.x)

        member this.wxzy
            with get() =
                char4(this.w, this.x, this.z, this.y)
            and set(v: char4) =
                this.w <- v.x
                this.x <- v.y
                this.z <- v.z
                this.y <- v.w


        member this.wxzz
            with get() =
                char4(this.w, this.x, this.z, this.z)

        member this.wxzw
            with get() =
                char4(this.w, this.x, this.z, this.w)

        member this.wxwx
            with get() =
                char4(this.w, this.x, this.w, this.x)

        member this.wxwy
            with get() =
                char4(this.w, this.x, this.w, this.y)

        member this.wxwz
            with get() =
                char4(this.w, this.x, this.w, this.z)

        member this.wxww
            with get() =
                char4(this.w, this.x, this.w, this.w)

        member this.wyxx
            with get() =
                char4(this.w, this.y, this.x, this.x)

        member this.wyxy
            with get() =
                char4(this.w, this.y, this.x, this.y)

        member this.wyxz
            with get() =
                char4(this.w, this.y, this.x, this.z)
            and set(v: char4) =
                this.w <- v.x
                this.y <- v.y
                this.x <- v.z
                this.z <- v.w
            
        member this.wyxw
            with get() =
                char4(this.w, this.y, this.x, this.w)

        member this.wyyx
            with get() =
                char4(this.w, this.y, this.y, this.x)

        member this.wyyy
            with get() =
                char4(this.w, this.y, this.y, this.y)

        member this.wyyz
            with get() =
                char4(this.w, this.y, this.y, this.z)

        member this.wyyw
            with get() =
                char4(this.w, this.y, this.y, this.w)

        member this.wyzx
            with get() =
                char4(this.w, this.y, this.z, this.x)
            and set(v: char4) =
                this.w <- v.x
                this.y <- v.y
                this.z <- v.z
                this.x <- v.w
            
        member this.wyzy
            with get() =
                char4(this.w, this.y, this.z, this.y)

        member this.wyzz
            with get() =
                char4(this.w, this.y, this.z, this.z)

        member this.wyzw
            with get() =
                char4(this.w, this.y, this.z, this.w)

        member this.wywx
            with get() =
                char4(this.w, this.y, this.w, this.x)

        member this.wywy
            with get() =
                char4(this.w, this.y, this.w, this.y)

        member this.wywz
            with get() =
                char4(this.w, this.y, this.w, this.z)

        member this.wyww
            with get() =
                char4(this.w, this.y, this.w, this.w)

        member this.wzxx
            with get() =
                char4(this.w, this.z, this.x, this.x)

        member this.wzxy
            with get() =
                char4(this.w, this.z, this.x, this.y)
            and set(v: char4) =
                this.w <- v.x
                this.z <- v.y
                this.x <- v.z
                this.y <- v.w
            
        member this.wzxz
            with get() =
                char4(this.w, this.z, this.x, this.z)

        member this.wzxw
            with get() =
                char4(this.w, this.z, this.x, this.w)

        member this.wzyx
            with get() =
                char4(this.w, this.z, this.y, this.x)
            and set(v: char4) =
                this.w <- v.x
                this.z <- v.y
                this.y <- v.z
                this.x <- v.w
            
        member this.wzyy
            with get() =
                char4(this.w, this.z, this.y, this.y)

        member this.wzyz
            with get() =
                char4(this.w, this.z, this.y, this.z)

        member this.wzyw
            with get() =
                char4(this.w, this.z, this.y, this.w)

        member this.wzzx
            with get() =
                char4(this.w, this.z, this.z, this.x)

        member this.wzzy
            with get() =
                char4(this.w, this.z, this.z, this.y)

        member this.wzzz
            with get() =
                char4(this.w, this.z, this.z, this.z)

        member this.wzzw
            with get() =
                char4(this.w, this.z, this.z, this.w)

        member this.wzwx
            with get() =
                char4(this.w, this.z, this.w, this.x)

        member this.wzwy
            with get() =
                char4(this.w, this.z, this.w, this.y)

        member this.wzwz
            with get() =
                char4(this.w, this.z, this.w, this.z)

        member this.wzww
            with get() =
                char4(this.w, this.z, this.w, this.w)

        member this.wwxx
            with get() =
                char4(this.w, this.w, this.x, this.x)

        member this.wwxy
            with get() =
                char4(this.w, this.w, this.x, this.y)

        member this.wwxz
            with get() =
                char4(this.w, this.w, this.x, this.z)

        member this.wwxw
            with get() =
                char4(this.w, this.w, this.x, this.w)

        member this.wwyx
            with get() =
                char4(this.w, this.w, this.y, this.x)

        member this.wwyy
            with get() =
                char4(this.w, this.w, this.y, this.y)

        member this.wwyz
            with get() =
                char4(this.w, this.w, this.y, this.z)

        member this.wwyw
            with get() =
                char4(this.w, this.w, this.y, this.w)

        member this.wwzx
            with get() =
                char4(this.w, this.w, this.z, this.x)

        member this.wwzy
            with get() =
                char4(this.w, this.w, this.z, this.y)

        member this.wwzz
            with get() =
                char4(this.w, this.w, this.z, this.z)

        member this.wwzw
            with get() =
                char4(this.w, this.w, this.z, this.w)

        member this.wwwx
            with get() =
                char4(this.w, this.w, this.w, this.x)

        member this.wwwy
            with get() =
                char4(this.w, this.w, this.w, this.y)

        member this.wwwz
            with get() =
                char4(this.w, this.w, this.w, this.z)

        member this.wwww
            with get() =
                char4(this.w, this.w, this.w, this.w)

        member this.lo 
            with get() =
                char2(this.x, this.y)
            and set (v:char2) =
                this.x <- v.x
                this.y <- v.y
            
        member this.hi 
            with get() =
                char2(this.y, this.w)
            and set (v:char2) =
                this.z <- v.x
                this.w <- v.y
            
        member this.even 
            with get() =
                char2(this.x, this.z)
            and set (v:char2) =
                this.x <- v.x
                this.z <- v.y
            
        member this.odd 
            with get() =
                char2(this.y, this.w)
            and set (v:char2) =
                this.y <- v.x
                this.w <- v.y

        internal new(c: char[]) =
            char4(c.[0], c.[1], c.[2], c.[3])

        internal new(c: sbyte[]) =
            char4(c.[0] |> char, c.[1] |> char, c.[2] |> char, c.[3] |> char)

        static member (+) (f1: char4, f2: char4) =
            char4(Array.map2 (+) (f1.ByteComponents) (f2.ByteComponents))
        static member (-) (f1: char4, f2: char4) =
            char4(Array.map2 (-) (f1.ByteComponents) (f2.ByteComponents))
        static member (*) (f1: char4, f2: char4) =
            char4(Array.map2 (*) (f1.ByteComponents) (f2.ByteComponents))
        static member (/) (f1: char4, f2: char4) =
            char4(Array.map2 (/) (f1.ByteComponents) (f2.ByteComponents))
        
        static member (>>=) (f1: char4, f2: char4) =
            char4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (<<=) (f1: char4, f2: char4) =
            char4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (===) (f1: char4, f2: char4) =
            char4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
        static member (<=>) (f1: char4, f2: char4) =
            char4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1y else 0y) (f1.ByteComponents) (f2.ByteComponents))
            
        static member vload(offset: int64, p: Array) =
            let stream = new MemoryStream()
            let f = new Binary.BinaryFormatter()
            f.Serialize(stream, p)
            stream.Seek(offset * 4L, SeekOrigin.Begin) |> ignore
            let data = f.Deserialize(stream) :?> char4
            stream.Close()
            data
    end    
  
// **************************************************************************************************************
