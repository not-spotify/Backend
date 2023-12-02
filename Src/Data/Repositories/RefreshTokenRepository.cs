using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IRefreshTokenRepository : IEntityRepository<Guid, RefreshToken>;

public sealed class RefreshTokenRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, RefreshToken>(dbContext), IRefreshTokenRepository;
