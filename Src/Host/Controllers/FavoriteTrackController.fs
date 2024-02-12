namespace MusicPlayerBackend.Host

open System
open System.Net.Mime
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

open MusicPlayerBackend.Persistence
open MusicPlayerBackend.Persistence.Entities
open MusicPlayerBackend.TransferObjects

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<ProducesResponseType(typeof<UnauthorizedResponse>, StatusCodes.Status401Unauthorized)>]
[<Route("Track/")>]
type FavoriteTrackController(userProvider: UserProvider,
                            trackRepository: FsharpTrackRepository,
                            playlistRepository: FsharpPlaylistRepository,
                            trackPlaylistRepository: FsharpTrackPlaylistRepository,
                            unitOfWork: FsharpUnitOfWork) =
    inherit ControllerBase()

    /// <summary>
    ///     Adds track to "liked" playlist.
    /// </summary>
    [<HttpPut("{trackId:guid}/Favorite", Name = "FavoriteTrack")>]
    [<ProducesResponseType(StatusCodes.Status204NoContent)>]
    member this.Favorite(trackId: Guid) : Task<IActionResult> = task {
        let! user = userProvider.GetUser()
        let userId = user.Id
        let! track = trackRepository.TryGetVisible(trackId, userId)
        match track with
        | None ->
            return this.NoContent() :> IActionResult
        | Some track ->
            do! trackPlaylistRepository.AddTrackIfNotAdded(user.FavoritePlaylistId, track.Id)
            do! unitOfWork.SaveChanges()
            return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Removes track to favorite playlist.
    /// </summary>
    [<HttpDelete("{trackId:guid}/Favorite", Name = "UnfavoriteTrack")>]
    [<ProducesResponseType(StatusCodes.Status204NoContent)>]
    member this.RemoveLike(trackId: Guid) = task {
        let! user = userProvider.GetUser()
        do! trackPlaylistRepository.AddTrackIfNotAdded(user.FavoritePlaylistId, trackId)
        do! unitOfWork.SaveChanges()
        return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Get favorite tracks.
    /// </summary>
    [<HttpGet("GetFavoriteTrackList", Name = "FavoriteTrackList")>]
    [<ProducesResponseType(typeof<FavoriteTrackListResponse>, StatusCodes.Status200OK)>]
    member this.List([<FromQuery>] request: FavoriteTrackListRequest, ct: CancellationToken) = task {
        let! user = userProvider.GetUser()
        let mapping (playlist: MusicPlayerBackend.Persistence.Entities.Track) : FavoriteTrackListItem =
            FavoriteTrackListItem(
                Id = playlist.Id,
                CoverUri = (playlist.CoverUri |> Option.toObj),
                Name = playlist.Name,
                Visibility =
                    match playlist.Visibility with
                    | TrackVisibility.Hidden -> MusicPlayerBackend.TransferObjects.Track.TrackVisibility.Hidden
                    | TrackVisibility.Visible -> MusicPlayerBackend.TransferObjects.Track.TrackVisibility.Visible
                    | _ -> ArgumentOutOfRangeException() |> raise
                )

        let! count, playlistItems =
            playlistRepository
                .GetVisibleTracks(
                    user.FavoritePlaylistId,
                    request.Page,
                    request.PageSize,
                    mapping,
                    ct)
        return this.Ok(FavoriteTrackListResponse(
            Items = playlistItems,
            Count = count))
        :> IActionResult
    }


