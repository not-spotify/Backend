namespace Domain.Shared

open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns

type Password = internal Password of string

[<RequireQualifiedAccess>]
type PasswordError =
    | TooShort of minLength: int * actualLength: int
    | TooLong of maxLength: int * actualLength: int

[<RequireQualifiedAccess>]
module Password =

    let [<Literal>] MinLength = 8
    let [<Literal>] MaxLength = 67

    let create (password: string) =
        match password with
        | String.Length (LtEq MinLength as actualLength) ->
            Error (PasswordError.TooShort (MinLength, actualLength))

        | String.Length (GtEq MaxLength as actualLength) ->
            Error (PasswordError.TooLong (MaxLength, actualLength))

        | _ ->
            Ok (Password password)

    let value (Password password) = password
