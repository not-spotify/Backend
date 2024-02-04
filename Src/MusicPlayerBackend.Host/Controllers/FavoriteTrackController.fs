namespace MusicPlayerBackend.Host

open System
open System.Linq
open System.Net.Mime
open System.Threading
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open Microsoft.EntityFrameworkCore
open MusicPlayerBackend.Data
open MusicPlayerBackend.Data.Entities
open MusicPlayerBackend.Data.Repositories
open MusicPlayerBackend.TransferObjects

[<ApiController>]
[<Consumes(MediaTypeNames.Application.Json)>]
[<Produces(MediaTypeNames.Application.Json)>]
[<ProducesResponseType(StatusCodes.Status400BadRequest)>]
[<ProducesResponseType(typeof<UnauthorizedResponse>, StatusCodes.Status401Unauthorized)>]
[<Route("Track/")>]
type FavoriteTrackController(userProvider: IUserProvider,
                            trackRepository: ITrackRepository,
                            trackPlaylistRepository: ITrackPlaylistRepository,
                            unitOfWork: IUnitOfWork) =
    inherit ControllerBase()

    /// <summary>
    ///     Adds track to "liked" playlist.
    /// </summary>
    [<HttpPut("{trackId:guid}/Favorite", Name = "FavoriteTrack")>]
    [<ProducesResponseType(StatusCodes.Status204NoContent)>]
    member this.Favorite(trackId: Guid) : Task<IActionResult> = task {
        let! user = userProvider.GetUser()
        let userId = user.Id
        let! track = trackRepository.GetByIdIfVisibleOrDefault(trackId, userId)
        if track = null then
            return this.NoContent() :> IActionResult
        else
            do! trackPlaylistRepository.AddTrackIfNotAdded(user.FavoritePlaylistId, trackId)
            do! unitOfWork.SaveChangesAsync()
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
        do! unitOfWork.SaveChangesAsync()
        return this.NoContent() :> IActionResult
    }

    /// <summary>
    ///     Get favorite tracks.
    /// </summary>
    [<HttpGet("GetFavoriteTrackList", Name = "FavoriteTrackList")>]
    [<ProducesResponseType(typeof<FavoriteTrackListResponse>, StatusCodes.Status200OK)>]
    member this.List(request: FavoriteTrackListRequest, ct: CancellationToken) = task {
        let! user = userProvider.GetUser()
        let playlistItemsQuery = trackPlaylistRepository.QueryMany(
            (fun tp -> tp.PlaylistId = user.FavoritePlaylistId),
            (fun tp -> FavoriteTrackListItem(
                CoverUri = tp.Track.CoverUri,
                TrackUri = (if tp.Track.OwnerUserId = user.Id || tp.Track.Visibility = TrackVisibility.Visible then tp.Track.TrackUri else null),
                Name = tp.Track.Name,
                Author = tp.Track.Author,
                IsAvailable = (tp.Playlist.OwnerUserId = user.Id || tp.Playlist.Visibility = PlaylistVisibility.Public)
            )))
        let! playlistItems = playlistItemsQuery
                                .Skip(request.PageSize * request.Page)
                                .Take(request.PageSize)
                                .ToArrayAsync(ct)
        let! c = playlistItemsQuery.CountAsync(ct)
        return this.Ok(FavoriteTrackListResponse(Items = playlistItems, Count = c)) :> IActionResult
    }


