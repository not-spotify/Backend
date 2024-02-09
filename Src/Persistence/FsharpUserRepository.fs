namespace MusicPlayerBackend.Persistence

open System
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities.User

type FsharpUserRepository(dbContext: FsharpAppDbContext) =
    let users = dbContext.Set<User>()

    member _.QueryAll() = users.AsQueryable()

    member _.Save(playlist) =
        if playlist.Id = Guid.Empty && playlist.CreatedAt = DateTimeOffset.MinValue then
            playlist.CreatedAt <- DateTimeOffset.UtcNow
        elif playlist.Id <> Guid.Empty then
            let playlistEntry = dbContext.Entry(playlist)
            match playlistEntry.State with
            | EntityState.Modified when playlistEntry.Property("UpdatedAt").IsModified |> not ->
                playlist.UpdatedAt <- ValueSome DateTimeOffset.UtcNow
            | EntityState.Detached ->
                raise ^ InvalidOperationException("Can't save detached playlist.")
            | _ -> ()

        users.Update(playlist)



    member _.TryGetById(id: Id, ?ct) = task {
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
