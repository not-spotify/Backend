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
    let create (userName: string) =
        match userName with
        | String.Length (LtEq 3) ->
            Error UserNameError.TooShort
        | String.Length (GtEq 20) ->
            Error UserNameError.TooLong
        | _ ->
            Ok ^ UserName userName

    let value (UserName userName) = userName
