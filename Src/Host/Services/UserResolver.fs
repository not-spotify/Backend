namespace MusicPlayerBackend.Host

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity

open MusicPlayerBackend.Data.Entities

[<Sealed>]
type UserProvider(httpContextAccessor: IHttpContextAccessor, userManager: UserManager<User>) =
    member this.GetUser() = task {
        return! userManager.GetUserAsync(httpContextAccessor.HttpContext.User)
    }

    member this.GetUserId() = task {
        let! user = this.GetUser()
        return user.Id
    }

    member this.TryGetUserId() = task {
        let! user = this.TryGetUser()
        return user |> Option.map (fun (u: User) -> u.Id)
    }

    member this.TryGetUser() = task {
        match Option.ofObj httpContextAccessor.HttpContext.User with
        | None ->
            return None
        | Some user ->
            let! user = userManager.GetUserAsync(user)
            return Some user
    }
