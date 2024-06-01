namespace MusicPlayerBackend.Host.Controllers

open System.Net.Mime
open Domain.Playlist
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<Route("api/v0/[controller]/[action]")>]
type PlaylistController() as this =

    inherit ControllerBase()

    /// Creates new playlist
    [<HttpPost(Name = "CreatePlaylist")>]
    member _.Create(request, [<FromServices>] createPlaylist: CreatePlaylist) = task {
        let! x = createPlaylist request
        return this.Ok(x)
    }

    /// Removes a playlist
    [<HttpDelete(Name = "RemovePlaylist")>]
    member _.Remove(request, [<FromServices>] removePlaylist: RemovePlaylist) = task {
        let! x = removePlaylist request
        return this.Ok(x)
    }

    /// Adds track to playlist
    [<HttpPost(Name = "AddTrackToPlaylist")>]
    member _.AddTrackToPlaylist(request, [<FromServices>] handler: AddTrackToPlaylist) = task {
        let! x = handler request
        return this.Ok(x)
    }

    /// Removes track to playlist
    [<HttpDelete(Name = "RemoveTrackToPlaylist")>]
    member _.RemoveTrackToPlaylist(request, [<FromServices>] handler: RemoveTrackFromPlaylist) = task {
        let! x = handler request
        return this.Ok(x)
    }
