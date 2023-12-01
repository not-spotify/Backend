using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface ILikedTrackRepository : IEntityRepository<Guid, LikedTrack>
{
    public Task<LikedTrack?> GetOrDefault(Guid userId, Guid trackId);
}

public sealed class LikedTrackRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, LikedTrack>(dbContext), ILikedTrackRepository
{
    public async Task<LikedTrack?> GetOrDefault(Guid userId, Guid trackId)
    {
        return await SingleOrDefaultAsync(lt => lt.UserId == userId && lt.TrackId == trackId);
    }
}
