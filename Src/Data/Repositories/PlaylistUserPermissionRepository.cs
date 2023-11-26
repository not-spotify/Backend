using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IPlaylistUserPermissionRepository : IEntityRepository<Guid, PlaylistUserPermission>
{
    Task<bool> HasAccessForView(Guid playlistId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasAccessForChange(Guid playlistId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasAccessFullAccess(Guid playlistId, Guid userId, CancellationToken cancellationToken = default);
}

public sealed class PlaylistUserPermissionRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, PlaylistUserPermission>(dbContext), IPlaylistUserPermissionRepository
{
    public async Task<bool> HasAccessForView(Guid playlistId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await AnyAsync(p => p.PlaylistId == playlistId && p.UserId == userId && (p.Permission == PlaylistPermission.AllowedToView || p.Permission == PlaylistPermission.AllowedToModifyTracks || p.Permission == PlaylistPermission.Full), cancellationToken);
    }

    public async Task<bool> HasAccessForChange(Guid playlistId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await AnyAsync(p => p.PlaylistId == playlistId && p.UserId == userId && (p.Permission == PlaylistPermission.AllowedToModifyTracks || p.Permission == PlaylistPermission.Full), cancellationToken);
    }

    public async Task<bool> HasAccessFullAccess(Guid playlistId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await AnyAsync(p => p.PlaylistId == playlistId && p.UserId == userId && p.Permission == PlaylistPermission.Full, cancellationToken);
    }
}
