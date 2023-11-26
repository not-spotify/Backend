using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface ITrackRepository : IEntityRepository<Guid, Track>
{
    Task<Track?> GetByIdVisibleForOrDefault(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Track?> GetByIdAllowedForChangeOrDefault(Guid id, Guid userId, CancellationToken cancellationToken = default);
    Task<Track?> GetByIdAllowedForFullAccessOrDefault(Guid id, Guid userId, CancellationToken cancellationToken = default);
}

public sealed class TrackRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, Track>(dbContext), ITrackRepository
{
    public Task<Track?> GetByIdVisibleForOrDefault(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return SingleOrDefaultAsync(t => t.Id == id && (t.Visibility == TrackVisibility.Visible || t.OwnerUserId == userId), cancellationToken);
    }

    public Task<Track?> GetByIdAllowedForChangeOrDefault(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return SingleOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId, cancellationToken); // TODO: Implement permissions
    }

    public Task<Track?> GetByIdAllowedForFullAccessOrDefault(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        return SingleOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId, cancellationToken);
    }
}
