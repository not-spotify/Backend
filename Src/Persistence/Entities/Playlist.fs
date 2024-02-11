namespace MusicPlayerBackend.Persistence.Entities

open System
open MusicPlayerBackend.Persistence.Entities

type Visibility =
    | Private
    | Public

[<CLIMutable>]
type Playlist = {
    mutable Id: PlaylistId

    mutable Name: string
    mutable Visibility: Visibility
    mutable CoverUri: string option
    mutable OwnerUserId: UserId

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset voption
}
