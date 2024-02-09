namespace MusicPlayerBackend.Host.Contracts.Playlist

type Id = System.Guid
type UserId = System.Guid

type Visibility =
    | Private
    | Public

type Playlist = {
    Id: Id
    Name: string
    OwnerUserId: UserId
    CoverUri: string option
    Visibility: Visibility
}
