namespace MusicPlayerBackend.Persistence.Entities

open System

type TrackPlaylist = {
    TrackId: TrackId
    PlaylistId: PlaylistId

    AddedAt: DateTimeOffset
}
