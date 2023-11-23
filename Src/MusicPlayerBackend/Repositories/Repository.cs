using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MusicPlayerBackend.Data;

namespace MusicPlayerBackend.Repositories;

public interface IEntityRepository<TKey, TEntity>
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    void Save(TEntity entity);
    void Delete(TEntity entity);

    Task<TEntity> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids);

    Task<TResult> GetByIdAsync<TResult>(Guid id, Expression<Func<TEntity, TResult>> selector);

    Task<TEntity?> GetByIdOrDefaultAsync(Guid id);

    Task<TResult?> GetByIdOrDefaultAsync<TResult>(Guid id, Expression<Func<TEntity, TResult>> selector);
    
    IQueryable<TEntity> QueryAll();
    IQueryable<TResult> QueryAll<TResult>(Expression<Func<TEntity, TResult>> selector);
    IQueryable<TEntity> QueryMany(Expression<Func<TEntity, bool>> predicate);

    IQueryable<TResult> QueryMany<TResult>(Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector);

    Task<IReadOnlyCollection<TEntity?>> GetAllAsync();
    Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> selector);
    Task<IReadOnlyCollection<TEntity?>> GetManyAsync(Expression<Func<TEntity?, bool>> predicate);

    Task<IReadOnlyCollection<TResult>> GetManyAsync<TResult>(Expression<Func<TEntity?, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector);

    Task<TEntity?> FirstOrDefaultAsync();
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where);
    Task<TEntity> FirstAsync();
    Task<TEntity> FirstAsync(Expression<Func<TEntity?, bool>> where);
    Task<TEntity?> SingleOrDefaultAsync();
    Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity?, bool>> where);
    Task<TEntity> SingleAsync();
    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> where);

    Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult?>> selector);

    Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult?>> selector);

    Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector);

    Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity?, bool>> where,
        Expression<Func<TEntity, TResult>> selector);

    Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult?>> selector);

    Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity?, bool>> where,
        Expression<Func<TEntity, TResult?>> selector);

    Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector);

    Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity?, bool>> where, Expression<Func<TEntity, TResult>> selector);

    Task<bool> AnyAsync();
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
}

[SuppressMessage("Design", "CA1012:Abstract types should not have constructors", Justification = "<Pending>")]
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

    public async Task<TEntity> GetByIdAsync(Guid id)
    {
        var result = await DbSet.FindAsync(id);
        if (result == null)
            ThrowEntityNotFoundException(id);

        return result;
    }

    public async Task<IReadOnlyCollection<TEntity>> GetByIdsAsync(IEnumerable<TKey> ids)
    {
        var result = await QueryAll().Where(e => ids.Contains(e.Id)).ToArrayAsync();
        return result;
    }

    public async Task<TResult> GetByIdAsync<TResult>(Guid id, Expression<Func<TEntity, TResult>> selector)
    {
        var result = await QueryMany(e => e.Id.Equals(id), selector).Take(1).ToArrayAsync();
        
        if (result.Length == 0)
            ThrowEntityNotFoundException(id);

        return result.Single();
    }

    public async Task<TEntity?> GetByIdOrDefaultAsync(Guid id)
    {
        var result = await DbSet.FindAsync(id);
        return result;
    }

    public async Task<TResult?> GetByIdOrDefaultAsync<TResult>(Guid id, Expression<Func<TEntity, TResult>> selector)
    {
        var result = await QueryMany(e => e.Id.Equals(id), selector).Take(1).ToArrayAsync();
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

    public IQueryable<TEntity?> QueryMany(Expression<Func<TEntity, bool>> predicate)
    {
        return QueryAll().Where(predicate);
    }

    public IQueryable<TResult> QueryMany<TResult>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TResult>> selector)
    {
        return QueryMany(predicate).Select(selector);
    }

    public async Task<IReadOnlyCollection<TEntity?>> GetAllAsync()
    {
        return await QueryAll().ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<TResult>> GetAllAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
    {
        return await QueryAll(selector).ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<TEntity?>> GetManyAsync(Expression<Func<TEntity?, bool>> predicate)
    {
        return await QueryMany(predicate).ToArrayAsync();
    }

    public async Task<IReadOnlyCollection<TResult>> GetManyAsync<TResult>(Expression<Func<TEntity?, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector)
    {
        return await QueryMany(predicate, selector).ToArrayAsync();
    }

    public async Task<TEntity?> FirstOrDefaultAsync()
    {
        return await QueryAll().FirstOrDefaultAsync();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> where)
    {
        return await QueryAll().FirstOrDefaultAsync(where);
    }

    public async Task<TEntity?> FirstAsync()
    {
        return await QueryAll().FirstAsync();
    }

    public async Task<TEntity?> FirstAsync(Expression<Func<TEntity?, bool>> where)
    {
        return await QueryAll().FirstAsync(where);
    }

    public async Task<TEntity?> SingleOrDefaultAsync()
    {
        return await QueryAll().SingleOrDefaultAsync();
    }

    public async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity?, bool>> where)
    {
        return await QueryAll().SingleOrDefaultAsync(where);
    }

    public async Task<TEntity?> SingleAsync()
    {
        return await QueryAll().SingleAsync();
    }

    public async Task<TEntity?> SingleAsync(Expression<Func<TEntity, bool>> where)
    {
        return await QueryAll().SingleAsync(where);
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult?>> selector)
    {
        return await QueryAll(selector).FirstOrDefaultAsync();
    }

    public async Task<TResult?> FirstOrDefaultAsync<TResult>(Expression<Func<TEntity, bool>> where, Expression<Func<TEntity, TResult?>> selector)
    {
        return await QueryMany(where, selector).FirstOrDefaultAsync();
    }

    public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
    {
        return await QueryAll(selector).FirstAsync();
    }

    public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntity?, bool>> where,
        Expression<Func<TEntity, TResult>> selector)
    {
        return await QueryMany(where, selector).FirstAsync();
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity, TResult?>> selector)
    {
        return await QueryAll(selector).SingleOrDefaultAsync();
    }

    public async Task<TResult?> SingleOrDefaultAsync<TResult>(Expression<Func<TEntity?, bool>> where,
        Expression<Func<TEntity, TResult?>> selector)
    {
        return await QueryMany(where, selector).SingleOrDefaultAsync();
    }

    public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity, TResult>> selector)
    {
        return await QueryAll(selector).SingleAsync();
    }

    public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntity?, bool>> where,
        Expression<Func<TEntity, TResult>> selector)
    {
        return await QueryMany(where, selector).SingleAsync();
    }

    public async Task<bool> AnyAsync()
    {
        return await QueryAll().AnyAsync();
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity?, bool>> predicate)
    {
        return await QueryAll().AnyAsync(predicate);
    }

    public async Task<int> CountAsync()
    {
        return await QueryAll().CountAsync();
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await QueryMany(predicate).CountAsync();
    }

    #region Helpers

    private static void ThrowEntityNotFoundException(Guid entityId)
    {
        throw new InvalidOperationException($"Failed to find entity of type '{typeof(TEntity)}' by id '{entityId}'.");
    }

    #endregion
}