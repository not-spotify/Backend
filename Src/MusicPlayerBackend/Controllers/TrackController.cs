using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;
using MusicPlayerBackend.Data.Repositories;
using MusicPlayerBackend.Services;
using MusicPlayerBackend.TransferObjects;

namespace MusicPlayerBackend.Controllers;

public enum TrackVisibilityRequest
{
    Hidden = 0,
    Visible = 1
}

public sealed class TrackUpdateRequest
{
    public TrackVisibilityRequest Visibility { get; set; }
    public string? CoverUri { get; set; }
    public string? Name { get; set; }
}

public sealed class TrackCreateRequest
{
    public string Name { get; set; } = null!;
    public string Author { get; set; } = null!;

    public TrackVisibilityRequest Visibility { get; set; }
}

[ApiController]
[Consumes(MediaTypeNames.Application.Json)]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(UnauthorizedResponse), StatusCodes.Status401Unauthorized)]
[Authorize]
[Route("[controller]")]
public sealed class TrackController(ITrackRepository trackRepository, IS3Service s3Service, IUnitOfWork unitOfWork, IUserResolver userResolver) : ControllerBase
{
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

    [HttpPost]
    public async Task<IActionResult> Create([ModelBinder(BinderType = typeof(JsonModelBinder))] TrackCreateRequest request, IFormFile uploadingTrack, IFormFile? uploadingCover, CancellationToken cancellationToken)
    {
        var trackUri = s3Service.TryUploadFileStream("tracks", Guid.NewGuid().ToString(), uploadingTrack.OpenReadStream(), cancellationToken);

        if (trackUri == default)
            return BadRequest(new { Error = "Failed to upload track" });

        string? coverUri = default;
        if (uploadingCover != default)
        {
            var res = await s3Service.TryUploadFileStream("covers", Guid.NewGuid().ToString(), uploadingCover.OpenReadStream(), cancellationToken);
            if (res != default)
                coverUri = res;
        }

        var track = new Track
        {
            CoverUri = coverUri,
            Name = request.Name,
            Author = request.Author,
            Visibility = (TrackVisibility)request.Visibility
        };

        trackRepository.Save(track);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(track);
    }

    [HttpPut("{trackId:guid}/Cover")]
    [Consumes(MediaTypeNames.Multipart.FormData)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCover(Guid trackId, IFormFile cover, CancellationToken ct)
    {
        var track = await trackRepository.GetByIdOrDefaultAsync(trackId, ct);
        if (track == default)
            return BadRequest();

        var coverUri = await s3Service.TryUploadFileStream("covers", Guid.NewGuid().ToString(), cover.OpenReadStream(), ct);
        if (coverUri == default)
            return BadRequest();

        track.CoverUri = coverUri;
        trackRepository.Save(track);
        await unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
