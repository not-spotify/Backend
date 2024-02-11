namespace MusicPlayerBackend.Persistence.Entities

open System

type AlbumTrack = {
    mutable AlbumId: AlbumId
    mutable TrackId: TrackId

    mutable CreatedAt: DateTimeOffset
}
