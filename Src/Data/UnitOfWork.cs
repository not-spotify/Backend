using Microsoft.EntityFrameworkCore.Storage;

namespace MusicPlayerBackend.Data;

public interface IUnitOfWork
{
    void Commit();
    Task CommitAsync();
    Task CommitAsync(CancellationToken cancellationToken);

    void SaveChanges();
    Task SaveChangesAsync();
    Task SaveChangesAsync(CancellationToken cancellationToken);

    bool HasActiveTransaction { get; }
    IDbContextTransaction? GetCurrentTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
}

public sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    private IDbContextTransaction _dbContextTransaction = null!; // Let it crash.

    public void Commit()
    {
        _dbContextTransaction.Commit();
    }

    public Task CommitAsync()
    {
        return _dbContextTransaction.CommitAsync();
    }

    public Task CommitAsync(CancellationToken cancellationToken)
    {
        return _dbContextTransaction.CommitAsync(cancellationToken);
    }

    public void SaveChanges()
    {
        dbContext.SaveChanges();
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public bool HasActiveTransaction => ReferenceEquals(_dbContextTransaction, null);

    public IDbContextTransaction GetCurrentTransaction()
    {
        return _dbContextTransaction;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _dbContextTransaction = await dbContext.Database.BeginTransactionAsync();
        return _dbContextTransaction;
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct)
    {
        _dbContextTransaction = await dbContext.Database.BeginTransactionAsync(ct);
        return _dbContextTransaction;
    }
}
