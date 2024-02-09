namespace MusicPlayerBackend.Persistence.Entities.Playlist

open System

type UserId = Guid
type Id = Guid

type Visibility =
    | Private
    | Public

[<CLIMutable>]
type Playlist = {
    mutable Id: Id

    mutable Name: string
    mutable Visibility: Visibility
    mutable CoverUri: string option
    mutable OwnerUserId: UserId

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset voption
}
