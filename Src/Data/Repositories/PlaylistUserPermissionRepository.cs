using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IPlaylistUserPermissionRepository : IEntityRepository<Guid, PlaylistUserPermission>;

public sealed class PlaylistUserPermissionRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, PlaylistUserPermission>(dbContext), IPlaylistUserPermissionRepository;
