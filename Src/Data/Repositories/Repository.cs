using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MusicPlayerBackend.Data.Repositories;

public interface IEntityRepository<in TKey, TEntity> where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>
{
    void Save(TEntity entity);
    void Delete(TEntity entity);

    Task<TEntity> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<TEntity[]> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default);

    Task<TResult> GetByIdAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    Task<TEntity?> GetByIdOrDefaultAsync(TKey id, CancellationToken ct = default);

    Task<TResult?> GetByIdOrDefaultAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    IQueryable<TEntity> QueryAll();
    IQueryable<TResult> QueryAll<TResult>(Expression<Func<TEntity, TResult>> selector);
    IQueryable<TEntity> QueryMany(Expression<Func<TEntity, bool>> predicate);

    IQueryable<TResult> QueryMany<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector);

    Task<TEntity[]> GetAllAsync(CancellationToken ct = default);
    Task<TResult[]> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<TResult[]> GetManyAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(CancellationToken ct = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where, CancellationToken ct = default);
    Task<TEntity> FirstAsync(CancellationToken ct = default);
    Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default);
    Task<TEntity?> SingleOrDefaultAsync(CancellationToken ct = default);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default);
    Task<TEntity> SingleAsync(CancellationToken ct = default);
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default);

    Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult?>> selector, CancellationToken ct = default);

    Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);
    Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);
    Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);
    Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    Task<bool> AnyAsync(CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
}

public abstract class EntityRepositoryBase<TKey, TEntity>(AppDbContext dbContext) : IEntityRepository<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private DbContext DbContext { get; } = dbContext;
    private DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();

    public void Save(TEntity entity)
    {
        if (entity.IsNew() && entity.CreatedAt == default)
            entity.CreatedAt = DateTimeOffset.UtcNow;
        else if (!entity.IsNew())
        {
            var entityEntry = DbContext.Entry(entity);
            entity.UpdatedAt = entityEntry.State switch
            {
                EntityState.Modified when !entityEntry.Property(nameof(EntityBase.UpdatedAt)).IsModified =>
                    DateTimeOffset.UtcNow,
                EntityState.Detached => throw new InvalidOperationException("Can't save detached entity."),
                _ => entity.UpdatedAt
            };
        }

        DbSet.Update(entity);
    }

    public void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public async Task<TEntity> GetByIdAsync(TKey id, CancellationToken ct = default)
    {
        var result = await DbSet.FindAsync(new object?[] { id }, ct);
        if (result == default)
            ThrowEntityNotFoundException(id);

        return result!;
    }

    public async Task<TEntity[]> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default)
    {
        var result = await QueryAll().Where(e => ids.Contains(e.Id)).ToArrayAsync(ct);
        return result;
    }

    public async Task<TResult> GetByIdAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        var result = await QueryMany(e => e.Id.Equals(id), selector).Take(1).ToArrayAsync(ct);

        if (result.Length == 0)
            ThrowEntityNotFoundException(id);

        return result.Single();
    }

    public async Task<TEntity?> GetByIdOrDefaultAsync(TKey id, CancellationToken ct = default)
    {
        var result = await DbSet.FindAsync(new object?[] { id }, ct);
        return result;
    }

    public async Task<TResult?> GetByIdOrDefaultAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        var result = await QueryMany(e => e.Id.Equals(id), selector).Take(1).ToArrayAsync(ct);
        return result.SingleOrDefault();
    }

    public IQueryable<TEntity> QueryAll()
    {
        return DbSet;
    }

    public IQueryable<TResult> QueryAll<TResult>(Expression<Func<TEntity, TResult>> selector)
    {
        return QueryAll().Select(selector);
    }

    public IQueryable<TEntity> QueryMany(Expression<Func<TEntity, bool>> predicate)
    {
        return QueryAll().Where(predicate);
    }

    public IQueryable<TResult> QueryMany<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector)
    {
        return QueryMany(predicate).Select(selector);
    }

    public async Task<TEntity[]> GetAllAsync(CancellationToken ct = default)
    {
        return await QueryAll().ToArrayAsync(ct);
    }

    public async Task<TResult[]> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryAll(selector).ToArrayAsync(ct);
    }

    public async Task<TEntity[]> GetManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await QueryMany(predicate).ToArrayAsync(ct);
    }

    public async Task<TResult[]> GetManyAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryMany(predicate, selector).ToArrayAsync(ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(CancellationToken ct = default)
    {
        return await QueryAll().FirstOrDefaultAsync(ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where, CancellationToken ct = default)
    {
        return await QueryAll().FirstOrDefaultAsync(where, ct);
    }

    public async Task<TEntity> FirstAsync(CancellationToken ct = default)
    {
        return await QueryAll().FirstAsync(ct);
    }

    public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default)
    {
        return await QueryAll().FirstAsync(where, ct);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(CancellationToken ct = default)
    {
        return await QueryAll().SingleOrDefaultAsync(ct);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default)
    {
        return await QueryAll().SingleOrDefaultAsync(where, ct);
    }

    public async Task<TEntity> SingleAsync(CancellationToken ct = default)
    {
        return await QueryAll().SingleAsync(ct);
    }

    public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default)
    {
        return await QueryAll().SingleAsync(where, ct);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryAll(selector).FirstOrDefaultAsync(ct);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult?>> selector, CancellationToken ct = default)
    {
        return await QueryMany(where, selector).FirstOrDefaultAsync(ct);
    }

    public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryAll(selector).FirstAsync(ct);
    }

    public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryMany(where, selector).FirstAsync(ct);
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryAll(selector).SingleOrDefaultAsync(ct);
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryMany(where, selector).SingleOrDefaultAsync(ct);
    }

    public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryAll(selector).SingleAsync(ct);
    }

    public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryMany(where, selector).SingleAsync(ct);
    }

    public async Task<bool> AnyAsync(CancellationToken ct = default)
    {
        return await QueryAll().AnyAsync(ct);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await QueryAll().AnyAsync(predicate, ct);
    }

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await QueryAll().CountAsync(ct);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await QueryMany(predicate).CountAsync(ct);
    }

    #region Helpers

    private static void ThrowEntityNotFoundException(TKey entityId)
    {
        throw new InvalidOperationException($"Failed to find entity of type '{typeof(TEntity)}' by id '{entityId}'.");
    }

    #endregion
}
