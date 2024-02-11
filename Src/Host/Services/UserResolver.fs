namespace MusicPlayerBackend.Host

open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence.Entities

[<Sealed>]
type UserProvider(httpContextAccessor: IHttpContextAccessor, userManager: UserManager<User>) =
    member this.GetUser() = task {
        return! userManager.GetUserAsync(httpContextAccessor.HttpContext.User)
    }

    member this.GetUserId() = task {
        return!
            this.GetUser()
            |> Task.map _.Id
    }

    member this.TryGetUserId() = task {
        return!
            this.TryGetUser()
            |> TaskOption.map _.Id
    }

    // TODO: Don't use database model
    member this.TryGetUser() : Task<User option> = task {
        match Option.ofObj httpContextAccessor.HttpContext.User with
        | None ->
            return None
        | Some user ->
            let! user = userManager.GetUserAsync(user)
            return Some user
    }
