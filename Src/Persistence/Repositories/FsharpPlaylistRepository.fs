namespace MusicPlayerBackend.Persistence

open System
open System.Linq
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.Persistence.Repositories

[<Sealed>]
type FsharpPlaylistRepository(dbContext: FsharpAppDbContext) =
    let playlists = dbContext.Set<Playlist>()

    member _.Query with get() = playlists.AsQueryable()

    member _.Save(playlist : Playlist) =
        if playlist.Id = Guid.Empty && playlist.CreatedAt = DateTimeOffset.MinValue then
            playlist.CreatedAt <- DateTimeOffset.UtcNow
        elif playlist.Id <> Guid.Empty then
            let playlistEntry = dbContext.Entry(playlist)
            match playlistEntry.State with
            | EntityState.Modified when playlistEntry.Property(nameof playlist.UpdatedAt).IsModified |> not ->
                playlist.UpdatedAt <- Some DateTimeOffset.UtcNow
            | EntityState.Detached ->
                raise ^ InvalidOperationException("Can't save detached playlist.")
            | _ -> ()

        playlists.Update(playlist)

    member _.Delete(playlist) = playlists.Remove(playlist)

    member _.TryGetById(id: PlaylistId, ?ct) = task {
        return!
            query {
                for playlist in playlists do
                    where (playlist.Id = id)
                    select playlist
            } |> _.TrySingle(ct)
    }

    member _.GetVisibleTracks(id: PlaylistId, page: int, pageSize: int, mapping, ?ct) = task {
        let visibleTracksQuery =
            query {
                for playlist in playlists do
                    where (playlist.Id = id)
                    for track in playlist.TrackPlaylists do
                        select (mapping track.Track)
            }

        let! total = visibleTracksQuery.Count(ct)
        let! result = visibleTracksQuery.Skip(page * pageSize).Take(pageSize).ToArray(ct)
        return total, result
    }
