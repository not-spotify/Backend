using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Repositories;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.TransferObjects;

namespace MusicPlayerBackend.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(UnauthorizedResponse), StatusCodes.Status401Unauthorized)]
[Route("Track/")]
public sealed class FavoriteTrackController(
    IUserProvider userProvider,
    ITrackRepository trackRepository,
    ITrackPlaylistRepository trackPlaylistRepository,
    IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>
    ///     Adds track to "liked" playlist.
    /// </summary>
    [HttpPut("{trackId:guid}/Favorite", Name = "FavoriteTrack")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Favorite(Guid trackId)
    {
        var user = await userProvider.GetUserAsync();
        var userId = user.Id;

        var track = await trackRepository.GetByIdIfVisibleOrDefault(trackId, userId);
        if (track == default)
            return NoContent();

        await trackPlaylistRepository.AddTrackIfNotAdded(user.FavoritePlaylistId, trackId);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    ///     Removes track to favorite playlist.
    /// </summary>
    [HttpDelete("{trackId:guid}/Favorite", Name = "UnfavoriteTrack")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveLike(Guid trackId)
    {
        var user = await userProvider.GetUserAsync();

        await trackPlaylistRepository.AddTrackIfNotAdded(user.FavoritePlaylistId, trackId);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    ///     Get favorite tracks.
    /// </summary>
    [HttpGet("GetFavoriteTrackList", Name = "FavoriteTrackList")]
    [ProducesResponseType(typeof(FavoriteTrackListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromBody] FavoriteTrackListRequest request, CancellationToken ct)
    {
        var user = await userProvider.GetUserAsync();
        var playlistItemsQuery = trackPlaylistRepository
            .QueryMany(tp => tp.PlaylistId == user.FavoritePlaylistId, tp => new FavoriteTrackListItem {
                    CoverUri = tp.Track.CoverUri,
                    TrackUri = tp.Track.OwnerUserId == user.Id || tp.Track.Visibility == TrackVisibility.Visible ? tp.Track.TrackUri : null,
                    Name = tp.Track.Name,
                    Author = tp.Track.Author,
                    IsAvailable = tp.Playlist.OwnerUserId == user.Id || tp.Playlist.Visibility == PlaylistVisibility.Public
            });

        var playlistItems = await playlistItemsQuery
            .Skip(request.PageSize * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync(ct);

        return Ok(new FavoriteTrackListResponse { Items = playlistItems, Count = await playlistItemsQuery.CountAsync(ct) });
    }
}
