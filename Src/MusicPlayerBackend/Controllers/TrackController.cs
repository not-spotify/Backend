using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Repositories;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.TransferObjects;
using MusicPlayerBackend.TransferObjects.Track;
using TrackListItem = MusicPlayerBackend.TransferObjects.Track.TrackListItem;
using TrackVisibility = MusicPlayerBackend.Data.Entities.TrackVisibility;

namespace MusicPlayerBackend.Controllers;

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(UnauthorizedResponse), StatusCodes.Status401Unauthorized)]
[Authorize]
[Route("[controller]")]
public sealed class TrackController(
    ITrackRepository trackRepository,
    IS3Service s3Service,
    IUnitOfWork unitOfWork,
    IUserProvider userProvider) : ControllerBase
{
    /// <summary>
    /// Gets track list
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet(Name = "GetTracks")]
    [ProducesResponseType(typeof(TrackListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PlaylistListRequest request, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();

        var tracks = await trackRepository.QueryAll()
            .Where(t => t.OwnerUserId == userId)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TrackListItem
            {
                Author = t.Author,
                CoverUri = t.CoverUri,
                Name = t.Name,
                OwnerUserId = t.OwnerUserId,
                TrackUri = t.TrackUri,
                Visibility = (MusicPlayerBackend.TransferObjects.Track.TrackVisibility)(int)t.Visibility
            })
            .ToArrayAsync(ct);

        var trackCount = await trackRepository.QueryAll()
            .Where(t => t.OwnerUserId == userId)
            .CountAsync(ct);

        return Ok(new TrackListResponse { Items = tracks, Count = trackCount });
    }

    /// <summary>
    ///     Get track by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}", Name = "GetTrack")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();
        var track = await trackRepository.GetByIdIfVisibleOrDefault(id, userId, ct);

        if (track == default)
            return BadRequest();

        return Ok(track);
    }

    [HttpPut("{id:guid}", Name = "UpdateTrack")]
    public async Task<IActionResult> Update(Guid id, TrackUpdateRequest request, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();
        var track = await trackRepository.GetByIdIfOwnerOrDefault(userId, id, ct);

        if (track == default)
            return BadRequest();

        track.Visibility = (TrackVisibility)request.Visibility;
        track.CoverUri = request.CoverUri;

        trackRepository.Save(track);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:guid}", Name = "DeleteTrack")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();
        var track = await trackRepository.GetByIdIfOwnerOrDefault(id, userId, ct);

        if (track == default)
            return BadRequest();

        trackRepository.Delete(track);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    [Consumes(MediaTypeNames.Multipart.FormData)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost(Name = "CreateTrack")]
    public async Task<IActionResult> Create([FromForm] TrackCreateRequest request, CancellationToken ct = default)
    {
        var uploadedTrackUri = await s3Service.TryUploadFileStream("tracks", request.Name, request.Track.OpenReadStream(), Path.GetExtension(request.Track.FileName), ct);

        if (uploadedTrackUri == default)
            return BadRequest(new { Error = "Failed to upload track" });

        string? coverUri = default;
        if (request.Cover is not null)
        {
            var uploadedCoverUri = await s3Service.TryUploadFileStream("covers", request.Name, request.Cover.OpenReadStream(), Path.GetExtension(request.Cover.FileName), ct);
            if (uploadedCoverUri == default)
                return BadRequest("Failed to upload cover");

            coverUri = uploadedCoverUri;
        }

        var trackEntity = new Track
        {
            CoverUri = coverUri,
            Name = request.Name,
            Author = request.Author,
            Visibility = (TrackVisibility)request.Visibility,
            TrackUri = uploadedTrackUri,
            OwnerUserId = await userProvider.GetUserIdAsync(),
        };

        trackRepository.Save(trackEntity);
        await unitOfWork.SaveChangesAsync(ct);

        return Ok(trackEntity);
    }
}
