namespace Contracts.Shared

open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns

type AuthorName = internal AuthorName of string

type AuthorNameError =
    | TooShort of minLength: int * actualLength: int
    | TooLong of maxLength: int * actualLength: int

[<RequireQualifiedAccess>]
module AuthorName =

    let MinLength = 3
    let MaxLength = 20

    let create (name: string) =
        match name with
        | String.Length (LtEq MinLength as actualLength) ->
            Error (AuthorNameError.TooShort (MinLength, actualLength))
        | String.Length (GtEq MaxLength as actualLength) ->
            Error (AuthorNameError.TooLong (MaxLength, actualLength))
        | _ ->
            Ok (AuthorName name)
