module MusicPlayerBackend.Host.Models.Playlist

open System
open Microsoft.AspNetCore.Http

open MusicPlayerBackend.Host

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
    DeleteCover: bool
    Visibility: PlaylistVisibility
}

type PlaylistResponse = {
    Id: Guid
    Name: string
    CoverUri: string option
    Visibility: PlaylistVisibility
}

module Utils =
    let ofCommand = function
        | Private ->
            Contracts.Playlist.Private
        | Public ->
            Contracts.Playlist.Public

    let ofDtoCommand = function
        | Contracts.Playlist.Private ->
            Private
        | Contracts.Playlist.Public ->
            Public
