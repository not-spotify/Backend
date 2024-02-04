using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace MusicPlayerBackend.Data.Repositories;

public interface IEntityRepository<in TKey, TEntity> where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>
{
    void Save(TEntity entity);
    void Delete(TEntity entity);

    Task<TEntity> GetByIdAsync(TKey id, CancellationToken ct = default);
    Task<TEntity[]> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken ct = default);

    Task<TEntity?> GetByIdOrDefaultAsync(TKey id, CancellationToken ct = default);

    IQueryable<TEntity> QueryAll();
    IQueryable<TEntity> QueryMany(Expression<Func<TEntity, bool>> predicate);

    IQueryable<TResult> QueryMany<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector);

    Task<TResult[]> GetManyAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where, CancellationToken ct = default);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default);
    Task<TEntity> SingleAsync(CancellationToken ct = default);
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, CancellationToken ct = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);
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

    public async Task<TEntity?> GetByIdOrDefaultAsync(TKey id, CancellationToken ct = default)
    {
        var result = await DbSet.FindAsync(new object?[] { id }, ct);
        return result;
    }

    public IQueryable<TEntity> QueryAll()
    {
        return DbSet;
    }

    public IQueryable<TEntity> QueryMany(Expression<Func<TEntity, bool>> predicate)
    {
        return QueryAll().Where(predicate);
    }

    public IQueryable<TResult> QueryMany<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector)
    {
        return QueryMany(predicate).Select(selector);
    }

    public async Task<TResult[]> GetManyAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken ct = default)
    {
        return await QueryMany(predicate, selector).ToArrayAsync(ct);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where, CancellationToken ct = default)
    {
        return await QueryAll().Where(where).FirstOrDefaultAsync(ct);
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

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
    {
        return await QueryAll().AnyAsync(predicate, ct);
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
