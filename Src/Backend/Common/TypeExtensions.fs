[<AutoOpen>]
module MusicPlayerBackend.Common.TypeExtensions

#nowarn "0042"

// By default, function should pass value without modification (fault-tolerant)
// For example, "ToUpper" on null string should return null

open System.Collections.Generic

[<RequireQualifiedAccess>]
module Option =
    let unionUnit = Some ()

    let inline ofTry (opResult, result) =
        if opResult then
            Some result
        else
            None

    let inline ofBool (v: bool) =
        if v then
            unionUnit else None

    let inline ofStringW (str: string) =
        if str |> System.String.IsNullOrWhiteSpace then
            None
        else
            Some str

[<RequireQualifiedAccess>]
module ValueOption =
    let inline ofTry (opResult, result) =
        if opResult then
            ValueSome result
        else
            ValueNone

[<RequireQualifiedAccess>]
module Result =
    let isOk = function
        | Error _ -> false
        | Ok _ -> true

let inline isNotNull value =
    value
    |> isNull
    |> not

let inline (^) f x = f x
let inline (~%) x = ignore x
let inline ucast<'a, 'b> (a: 'a): 'b = (# "" a: 'b #)
let inline tryUcast<'a, 'b> (a: 'a): 'b option =
    if typeof<'b>.IsAssignableFrom(typeof<'a>) then
        Some ^ ucast a
    else
        None

let inline (|NotNull|_|) value =
    value
    |> isNotNull
    |> Option.ofBool

let inline (|Null|_|) value =
    value
    |> isNull
    |> Option.ofBool

let inline (|Eq|_|) a b =
    a = b
    |> Option.ofBool

let inline (|NEq|_|) a b =
    b = a
    |> not
    |> Option.ofBool

let inline (|Gt|_|) a b =
    b > a
    |> Option.ofBool

let inline (|Lt|_|) a b =
    b < a
    |> Option.ofBool

let inline (|LtEq|_|) a b =
    b <= a
    |> Option.ofBool

let inline (|GtEq|_|) a b =
    b >= a
    |> Option.ofBool

[<RequireQualifiedAccess>]
module String =
    let inline toLowerInv (s: string) =
        s.ToLowerInvariant()

    let inline toUpperInv (s: string) =
        s.ToUpperInvariant()

[<RequireQualifiedAccess>]
module StringOption =
    let inline toLowerInv v =
        v |> Option.map String.toLowerInv

    let inline toUpperInv v =
        v |> Option.map String.toUpperInv

[<RequireQualifiedAccess>]
module Task =
    open System.Threading.Tasks

    let bTrue = Task.FromResult(true)
    let bFalse = Task.FromResult(false)
    let completed = Task.CompletedTask

    let inline map ([<InlineIfLambda>] mapping) taskValue = task {
        let! taskValue = taskValue
        return mapping taskValue
    }

    let inline fromResult value = Task.FromResult(value)

[<RequireQualifiedAccess>]
module TaskOption =
    let inline map ([<InlineIfLambda>] mapping) option = task {
        let! option = option
        return option |> Option.map mapping
    }

[<RequireQualifiedAccess>]
module Dictionary =
    let inline tryGet key (dict: Dictionary<'TKey, 'TValue>) =
        dict.TryGetValue(key) |> Option.ofTry

    let inline tryFindByValue predicate (dict: Dictionary<'TKey, 'TValue>) =
        dict.Values |> Seq.tryFind predicate
