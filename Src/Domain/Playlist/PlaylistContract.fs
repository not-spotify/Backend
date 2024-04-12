namespace MusicPlayerBackend.Contracts.Playlist

open System

type CreateRequest = {
    UserId: Guid
    Name: string
    Visibility: Visibility
    CoverFileLink: string option
}

type UpdateRequest = {
    UserId: UserId
    Id: PlaylistId
    Name: string
    DeleteCover: bool
    CoverFileLink: string option
}

type DeleteRequest = {
    UserId: Guid
    PlaylistId: Guid
}

type CreatePlaylistError =
    | Empty

type UpdatePlaylistError =
    | NonExistingPlaylist

type DeletePlaylistError =
    | AccountLocked
    | Empty

type PlaylistCommandRequest =
    | Create of CreateRequest
    | Update of UpdateRequest
    | Delete of DeleteRequest

type ListQuery = {
    UserId: UserId

    PageNumber: int
    PageSize: int
}

type GetQuery = {
    UserId: UserId

    PageNumber: int
    PageSize: int
}

type PlaylistQueryRequest =
    | List of ListQuery
    | Get of GetQuery

type Playlist = {
    Id: PlaylistId
    Name: string
    CoverUri: string option
    Visibility: Visibility
    OwnerUserId: UserId
}

type List = {
    Items: Playlist[]

    PageNumber: int
    TotalCount: int
}

type GetError =
    | NotFound

type ListError =
    | NotFound

type PlaylistQueryResponse =
    | List of Result<List, CreatePlaylistError>
    | Get of Result<Playlist, CreatePlaylistError>

type PlaylistCommandResponse =
    | Create of Result<Playlist, CreatePlaylistError>
    | Update of Result<Playlist, UpdatePlaylistError>
    | Delete of DeletePlaylistError option
