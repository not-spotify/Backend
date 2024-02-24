namespace MusicPlayerBackend.Host.Models

open System
open Microsoft.AspNetCore.Http
open MusicPlayerBackend.Contracts.Track

type PlaylistVisibility =
    | Private
    | Public

module PlaylistVisibility =
    let ofContract (cv: MusicPlayerBackend.Contracts.Track.Visibility) =
        match cv with
        | Visibility.Private ->
            PlaylistVisibility.Private
        | Visibility.Public ->
            PlaylistVisibility.Public

    let toContract (mv: PlaylistVisibility) =
        match mv with
        | Private ->
            MusicPlayerBackend.Contracts.Track.Visibility.Private
        | Public ->
            MusicPlayerBackend.Contracts.Track.Visibility.Public

type CreatePlaylist = {
    Name: string
    Cover: IFormFile option
    Visibility: PlaylistVisibility
}

type UpdatePlaylist = {
    Name: string
    Cover: IFormFile option
    DeleteCover: bool
    Visibility: PlaylistVisibility
}

type Playlist = {
    Id: Guid
    Name: string
    CoverUri: string option
    Visibility: PlaylistVisibility
}
