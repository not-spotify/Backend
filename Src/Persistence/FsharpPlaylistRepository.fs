namespace MusicPlayerBackend.Persistence

open System
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities.Playlist

[<Sealed>]
type FsharpPlaylistRepository(dbContext: FsharpAppDbContext) =
    let playlists = dbContext.Set<Playlist>()

    member _.QueryAll() = playlists.AsQueryable()

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

        playlists.Update(playlist)

    member _.Delete(playlist) = playlists.Remove(playlist)

    member _.TryGetById(id: Id, ?ct) = task {
        let getByIdQuery =
            query {
                for playlist in playlists do
                    where (playlist.Id = id)
                    select playlist
            }

        let! playlist =
            match ct with
            | None ->
                getByIdQuery.SingleOrDefaultAsync()
            | Some ct ->
                getByIdQuery.SingleOrDefaultAsync(ct)

        return Option.ofUncheckedObj playlist
    }
