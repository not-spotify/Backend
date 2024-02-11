namespace MusicPlayerBackend.Persistence

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.Persistence.Repositories

[<Sealed>]
type FsharpTrackPlaylistRepository(dbContext: FsharpAppDbContext) =
    let trackPlaylists = dbContext.Set<TrackPlaylist>()

    member _.Query with get() = trackPlaylists.AsQueryable()

    member _.AddTrackIfNotAdded(playlistId, trackId, ?ct) = task {
        let! existingTrackPlaylist =
            query {
                for tp in trackPlaylists do
                    where (tp.TrackId = trackId && tp.PlaylistId = playlistId)
            } |> _.Any(ct)

        match existingTrackPlaylist with
        | false ->
            let trackPlaylist = TrackPlaylist.Create(trackId, playlistId)
            %trackPlaylists.Update(trackPlaylist)
        | true ->
            ()
    }

    member _.Delete(playlist) = trackPlaylists.Remove(playlist)
