namespace MusicPlayerBackend.Persistence.Entities

open System

type TrackId = Guid

type TrackVisibility =
    | Hidden = 0
    | Visible = 1

type Track = {
    mutable Id: TrackId
    mutable OwnerUserId: UserId

    mutable CoverUri: string option
    mutable TrackUri: string option
    mutable Visibility: TrackVisibility

    mutable Name: string
    mutable Author: string

    mutable CreatedAt: DateTimeOffset
    mutable UpdatedAt: DateTimeOffset voption
}
