﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Repositories;
using MusicPlayerBackend.TransferObjects.Playlist;

namespace MusicPlayerBackend.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[Route("[controller]")]
public sealed class PlaylistController(IPlaylistRepository playlistRepository, ITrackRepository trackRepository, ITrackPlaylistRepository trackPlaylistRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var playlists = await playlistRepository.QueryAll().ToArrayAsync(ct);
        return Ok(playlists);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PlaylistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create()
    {
        var playlist = new Playlist();
        playlistRepository.Save(playlist);

        await unitOfWork.SaveChangesAsync();
        
        return Ok(playlist);
    }

    [HttpPut("{id:guid}/Clone")]
    [ProducesResponseType(typeof(PlaylistResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Clone(Guid id, ClonePlaylistRequest request, CancellationToken cancellationToken)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(id);
        if (playlist == null)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        var originalPlaylistId = playlist.Id;

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
        if (playlist == null)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(id);
        if (playlist == null)
            return BadRequest(new { Error = $"Can't find playlist with id {id}" });

        playlistRepository.Delete(playlist);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{playlistId:guid}/Tracks")]
    [ProducesResponseType(typeof(BulkTrackActionResponse), StatusCodes.Status200OK)]
    [SuppressMessage("ReSharper", "SwitchStatementHandlesSomeKnownEnumValuesWithDefault")]
    public async Task<IActionResult> BulkTrackActions(Guid playlistId, BulkTrackActionRequest request)
    {
        var playlist = await playlistRepository.GetByIdOrDefaultAsync(playlistId);
        if (playlist == null)
            return BadRequest(new { Error = $"Can't find playlist with id {playlistId}" });

        var trackIds = request.Tracks.Select(t => t.Id).Distinct();
        var tracks = (await trackRepository.GetByIdsAsync(trackIds)).ToDictionary(t => t.Id);
        var addedTracks = await trackPlaylistRepository.QueryAll().Where(tp => tp.PlaylistId == playlistId).Select(tp => tp.TrackId).ToListAsync();

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
                    var trackPlaylist = await trackPlaylistRepository.SingleAsync(tp => tp.PlaylistId == playlistId && tp.TrackId == id);
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

        await unitOfWork.SaveChangesAsync();
        return Ok(new BulkTrackActionResponse { Tracks = responseTrackItems });
    }
}