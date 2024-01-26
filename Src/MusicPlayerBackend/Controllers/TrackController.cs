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
    IUserResolver userResolver) : ControllerBase
{
    /// <summary>
    /// Show track uploaded by authorized user
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(TrackListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PlaylistListRequest request, CancellationToken ct)
    {
        var userId = await userResolver.GetUserIdAsync();

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
            .ToArrayAsync(cancellationToken: ct);

        var trackCount = await trackRepository.QueryAll()
            .Where(t => t.OwnerUserId == userId)
            .CountAsync(cancellationToken: ct);

        return Ok(new TrackListResponse { Items = tracks, TotalCount = trackCount });
    }

    /// <summary>
    ///     Get track by Id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var userId = await userResolver.GetUserIdAsync();
        var track = await trackRepository.GetByIdVisibleForOrDefault(id, userId, cancellationToken);

        if (track == default)
            return BadRequest();

        return Ok(track);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, TrackUpdateRequest request, CancellationToken cancellationToken)
    {
        var userId = await userResolver.GetUserIdAsync();
        var track = await trackRepository.GetByIdAllowedForFullAccessOrDefault(userId, id, cancellationToken);

        if (track == default)
            return BadRequest();

        track.Visibility = (TrackVisibility)request.Visibility;
        track.CoverUri = request.CoverUri;

        trackRepository.Save(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = await userResolver.GetUserIdAsync();
        var track = await trackRepository.GetByIdAllowedForFullAccessOrDefault(id, userId, cancellationToken);

        if (track == default)
            return BadRequest();

        trackRepository.Delete(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [Consumes(MediaTypeNames.Multipart.FormData)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] TrackCreateRequest request, CancellationToken cancellationToken = default)
    {
        var uploadedTrackUri = await s3Service.TryUploadFileStream("tracks", Guid.NewGuid().ToString(), request.Track.OpenReadStream(), Path.GetExtension(request.Track.FileName), cancellationToken);

        if (uploadedTrackUri == default)
            return BadRequest(new { Error = "Failed to upload track" });

        string? coverUri = default;
        if (request.Cover is not null)
        {
            var uploadedCoverUri = await s3Service.TryUploadFileStream("covers", Guid.NewGuid().ToString(), request.Cover.OpenReadStream(), Path.GetExtension(request.Cover.FileName), cancellationToken);
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
            OwnerUserId = await userResolver.GetUserIdAsync(),
        };

        trackRepository.Save(trackEntity);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(trackEntity);
    }
}
