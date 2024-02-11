namespace MusicPlayerBackend.Persistence.Entities

open System

type AlbumId = Guid

type Album = {
    mutable Id: AlbumId

    mutable CoverUri: string option

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset voption
}
