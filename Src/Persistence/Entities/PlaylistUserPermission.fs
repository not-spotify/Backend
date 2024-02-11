namespace MusicPlayerBackend.Persistence.Entities

open System
open MusicPlayerBackend.Persistence.Entities

type PlaylistPermission =
    | Full = 0
    | ModifyTrack = 1
    | View = 2

type PlaylistUserPermission = {
    PlaylistId: PlaylistId
    UserId: UserId

    CreatedAt: DateTimeOffset
    UpdatedAt: DateTimeOffset voption
}
