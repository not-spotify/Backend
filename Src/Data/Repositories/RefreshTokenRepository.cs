using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IRefreshTokenRepository : IEntityRepository<Guid, RefreshToken>
{
    Task<RefreshToken?> GetValidTokenOrDefault(Guid userId, Guid jti, Guid token);
}

public sealed class RefreshTokenRepository(AppDbContext dbContext) : EntityRepositoryBase<Guid, RefreshToken>(dbContext), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetValidTokenOrDefault(Guid userId, Guid jti, Guid token)
    {
        return await SingleOrDefaultAsync(rt => rt.UserId == userId && rt.Jti == jti && rt.Token == token && rt.Revoked == false && rt.ValidDue < DateTimeOffset.UtcNow);
    }
}
