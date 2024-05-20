namespace Contracts.Shared

open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns

type TrackName = internal TrackName of string

type TrackNameError =
    | TooShort of minLength: int * actualLength: int
    | TooLong of maxLength: int * actualLength: int

[<RequireQualifiedAccess>]
module TrackName =

    let MinLength = 3
    let MaxLength = 20

    let create (name: string) =
        match name with
        | String.Length (LtEq MinLength as actualLength) ->
            Error (TrackNameError.TooShort (MinLength, actualLength))
        | String.Length (GtEq MaxLength as actualLength) ->
            Error (TrackNameError.TooLong (MaxLength, actualLength))
        | _ ->
            Ok (TrackName name)

