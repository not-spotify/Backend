using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Repositories;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.TransferObjects;
using MusicPlayerBackend.TransferObjects.Playlist;

namespace MusicPlayerBackend.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(UnauthorizedResponse), StatusCodes.Status401Unauthorized)]
[Authorize]
[Route("[controller]")]
public sealed class PlaylistController(
    IPlaylistRepository playlistRepository,
    ITrackRepository trackRepository,
    ITrackPlaylistRepository trackPlaylistRepository,
    IPlaylistUserPermissionRepository playlistUserPermissionRepository,
    IUnitOfWork unitOfWork,
    IS3Service s3Service,
    IUserProvider userProvider) : ControllerBase
{
    /// <summary>
    ///     Get visible to user playlists if authorized
    ///     For unauthorized only visible playlists will be returned
    /// </summary>
    [AllowAnonymous]
    [HttpGet("List", Name = "GetPlaylists")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PlaylistListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PlaylistListRequest request, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdOrDefaultAsync();

        IQueryable<Playlist> visiblePlaylistsQuery;

        if (userId != default)
        {
            visiblePlaylistsQuery = playlistRepository.QueryAll()
                .Where(p =>
                    p.Visibility == PlaylistVisibility.Public
                    || p.OwnerUserId == userId
                    || p.Permissions.Any(np => np.UserId == userId && np.PlaylistId == p.Id));
        }
        else
        {
            visiblePlaylistsQuery = playlistRepository.QueryAll()
                .Where(p => p.Visibility == PlaylistVisibility.Public);
        }

        var totalCount = await visiblePlaylistsQuery.CountAsync(ct);
        var playlists = await visiblePlaylistsQuery.OrderBy(p => p.CreatedAt).Select(p => new PlaylistListItemResponse
            {
                CoverUri = p.CoverUri,
                Id = p.Id,
                Name = p.Name
            }).Skip(request.PageSize * request.Page).Take(request.PageSize).ToArrayAsync(ct);

        return Ok(new PlaylistListResponse { Items = playlists, TotalCount = totalCount });
    }

    /// <summary>
    ///     Gets playlist information
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetPlaylist")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PlaylistListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();

        var visiblePlaylistsQuery = playlistRepository.QueryAll()
            .Where(p => p.Visibility == PlaylistVisibility.Public || p.OwnerUserId == userId || p.Permissions.Any(np => np.UserId == userId && np.PlaylistId == p.Id));

        var playlist = await visiblePlaylistsQuery.SingleOrDefaultAsync(p => p.Id == id, ct);

        if (playlist == default)
            return NotFound();

        return Ok(playlist);
    }

    [HttpPost(Name = "CreatePlaylist")]
    [Authorize]
    [ProducesResponseType(typeof(PlaylistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request)
    {
        var userId = await userProvider.GetUserIdAsync();

        var playlist = new Playlist
        {
            Name = request.Name,
            Visibility = (PlaylistVisibility) request.Visibility!,
            OwnerUserId = userId
        };

        request.Visibility = request.Visibility;
        playlistRepository.Save(playlist);

        await unitOfWork.SaveChangesAsync();
        return Ok(playlist);
    }

    /// <summary>
    ///     Creates clone of existing playlist
    /// </summary>
    [HttpPost("{id:guid}/Clone", Name = "ClonePlaylist")]
    [ProducesResponseType(typeof(PlaylistResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Clone(Guid id, ClonePlaylistRequest request, CancellationToken ct)
    {
        var user = await userProvider.GetUserAsync();

        if (user.FavoriteTracksPlaylistId == id)
            return BadRequest(new { Error = "You can't create copy of liked playlist" });

        var playlist = await playlistRepository.GetByIdOrDefaultAsync(id, ct);
        if (playlist == default)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        var currentUserId = await userProvider.GetUserIdAsync();
        var createdPlaylistNames = await playlistRepository.GetManyAsync(p => p.OwnerUserId == currentUserId, p => p.Name, ct);

        if (createdPlaylistNames.Length >= 10)
            return BadRequest(new { Error = "Current limit of playlists is 10. Sorry." });

        var originalPlaylistId = playlist.Id;

        if (playlist.OwnerUserId != currentUserId && !await playlistUserPermissionRepository.HasAccessForView(originalPlaylistId, currentUserId, ct))
            return BadRequest(new { Error = $"Can't find playlist with id {originalPlaylistId}" });

        await unitOfWork.BeginTransactionAsync(ct);

        var playlistName = playlist.Name;
        if (!string.IsNullOrWhiteSpace(request.Name?.Trim()))
        {
            playlistName = request.Name;
            if (createdPlaylistNames.Contains(playlistName))
                playlistName += " (1)";
        }

        playlist.Id = Guid.Empty;
        playlist.CreatedAt = DateTimeOffset.UtcNow;
        playlist.UpdatedAt = null;
        playlist.Name = playlistName;
        playlistRepository.Save(playlist);

        await unitOfWork.SaveChangesAsync(ct);

        var tracks =
            await trackPlaylistRepository
                .QueryMany(tp => tp.PlaylistId == originalPlaylistId)
                .Where(tp => tp.Track.Visibility == TrackVisibility.Visible || tp.Track.OwnerUserId == user.Id)
                .AsNoTracking()
            .ToArrayAsync(ct);

        foreach (var trackPlaylist in tracks)
        {
            trackPlaylist.Id = Guid.Empty;
            trackPlaylist.PlaylistId = playlist.Id;
            trackPlaylistRepository.Save(trackPlaylist);
        }

        await unitOfWork.SaveChangesAsync(ct);
        await unitOfWork.CommitAsync(ct);

        return Ok(playlist);
    }

    [HttpDelete("{id:guid}", Name = "DeletePlaylist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(id);
        if (playlist == default)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        playlistRepository.Delete(playlist);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{playlistId:guid}/Tracks", Name = "EditTracks")]
    [ProducesResponseType(typeof(BulkTrackActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkTrackActions(Guid playlistId, BulkTrackActionRequest request, CancellationToken ct)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(playlistId, ct);
        if (playlist == default)
            return BadRequest(new { Error = $"Can't find playlist with id {playlistId}" });

        var userId = await userProvider.GetUserIdAsync();
        if (playlist.OwnerUserId != userId && !await playlistUserPermissionRepository.HasAccessForChange(playlist.Id, userId, ct))
            return BadRequest(new { Error = $"Can't find playlist with id {playlistId}" });

        var trackIds = request.Tracks.Select(t => t.Id).Distinct();
        var tracks = (await trackRepository.GetByIdsAsync(trackIds, ct)).ToDictionary(t => t.Id);
        var addedTracks = await trackPlaylistRepository
                                .QueryMany(tp => tp.PlaylistId == playlistId, tp => tp.TrackId)
                                .ToListAsync(ct);

        var responseTrackItems = new List<TrackResponseItem>(request.Tracks.Length);
        foreach (var track in request.Tracks)
        {
            var id = track.Id;

            if (!tracks.ContainsKey(id))
                responseTrackItems.Add(new TrackResponseItem { Id = id, Action = TrackActionResponse.NotFound });

            else switch (track.Action)
            {
                case TrackActionRequest.Delete:
                {
                    var trackPlaylist = await trackPlaylistRepository.SingleAsync(tp => tp.PlaylistId == playlistId && tp.TrackId == id, ct);
                    trackPlaylistRepository.Delete(trackPlaylist);
                    addedTracks.Remove(id);

                    responseTrackItems.Add(new TrackResponseItem { Id = id, Action = TrackActionResponse.Deleted });
                    break;
                }
                case TrackActionRequest.Add:
                {
                    if (!addedTracks.Contains(id))
                    {
                        trackPlaylistRepository.Save(new TrackPlaylist { PlaylistId = playlistId, TrackId = id });
                        responseTrackItems.Add(new TrackResponseItem { Id = id, Action = TrackActionResponse.Added });
                    }
                    else
                        responseTrackItems.Add(new TrackResponseItem { Id = id, Action = TrackActionResponse.AlreadyAdded });

                    break;
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Ok(new BulkTrackActionResponse { Tracks = responseTrackItems.ToArray() });
    }

    [HttpPut("{playlistId:guid}", Name = "EditPlaylist")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(UpdatePlaylistErrorResponse), StatusCodes.Status400BadRequest)]
    [Consumes(MediaTypeNames.Application.FormUrlEncoded)]
    public async Task<IActionResult> Update(Guid playlistId, UpdatePlaylistRequest request, CancellationToken ct = default)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(playlistId, ct);
        if (playlist == default)
            return BadRequest(new UpdatePlaylistErrorResponse { Error = "Can't find playlist" });

        if (request is { RemoveCover: true, Cover: not null })
            return BadRequest(new UpdatePlaylistErrorResponse { Error = $"{nameof(request.RemoveCover)} is true" });

        if (request.Cover != default)
        {
            var coverUri = await s3Service.TryUploadFileStream("covers", Path.GetFileNameWithoutExtension(request.Cover.Name), request.Cover.OpenReadStream(), Path.GetExtension(request.Cover.FileName), ct);
            playlist.CoverUri = coverUri;

            if (coverUri == default)
                return BadRequest(new UpdatePlaylistErrorResponse { Error = "Can't update cover" });
        }

        if (request.Name != default)
            playlist.Name = request.Name;

        playlistRepository.Save(playlist);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
