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
public sealed class LikedTrackController(
    IUserProvider userProvider,
    ITrackRepository trackRepository,
    ITrackPlaylistRepository trackPlaylistRepository,
    IUnitOfWork unitOfWork) : ControllerBase
{
    /// <summary>
    ///     Adds track to "liked" playlist
    /// </summary>
    [HttpPut("{trackId:guid}/Like", Name = "LikeTrack")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Like(Guid trackId)
    {
        var user = await userProvider.GetUserAsync();
        var userId = user.Id;

        var track = await trackRepository.GetByIdIfVisibleOrDefault(trackId, userId);
        if (track == default)
            return NoContent();

        await trackPlaylistRepository.AddTrackIfNotAdded(user.FavoriteTracksPlaylistId, trackId);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    ///     Removes track to "liked" playlist
    /// </summary>
    [HttpDelete("{trackId:guid}/Like", Name = "DislikeTrack")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveLike(Guid trackId)
    {
        var user = await userProvider.GetUserAsync();

        await trackPlaylistRepository.AddTrackIfNotAdded(user.FavoriteTracksPlaylistId, trackId);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    ///     Get liked tracks
    /// </summary>
    [HttpGet("LikedList", Name = "LikedTrackList")]
    [ProducesResponseType(typeof(LikedTrackListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromBody] LikedTrackListRequest request, CancellationToken ct)
    {
        var user = await userProvider.GetUserAsync();
        var playlistItemsQuery = trackPlaylistRepository
            .QueryMany(tp => tp.PlaylistId == user.FavoriteTracksPlaylistId, tp =>

                new LikedTrackListItem
                {
                    CoverUri = tp.Track.CoverUri,
                    TrackUri = tp.Track.TrackUri,
                    Name = tp.Track.Name,
                    Author = tp.Track.Author,
                    IsAvailable = tp.Playlist.OwnerUserId == user.Id || tp.Playlist.Visibility == PlaylistVisibility.Public
            });

        var playlistItems = await playlistItemsQuery
            .Skip(request.PageSize * request.PageSize)
            .Take(request.PageSize)
            .ToArrayAsync(ct);

        return Ok(new LikedTrackListResponse { Items = playlistItems, Count = await playlistItemsQuery.CountAsync(ct) });
    }
}
