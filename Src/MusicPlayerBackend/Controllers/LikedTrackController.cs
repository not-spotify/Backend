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
[Route("Track/[action]")]
public sealed class LikedTrackController(IUserResolver userResolver, ILikedTrackRepository likedTrackRepository, IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpPut("{trackId:guid}/Like")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Like(Guid trackId)
    {
        var userId = await userResolver.GetUserIdAsync();
        var likedTrackEntity = await likedTrackRepository.GetOrDefault(userId, trackId);

        if (likedTrackEntity != default)
            return NoContent();

        likedTrackEntity = new LikedTrack { UserId = userId, TrackId = trackId };
        likedTrackRepository.Delete(likedTrackEntity);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{trackId:guid}/Like")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveLike(Guid trackId)
    {
        var userId = await userResolver.GetUserIdAsync();
        var likedTrackEntity = await likedTrackRepository.GetOrDefault(userId, trackId);

        if (likedTrackEntity == default)
            return NoContent();

        likedTrackRepository.Delete(likedTrackEntity);
        await unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    [ProducesResponseType(typeof(LikedTrackListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromBody] LikedTrackListRequest request, CancellationToken cancellationToken)
    {
        var userId = await userResolver.GetUserIdAsync();
        var trackIds = await likedTrackRepository
            .QueryAll()
            .Where(lt => lt.UserId == userId)
            .OrderBy(lt => lt.CreatedAt)
            .Skip(request.Page + request.PageSize)
            .Take(request.PageSize)
            .Select(lt => lt.Id)
            .ToArrayAsync(cancellationToken);

        return Ok(new LikedTrackListResponse { TrackIds = trackIds });
    }
}
