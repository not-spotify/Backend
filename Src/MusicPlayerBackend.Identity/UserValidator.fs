namespace MusicPlayerBackend.Identity

open System.Threading.Tasks
open Microsoft.AspNetCore.Identity
open MusicPlayerBackend.Data.Entities

type UserValidator() =
    interface IUserValidator<User> with
        member this.ValidateAsync(_, _) =
            Task.FromResult(IdentityResult.Success)
