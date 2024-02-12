module MusicPlayerBackend.Host.Models.Track

open System

type TrackVisibility =
    | Hidden
    | Public

type TrackFilterRequest = {
    Page: int
    PageSize: int
}

type TrackResponse = {
    Id: Guid
    CoverUri: string
    TrackUri: string
    Visibility: TrackVisibility
}
