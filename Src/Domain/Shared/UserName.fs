namespace Domain.Shared

open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns

type UserName = internal UserName of string

[<RequireQualifiedAccess>]
type UserNameError =
    | TooShort
    | TooLong

[<RequireQualifiedAccess>]
module UserName =
    let [<Literal>] MinLength = 3
    let [<Literal>] MaxLength = 20

    let create (userName: string) =
        match userName with
        | String.Length (LtEq MinLength) ->
            Error UserNameError.TooShort
        | String.Length (GtEq MaxLength) ->
            Error UserNameError.TooLong
        | _ ->
            Ok ^ UserName userName

    let value (UserName userName) = userName
