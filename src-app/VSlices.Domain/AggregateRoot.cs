using VSlices.Domain.Interfaces;

namespace VSlices.Domain;

/// <inheritdoc cref="IEntity{TKey}" />
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot<TKey>
    where TKey : struct, IEquatable<TKey>
{
    /// <inheritdoc />
    public TKey Id { get; private set; }

    /// <summary>
    /// Empty constructor to use in serialization scenarios
    /// </summary>
    /// <remarks>Do not use this constructor in your code, if is not for serialization</remarks>
    protected AggregateRoot()
    {
        Id = default!;
    }

    /// <summary>
    /// Creates a new entity with the specified key
    /// </summary>
    /// <param name="id">The key of the entity</param>
    protected AggregateRoot(TKey id)
    {
        Id = id;
    }

    /// <inheritdoc/>
    public override string ToString() => this.EntityToString();

    /// <inheritdoc/>
    public virtual bool Equals(IEntity<TKey>? other) => this.EntityEquals(other);

    /// <inheritdoc/>
    public virtual bool Equals(IEntity? other) => this.EntityEquals(other);

}
