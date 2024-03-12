namespace MusicPlayerBackend.Contracts.Playlist

open MusicPlayerBackend.Contracts.User

type GetItem = {
    Id: PlaylistId
    UserId: UserId
}
