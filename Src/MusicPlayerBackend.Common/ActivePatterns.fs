namespace MusicPlayerBackend.Common.ActivePatterns

open  MusicPlayerBackend.Common.TypeExtensions

[<AutoOpen>]
module String =
    open System

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
