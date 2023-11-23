using MusicPlayerBackend.Data;
using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Repositories;

public interface ITrackPlaylistRepository : IEntityRepository<Guid, TrackPlaylist>
{
}

public sealed class TrackPlaylistRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, TrackPlaylist>(dbContext), ITrackPlaylistRepository;