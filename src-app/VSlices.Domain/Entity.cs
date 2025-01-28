using VSlices.Domain.Interfaces;

namespace VSlices.Domain;

/// <inheritdoc cref="IEntity{TKey}" />
public abstract class Entity<TKey> : IEntity<TKey>
    where TKey : class, IEquatable<TKey>
{
    /// <inheritdoc />
    public TKey Id { get; private set; }

    /// <summary>
    /// Empty constructor to use in serialization scenarios
    /// </summary>
    /// <remarks>Do not use this constructor in your code, if is not for serialization</remarks>
    protected Entity()
    {
        Id = default!;
    }

    /// <summary>
    /// Creates a new entity with the specified key
    /// </summary>
    /// <param name="id">The key of the entity</param>
    protected Entity(TKey id) : this()
    {
        Id = id;
    }

    /// <inheritdoc/>
    public override string ToString() => this.EntityToString();

    /// <inheritdoc/>
    public virtual bool Equals(IEntity<TKey>? other) => this.EntityEquals(other);

}
