namespace MusicPlayerBackend.Contracts.Track

open System

open Domain

type TrackId = Guid

type Visibility =
    | Private
    | Public

type CreateRequest = {
    UserId: UserId
    Name: string
    Author: string
    TrackFileLink: string
    CoverFileLink: string option
    Visibility: Visibility
}

type DeleteRequest = {
    UserId: UserId
    TrackId: TrackId
}
