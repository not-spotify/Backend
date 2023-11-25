using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Repositories;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.TransferObjects.Playlist;

namespace MusicPlayerBackend.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[Route("[controller]")]
public sealed class PlaylistController(IPlaylistRepository playlistRepository, ITrackRepository trackRepository, ITrackPlaylistRepository trackPlaylistRepository,
    IPlaylistUserPermissionRepository playlistUserPermissionRepository, IUnitOfWork unitOfWork, IS3Service s3Service, IUserResolver userResolver) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PlaylistListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(PlaylistListRequest request, CancellationToken ct)
    {
        var playlists = await playlistRepository.QueryAll().OrderBy(p => p.CreatedAt).Select(p => new PlaylistListItemResponse
        {
            CoverUri = p.CoverUri,
            Id = p.Id,
            Name = p.Name
        }).Skip(request.PageSize * request.PageSize).Take(request.PageSize).ToArrayAsync(ct);

        return Ok(new PlaylistListResponse { Items = playlists });
    }

    [HttpPost]
    [ProducesResponseType(typeof(PlaylistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request)
    {
        var playlist = new Playlist
        {
            Name = request.Name,
            Visibility = (PlaylistVisibility) request.Visibility!
        };

        request.Visibility = request.Visibility;
        playlistRepository.Save(playlist);

        await unitOfWork.SaveChangesAsync();
        return Ok(playlist);
    }

    [HttpPut("{id:guid}/Clone")]
    [ProducesResponseType(typeof(PlaylistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Clone(Guid id, ClonePlaylistRequest request, CancellationToken cancellationToken)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(id, cancellationToken);
        if (playlist == default)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        var originalPlaylistId = playlist.Id;

        var currentUserId = await userResolver.GetUserIdAsync();
        if (playlist.OwnerUserId != currentUserId && !await playlistUserPermissionRepository.AnyAsync(p => p.PlaylistId == originalPlaylistId && p.UserId == currentUserId && (p.Permission == PlaylistPermission.AllowedToView || p.Permission == PlaylistPermission.AllowedToModifyTracks || p.Permission == PlaylistPermission.Full), cancellationToken))
            return BadRequest(new { Error = $"Can't find playlist with id {originalPlaylistId}" });

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        playlist.Id = Guid.Empty;
        playlist.Name = request.Name ?? $"{playlist.Name} (1)";
        playlistRepository.Save(playlist);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var tracks = await trackPlaylistRepository.QueryMany(tp => tp.PlaylistId == originalPlaylistId).ToArrayAsync(cancellationToken);
        foreach (var trackPlaylist in tracks)
        {
            trackPlaylist.Id = Guid.Empty;
            trackPlaylist.PlaylistId = playlist.Id;
            trackPlaylistRepository.Save(trackPlaylist);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Ok(playlist);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(id);
        if (playlist == default)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(id);
        if (playlist == default)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        playlistRepository.Delete(playlist);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{playlistId:guid}/Tracks")]
    [ProducesResponseType(typeof(BulkTrackActionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkTrackActions(Guid playlistId, BulkTrackActionRequest request, CancellationToken cancellationToken)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(playlistId, cancellationToken);
        if (playlist == default)
            return BadRequest(new { Error = $"Can't find playlist with id {playlistId}" });

        var currentUserId = await userResolver.GetUserIdAsync();
        if (playlist.OwnerUserId != currentUserId && !await playlistUserPermissionRepository.AnyAsync(p => p.PlaylistId == playlistId && p.UserId == currentUserId && (p.Permission == PlaylistPermission.AllowedToModifyTracks || p.Permission == PlaylistPermission.Full), cancellationToken))
            return BadRequest(new { Error = $"Can't find playlist with id {playlistId}" });

        var trackIds = request.Tracks.Select(t => t.Id).Distinct();
        var tracks = (await trackRepository.GetByIdsAsync(trackIds, cancellationToken)).ToDictionary(t => t.Id);
        var addedTracks = await trackPlaylistRepository.QueryAll().Where(tp => tp.PlaylistId == playlistId).Select(tp => tp.TrackId).ToListAsync(cancellationToken: cancellationToken);

        var responseTrackItems = new List<TrackResponseItem>(request.Tracks.Count());
        foreach (var track in request.Tracks)
        {
            var id = track.Id;

            if (!tracks.ContainsKey(id))
                responseTrackItems.Add(new TrackResponseItem { Id = id, Action = TrackActionResponse.NotFound });

            else switch (track.Action)
            {
                case TrackActionRequest.Delete:
                {
                    var trackPlaylist = await trackPlaylistRepository.SingleAsync(tp => tp.PlaylistId == playlistId && tp.TrackId == id, cancellationToken);
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

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Ok(new BulkTrackActionResponse { Tracks = responseTrackItems });
    }

    [HttpPut("{playlistId:guid}/Cover")]
    public async Task<IActionResult> UpdateCover(Guid playlistId, IFormFile cover, CancellationToken ct)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(playlistId, ct);
        if (playlist == default)
            return NotFound();

        var coverUri = await s3Service.TryUploadFileStream("covers", Guid.NewGuid().ToString(), cover.OpenReadStream(), ct);
        if (coverUri == default)
            return BadRequest();

        playlist.CoverUri = coverUri;
        playlistRepository.Save(playlist);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
