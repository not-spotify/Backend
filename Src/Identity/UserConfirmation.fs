namespace MusicPlayerBackend.Identity

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open MusicPlayerBackend.Data.Entities

// TODO: Implement mail confirmation
[<Sealed>]
type UserConfirmation() =
    interface IUserConfirmation<User> with
        member this.IsConfirmedAsync(_, _) =
            Task.FromResult(true)
