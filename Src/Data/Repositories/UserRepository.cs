using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IUserRepository : IEntityRepository<Guid, User>
{
    Task<User> FindByNormalizedEmailAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> FindByNormalizedEmailOrDefaultAsync(string userName, CancellationToken cancellationToken = default);
}

public sealed class UserRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, User>(dbContext), IUserRepository
{
    public async Task<User> FindByNormalizedEmailAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await SingleAsync(u => u.NormalizedEmail == userName, cancellationToken: cancellationToken);
    }

    public async Task<User?> FindByNormalizedEmailOrDefaultAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await SingleOrDefaultAsync(u => u.NormalizedEmail == userName, cancellationToken: cancellationToken);
    }
}
