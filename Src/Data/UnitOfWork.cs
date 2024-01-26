using Microsoft.EntityFrameworkCore.Storage;

namespace MusicPlayerBackend.Data;

public interface IUnitOfWork
{
    void Commit();
    Task CommitAsync();
    Task CommitAsync(CancellationToken ct);

    void SaveChanges();
    Task SaveChangesAsync();
    Task SaveChangesAsync(CancellationToken ct);

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

    public Task CommitAsync(CancellationToken ct)
    {
        return _dbContextTransaction.CommitAsync(ct);
    }

    public void SaveChanges()
    {
        dbContext.SaveChanges();
    }

    public async Task SaveChangesAsync()
    {
        await dbContext.SaveChangesAsync();
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await dbContext.SaveChangesAsync(ct);
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
