namespace FSCL.Compiler

// Integer vector types
type int2(x: int32, y: int32) =
    let mutable comp = [| x; y |]

    member internal this.Components
        with get() =
            comp

    member this.x 
        with get() =
            comp.[0]
        and set v =
            comp.[0] <- v
            
    member this.y 
        with get() =
            comp.[1]
        and set v =
            comp.[1] <- v
            
    member this.xy 
        with get() =
            int2(this.x, this.y)
        and set (v: int2) =
            this.x <- v.x
            this.y <- v.y
            
    member this.yx 
        with get() =
            int2(this.y, this.x)
        and set (v: int2) =
            this.x <- v.y
            this.y <- v.x
            
    member this.xx 
        with get() =
            int2(this.x, this.x)
            
    member this.yy 
        with get() =
            int2(this.y, this.y)

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

    new() =
        int2(0, 0)

    internal new(c: int32[]) =
        int2(c.[0], c.[1])

    static member (+) (f1: int2, f2: int2) =
        int2(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: int2, f2: int2) =
        int2(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: int2, f2: int2) =
        int2(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: int2, f2: int2) =
        int2(Array.map2 (/) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: int2, f2: int2) =
        int2(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: int2, f2: int2) =
        int2(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: int2, f2: int2) =
        int2(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: int2, f2: int2) =
        int2(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
                
type int3(x: int32, y: int32, z: int32) =
    let mutable comp = [| x; y; z |]

    member internal this.Components
        with get() =
            comp

    member this.x 
        with get() =
            comp.[0]
        and set v =
            comp.[0] <- v
            
    member this.y 
        with get() =
            comp.[1]
        and set v =
            comp.[1] <- v
            
    member this.z 
        with get() =
            comp.[2]
        and set v =
            comp.[2] <- v
            
    member this.xy 
        with get() =
            int2(this.x, this.y)
        and set (v: int2) =
            this.x <- v.x
            this.y <- v.y
            
    member this.xz 
        with get() =
            int2(this.x, this.z)
        and set (v: int2) =
            this.x <- v.x
            this.z <- v.y

    member this.yx 
        with get() =
            int2(this.y, this.x)
        and set (v: int2) =
            this.x <- v.y
            this.y <- v.x
            
    member this.yz 
        with get() =
            int2(this.y, this.z)
        and set (v: int2) =
            this.y <- v.x
            this.z <- v.y
                        
    member this.zx 
        with get() =
            int2(this.z, this.x)
        and set (v: int2) =
            this.z <- v.x
            this.x <- v.y

    member this.zy 
        with get() =
            int2(this.z, this.y)
        and set (v: int2) =
            this.z <- v.x
            this.y <- v.y
            
    member this.xx 
        with get() =
            int2(this.x, this.x)
            
    member this.yy 
        with get() =
            int2(this.y, this.y)
            
    member this.zz 
        with get() =
            int2(this.z, this.z)

    // 3-comps    
    member this.xyz 
        with get() =
            int3(this.x, this.y, this.z)
        and set (v: int3) =
            this.x <- v.x
            this.y <- v.y
            this.z <- v.z
            
    member this.xzy
        with get() =
            int3(this.x, this.z, this.y)
        and set (v: int3) =
            this.x <- v.x
            this.z <- v.y
            this.y <- v.z
            
    member this.yxz 
        with get() =
            int3(this.y, this.x, this.z)
        and set (v: int3) =
            this.y <- v.x
            this.x <- v.y
            this.z <- v.z
            
    member this.yzx 
        with get() =
            int3(this.y, this.z, this.x)
        and set (v: int3) =
            this.y <- v.x
            this.z <- v.y
            this.x <- v.z
            
    member this.zxy 
        with get() =
            int3(this.z, this.x, this.y)
        and set (v: int3) =
            this.z <- v.x
            this.x <- v.y
            this.y <- v.z
            
    member this.zyx 
        with get() =
            int3(this.z, this.y, this.x)
        and set (v: int3) =
            this.z <- v.x
            this.y <- v.y
            this.x <- v.z
            
    member this.xxy 
        with get() =
            int3(this.x, this.x, this.y)
            
    member this.xyx 
        with get() =
            int3(this.x, this.y, this.x)
            
    member this.yxx 
        with get() =
            int3(this.y, this.x, this.x)
            
    member this.xxz 
        with get() =
            int3(this.x, this.x, this.z)
            
    member this.xzx 
        with get() =
            int3(this.x, this.z, this.x)

    member this.zxx 
        with get() =
            int3(this.z, this.x, this.x)
                        
    member this.yyx 
        with get() =
            int3(this.y, this.y, this.x)
            
    member this.yxy 
        with get() =
            int3(this.y, this.x, this.y)
            
    member this.xyy 
        with get() =
            int3(this.x, this.y, this.y)
            
    member this.yyz 
        with get() =
            int3(this.y, this.y, this.z)
            
    member this.yzy 
        with get() =
            int3(this.y, this.z, this.y)

    member this.zyy 
        with get() =
            int3(this.z, this.y, this.y)
            
    member this.zzx 
        with get() =
            int3(this.z, this.z, this.x)
            
    member this.zxz 
        with get() =
            int3(this.z, this.x, this.z)
            
    member this.xzz 
        with get() =
            int3(this.x, this.z, this.z)
            
    member this.zzy 
        with get() =
            int3(this.z, this.z, this.y)
            
    member this.zyz 
        with get() =
            int3(this.z, this.y, this.z)

    member this.yzz 
        with get() =
            int3(this.y, this.z, this.z)
            
    member this.xxx 
        with get() =
            int3(this.x, this.x, this.x)
            
    member this.yyy 
        with get() =
            int3(this.y, this.y, this.y)
            
    member this.zzz 
        with get() =
            int3(this.z, this.z, this.z)

    member this.lo 
        with get() =
            int2(this.x, this.y)
        and set (v:int2) =
            this.x <- v.x
            this.y <- v.y
            
    member this.hi 
        with get() =
            int2(this.y, 0)
        and set (v:int2) =
            this.z <- v.x
            
    member this.even 
        with get() =
            int2(this.x, this.z)
        and set (v:int2) =
            this.x <- v.x
            this.z <- v.y
            
    member this.odd 
        with get() =
            int2(this.y, 0)
        and set (v:int2) =
            this.y <- v.x

    new() =
        int3(0, 0, 0)

    internal new(c: int32[]) =
        int3(c.[0], c.[1], c.[2])

    static member (+) (f1: int3, f2: int3) =
        int3(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: int3, f2: int3) =
        int3(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: int3, f2: int3) =
        int3(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: int3, f2: int3) =
        int3(Array.map2 (/) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: int3, f2: int3) =
        int3(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: int3, f2: int3) =
        int3(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: int3, f2: int3) =
        int3(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: int3, f2: int3) =
        int3(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
            
type int4(x: int32, y: int32, z: int32, w: int32) =
    let mutable comp = [| x; y; z; w |]

    member internal this.Components
        with get() =
            comp

    member this.x 
        with get() =
            comp.[0]
        and set v =
            comp.[0] <- v
            
    member this.y 
        with get() =
            comp.[1]
        and set v =
            comp.[1] <- v
            
    member this.z 
        with get() =
            comp.[2]
        and set v =
            comp.[2] <- v
            
    member this.w 
        with get() =
            comp.[3]
        and set v =
            comp.[3] <- v
            
    member this.xy 
        with get() =
            int2(this.x, this.y)
        and set (v: int2) =
            this.x <- v.x
            this.y <- v.y
            
    member this.xz 
        with get() =
            int2(this.x, this.z)
        and set (v: int2) =
            this.x <- v.x
            this.z <- v.y
            
    member this.xw 
        with get() =
            int2(this.x, this.w)
        and set (v: int2) =
            this.x <- v.x
            this.w <- v.y

    member this.yx 
        with get() =
            int2(this.y, this.x)
        and set (v: int2) =
            this.x <- v.y
            this.y <- v.x
            
    member this.yz 
        with get() =
            int2(this.y, this.z)
        and set (v: int2) =
            this.y <- v.x
            this.z <- v.y
                        
    member this.yw 
        with get() =
            int2(this.y, this.w)
        and set (v: int2) =
            this.y <- v.x
            this.w <- v.y

    member this.zx 
        with get() =
            int2(this.z, this.x)
        and set (v: int2) =
            this.z <- v.x
            this.x <- v.y

    member this.zy 
        with get() =
            int2(this.z, this.y)
        and set (v: int2) =
            this.z <- v.x
            this.y <- v.y
            
    member this.zw 
        with get() =
            int2(this.z, this.w)
        and set (v: int2) =
            this.z <- v.x
            this.w <- v.y
                          
    member this.wx 
        with get() =
            int2(this.w, this.x)
        and set (v: int2) =
            this.w <- v.x
            this.x <- v.y
            
    member this.wy 
        with get() =
            int2(this.w, this.y)
        and set (v: int2) =
            this.w <- v.x
            this.y <- v.y
            
    member this.wz 
        with get() =
            int2(this.w, this.z)
        and set (v: int2) =
            this.w <- v.x
            this.z <- v.y

    member this.xx 
        with get() =
            int2(this.x, this.x)
            
    member this.yy 
        with get() =
            int2(this.y, this.y)
            
    member this.zz 
        with get() =
            int2(this.z, this.z)
            
    member this.ww 
        with get() =
            int2(this.w, this.w)

    // 3-comps    
    member this.xyz 
        with get() =
            int3(this.x, this.y, this.z)
        and set (v: int3) =
            this.x <- v.x
            this.y <- v.y
            this.z <- v.z
            
    member this.xzy
        with get() =
            int3(this.x, this.z, this.y)
        and set (v: int3) =
            this.x <- v.x
            this.z <- v.y
            this.y <- v.z
            
    member this.xyw
        with get() =
            int3(this.x, this.y, this.w)
        and set (v: int3) =
            this.x <- v.x
            this.y <- v.y
            this.w <- v.z
            
    member this.xwy
        with get() =
            int3(this.x, this.w, this.y)
        and set (v: int3) =
            this.x <- v.x
            this.w <- v.y
            this.y <- v.z
            
    member this.xzw
        with get() =
            int3(this.x, this.z, this.w)
        and set (v: int3) =
            this.x <- v.x
            this.z <- v.y
            this.w <- v.z
            
    member this.xwz
        with get() =
            int3(this.x, this.w, this.z)
        and set (v: int3) =
            this.x <- v.x
            this.w <- v.y
            this.z <- v.z

    member this.yxz 
        with get() =
            int3(this.y, this.x, this.z)
        and set (v: int3) =
            this.y <- v.x
            this.x <- v.y
            this.z <- v.z
            
    member this.yzx 
        with get() =
            int3(this.y, this.z, this.x)
        and set (v: int3) =
            this.y <- v.x
            this.z <- v.y
            this.x <- v.z
            
    member this.yxw 
        with get() =
            int3(this.y, this.x, this.w)
        and set (v: int3) =
            this.y <- v.x
            this.x <- v.y
            this.w <- v.z
            
    member this.ywx 
        with get() =
            int3(this.y, this.w, this.x)
        and set (v: int3) =
            this.y <- v.x
            this.w <- v.y
            this.x <- v.z
            
    member this.yzw 
        with get() =
            int3(this.y, this.z, this.w)
        and set (v: int3) =
            this.y <- v.x
            this.z <- v.y
            this.w <- v.z
            
    member this.ywz 
        with get() =
            int3(this.y, this.w, this.z)
        and set (v: int3) =
            this.y <- v.x
            this.w <- v.y
            this.z <- v.z

    member this.zxy 
        with get() =
            int3(this.z, this.x, this.y)
        and set (v: int3) =
            this.z <- v.x
            this.x <- v.y
            this.y <- v.z
            
    member this.zyx 
        with get() =
            int3(this.z, this.y, this.x)
        and set (v: int3) =
            this.z <- v.x
            this.y <- v.y
            this.x <- v.z
            
    member this.zxw 
        with get() =
            int3(this.z, this.x, this.w)
        and set (v: int3) =
            this.z <- v.x
            this.x <- v.y
            this.w <- v.z
            
    member this.zwx 
        with get() =
            int3(this.z, this.w, this.x)
        and set (v: int3) =
            this.z <- v.x
            this.w <- v.y
            this.x <- v.z
            
    member this.zyw 
        with get() =
            int3(this.z, this.y, this.w)
        and set (v: int3) =
            this.z <- v.x
            this.y <- v.y
            this.w <- v.z
            
    member this.zwy 
        with get() =
            int3(this.z, this.w, this.y)
        and set (v: int3) =
            this.z <- v.x
            this.w <- v.y
            this.y <- v.z
                        
    member this.wxy 
        with get() =
            int3(this.w, this.x, this.y)
        and set (v: int3) =
            this.w <- v.x
            this.x <- v.y
            this.y <- v.z
            
    member this.wyx 
        with get() =
            int3(this.w, this.y, this.x)
        and set (v: int3) =
            this.w <- v.x
            this.y <- v.y
            this.x <- v.z
            
    member this.wxz 
        with get() =
            int3(this.w, this.x, this.z)
        and set (v: int3) =
            this.w <- v.x
            this.x <- v.y
            this.z <- v.z
            
    member this.wzx 
        with get() =
            int3(this.w, this.z, this.x)
        and set (v: int3) =
            this.w <- v.x
            this.z <- v.y
            this.x <- v.z
            
    member this.wyz 
        with get() =
            int3(this.w, this.y, this.z)
        and set (v: int3) =
            this.w <- v.x
            this.y <- v.y
            this.z <- v.z
            
    member this.wzy 
        with get() =
            int3(this.w, this.z, this.y)
        and set (v: int3) =
            this.w <- v.x
            this.z <- v.y
            this.y <- v.z

    member this.xxy 
        with get() =
            int3(this.x, this.x, this.y)
            
    member this.xyx 
        with get() =
            int3(this.x, this.y, this.x)
            
    member this.yxx 
        with get() =
            int3(this.y, this.x, this.x)
            
    member this.xxz 
        with get() =
            int3(this.x, this.x, this.z)
            
    member this.xzx 
        with get() =
            int3(this.x, this.z, this.x)

    member this.zxx 
        with get() =
            int3(this.z, this.x, this.x)
                    
    member this.xxw 
        with get() =
            int3(this.x, this.x, this.w)
            
    member this.xwx 
        with get() =
            int3(this.x, this.w, this.x)

    member this.wxx 
        with get() =
            int3(this.w, this.x, this.x)
                            
    member this.yyx 
        with get() =
            int3(this.y, this.y, this.x)
            
    member this.yxy 
        with get() =
            int3(this.y, this.x, this.y)
            
    member this.xyy 
        with get() =
            int3(this.x, this.y, this.y)
            
    member this.yyz 
        with get() =
            int3(this.y, this.y, this.z)
            
    member this.yzy 
        with get() =
            int3(this.y, this.z, this.y)

    member this.zyy 
        with get() =
            int3(this.z, this.y, this.y)
            
    member this.yyw 
        with get() =
            int3(this.y, this.y, this.w)
            
    member this.ywy 
        with get() =
            int3(this.y, this.w, this.y)

    member this.wyy
        with get() =
            int3(this.w, this.y, this.y)

    member this.zzx 
        with get() =
            int3(this.z, this.z, this.x)
            
    member this.zxz 
        with get() =
            int3(this.z, this.x, this.z)
            
    member this.xzz 
        with get() =
            int3(this.x, this.z, this.z)
            
    member this.zzy 
        with get() =
            int3(this.z, this.z, this.y)
            
    member this.zyz 
        with get() =
            int3(this.z, this.y, this.z)

    member this.yzz 
        with get() =
            int3(this.y, this.z, this.z)
            
    member this.zzw 
        with get() =
            int3(this.z, this.z, this.w)
            
    member this.zwz 
        with get() =
            int3(this.z, this.w, this.z)
            
    member this.wzz 
        with get() =
            int3(this.w, this.z, this.z)
            
    member this.xxx 
        with get() =
            int3(this.x, this.x, this.x)
            
    member this.yyy 
        with get() =
            int3(this.y, this.y, this.y)
            
    member this.zzz 
        with get() =
            int3(this.z, this.z, this.z)
 
    member this.www 
        with get() =
            int3(this.w, this.w, this.w)
     
    // 4-comps       
    member this.xxxx
        with get() =
            int4(this.x, this.x, this.x, this.x)

    member this.xxxy
        with get() =
            int4(this.x, this.x, this.x, this.y)

    member this.xxxz
        with get() =
            int4(this.x, this.x, this.x, this.z)

    member this.xxxw
        with get() =
            int4(this.x, this.x, this.x, this.w)

    member this.xxyx
        with get() =
            int4(this.x, this.x, this.y, this.x)

    member this.xxyy
        with get() =
            int4(this.x, this.x, this.y, this.y)

    member this.xxyz
        with get() =
            int4(this.x, this.x, this.y, this.z)

    member this.xxyw
        with get() =
            int4(this.x, this.x, this.y, this.w)

    member this.xxzx
        with get() =
            int4(this.x, this.x, this.z, this.x)

    member this.xxzy
        with get() =
            int4(this.x, this.x, this.z, this.y)

    member this.xxzz
        with get() =
            int4(this.x, this.x, this.z, this.z)

    member this.xxzw
        with get() =
            int4(this.x, this.x, this.z, this.w)

    member this.xxwx
        with get() =
            int4(this.x, this.x, this.w, this.x)

    member this.xxwy
        with get() =
            int4(this.x, this.x, this.w, this.y)

    member this.xxwz
        with get() =
            int4(this.x, this.x, this.w, this.z)

    member this.xxww
        with get() =
            int4(this.x, this.x, this.w, this.w)

    member this.xyxx
        with get() =
            int4(this.x, this.y, this.x, this.x)

    member this.xyxy
        with get() =
            int4(this.x, this.y, this.x, this.y)

    member this.xyxz
        with get() =
            int4(this.x, this.y, this.x, this.z)

    member this.xyxw
        with get() =
            int4(this.x, this.y, this.x, this.w)

    member this.xyyx
        with get() =
            int4(this.x, this.y, this.y, this.x)

    member this.xyyy
        with get() =
            int4(this.x, this.y, this.y, this.y)

    member this.xyyz
        with get() =
            int4(this.x, this.y, this.y, this.z)

    member this.xyyw
        with get() =
            int4(this.x, this.y, this.y, this.w)

    member this.xyzx
        with get() =
            int4(this.x, this.y, this.z, this.x)

    member this.xyzy
        with get() =
            int4(this.x, this.y, this.z, this.y)

    member this.xyzz
        with get() =
            int4(this.x, this.y, this.z, this.z)

    member this.xyzw
        with get() =
            int4(this.x, this.y, this.z, this.w)
        and set(v: int4) =
            this.x <- v.x
            this.y <- v.y
            this.z <- v.z
            this.w <- v.w
            
    member this.xywx
        with get() =
            int4(this.x, this.y, this.w, this.x)

    member this.xywy
        with get() =
            int4(this.x, this.y, this.w, this.y)

    member this.xywz
        with get() =
            int4(this.x, this.y, this.w, this.z)
        and set(v: int4) =
            this.x <- v.x
            this.y <- v.y
            this.w <- v.z
            this.z <- v.w
            
    member this.xyww
        with get() =
            int4(this.x, this.y, this.w, this.w)

    member this.xzxx
        with get() =
            int4(this.x, this.z, this.x, this.x)

    member this.xzxy
        with get() =
            int4(this.x, this.z, this.x, this.y)

    member this.xzxz
        with get() =
            int4(this.x, this.z, this.x, this.z)

    member this.xzxw
        with get() =
            int4(this.x, this.z, this.x, this.w)

    member this.xzyx
        with get() =
            int4(this.x, this.z, this.y, this.x)

    member this.xzyy
        with get() =
            int4(this.x, this.z, this.y, this.y)

    member this.xzyz
        with get() =
            int4(this.x, this.z, this.y, this.z)

    member this.xzyw
        with get() =
            int4(this.x, this.z, this.y, this.w)
        and set(v: int4) =
            this.x <- v.x
            this.z <- v.y
            this.y <- v.z
            this.w <- v.w
            
    member this.xzzx
        with get() =
            int4(this.x, this.z, this.z, this.x)

    member this.xzzy
        with get() =
            int4(this.x, this.z, this.z, this.y)

    member this.xzzz
        with get() =
            int4(this.x, this.z, this.z, this.z)

    member this.xzzw
        with get() =
            int4(this.x, this.z, this.z, this.w)

    member this.xzwx
        with get() =
            int4(this.x, this.z, this.w, this.x)

    member this.xzwy
        with get() =
            int4(this.x, this.z, this.w, this.y)
        and set(v: int4) =
            this.x <- v.x
            this.z <- v.y
            this.w <- v.z
            this.y <- v.w
            
    member this.xzwz
        with get() =
            int4(this.x, this.z, this.w, this.z)

    member this.xzww
        with get() =
            int4(this.x, this.z, this.w, this.w)

    member this.xwxx
        with get() =
            int4(this.x, this.w, this.x, this.x)

    member this.xwxy
        with get() =
            int4(this.x, this.w, this.x, this.y)

    member this.xwxz
        with get() =
            int4(this.x, this.w, this.x, this.z)

    member this.xwxw
        with get() =
            int4(this.x, this.w, this.x, this.w)

    member this.xwyx
        with get() =
            int4(this.x, this.w, this.y, this.x)

    member this.xwyy
        with get() =
            int4(this.x, this.w, this.y, this.y)

    member this.xwyz
        with get() =
            int4(this.x, this.w, this.y, this.z)
        and set(v: int4) =
            this.x <- v.x
            this.w <- v.y
            this.y <- v.z
            this.z <- v.w
            
    member this.xwyw
        with get() =
            int4(this.x, this.w, this.y, this.w)

    member this.xwzx
        with get() =
            int4(this.x, this.w, this.z, this.x)

    member this.xwzy
        with get() =
            int4(this.x, this.w, this.z, this.y)
        and set(v: int4) =
            this.x <- v.x
            this.w <- v.y
            this.z <- v.z
            this.y <- v.w
            
    member this.xwzz
        with get() =
            int4(this.x, this.w, this.z, this.z)

    member this.xwzw
        with get() =
            int4(this.x, this.w, this.z, this.w)

    member this.xwwx
        with get() =
            int4(this.x, this.w, this.w, this.x)

    member this.xwwy
        with get() =
            int4(this.x, this.w, this.w, this.y)

    member this.xwwz
        with get() =
            int4(this.x, this.w, this.w, this.z)

    member this.xwww
        with get() =
            int4(this.x, this.w, this.w, this.w)

    member this.yxxx
        with get() =
            int4(this.y, this.x, this.x, this.x)

    member this.yxxy
        with get() =
            int4(this.y, this.x, this.x, this.y)

    member this.yxxz
        with get() =
            int4(this.y, this.x, this.x, this.z)

    member this.yxxw
        with get() =
            int4(this.y, this.x, this.x, this.w)

    member this.yxyx
        with get() =
            int4(this.y, this.x, this.y, this.x)

    member this.yxyy
        with get() =
            int4(this.y, this.x, this.y, this.y)

    member this.yxyz
        with get() =
            int4(this.y, this.x, this.y, this.z)

    member this.yxyw
        with get() =
            int4(this.y, this.x, this.y, this.w)

    member this.yxzx
        with get() =
            int4(this.y, this.x, this.z, this.x)

    member this.yxzy
        with get() =
            int4(this.y, this.x, this.z, this.y)

    member this.yxzz
        with get() =
            int4(this.y, this.x, this.z, this.z)

    member this.yxzw
        with get() =
            int4(this.y, this.x, this.z, this.w)
        and set(v: int4) =
            this.y <- v.x
            this.x <- v.y
            this.z <- v.z
            this.w <- v.w


    member this.yxwx
        with get() =
            int4(this.y, this.x, this.w, this.x)

    member this.yxwy
        with get() =
            int4(this.y, this.x, this.w, this.y)

    member this.yxwz
        with get() =
            int4(this.y, this.x, this.w, this.z)
        and set(v: int4) =
            this.y <- v.x
            this.x <- v.y
            this.w <- v.z
            this.z <- v.w
            
    member this.yxww
        with get() =
            int4(this.y, this.x, this.w, this.w)

    member this.yyxx
        with get() =
            int4(this.y, this.y, this.x, this.x)

    member this.yyxy
        with get() =
            int4(this.y, this.y, this.x, this.y)

    member this.yyxz
        with get() =
            int4(this.y, this.y, this.x, this.z)

    member this.yyxw
        with get() =
            int4(this.y, this.y, this.x, this.w)

    member this.yyyx
        with get() =
            int4(this.y, this.y, this.y, this.x)

    member this.yyyy
        with get() =
            int4(this.y, this.y, this.y, this.y)

    member this.yyyz
        with get() =
            int4(this.y, this.y, this.y, this.z)

    member this.yyyw
        with get() =
            int4(this.y, this.y, this.y, this.w)

    member this.yyzx
        with get() =
            int4(this.y, this.y, this.z, this.x)

    member this.yyzy
        with get() =
            int4(this.y, this.y, this.z, this.y)

    member this.yyzz
        with get() =
            int4(this.y, this.y, this.z, this.z)

    member this.yyzw
        with get() =
            int4(this.y, this.y, this.z, this.w)

    member this.yywx
        with get() =
            int4(this.y, this.y, this.w, this.x)

    member this.yywy
        with get() =
            int4(this.y, this.y, this.w, this.y)

    member this.yywz
        with get() =
            int4(this.y, this.y, this.w, this.z)

    member this.yyww
        with get() =
            int4(this.y, this.y, this.w, this.w)

    member this.yzxx
        with get() =
            int4(this.y, this.z, this.x, this.x)

    member this.yzxy
        with get() =
            int4(this.y, this.z, this.x, this.y)

    member this.yzxz
        with get() =
            int4(this.y, this.z, this.x, this.z)

    member this.yzxw
        with get() =
            int4(this.y, this.z, this.x, this.w)
        and set(v: int4) =
            this.y <- v.x
            this.z <- v.y
            this.x <- v.z
            this.w <- v.w
            
    member this.yzyx
        with get() =
            int4(this.y, this.z, this.y, this.x)

    member this.yzyy
        with get() =
            int4(this.y, this.z, this.y, this.y)

    member this.yzyz
        with get() =
            int4(this.y, this.z, this.y, this.z)

    member this.yzyw
        with get() =
            int4(this.y, this.z, this.y, this.w)

    member this.yzzx
        with get() =
            int4(this.y, this.z, this.z, this.x)

    member this.yzzy
        with get() =
            int4(this.y, this.z, this.z, this.y)

    member this.yzzz
        with get() =
            int4(this.y, this.z, this.z, this.z)

    member this.yzzw
        with get() =
            int4(this.y, this.z, this.z, this.w)

    member this.yzwx
        with get() =
            int4(this.y, this.z, this.w, this.x)
        and set(v: int4) =
            this.y <- v.x
            this.z <- v.y
            this.w <- v.z
            this.x <- v.w
            
    member this.yzwy
        with get() =
            int4(this.y, this.z, this.w, this.y)

    member this.yzwz
        with get() =
            int4(this.y, this.z, this.w, this.z)

    member this.yzww
        with get() =
            int4(this.y, this.z, this.w, this.w)

    member this.ywxx
        with get() =
            int4(this.y, this.w, this.x, this.x)

    member this.ywxy
        with get() =
            int4(this.y, this.w, this.x, this.y)

    member this.ywxz
        with get() =
            int4(this.y, this.w, this.x, this.z)
        and set(v: int4) =
            this.y <- v.x
            this.w <- v.y
            this.x <- v.z
            this.z <- v.w


    member this.ywxw
        with get() =
            int4(this.y, this.w, this.x, this.w)

    member this.ywyx
        with get() =
            int4(this.y, this.w, this.y, this.x)

    member this.ywyy
        with get() =
            int4(this.y, this.w, this.y, this.y)

    member this.ywyz
        with get() =
            int4(this.y, this.w, this.y, this.z)

    member this.ywyw
        with get() =
            int4(this.y, this.w, this.y, this.w)

    member this.ywzx
        with get() =
            int4(this.y, this.w, this.z, this.x)
        and set(v: int4) =
            this.y <- v.x
            this.w <- v.y
            this.z <- v.z
            this.x <- v.w
            
    member this.ywzy
        with get() =
            int4(this.y, this.w, this.z, this.y)

    member this.ywzz
        with get() =
            int4(this.y, this.w, this.z, this.z)

    member this.ywzw
        with get() =
            int4(this.y, this.w, this.z, this.w)

    member this.ywwx
        with get() =
            int4(this.y, this.w, this.w, this.x)

    member this.ywwy
        with get() =
            int4(this.y, this.w, this.w, this.y)

    member this.ywwz
        with get() =
            int4(this.y, this.w, this.w, this.z)

    member this.ywww
        with get() =
            int4(this.y, this.w, this.w, this.w)

    member this.zxxx
        with get() =
            int4(this.z, this.x, this.x, this.x)

    member this.zxxy
        with get() =
            int4(this.z, this.x, this.x, this.y)

    member this.zxxz
        with get() =
            int4(this.z, this.x, this.x, this.z)

    member this.zxxw
        with get() =
            int4(this.z, this.x, this.x, this.w)

    member this.zxyx
        with get() =
            int4(this.z, this.x, this.y, this.x)

    member this.zxyy
        with get() =
            int4(this.z, this.x, this.y, this.y)

    member this.zxyz
        with get() =
            int4(this.z, this.x, this.y, this.z)

    member this.zxyw
        with get() =
            int4(this.z, this.x, this.y, this.w)
        and set(v: int4) =
            this.z <- v.x
            this.x <- v.y
            this.y <- v.z
            this.w <- v.w
            
    member this.zxzx
        with get() =
            int4(this.z, this.x, this.z, this.x)

    member this.zxzy
        with get() =
            int4(this.z, this.x, this.z, this.y)

    member this.zxzz
        with get() =
            int4(this.z, this.x, this.z, this.z)

    member this.zxzw
        with get() =
            int4(this.z, this.x, this.z, this.w)

    member this.zxwx
        with get() =
            int4(this.z, this.x, this.w, this.x)

    member this.zxwy
        with get() =
            int4(this.z, this.x, this.w, this.y)
        and set(v: int4) =
            this.z <- v.x
            this.x <- v.y
            this.w <- v.z
            this.y <- v.w
            
    member this.zxwz
        with get() =
            int4(this.z, this.x, this.w, this.z)

    member this.zxww
        with get() =
            int4(this.z, this.x, this.w, this.w)

    member this.zyxx
        with get() =
            int4(this.z, this.y, this.x, this.x)

    member this.zyxy
        with get() =
            int4(this.z, this.y, this.x, this.y)

    member this.zyxz
        with get() =
            int4(this.z, this.y, this.x, this.z)

    member this.zyxw
        with get() =
            int4(this.z, this.y, this.x, this.w)
        and set(v: int4) =
            this.z <- v.x
            this.y <- v.y
            this.x <- v.z
            this.w <- v.w
            
    member this.zyyx
        with get() =
            int4(this.z, this.y, this.y, this.x)

    member this.zyyy
        with get() =
            int4(this.z, this.y, this.y, this.y)

    member this.zyyz
        with get() =
            int4(this.z, this.y, this.y, this.z)

    member this.zyyw
        with get() =
            int4(this.z, this.y, this.y, this.w)

    member this.zyzx
        with get() =
            int4(this.z, this.y, this.z, this.x)

    member this.zyzy
        with get() =
            int4(this.z, this.y, this.z, this.y)

    member this.zyzz
        with get() =
            int4(this.z, this.y, this.z, this.z)

    member this.zyzw
        with get() =
            int4(this.z, this.y, this.z, this.w)

    member this.zywx
        with get() =
            int4(this.z, this.y, this.w, this.x)
        and set(v: int4) =
            this.z <- v.x
            this.y <- v.y
            this.w <- v.z
            this.x <- v.w
            
    member this.zywy
        with get() =
            int4(this.z, this.y, this.w, this.y)

    member this.zywz
        with get() =
            int4(this.z, this.y, this.w, this.z)

    member this.zyww
        with get() =
            int4(this.z, this.y, this.w, this.w)

    member this.zzxx
        with get() =
            int4(this.z, this.z, this.x, this.x)

    member this.zzxy
        with get() =
            int4(this.z, this.z, this.x, this.y)

    member this.zzxz
        with get() =
            int4(this.z, this.z, this.x, this.z)

    member this.zzxw
        with get() =
            int4(this.z, this.z, this.x, this.w)

    member this.zzyx
        with get() =
            int4(this.z, this.z, this.y, this.x)

    member this.zzyy
        with get() =
            int4(this.z, this.z, this.y, this.y)

    member this.zzyz
        with get() =
            int4(this.z, this.z, this.y, this.z)

    member this.zzyw
        with get() =
            int4(this.z, this.z, this.y, this.w)

    member this.zzzx
        with get() =
            int4(this.z, this.z, this.z, this.x)

    member this.zzzy
        with get() =
            int4(this.z, this.z, this.z, this.y)

    member this.zzzz
        with get() =
            int4(this.z, this.z, this.z, this.z)

    member this.zzzw
        with get() =
            int4(this.z, this.z, this.z, this.w)

    member this.zzwx
        with get() =
            int4(this.z, this.z, this.w, this.x)

    member this.zzwy
        with get() =
            int4(this.z, this.z, this.w, this.y)

    member this.zzwz
        with get() =
            int4(this.z, this.z, this.w, this.z)

    member this.zzww
        with get() =
            int4(this.z, this.z, this.w, this.w)

    member this.zwxx
        with get() =
            int4(this.z, this.w, this.x, this.x)

    member this.zwxy
        with get() =
            int4(this.z, this.w, this.x, this.y)
        and set(v: int4) =
            this.z <- v.x
            this.w <- v.y
            this.x <- v.z
            this.y <- v.w
            
    member this.zwxz
        with get() =
            int4(this.z, this.w, this.x, this.z)

    member this.zwxw
        with get() =
            int4(this.z, this.w, this.x, this.w)

    member this.zwyx
        with get() =
            int4(this.z, this.w, this.y, this.x)
        and set(v: int4) =
            this.z <- v.x
            this.w <- v.y
            this.y <- v.z
            this.x <- v.w
            
    member this.zwyy
        with get() =
            int4(this.z, this.w, this.y, this.y)

    member this.zwyz
        with get() =
            int4(this.z, this.w, this.y, this.z)

    member this.zwyw
        with get() =
            int4(this.z, this.w, this.y, this.w)

    member this.zwzx
        with get() =
            int4(this.z, this.w, this.z, this.x)

    member this.zwzy
        with get() =
            int4(this.z, this.w, this.z, this.y)

    member this.zwzz
        with get() =
            int4(this.z, this.w, this.z, this.z)

    member this.zwzw
        with get() =
            int4(this.z, this.w, this.z, this.w)

    member this.zwwx
        with get() =
            int4(this.z, this.w, this.w, this.x)

    member this.zwwy
        with get() =
            int4(this.z, this.w, this.w, this.y)

    member this.zwwz
        with get() =
            int4(this.z, this.w, this.w, this.z)

    member this.zwww
        with get() =
            int4(this.z, this.w, this.w, this.w)

    member this.wxxx
        with get() =
            int4(this.w, this.x, this.x, this.x)

    member this.wxxy
        with get() =
            int4(this.w, this.x, this.x, this.y)

    member this.wxxz
        with get() =
            int4(this.w, this.x, this.x, this.z)

    member this.wxxw
        with get() =
            int4(this.w, this.x, this.x, this.w)

    member this.wxyx
        with get() =
            int4(this.w, this.x, this.y, this.x)

    member this.wxyy
        with get() =
            int4(this.w, this.x, this.y, this.y)

    member this.wxyz
        with get() =
            int4(this.w, this.x, this.y, this.z)
        and set(v: int4) =
            this.w <- v.x
            this.x <- v.y
            this.y <- v.z
            this.z <- v.w
            
    member this.wxyw
        with get() =
            int4(this.w, this.x, this.y, this.w)

    member this.wxzx
        with get() =
            int4(this.w, this.x, this.z, this.x)

    member this.wxzy
        with get() =
            int4(this.w, this.x, this.z, this.y)
        and set(v: int4) =
            this.w <- v.x
            this.x <- v.y
            this.z <- v.z
            this.y <- v.w


    member this.wxzz
        with get() =
            int4(this.w, this.x, this.z, this.z)

    member this.wxzw
        with get() =
            int4(this.w, this.x, this.z, this.w)

    member this.wxwx
        with get() =
            int4(this.w, this.x, this.w, this.x)

    member this.wxwy
        with get() =
            int4(this.w, this.x, this.w, this.y)

    member this.wxwz
        with get() =
            int4(this.w, this.x, this.w, this.z)

    member this.wxww
        with get() =
            int4(this.w, this.x, this.w, this.w)

    member this.wyxx
        with get() =
            int4(this.w, this.y, this.x, this.x)

    member this.wyxy
        with get() =
            int4(this.w, this.y, this.x, this.y)

    member this.wyxz
        with get() =
            int4(this.w, this.y, this.x, this.z)
        and set(v: int4) =
            this.w <- v.x
            this.y <- v.y
            this.x <- v.z
            this.z <- v.w
            
    member this.wyxw
        with get() =
            int4(this.w, this.y, this.x, this.w)

    member this.wyyx
        with get() =
            int4(this.w, this.y, this.y, this.x)

    member this.wyyy
        with get() =
            int4(this.w, this.y, this.y, this.y)

    member this.wyyz
        with get() =
            int4(this.w, this.y, this.y, this.z)

    member this.wyyw
        with get() =
            int4(this.w, this.y, this.y, this.w)

    member this.wyzx
        with get() =
            int4(this.w, this.y, this.z, this.x)
        and set(v: int4) =
            this.w <- v.x
            this.y <- v.y
            this.z <- v.z
            this.x <- v.w
            
    member this.wyzy
        with get() =
            int4(this.w, this.y, this.z, this.y)

    member this.wyzz
        with get() =
            int4(this.w, this.y, this.z, this.z)

    member this.wyzw
        with get() =
            int4(this.w, this.y, this.z, this.w)

    member this.wywx
        with get() =
            int4(this.w, this.y, this.w, this.x)

    member this.wywy
        with get() =
            int4(this.w, this.y, this.w, this.y)

    member this.wywz
        with get() =
            int4(this.w, this.y, this.w, this.z)

    member this.wyww
        with get() =
            int4(this.w, this.y, this.w, this.w)

    member this.wzxx
        with get() =
            int4(this.w, this.z, this.x, this.x)

    member this.wzxy
        with get() =
            int4(this.w, this.z, this.x, this.y)
        and set(v: int4) =
            this.w <- v.x
            this.z <- v.y
            this.x <- v.z
            this.y <- v.w
            
    member this.wzxz
        with get() =
            int4(this.w, this.z, this.x, this.z)

    member this.wzxw
        with get() =
            int4(this.w, this.z, this.x, this.w)

    member this.wzyx
        with get() =
            int4(this.w, this.z, this.y, this.x)
        and set(v: int4) =
            this.w <- v.x
            this.z <- v.y
            this.y <- v.z
            this.x <- v.w
            
    member this.wzyy
        with get() =
            int4(this.w, this.z, this.y, this.y)

    member this.wzyz
        with get() =
            int4(this.w, this.z, this.y, this.z)

    member this.wzyw
        with get() =
            int4(this.w, this.z, this.y, this.w)

    member this.wzzx
        with get() =
            int4(this.w, this.z, this.z, this.x)

    member this.wzzy
        with get() =
            int4(this.w, this.z, this.z, this.y)

    member this.wzzz
        with get() =
            int4(this.w, this.z, this.z, this.z)

    member this.wzzw
        with get() =
            int4(this.w, this.z, this.z, this.w)

    member this.wzwx
        with get() =
            int4(this.w, this.z, this.w, this.x)

    member this.wzwy
        with get() =
            int4(this.w, this.z, this.w, this.y)

    member this.wzwz
        with get() =
            int4(this.w, this.z, this.w, this.z)

    member this.wzww
        with get() =
            int4(this.w, this.z, this.w, this.w)

    member this.wwxx
        with get() =
            int4(this.w, this.w, this.x, this.x)

    member this.wwxy
        with get() =
            int4(this.w, this.w, this.x, this.y)

    member this.wwxz
        with get() =
            int4(this.w, this.w, this.x, this.z)

    member this.wwxw
        with get() =
            int4(this.w, this.w, this.x, this.w)

    member this.wwyx
        with get() =
            int4(this.w, this.w, this.y, this.x)

    member this.wwyy
        with get() =
            int4(this.w, this.w, this.y, this.y)

    member this.wwyz
        with get() =
            int4(this.w, this.w, this.y, this.z)

    member this.wwyw
        with get() =
            int4(this.w, this.w, this.y, this.w)

    member this.wwzx
        with get() =
            int4(this.w, this.w, this.z, this.x)

    member this.wwzy
        with get() =
            int4(this.w, this.w, this.z, this.y)

    member this.wwzz
        with get() =
            int4(this.w, this.w, this.z, this.z)

    member this.wwzw
        with get() =
            int4(this.w, this.w, this.z, this.w)

    member this.wwwx
        with get() =
            int4(this.w, this.w, this.w, this.x)

    member this.wwwy
        with get() =
            int4(this.w, this.w, this.w, this.y)

    member this.wwwz
        with get() =
            int4(this.w, this.w, this.w, this.z)

    member this.wwww
        with get() =
            int4(this.w, this.w, this.w, this.w)

    member this.lo 
        with get() =
            int2(this.x, this.y)
        and set (v:int2) =
            this.x <- v.x
            this.y <- v.y
            
    member this.hi 
        with get() =
            int2(this.y, this.w)
        and set (v:int2) =
            this.z <- v.x
            this.w <- v.y
            
    member this.even 
        with get() =
            int2(this.x, this.z)
        and set (v:int2) =
            this.x <- v.x
            this.z <- v.y
            
    member this.odd 
        with get() =
            int2(this.y, this.w)
        and set (v:int2) =
            this.y <- v.x
            this.w <- v.y

    new() =
        int4(0, 0, 0, 0)

    internal new(c: int32[]) =
        int4(c.[0], c.[1], c.[2], c.[3])

    static member (+) (f1: int4, f2: int4) =
        int4(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: int4, f2: int4) =
        int4(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: int4, f2: int4) =
        int4(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: int4, f2: int4) =
        int4(Array.map2 (/) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: int4, f2: int4) =
        int4(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: int4, f2: int4) =
        int4(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: int4, f2: int4) =
        int4(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: int4, f2: int4) =
        int4(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
        
// **************************************************************************************************************
