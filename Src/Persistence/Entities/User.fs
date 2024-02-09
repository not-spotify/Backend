module MusicPlayerBackend.Persistence.Entities.User

open System

type Id = Guid
type PlaylistId = Guid

[<CLIMutable>]
type User = {
    mutable Id: Id

    mutable UserName: string
    mutable NormalizedUserName: string
    mutable Email: string
    mutable NormalizedEmail: string
    mutable HashedPassword: string
    mutable FavoritePlaylistId: PlaylistId

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset voption
}
