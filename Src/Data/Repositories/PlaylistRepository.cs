using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IPlaylistRepository : IEntityRepository<Guid, Playlist>;

public sealed class PlaylistRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, Playlist>(dbContext), IPlaylistRepository;
