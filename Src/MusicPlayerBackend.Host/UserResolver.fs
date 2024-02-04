namespace MusicPlayerBackend.Host

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity

open MusicPlayerBackend.Data.Entities

type IUserProvider =
    abstract GetUser: unit -> Task<User>
    abstract TryGetUser: unit -> Task<User option>
    abstract GetUserId: unit -> Task<Guid>
    abstract TryGetUserId: unit -> Task<Guid option>

[<Sealed>]
type UserProvider(httpContextAccessor: IHttpContextAccessor, userManager: UserManager<User>) as this =
    let self = (this :> IUserProvider)

    interface IUserProvider with
        member this.GetUser() = task {
            return! userManager.GetUserAsync(httpContextAccessor.HttpContext.User)
        }

        member this.GetUserId() = task {
            let! user = self.GetUser()
            return user.Id
        }

        member this.TryGetUserId() = task {
            let! user = self.TryGetUser()
            return user |> Option.map _.Id
        }

        member this.TryGetUser() = task {
            match Option.ofObj httpContextAccessor.HttpContext.User with
            | None ->
                return None
            | Some user ->
                let! user = userManager.GetUserAsync(user)
                return Some user
        }
