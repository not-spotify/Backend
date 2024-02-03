using System;
using System.Threading;
using System.Threading.Tasks;

using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface ITrackRepository : IEntityRepository<Guid, Track>
{
    Task<Track?> GetByIdIfVisibleOrDefault(Guid id, Guid userId, CancellationToken ct = default);
    Task<Track?> GetByIdIfCanChangeOrDefault(Guid id, Guid userId, CancellationToken ct = default);
    Task<Track?> GetByIdIfOwnerOrDefault(Guid id, Guid userId, CancellationToken ct = default);
}

public sealed class TrackRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, Track>(dbContext), ITrackRepository
{
    public Task<Track?> GetByIdIfVisibleOrDefault(Guid id, Guid userId, CancellationToken ct = default)
    {
        return SingleOrDefaultAsync(t => t.Id == id && (t.Visibility == TrackVisibility.Visible || t.OwnerUserId == userId), ct);
    }

    public Task<Track?> GetByIdIfCanChangeOrDefault(Guid id, Guid userId, CancellationToken ct = default)
    {
        return SingleOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId, ct); // TODO: Implement permissions
    }

    public Task<Track?> GetByIdIfOwnerOrDefault(Guid id, Guid userId, CancellationToken ct = default)
    {
        return SingleOrDefaultAsync(t => t.Id == id && t.OwnerUserId == userId, ct);
    }
}
