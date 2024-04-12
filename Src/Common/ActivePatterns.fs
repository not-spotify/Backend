namespace MusicPlayerBackend.Common.ActivePatterns

open System
open  MusicPlayerBackend.Common.TypeExtensions

[<AutoOpen>]
module String =
    let inline (|NullOrEmpty|_|) (str: string) =
        String.IsNullOrEmpty(str)
        |> Option.ofBool

    let inline (|NotNullOrEmpty|_|) (str: string) =
        String.IsNullOrEmpty(str)
        |> not
        |> Option.ofBool

    let inline (|NullOrWhiteSpace|_|) (str: string) =
        String.IsNullOrWhiteSpace(str)
        |> Option.ofBool

    let inline (|NotNullOrWhiteSpace|_|) (str: string) =
        String.IsNullOrWhiteSpace(str)
        |> not
        |> Option.ofBool

    let inline (|Length|) (str: string) =
        str
        |> String.length

[<AutoOpen>]
module Object =
    let inline (|UncheckedNull|_|) obj =
        if Object.ReferenceEquals(obj, null) then
            Option.unionUnit
        else
            None

module FSharpType =
    open Microsoft.FSharp.Reflection

    let inline (|Function|Module|Tuple|Record|Union|ExceptionRepresentation|Object|) t =
        if FSharpType.IsFunction t then
            Function
        elif FSharpType.IsModule t then
            Module
        elif FSharpType.IsTuple t then
            Tuple
        elif FSharpType.IsRecord t then
            Record
        elif FSharpType.IsUnion t then
            Union
        elif FSharpType.IsExceptionRepresentation t then
            ExceptionRepresentation
        else
            Object
