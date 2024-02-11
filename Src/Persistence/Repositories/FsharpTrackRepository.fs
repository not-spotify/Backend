namespace MusicPlayerBackend.Persistence

open System
open Microsoft.EntityFrameworkCore

open MusicPlayerBackend.Common
open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.Persistence.Repositories

[<Sealed>]
type FsharpTrackRepository(dbContext: FsharpAppDbContext) =
    let tracks = dbContext.Set<Track>()

    member _.Query with get() = tracks.AsQueryable()

    member _.Save(track: Track) =
        if track.Id = Guid.Empty && track.CreatedAt = DateTimeOffset.MinValue then
            track.CreatedAt <- DateTimeOffset.UtcNow
        elif track.Id <> Guid.Empty then
            let playlistEntry = dbContext.Entry(track)
            match playlistEntry.State with
            | EntityState.Modified when playlistEntry.Property(nameof track.UpdatedAt).IsModified |> not ->
                track.UpdatedAt <- ValueSome DateTimeOffset.UtcNow
            | EntityState.Detached ->
                raise ^ InvalidOperationException("Can't save detached track.")
            | _ -> ()

        tracks.Update(track)

    member _.Delete(track) = tracks.Remove(track)

    member _.TryGetById(id: TrackId, ?ct) = task {
        return!
            query {
                for track in tracks do
                    where (track.Id = id)
                    select track
            } |> _.TrySingle(ct)
    }

    // TODO: Move logic to service layer
    member _.TryGetVisible(trackId, userId, ?ct) = task {
        return!
            query {
                for track in tracks do
                    where (track.Id = trackId && (track.OwnerUserId = userId || track.Visibility = TrackVisibility.Visible))
                    select track
            } |> _.TrySingle(ct)
    }

    member _.TryGetIfOwner(trackId, userId, ?ct) = task {
        return!
            query {
                for track in tracks do
                    where (track.Id = trackId && track.OwnerUserId = userId)
                    select track
            } |> _.TrySingle(ct)
    }

    member this.DeleteIfOwner(trackId, userId, ?ct) = task {
        let! track =
            match ct with
            | None -> this.TryGetIfOwner(trackId, userId)
            | Some ct -> this.TryGetIfOwner(trackId, userId, ct)

        match track with
        | None -> ()
        | Some track ->
            %tracks.Remove(track)
    }
