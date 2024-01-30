[<AutoOpen>]
module MusicPlayerBackend.Common.TypeExtensions

// By default, function should pass value without modification
// For example, "ToUpper" on null string should return null

module Option =
    let unionUnit = Some ()

    let inline ofBool (v: bool) =
        if v then
            unionUnit else None

    let inline ofStringW (str: string) =
        if str |> System.String.IsNullOrWhiteSpace then
            None
        else
            Some str

    let inline ofTryPattern (success, result) =
        if success then
            Some result else None

let inline isNotNull value =
    value
    |> isNull
    |> not

let inline (^) f x = f x
let inline (~%) x = ignore x

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

module String =
    let inline toLowerInv (s: string) =
        s.ToLowerInvariant()

    let inline toUpperInv (s: string) =
        s.ToUpperInvariant()

module StringOption =
    let inline toLowerInv v =
        v |> Option.map String.toLowerInv

    let inline toUpperInv v =
        v |> Option.map String.toUpperInv

