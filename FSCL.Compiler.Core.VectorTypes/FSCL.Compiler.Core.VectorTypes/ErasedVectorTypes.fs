namespace FSCL.Compiler.Types

open System

// Erased type
type FloatVector(components: float32 array) =

    member val Components = components with get
    member val Count = components.Length with get

    member this.Get(i) =
        this.Components.[i]
    member this.Set(i, v) =
        this.Components.[i] <- v

    new(count: int) =
        FloatVector(Array.zeroCreate(count))

    static member (+) (f1: FloatVector, f2: FloatVector) =
        FloatVector(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: FloatVector, f2: FloatVector) =
        FloatVector(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: FloatVector, f2: FloatVector) =
        FloatVector(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: FloatVector, f2: FloatVector) =
        FloatVector(Array.map2 (/) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: FloatVector, f2: FloatVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: FloatVector, f2: FloatVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: FloatVector, f2: FloatVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: FloatVector, f2: FloatVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))

// Erased type
and IntVector(components: int array) =
    member val Components = components with get
    member val Count = components.Length with get

    member this.Get(i) =
        this.Components.[i]
    member this.Set(i, v) =
        this.Components.[i] <- v

    new(count: int) =
        IntVector(Array.zeroCreate(count))

    static member (+) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (/) (f1.Components) (f2.Components))
    static member (%) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (%) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: IntVector, f2: IntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
        
// Erased type
type LongVector(components: int64 array) =
    member val Components = components with get
    member val Count = components.Length with get

    member this.Get(i) =
        this.Components.[i]
    member this.Set(i, v) =
        this.Components.[i] <- v

    new(count: int) =
        LongVector(Array.zeroCreate(count))
        
    static member (+) (f1: LongVector, f2: LongVector) =
        LongVector(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: LongVector, f2: LongVector) =
        LongVector(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: LongVector, f2: LongVector) =
        LongVector(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: LongVector, f2: LongVector) =
        LongVector(Array.map2 (/) (f1.Components) (f2.Components))
    static member (%) (f1: LongVector, f2: LongVector) =
        LongVector(Array.map2 (%) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: LongVector, f2: LongVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: LongVector, f2: LongVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: LongVector, f2: LongVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: LongVector, f2: LongVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))

// Erased type
type CharVector(components: int8 array) =
    member val Components = components with get
    member val Count = components.Length with get

    member this.Get(i) =
        this.Components.[i]
    member this.Set(i, v) =
        this.Components.[i] <- v

    new(count: int) =
        CharVector(Array.zeroCreate(count))

    static member (+) (f1: CharVector, f2: CharVector) =
        CharVector(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: CharVector, f2: CharVector) =
        CharVector(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: CharVector, f2: CharVector) =
        CharVector(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: CharVector, f2: CharVector) =
        CharVector(Array.map2 (/) (f1.Components) (f2.Components))
    static member (%) (f1: CharVector, f2: CharVector) =
        CharVector(Array.map2 (%) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: CharVector, f2: CharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: CharVector, f2: CharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: CharVector, f2: CharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: CharVector, f2: CharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))
        
// Erased type
type UCharVector(components: uint8 array) =
    member val Components = components with get
    member val Count = components.Length with get

    member this.Get(i) =
        this.Components.[i]
    member this.Set(i, v) =
        this.Components.[i] <- v

    new(count: int) =
        UCharVector(Array.zeroCreate(count))
        
    static member (+) (f1: UCharVector, f2: UCharVector) =
        UCharVector(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: UCharVector, f2: UCharVector) =
        UCharVector(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: UCharVector, f2: UCharVector) =
        UCharVector(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: UCharVector, f2: UCharVector) =
        UCharVector(Array.map2 (/) (f1.Components) (f2.Components))
    static member (%) (f1: UCharVector, f2: UCharVector) =
        UCharVector(Array.map2 (%) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: UCharVector, f2: UCharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: UCharVector, f2: UCharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: UCharVector, f2: UCharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: UCharVector, f2: UCharVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))

// Erased type
type UIntVector(components: uint32 array) =
    member val Components = components with get
    member val Count = components.Length with get

    member this.Get(i) =
        this.Components.[i]
    member this.Set(i, v) =
        this.Components.[i] <- v

    new(count: int) =
        UIntVector(Array.zeroCreate(count))
        
    static member (+) (f1: UIntVector, f2: UIntVector) =
        UIntVector(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: UIntVector, f2: UIntVector) =
        UIntVector(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: UIntVector, f2: UIntVector) =
        UIntVector(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: UIntVector, f2: UIntVector) =
        UIntVector(Array.map2 (/) (f1.Components) (f2.Components))
    static member (%) (f1: UIntVector, f2: UIntVector) =
        UIntVector(Array.map2 (%) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: UIntVector, f2: UIntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: UIntVector, f2: UIntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: UIntVector, f2: UIntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: UIntVector, f2: UIntVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))

// Erased type
type DoubleVector(components: double array) =
    member val Components = components with get
    member val Count = components.Length with get

    member this.Get(i) =
        this.Components.[i]
    member this.Set(i, v) =
        this.Components.[i] <- v

    new(count: int) =
        DoubleVector(Array.zeroCreate(count))
        
    static member (+) (f1: DoubleVector, f2: DoubleVector) =
        DoubleVector(Array.map2 (+) (f1.Components) (f2.Components))
    static member (-) (f1: DoubleVector, f2: DoubleVector) =
        DoubleVector(Array.map2 (-) (f1.Components) (f2.Components))
    static member (*) (f1: DoubleVector, f2: DoubleVector) =
        DoubleVector(Array.map2 (*) (f1.Components) (f2.Components))
    static member (/) (f1: DoubleVector, f2: DoubleVector) =
        DoubleVector(Array.map2 (/) (f1.Components) (f2.Components))
        
    static member (>>=) (f1: DoubleVector, f2: DoubleVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 >= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<<=) (f1: DoubleVector, f2: DoubleVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <= e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (===) (f1: DoubleVector, f2: DoubleVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 = e2 then -1 else 0) (f1.Components) (f2.Components))
    static member (<=>) (f1: DoubleVector, f2: DoubleVector) =
        IntVector(Array.map2 (fun e1 e2 -> if e1 <> e2 then -1 else 0) (f1.Components) (f2.Components))

