namespace Domain.Shared

open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns

type Email = internal Email of string

[<RequireQualifiedAccess>]
type EmailError =
    | TooShort of minLength: int * actualLength: int
    | TooLong of maxLength: int * actualLength: int

[<RequireQualifiedAccess>]
module Email =

    let MinLength = 3
    let MaxLength = 20

    let create (email: string) =
        match email with
        | String.Length (LtEq MinLength as actualLength) ->
            Error (EmailError.TooShort (MinLength, actualLength))
        | String.Length (GtEq MaxLength as actualLength) ->
            Error (EmailError.TooLong (MaxLength, actualLength))
        | _ ->
            Ok (Email email)
