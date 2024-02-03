using System;

namespace MusicPlayerBackend.Data;

public interface IEntity<TKey> where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    bool IsNew();
}

public abstract record EntityBase : IEntity<Guid>
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual bool IsNew()
    {
        return Id == Guid.Empty;
    }
}
