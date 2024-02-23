module MusicPlayerBackend.Host.Models.Track

open System
open Microsoft.AspNetCore.Http

open MusicPlayerBackend.Host
open MusicPlayerBackend.Persistence.Entities

type Visibility =
    | Hidden
    | Public

type SearchTracksRequest = {
    Page: int
    PageSize: int
}

type CreateTrackRequest = {
    Visibility: Visibility
    Cover: IFormFile option
    Track: IFormFile
    Author: string
    Name: string
}

type UpdateTrackRequest = {
    TrackId: TrackId
    Visibility: Visibility option
    Cover: IFormFile option
    RemoveCover: bool
    Author: string
    Name: string
}

type TrackResponse = {
    Id: TrackId
    CoverUri: string option
    TrackUri: string

    Author: string
    Name: string

    Visibility: Visibility

    CreatedAt: DateTimeOffset
    UpdatedAt: DateTimeOffset option
}

module Utils =
    let ofCommand = function
        | MusicPlayerBackend.Host.Contracts.Track.Private ->
            Contracts.Track.Private
        | MusicPlayerBackend.Host.Contracts.Track.Public ->
            Contracts.Track.Public

    let ofDtoCommand = function
        | Contracts.Track.Private ->
            MusicPlayerBackend.Host.Contracts.Track.Private
        | Contracts.Track.Public ->
            MusicPlayerBackend.Host.Contracts.Track.Public

    let ofPersistenceCommand = function
        | MusicPlayerBackend.Host.Contracts.Track.Private ->
            TrackVisibility.Hidden
        | MusicPlayerBackend.Host.Contracts.Track.Public ->
            TrackVisibility.Visible
