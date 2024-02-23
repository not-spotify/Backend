namespace MusicPlayerBackend.Host.Contracts.Playlist

type PlaylistId = System.Guid
type UserId = System.Guid

type Visibility =
    | Private
    | Public

type Playlist = {
    Id: PlaylistId
    Name: string
    OwnerUserId: UserId
    CoverUri: string option
    Visibility: Visibility
}
