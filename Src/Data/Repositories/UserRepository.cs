using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IUserRepository : IEntityRepository<Guid, User>
{
    Task<User> FindByNormalizedEmailAsync(string userName, CancellationToken ct = default);
    Task<User?> FindByNormalizedEmailOrDefaultAsync(string userName, CancellationToken ct = default);
}

public sealed class UserRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, User>(dbContext), IUserRepository
{
    public async Task<User> FindByNormalizedEmailAsync(string userName, CancellationToken ct = default)
    {
        return await SingleAsync(u => u.NormalizedEmail == userName, ct);
    }

    public async Task<User?> FindByNormalizedEmailOrDefaultAsync(string userName, CancellationToken ct = default)
    {
        return await SingleOrDefaultAsync(u => u.NormalizedEmail == userName, ct);
    }
}
