namespace MusicPlayerBackend.Persistence

open System
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities

type FsharpUserRepository(dbContext: FsharpAppDbContext) =
    let users = dbContext.Set<User>()

    member _.QueryAll() = users.AsQueryable()

    member _.Delete(user) = users.Remove(user) |> ignore

    member _.Save(user: User) =
        if user.Id = Guid.Empty && user.CreatedAt = DateTimeOffset.MinValue then
            user.CreatedAt <- DateTimeOffset.UtcNow
        elif user.Id <> Guid.Empty then
            let userEntry = dbContext.Entry(user)
            match userEntry.State with
            | EntityState.Modified when userEntry.Property(nameof user.Id).IsModified |> not ->
                user.UpdatedAt <- ValueSome DateTimeOffset.UtcNow
            | EntityState.Detached ->
                raise ^ InvalidOperationException("Can't save detached playlist.")
            | _ -> ()

        users.Update(user)

    member _.TryGetById(id: UserId, ?ct) = task {
        let getByIdQuery =
            query {
                for user in users do
                    where (user.Id = id)
                    select user
            }

        let! playlist =
            match ct with
            | None ->
                getByIdQuery.SingleOrDefaultAsync()
            | Some ct ->
                getByIdQuery.SingleOrDefaultAsync(ct)

        return Option.ofUncheckedObj playlist
    }
