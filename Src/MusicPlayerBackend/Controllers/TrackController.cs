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
    ///     Gets track list.
    /// </summary>
    [HttpGet(Name = "GetTracks")]
    [ProducesResponseType(typeof(TrackListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] PlaylistListRequest request, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();

        var tracks = await trackRepository
            .QueryMany(t => t.OwnerUserId == userId)
            .Skip(request.Page * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TrackListItem {
                Author = t.Author,
                CoverUri = t.CoverUri,
                Name = t.Name,
                TrackUri = t.OwnerUserId == userId || t.Visibility == TrackVisibility.Visible ? t.TrackUri : null,
                Visibility = (MusicPlayerBackend.TransferObjects.Track.TrackVisibility)(int)t.Visibility
            })
            .ToArrayAsync(ct);

        var trackCount = await trackRepository.CountAsync(t => t.OwnerUserId == userId, ct);

        return Ok(new TrackListResponse { Items = tracks, Count = trackCount });
    }

    /// <summary>
    ///     Get track by Id.
    /// </summary>
    [HttpGet("{id:guid}", Name = "GetTrack")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();
        var track = await trackRepository.GetByIdIfVisibleOrDefault(id, userId, ct);

        if (track == default)
            return NotFound();

        return Ok(track);
    }

    /// <summary>
    ///     Updates track's visibility level, cover.
    /// </summary>
    [HttpPut("{id:guid}", Name = "UpdateTrack")]
    [Consumes(MediaTypeNames.Application.FormUrlEncoded)]
    public async Task<IActionResult> Update(Guid id, [FromForm] TrackUpdateRequest request, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();
        var track = await trackRepository.GetByIdIfOwnerOrDefault(userId, id, ct);

        if (track == default)
            return NotFound();

        if (request.Visibility != null)
            track.Visibility = (TrackVisibility)request.Visibility;

        if (request.Cover is not null)
        {
            if (request.RemoveCover)
                return BadRequest(new UpdateTrackErrorResponse { Error = $"{nameof(request.RemoveCover)} is true" });

            var uploadedCoverUri = await s3Service.TryUploadFileStream("covers", Guid.NewGuid() + "_" + track.Name, request.Cover.OpenReadStream(), Path.GetExtension(request.Cover.FileName), ct);
            if (uploadedCoverUri == default)
                return BadRequest("Failed to upload cover");

            track.CoverUri = uploadedCoverUri;
        }
        else if (request.RemoveCover)
            track.CoverUri = null;

        trackRepository.Save(track);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    ///     Deletes track.
    /// </summary>
    [HttpDelete("{id:guid}", Name = "DeleteTrack")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = await userProvider.GetUserIdAsync();
        var track = await trackRepository.GetByIdIfOwnerOrDefault(id, userId, ct);

        if (track == default)
            return NotFound();

        trackRepository.Delete(track);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

    /// <summary>
    ///     Uploads track.
    /// </summary>
    [HttpPost(Name = "UploadTrack")]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    public async Task<IActionResult> Upload([FromForm] TrackCreateRequest request, CancellationToken ct = default)
    {
        var uploadedTrackUri = await s3Service.TryUploadFileStream("tracks", Guid.NewGuid() + "_" + request.Name, request.Track.OpenReadStream(), Path.GetExtension(request.Track.FileName), ct);

        if (uploadedTrackUri == default)
            return BadRequest(new { Error = "Failed to upload track" });

        string? coverUri = default;
        if (request.Cover is not null)
        {
            var uploadedCoverUri = await s3Service.TryUploadFileStream("covers", Guid.NewGuid() + "_" + request.Name, request.Cover.OpenReadStream(), Path.GetExtension(request.Cover.FileName), ct);
            if (uploadedCoverUri == default)
                return BadRequest("Failed to upload cover");

            coverUri = uploadedCoverUri;
        }

        var trackEntity = new Track {
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
