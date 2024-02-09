namespace MusicPlayerBackend.Host.Contracts.Playlist

open System

type CreateRequest = {
    UserId: Guid
    Name: string
    Visibility: Visibility
    CoverFileLink: string
}

type UpdateRequest = {
    UserId: UserId
    Id: Id
    Name: string
    DeleteCover: bool
    CoverFileLink: string
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

type Item = {
    Id: Id
    Name: string
    OwnerUserId: UserId
}

type List = {
    Items: Item[]

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
