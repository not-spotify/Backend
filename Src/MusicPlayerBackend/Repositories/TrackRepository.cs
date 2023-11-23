using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Repositories;

public interface ITrackRepository : IEntityRepository<Guid, Track>
{
}

public sealed class TrackRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, Track>(dbContext), ITrackRepository;