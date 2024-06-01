namespace MusicPlayerBackend.Host.Models

open System.ComponentModel.DataAnnotations

[<CLIMutable>]
type AddTrackToPlaylist = {
    [<Required>]
    UserId: System.Guid

    [<Required>]
    TrackId: System.Guid

    [<Required>]
    PlaylistId: System.Guid
}
