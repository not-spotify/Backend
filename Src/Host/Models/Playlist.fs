module MusicPlayerBackend.Host.Models.Playlist

open System
open Microsoft.AspNetCore.Http

type PlaylistVisibility =
    | Private
    | Public

type PlaylistCreateRequest = {
    Name: string
    Cover: IFormFile option
    Visibility: PlaylistVisibility
}

type PlaylistUpdateRequest = {
    Name: string
    Cover: IFormFile option
    Visibility: PlaylistVisibility
}

type PlaylistResponse = {
    Id: Guid
    Name: string
    CoverUri: string option
    Visibility: PlaylistVisibility
}
