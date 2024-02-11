namespace MusicPlayerBackend.Persistence.Entities

open System

type UserId = Guid
type PlaylistId = Guid

[<CLIMutable>]
type User = {
    mutable Id: UserId

    mutable UserName: string
    mutable NormalizedUserName: string
    mutable Email: string
    mutable NormalizedEmail: string
    mutable HashedPassword: string
    mutable FavoritePlaylistId: PlaylistId

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset voption
}
