using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IAlbumRepository : IEntityRepository<Guid, Album>;

public sealed class AlbumRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, Album>(dbContext), IAlbumRepository;
