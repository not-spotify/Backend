using MusicPlayerBackend.Data.Entities;

namespace MusicPlayerBackend.Data.Repositories;

public interface IUserRepository : IEntityRepository<Guid, User>
{
    Task<User> FindByNameAsync(string userName, CancellationToken cancellationToken = default);
    Task<User?> FindByNameOrDefaultAsync(string userName, CancellationToken cancellationToken = default);
}

public sealed class UserRepository(AppDbContext dbContext)  : EntityRepositoryBase<Guid, User>(dbContext), IUserRepository
{
    public async Task<User> FindByNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await SingleAsync(u => u.UserName == userName, cancellationToken: cancellationToken);
    }

    public async Task<User?> FindByNameOrDefaultAsync(string userName, CancellationToken cancellationToken = default)
    {
        return await SingleOrDefaultAsync(u => u.UserName == userName, cancellationToken: cancellationToken);
    }
}
