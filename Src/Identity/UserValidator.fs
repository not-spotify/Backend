namespace MusicPlayerBackend.Identity

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

type UserValidator() =
    interface IUserValidator<User> with
        member this.ValidateAsync(_, _) =
            IdentityResult.Success
            |> Task.fromResult
