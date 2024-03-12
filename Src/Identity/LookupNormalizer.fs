namespace MusicPlayerBackend.Identity

open Microsoft.AspNetCore.Identity
open MusicPlayerBackend.Common

[<Sealed>]
type LookupNormalizer() =
    interface ILookupNormalizer with
        member this.NormalizeEmail(email) =
            email
            |> Option.ofStringW
            |> StringOption.toUpperInv
            |> Option.toObj

        member this.NormalizeName(name) =
            name
            |> Option.ofStringW
            |> StringOption.toUpperInv
            |> Option.toObj
