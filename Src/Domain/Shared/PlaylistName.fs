namespace Contracts.Shared

open MusicPlayerBackend.Common
open MusicPlayerBackend.Common.ActivePatterns

type PlaylistName = internal PlaylistName of string

type PlaylistNameError =
    | TooShort of minLength: int * actualLength: int
    | TooLong of maxLength: int * actualLength: int

[<RequireQualifiedAccess>]
module PlaylistName =

    let MinLength = 3
    let MaxLength = 20

    let create (name: string) =
        match name with
        | String.Length (LtEq MinLength as actualLength) ->
            Error (PlaylistNameError.TooShort (MinLength, actualLength))
        | String.Length (GtEq MaxLength as actualLength) ->
            Error (PlaylistNameError.TooLong (MaxLength, actualLength))
        | _ ->
            Ok (PlaylistName name)
