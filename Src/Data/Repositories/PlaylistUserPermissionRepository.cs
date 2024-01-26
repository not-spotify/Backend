using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IPlaylistUserPermissionRepository : IEntityRepository<Guid, PlaylistUserPermission>
{
    Task<bool> HasAccessForView(Guid playlistId, Guid userId, CancellationToken ct = default);
    Task<bool> HasAccessForChange(Guid playlistId, Guid userId, CancellationToken ct = default);
    Task<bool> HasAccessFullAccess(Guid playlistId, Guid userId, CancellationToken ct = default);
}

public sealed class PlaylistUserPermissionRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, PlaylistUserPermission>(dbContext), IPlaylistUserPermissionRepository
{
    public async Task<bool> HasAccessForView(Guid playlistId, Guid userId, CancellationToken ct = default)
    {
        return await AnyAsync(p =>
            p.PlaylistId == playlistId
            && p.UserId == userId
            && (p.Permission == PlaylistPermission.AllowedToView || p.Permission == PlaylistPermission.AllowedToModifyTracks || p.Permission == PlaylistPermission.Full), ct);
    }

    public async Task<bool> HasAccessForChange(Guid playlistId, Guid userId, CancellationToken ct = default)
    {
        return await AnyAsync(p =>
            p.PlaylistId == playlistId
            && p.UserId == userId
            && (p.Permission == PlaylistPermission.AllowedToModifyTracks || p.Permission == PlaylistPermission.Full), ct);
    }

    public async Task<bool> HasAccessFullAccess(Guid playlistId, Guid userId, CancellationToken ct = default)
    {
        return await AnyAsync(p =>
            p.PlaylistId == playlistId
            && p.UserId == userId
            && p.Permission == PlaylistPermission.Full, ct);
    }
}
