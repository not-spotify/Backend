namespace MusicPlayerBackend.Identity

open Microsoft.AspNetCore.Identity

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

[<Sealed>]
type UserConfirmation() =
    interface IUserConfirmation<User> with
        member this.IsConfirmedAsync(_, _) =
            Task.bTrue
