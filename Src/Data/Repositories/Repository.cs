using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MusicPlayerBackend.Data.Repositories;

public interface IEntityRepository<in TKey, TEntity> where TEntity : class, IEntity<TKey> where TKey : IEquatable<TKey>
{
    void Save(TEntity entity);
    void Delete(TEntity entity);

    Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default);

    Task<TResult> GetByIdAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    Task<TEntity?> GetByIdOrDefaultAsync(TKey id, CancellationToken cancellationToken = default);

    Task<TResult?> GetByIdOrDefaultAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    IQueryable<TEntity> QueryAll();
    IQueryable<TResult> QueryAll<TResult>(Expression<Func<TEntity, TResult>> selector);
    IQueryable<TEntity> QueryMany(Expression<Func<TEntity, bool>> predicate);

    IQueryable<TResult> QueryMany<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector);

    Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TResult>> GetManyAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where, CancellationToken cancellationToken = default);
    Task<TEntity> FirstAsync(CancellationToken cancellationToken = default);
    Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default);
    Task<TEntity?> SingleOrDefaultAsync(CancellationToken cancellationToken = default);
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default);
    Task<TEntity> SingleAsync(CancellationToken cancellationToken = default);
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default);

    Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);
    Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult?>> selector, CancellationToken cancellationToken = default);

    Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);
    Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);
    Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);
    Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}

public abstract class EntityRepositoryBase<TKey, TEntity>(AppDbContext dbContext) : IEntityRepository<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    private DbContext DbContext { get; } = dbContext;
    private DbSet<TEntity> DbSet { get; } = dbContext.Set<TEntity>();

    public void Save(TEntity entity)
    {
        if (entity.IsNew() && entity.CreatedAt == DateTimeOffset.MinValue)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
        }
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

    public async Task<TEntity> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var result = await DbSet.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
        if (result == default)
            ThrowEntityNotFoundException(id);

        return result!;
    }

    public async Task<IReadOnlyCollection<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids, CancellationToken cancellationToken = default)
    {
        var result = await QueryAll().Where(e => ids.Contains(e.Id)).ToArrayAsync(cancellationToken);
        return result;
    }

    public async Task<TResult> GetByIdAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        var result = await QueryMany(e => e.Id.Equals(id), selector).Take(1).ToArrayAsync(cancellationToken);

        if (result.Length == 0)
            ThrowEntityNotFoundException(id);

        return result.Single();
    }

    public async Task<TEntity?> GetByIdOrDefaultAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var result = await DbSet.FindAsync(new object?[] { id }, cancellationToken: cancellationToken);
        return result;
    }

    public async Task<TResult?> GetByIdOrDefaultAsync<TResult>(TKey id, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        var result = await QueryMany(e => e.Id.Equals(id), selector).Take(1).ToArrayAsync(cancellationToken);
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

    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAll().ToArrayAsync(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryAll(selector).ToArrayAsync(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await QueryMany(predicate).ToArrayAsync(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<TResult>> GetManyAsync<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryMany(predicate, selector).ToArrayAsync(cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAll().FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where, CancellationToken cancellationToken = default)
    {
        return await QueryAll().FirstOrDefaultAsync(where, cancellationToken: cancellationToken);
    }

    public async Task<TEntity> FirstAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAll().FirstAsync(cancellationToken: cancellationToken);
    }

    public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default)
    {
        return await QueryAll().FirstAsync(where, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAll().SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default)
    {
        return await QueryAll().SingleOrDefaultAsync(where, cancellationToken: cancellationToken);
    }

    public async Task<TEntity> SingleAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAll().SingleAsync(cancellationToken: cancellationToken);
    }

    public async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where, CancellationToken cancellationToken = default)
    {
        return await QueryAll().SingleAsync(where, cancellationToken: cancellationToken);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryAll(selector).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult?>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryMany(where, selector).FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryAll(selector).FirstAsync(cancellationToken: cancellationToken);
    }

    public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryMany(where, selector).FirstAsync(cancellationToken: cancellationToken);
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryAll(selector).SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryMany(where, selector).SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryAll(selector).SingleAsync(cancellationToken);
    }

    public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult>> selector, CancellationToken cancellationToken = default)
    {
        return await QueryMany(where, selector).SingleAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAll().AnyAsync(cancellationToken: cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await QueryAll().AnyAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await QueryAll().CountAsync(cancellationToken: cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await QueryMany(predicate).CountAsync(cancellationToken: cancellationToken);
    }

    #region Helpers

    private static void ThrowEntityNotFoundException(TKey entityId)
    {
        throw new InvalidOperationException($"Failed to find entity of type '{typeof(TEntity)}' by id '{entityId}'.");
    }

    #endregion
}
